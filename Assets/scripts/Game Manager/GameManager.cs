using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
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
    public NetworkObjectReference PlayerNetworkObjectReference;
    public bool Alive;
    public string[] PlayerLoadout;
    public string PlayerName;
    public int MoneyAmount;
    public Color PlayerColor;
    public int[] Kda;
    
    public PlayerData(int loadoutSize)
    {
        ClientId = 0; 
        Team = 0; 
        PlayerNetworkObjectReference = default; 
        Alive = false; 
        PlayerLoadout = new string[loadoutSize]; // Initialize the array with a specific size
        PlayerName = "player";
        MoneyAmount = 60;
        PlayerColor = Color.green;
        Kda = new int[3];
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
    private Transform _team0Spawn, _team1Spawn;
    public GameObject camera;
    [FormerlySerializedAs("pistol")] public GameObject weapon;
    private bool _teamAddingSetupDone = false, _roundIsRestarting = false;
    private static float _remainingTime = 120;
    private static Coroutine _timerCoroutine;
    public bool isAlive = true;
    public GameObject targetNameTag;

    
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

    private void Update()
    {
        if (!IsOwner) return;  
        
        FieldOfView.targetFovPositionOrigin = Utils.GetSpecificChild(gameObject, "head").transform.position;

        if (Input.GetKeyDown(KeyCode.P))
        {
            NetworkManager.Singleton.SceneManager.LoadScene("That’sWhatSheSaid", LoadSceneMode.Single);
        }
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
        GameObject.Find("UiControler").
            GetComponent<shopUi>().trackingTransform = transform;
        UpdateWinCounterServerRpc();
        GameObject.Find("Camera Control").
            GetComponent<CameraControl>().currentPlayer = transform;
        GameObject.Find("UiControler").GetComponent<uiControler>()
            .UpdateMoneyAmountUiServerRpc(60);
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconect;
        
        uiControler.Instance.AddNameTagsForEachPlayerServerRpc();
        
        //following is done for eatery connected client.
        //nice pattern for this inside this function.
        // if (!AllPlayersData[i].PlayerNetworkObjectReference.TryGet(out NetworkObject targetNetworkObject));
        AddLayerToBodyPartsForFovServerRpc("behindMask");
         // AddLayerToBodyPartsForFovClientRpc(AllPlayersData[i].PlayerNetworkObjectReference, "behindMask");
        print("start");
    }

    
    void OnDisable() {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconect;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientConnected;
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (!IsOwner) return;
        if (!IsClient) return;
        
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            gameObject.GetComponent<GameManager>().NewClientSetupServerRpc(gameObject, clientId, uiControler.Instance.playerSelectedName);
        }
    }
    
    private void OnClientDisconect(ulong clientId)
    {
        if(!IsServer) return;
        HandlePlayerDisconection(clientId);
    }

    private void HandlePlayerDisconection(ulong clientId)
    {
        print($"Client {clientId} disconnected.");
        for (int i = 0; i < AllPlayersData.Count; i++)
        {
            var data = AllPlayersData[i];
            if (data.ClientId == clientId)
            {
                _playersAlive[data.Team] -= 1;
                if (data.PlayerNetworkObjectReference.TryGet(out NetworkObject playerGameObject))
                {
                    playerGameObject.Despawn(destroy: true);
                }
                AllPlayersData.RemoveAt(i);
                break;
            }
        }
    }
    
    private void NewClientHealthSetup(ulong clientId ,ServerRpcParams serverRpcParams = default)
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
        gameObject.GetComponent<playerMovment>().camera = _createdCamera.GetComponent<Camera>();
    }
    
    private IEnumerator CountdownTimerStart(float time)
    {
        _remainingTime = time;
        while (_remainingTime >= 0)
        {
            
            uiControler.Instance.UpdateTimer(_remainingTime);
            yield return new WaitForSeconds(1f);
            _remainingTime -= 1f;
        }
        if (IsServer)
        {
            StartCoroutine(NextRoundCoroutine(2, 0, 10));
        }
    }

    [ClientRpc]
    private void StartCountdownTimerWithServerTimeClientRpc(float time, ClientRpcParams serverRpcParams = default)
    {
        //When called, it is being done on the first player game object.
        //But _timerCoroutine and time are static, so it doesn't matter, I think
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }
        _timerCoroutine = StartCoroutine(CountdownTimerStart(time));
    }
    
    // this is in ServerRpc
    public void HandleGame(ulong currentClientId, ulong hitClientId, string hitBodyPartString,
        DataToSendOverNetwork data, ServerRpcParams serverRpcParams = default)
    {
        int teamIndexOverwrite = 10;
        for (int i = 0; i < AllPlayersData.Count; i++)
        {
            if (AllPlayersData[i].ClientId == hitClientId)
            {
                PlayerData myStruct = AllPlayersData[i];
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
                else
                {
                    // Add death when killed by someone instead of by himself.
                    myStruct.Kda[1] += 1;
                }

                
                myStruct.Alive = false;
                AllPlayersData[i] = myStruct;
                _playersAlive[AllPlayersData[i].Team] -= 1;
                uiControler.Instance.UpdateTabStatisticsMenuClientRpc(currentClientId, AllPlayersData[i].Team, AllPlayersData[i].PlayerName, AllPlayersData[i].Kda[0], AllPlayersData[i].Kda[1], AllPlayersData[i].Kda[2], AllPlayersData[i].MoneyAmount, AllPlayersData[i].Alive);

                var clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { hitClientId }
                    }
                };

                SetPlayerIsAliveClientRpc(false, AllPlayersData[i].PlayerNetworkObjectReference, clientRpcParams);
                performRagdollOnSelectedPlayerClientRpc(AllPlayersData[i].PlayerNetworkObjectReference, hitBodyPartString, data);
            }
            
            // Add money for player after kill
            if (AllPlayersData[i].ClientId == currentClientId)
            {
                for (int j = 0; j < AllPlayersData.Count; j++)
                {
                    // Potencial fix to the Kda bug
                    // if (AllPlayersData[j].ClientId == hitClientId)
                    // {
                        if (currentClientId != hitClientId && AllPlayersData[i].Team != AllPlayersData[j].Team)
                        {
                            MoneyOperationUtils.Instance.UpdatePlayerMoneyAmountServerRpc(300, currentClientId);
                            PlayerData myStruct = AllPlayersData[i];
                            myStruct.Kda[0] += 1;
                            AllPlayersData[i] = myStruct;
                            uiControler.Instance.UpdateTabStatisticsMenuClientRpc(hitClientId,AllPlayersData[i].Team, AllPlayersData[i].PlayerName, AllPlayersData[i].Kda[0], AllPlayersData[i].Kda[1],AllPlayersData[i].Kda[2],AllPlayersData[i].MoneyAmount, AllPlayersData[i].Alive);
                        }
                    // }
                }
            }
        }
        //restart game after all players are dead
        if ((_playersAlive[0] <= 0 || _playersAlive[1] <= 0) && !_roundIsRestarting)
        {
            _roundIsRestarting = true;
            StartCoroutine(NextRoundCoroutine(2, currentClientId, teamIndexOverwrite));
        }
    }

    [ClientRpc]
    private void SetPlayerIsAliveClientRpc(bool isAliveBool,NetworkObjectReference networkObjectReferenceTarget = default, ClientRpcParams clientRpcParams = default)
    {
        if (!networkObjectReferenceTarget.Equals(default) && networkObjectReferenceTarget.TryGet(out NetworkObject targetNetworkObject))
        {
            targetNetworkObject.GetComponent<GameManager>().isAlive = isAliveBool;
        }
        else
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("player");

            foreach (GameObject obj in taggedObjects)
            {
                
                obj.GetComponent<GameManager>().isAlive = isAliveBool;
            }
        }
    }

    private void RestartPlayersAliveList()
    {
        _playersAlive[0] = 0;
        _playersAlive[1] = 0;
        for (int i = 0; i < AllPlayersData.Count; i++)
        {
            var data = AllPlayersData[i];
            if (data is { Team: 0 })
            {
                _playersAlive[0] += 1;
            }

            if (data is { Team: 1 })
            {
                _playersAlive[1] += 1;
            }

            data.Alive = true;
            AllPlayersData[i] = data;
            uiControler.Instance.UpdateTabStatisticsMenuClientRpc(AllPlayersData[i].ClientId, AllPlayersData[i].Team, AllPlayersData[i].PlayerName, AllPlayersData[i].Kda[0], AllPlayersData[i].Kda[1],AllPlayersData[i].Kda[2],AllPlayersData[i].MoneyAmount, AllPlayersData[i].Alive);
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
                SpawnPlayerOnSpawnPointClientRpc(record.PlayerNetworkObjectReference, netObject);
            }
            else
            {
                NetworkObjectReference netObject = new NetworkObjectReference(
                    _team1Spawn.transform.GetComponent<NetworkObject>());
                SpawnPlayerOnSpawnPointClientRpc(record.PlayerNetworkObjectReference, netObject);
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

    // public void AddClientToTeam(ulong clientId)
    // {
    //     if (!IsClient) return;
    //     AddClientToTeamServerRpc(gameObject, clientId);
    // }

    //Possible change for name of this function to ClientSetupServerRpc
    [ServerRpc]
    private void NewClientSetupServerRpc(NetworkObjectReference playerGameObject, ulong clientId, string playerName,
        ServerRpcParams serverRpcParams = default)
    {
        int floatIndex = AllPlayersData.Count;
        _team0Spawn = GameObject.Find("Team0Spawn").transform;
        _team1Spawn = GameObject.Find("Team1Spawn").transform;
        PlayerData newUser = new PlayerData(5);
        newUser.ClientId = clientId;
        newUser.Team = floatIndex % 2;
        newUser.PlayerNetworkObjectReference = playerGameObject;
        newUser.Alive = true;
        newUser.PlayerLoadout[0] = "pistol";
        if (playerName != "")
        {
            newUser.PlayerName = playerName;
        }
        else
        {
            newUser.PlayerName = "player: " + _playerNameCount;
        }
        
        if (newUser.Team == 0)
        {
            newUser.PlayerColor = new Color(0.2706f, 0.3098f, 0.1333f, 1f);
        }
        else
        {
            newUser.PlayerColor = new Color(0.1333f, 0.1608f, 0.3098f, 1f);
        }
        newUser.Kda[0] = 0;
        newUser.Kda[1] = 0;
        newUser.Kda[2] = 0;

        
        AllPlayersData.Add(newUser);
        _playersAlive[floatIndex % 2] += 1;
        
        // teamsDictionary.Add(clientId, floatIndex%2);

        _playerNameCount++;
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

        for (int i = 0; i<AllPlayersData.Count;i++)
        {
            //TODO meyby some of this can be done only for conected game ojbect on evry client. 
            //The following is done for every player that is connected.
            if (AllPlayersData[i].PlayerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
            {
                if (AllPlayersData[i].Team == 0)
                {
                    AddTagToNewPlayerClientRpc(playerNetworkObject, "playerColliderTeam0");
                }
                else
                {
                    AddTagToNewPlayerClientRpc(playerNetworkObject, "playerColliderTeam1");
                }
                //Add stats bar for every client for new player.
                uiControler.Instance.UpdateTabStatisticsMenuClientRpc(AllPlayersData[i].ClientId, AllPlayersData[i].Team, AllPlayersData[i].PlayerName, AllPlayersData[i].Kda[0], AllPlayersData[i].Kda[1],AllPlayersData[i].Kda[2],AllPlayersData[i].MoneyAmount, AllPlayersData[i].Alive);
                //Change name of new player on host and each client. 
                ChangeClientsNameClientRpc(playerNetworkObject, AllPlayersData[i].PlayerName);
                //Change name of new player on host and each client. 
                ChangeClientsColorClientRpc(playerNetworkObject, AllPlayersData[i].PlayerColor);
                
            }
        }
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        StartCountdownTimerWithServerTimeClientRpc(_remainingTime, clientRpcParams);
        NewClientHealthSetup(clientId);
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
    private void ChangeClientsColorClientRpc(NetworkObjectReference playerGameObject, Color color)
    {
        if(playerGameObject.TryGet(out NetworkObject playerNetworkObject))
        {
            playerNetworkObject.transform.Find("bodyDown").Find("AmmoBox").GetComponent<SpriteRenderer>().color = color;
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
    [ServerRpc]
    private void AddLayerToBodyPartsForFovServerRpc(string playerLayerName)
    {
        for (int i = 0; i < AllPlayersData.Count; i++)
        {
            AddLayerToBodyPartsForFovClientRpc(AllPlayersData[i].PlayerNetworkObjectReference, playerLayerName);

        }
    }
    
    [ClientRpc]
    private void AddLayerToBodyPartsForFovClientRpc(NetworkObjectReference playerGameObject, string playerLayerName)
    {
        if(playerGameObject.TryGet(out NetworkObject playerNetworkObject))
        {

            if (playerNetworkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                return;

            Utils.DoForAllChildren(playerNetworkObject.gameObject, (child) =>
            {
                if (child.GetComponent<SpriteRenderer>())
                {
                    child.gameObject.layer = LayerMask.NameToLayer(playerLayerName);
                }
            } );
            //Set behind mask to the name tag.
            playerNetworkObject.GetComponent<GameManager>().targetNameTag.layer = LayerMask.NameToLayer(playerLayerName);
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

    
    private void RestartTimer()
    {
        _remainingTime = 120;
        StartCountdownTimerWithServerTimeClientRpc(_remainingTime);
    }
    
    IEnumerator NextRoundCoroutine(float duration, ulong clientId, int teamIndexOverwrite)
    {
        UpdatePointScoreDictionary(clientId, teamIndexOverwrite);
        yield return new WaitForSeconds(duration);
        ResetHealthMap();
        RestartPlayersAliveList();
        SetPlayerIsAliveClientRpc(true);
        RestartPositions();
        RestartWeaponsMagazinesClientRpc();
        RestartTimer();
        foreach (PlayerData playerData in AllPlayersData)
        {
            turnOfRagdollOnSelectedPlayerClientRpc(playerData.PlayerNetworkObjectReference);
        }

        _roundIsRestarting = false;
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
