using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;
using Random = System.Random;

public class ObservableList<T>
{
    private List<T> _list = new List<T>();

    public event Action OnListChanged;

    public void Add(T item)
    {
        _list.Add(item);
        OnListChanged?.Invoke();
    }

    public void Remove(T item)
    {
        if (_list.Remove(item))
        {
            OnListChanged?.Invoke();
        }
    }

    // public int Count => _list.Count;

    public T this[int index]
    {
        get => _list[index];
        set
        {
            _list[index] = value;
            OnListChanged?.Invoke();
        }
    }
}


public struct PlayerData
{
    public ulong ClientId;
    public int Team;
    public NetworkObjectReference PlayerNetworkObject;
    public bool Alive;
    public string[] PlayerLoadout;
    public string PlayerName;
    public PlayerData(int loadoutSize)
    {
        ClientId = 0; 
        Team = 0; 
        PlayerNetworkObject = default; 
        Alive = false; 
        PlayerLoadout = new string[loadoutSize]; // Initialize the array with a specific size
        PlayerName = "player";
    }

}

public struct DataToSendOverNetwork: INetworkSerializable
{
    public Vector2 Direction;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Direction);
    }
}


public class GameManager : NetworkBehaviour
{
    public static List<PlayerData> AllPlayersData = new List<PlayerData>();
    private static Dictionary<ulong, int> teamsDictionary = new Dictionary<ulong, int>();
    private static List<int> _playersAlive = new List<int>();
    private TextMeshProUGUI teamOneWinCounter, teamTwoWinCounter;
    private static ObservableList<int> _pointScore = new ObservableList<int>();
    private static int floatIndex;
    private Transform _team0Spawn, _team1Spawn;
    public GameObject camera;
    [FormerlySerializedAs("pistol")] public GameObject weapon;
    private bool _teamAddingSetupDone = false;
    
    // private void Awake()
    // {
    //     if(!IsServer) return;
    //     _pointScore = new NetworkList<int>();
    // }
    
    private void Start()
    {
        teamOneWinCounter = FindObjectInHierarchy("Team 0").GetComponent<TextMeshProUGUI>();
        teamTwoWinCounter = FindObjectInHierarchy("Team 1").GetComponent<TextMeshProUGUI>();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private GameObject _createdCamera;
    private static bool _pointScoreInitialize = true;
    private static int _playerNameCount = 1;
    
    //this is called when someone connect
    public override void OnNetworkSpawn()
    {
        if (IsServer && _pointScoreInitialize)
        {
            _pointScore.Add(0);
            _pointScore.Add(0);
            _playersAlive.Add(0);
            _playersAlive.Add(0);
            _pointScoreInitialize = false;
        }

        base.OnNetworkSpawn();
        
        if(!IsOwner) return;
        
        StartCoroutine(StartGameLoadingQueue());
        if(!IsServer) return;
        _pointScore.OnListChanged += UpdateWinCounterServerRpc;
    }

    private IEnumerator StartGameLoadingQueue()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        yield return new WaitUntil(() => _teamAddingSetupDone);

        gameObject.GetComponent<weaponSpawning>().SpawnWeapon();
        
        yield return new WaitUntil(() => gameObject.GetComponent<weaponSpawning>().isWeaponSpawned);
        
        gameObject.GetComponent<GameManager>().CreateCamera();
        
        GameObject.Find("UiControler").
            GetComponent<uiControler>().trackingTransform = transform;
        UpdateWinCounterServerRpc();
        GameObject.Find("Camera Control").
            GetComponent<CameraControl>().currentPlayer = transform;
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (!IsOwner) return;
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            NewClientConnectionServerRpc(clientId);
            gameObject.GetComponent<GameManager>().AddClientToTeam(clientId);
        }
    }
    
    [ServerRpc]
    private void NewClientConnectionServerRpc(ulong clientId ,ServerRpcParams serverRpcParams = default)
    {
        PlayerHhandling.clientHealthMap.Add(clientId, 100);
        
        // update health ui
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        GameObject.Find("UiControler").GetComponent<uiControler>()
            .GetHealthForUiClientRpc(PlayerHhandling.clientHealthMap[clientId], clientRpcParams);
        AcknowledgeTeamAddingSetupClientRpc();
    }
    
    [ClientRpc]
    private void AcknowledgeTeamAddingSetupClientRpc()
    {
        _teamAddingSetupDone = true;
    }

    public void CreateCamera()
    {
        if (!IsOwner) return;
        _createdCamera = Camera.main.gameObject;
        // _createdCamera = Instantiate(camera, transform.position, transform.rotation);
        weapon.GetComponent<pistolMovment>().camera = _createdCamera.GetComponent<Camera>();
        gameObject.GetComponent<playerMovment>().camera = _createdCamera.GetComponent<Camera>();
    }



    public void HandleGame(ulong currentClientId, ulong hitClientId, string hitBodyPartString,
        DataToSendOverNetwork data)
    {
        int teamIndexOverwrite = 10;
        for (int i = 0; i < AllPlayersData.Count; i++)
        {
            if (AllPlayersData[i].ClientId == hitClientId)
            {

                //calculate teamIndexOverwrite for self killing.
                //if shoot himself
                if (currentClientId == hitClientId)
                {
                    if (AllPlayersData[i].Team == 0)
                    {
                        teamIndexOverwrite = 1;
                    }
                    else
                    {
                        teamIndexOverwrite = 0;
                    }

                }

                PlayerData myStruct = AllPlayersData[i];
                myStruct.Alive = false;
                AllPlayersData[i] = myStruct;
                _playersAlive[AllPlayersData[i].Team] -= 1;
                performRagdollOnSelectedPlayerClientRpc(AllPlayersData[i].PlayerNetworkObject, hitBodyPartString, data);
            }
        }

        //restart game after all players are dead
        if (_playersAlive[0] <= 0 || _playersAlive[1] <= 0)
        {
            StartCoroutine(NextRoundCoroutine(2, currentClientId, teamIndexOverwrite));
        }
    }

    private void RestartPlayersAliveList()
    {
        for (int i = 0; i < AllPlayersData.Count; i++)
        {
            var data = AllPlayersData[i];
            if (data is { Team: 0, Alive: false })
            {
                _playersAlive[0] += 1;
            }

            if (data is { Team: 1, Alive: false })
            {
                _playersAlive[1] += 1;
            }

            data.Alive = true;
            AllPlayersData[i] = data;
        }

        GameObject.Find("UiControler").GetComponent<uiControler>()
            .GetHealthForUiClientRpc(100, default);
    }

    public void ResetHealthMap()
    {
        List<ulong> keys = new List<ulong>(PlayerHhandling.clientHealthMap.Keys);

        foreach (var key in keys)
        {
            PlayerHhandling.clientHealthMap[key] = 100;
        }
    }

    public void RestartPositions()
    {
        //reset position list
        positionsDistance = new List<int> { -3, -2, -1, 0, 1, 2, 3 };

        _team0Spawn = GameObject.Find("Team0Spawn").transform;
        _team1Spawn = GameObject.Find("Team1Spawn").transform;
        foreach (var record in AllPlayersData)
        {
            if (AllPlayersData.FirstOrDefault(obj => obj.ClientId == record.ClientId).Team == 0)
            {
                NetworkObjectReference netObject = new NetworkObjectReference(
                    _team0Spawn.transform.GetComponent<NetworkObject>());
                SpawnPlayerOnSpawnPointClientRpc(record.PlayerNetworkObject, netObject);
            }
            else
            {
                NetworkObjectReference netObject = new NetworkObjectReference(
                    _team1Spawn.transform.GetComponent<NetworkObject>());
                SpawnPlayerOnSpawnPointClientRpc(record.PlayerNetworkObject, netObject);
            }
        }
    }

    [ClientRpc]
    private void RestartWeaponsMagazinesClientRpc()
    {
        weapon.GetComponent<weaponHandling>().bulletCounter = 0;
    }

    private void UpdatePointScoreDictionary(ulong clientId, int teamIndexOverwrite)
    {
        int teamIndex;
        if (teamIndexOverwrite == 10)
        {
            teamIndex = AllPlayersData.FirstOrDefault(obj => obj.ClientId == clientId).Team;
        }
        else
        {
            teamIndex = teamIndexOverwrite;
        }

        _pointScore[teamIndex] += 1;
    }
    
    [ServerRpc]
    private void UpdateWinCounterServerRpc()
    {
        UpdateWinCounterClientRpc(_pointScore[0], _pointScore[1]);
    }
    
    [ClientRpc]
    private void UpdateWinCounterClientRpc(int team0, int team1)
    {
        teamOneWinCounter.text = $"{team0}";
        teamTwoWinCounter.text = $"{team1}";
    }

    public void AddClientToTeam(ulong clientId)
    {
        if (!IsClient) return;
        AddClientToTeamServerRpc(gameObject, clientId);
    }

    //Possible change for name of this function to ClientSetupServerRpc
    [ServerRpc]
    private void AddClientToTeamServerRpc(NetworkObjectReference playerGameObject, ulong clientId,
        ServerRpcParams serverRpcParams = default)
    {
        _team0Spawn = GameObject.Find("Team0Spawn").transform;
        _team1Spawn = GameObject.Find("Team1Spawn").transform;
        PlayerData newUser = new PlayerData(5);
        newUser.ClientId = clientId;
        newUser.Team = floatIndex % 2;
        newUser.PlayerNetworkObject = playerGameObject;
        newUser.Alive = true;
        newUser.PlayerLoadout[0] = "pistol";
        newUser.PlayerName = "player: " + _playerNameCount;
        AllPlayersData.Add(newUser);
        _playersAlive[floatIndex % 2] += 1;
        // teamsDictionary.Add(clientId, floatIndex%2);

        _playerNameCount++;
        print(_playerNameCount);
        print(gameObject.name);
        floatIndex++;
        if (AllPlayersData.FirstOrDefault(obj => obj.ClientId == clientId).Team == 0)
        {
            NetworkObjectReference netObject = new NetworkObjectReference(
                _team0Spawn.transform.GetComponent<NetworkObject>());
            SpawnPlayerOnSpawnPointClientRpc(playerGameObject, netObject);
        }
        else
        {
            NetworkObjectReference netObject = new NetworkObjectReference(
                _team1Spawn.transform.GetComponent<NetworkObject>());
            SpawnPlayerOnSpawnPointClientRpc(playerGameObject, netObject);
        }

        for (int i =0; i<AllPlayersData.Count;i++)
        {
            //The following is done for every player that is connected.
            if (AllPlayersData[i].PlayerNetworkObject.TryGet(out NetworkObject playerNetworkObject))
            {
                if (AllPlayersData[i].Team == 0)
                {
                    AddTagToNewPlayerClientRpc(playerNetworkObject, "playerColliderTeam0");
                }
                else
                {
                    AddTagToNewPlayerClientRpc(playerNetworkObject, "playerColliderTeam1");
                }
                
                //Change name of new player on host and each client. 
                ChangeClientsNameClientRpc(playerNetworkObject, AllPlayersData[i].PlayerName);
            }
        }
        
        
    }
    
    [ClientRpc]
    private void ChangeClientsNameClientRpc(NetworkObjectReference playerGameObject, string playerName)
    {
        if(playerGameObject.TryGet(out NetworkObject playerNetworkObject))
        {
            playerNetworkObject.name = playerName;
        }
    }

    [ClientRpc]
    private void AddTagToNewPlayerClientRpc(NetworkObjectReference playerGameObject, string playerLayerName)
    {
        if(playerGameObject.TryGet(out NetworkObject playerNetworkObject))
        {
            //Check if current player is no longer assigned a layer
            if (playerNetworkObject.gameObject.layer == LayerMask.NameToLayer("playerColliderDeafult"))
            {
                playerNetworkObject.gameObject.layer = LayerMask.NameToLayer(playerLayerName);
            }
        }
    }
    public List<int> positionsDistance = new List<int> { -3, -2, -1, 0, 1, 2, 3 };
    
    [ClientRpc]
    private void SpawnPlayerOnSpawnPointClientRpc(NetworkObjectReference playerGameObject, NetworkObjectReference spawnGameObject)
    {
        if(playerGameObject.TryGet(out NetworkObject playerNetworkObject))
        {
            if(spawnGameObject.TryGet(out NetworkObject spawnNetworkObject))
            {
                if (positionsDistance.Count != 0)
                {
                    Random random = new Random();
                    int randomIndex = random.Next(positionsDistance.Count);
                    var randomNumberDistance = positionsDistance[randomIndex];
                    playerNetworkObject.transform.position = spawnNetworkObject.transform.position +
                                                             new Vector3(randomNumberDistance, 0, 0);
                    positionsDistance.RemoveAt(randomIndex);
                }
                else
                {
                    playerNetworkObject.transform.position = spawnNetworkObject.transform.position;
                }
            }
        }
    }

    
    [ClientRpc]
    private void performRagdollOnSelectedPlayerClientRpc(NetworkObjectReference playerNetworkObjectReference, string hitBodyPartString , DataToSendOverNetwork data)
    {
        if(playerNetworkObjectReference.TryGet(out NetworkObject playerGameObject))
        {
            gameObject.GetComponent<PlayerHhandling>().PerformRagdollOnPlayer(playerGameObject.transform, hitBodyPartString, data);
        }
    }
    
    IEnumerator NextRoundCoroutine(float duration, ulong clientId, int teamIndexOverwrite)
    {
        UpdatePointScoreDictionary(clientId, teamIndexOverwrite);
        yield return new WaitForSeconds(duration);
        ResetHealthMap();
        RestartPlayersAliveList();
        RestartPositions();
        RestartWeaponsMagazinesClientRpc();
        foreach (PlayerData playerData in AllPlayersData)
        {
            turnOfRagdollOnSelectedPlayerClientRpc(playerData.PlayerNetworkObject);
        }
    }
    
    [ClientRpc]
    private void turnOfRagdollOnSelectedPlayerClientRpc(NetworkObjectReference playerNetworkObjectReference)
    {
        if(playerNetworkObjectReference.TryGet(out NetworkObject playerGameObject))
        {
            gameObject.GetComponent<PlayerHhandling>().TurnRagdollOf(playerGameObject.transform);
        }
    }


    private GameObject FindObjectInHierarchy(string name)
    {
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>(true);
        foreach (GameObject go in allGameObjects)
        {
            if (go.name == name)
            {
                return go;
            }
        }
        return null;
    }
}
