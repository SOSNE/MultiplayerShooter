using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using Random = System.Random;

public struct PlayerData
{
    public ulong ClientId;
    public int Team;
    public NetworkObjectReference PlayerNetworkObject;
    public bool Alive;
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
    public static List<int> playersAlive = new List<int>();
    private TextMeshProUGUI teamOneWinCounter, teamTwoWinCounter;
    private static NetworkList<int> _pointScore  = new NetworkList<int>();
    private static int floatIndex;
    private Transform _team0Spawn, _team1Spawn;
    [SerializeField] private GameObject camera, pistol;
    public float cameraSmoothness;


    private void Start()
    {
        teamOneWinCounter = FindObjectInHierarchy("Team 0").GetComponent<TextMeshProUGUI>();
        teamTwoWinCounter = FindObjectInHierarchy("Team 1").GetComponent<TextMeshProUGUI>();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private GameObject _createdCamera;
    private static bool _pointScoreInitialize = true;
    public override void OnNetworkSpawn()
    {
        if (IsServer && _pointScoreInitialize)
        {
            _pointScore.Add(0);
            _pointScore.Add(0);
            playersAlive.Add(0);
            playersAlive.Add(0);
            _pointScoreInitialize = false;
        }
        base.OnNetworkSpawn();
        _pointScore.OnListChanged += UpdateWinCounter;
    }

    public void CreateCamera()
    {
        if(!IsOwner) return;
        _createdCamera = Camera.main.gameObject;
        // _createdCamera = Instantiate(camera, transform.position, transform.rotation);
        pistol.GetComponent<pistolMovment>().camera = _createdCamera.GetComponent<Camera>();
        gameObject.GetComponent<playerMovment>().camera = _createdCamera.GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        if(!IsOwner) return;
        // This part cause bug. I think this it should be in ServerRcp because AllPlayersData list is local for host.
        // if (AllPlayersData.FirstOrDefault(obj => obj.ClientId == NetworkManager.Singleton.LocalClientId).Alive)
        {
            Camera camera = _createdCamera.GetComponent<Camera>();
            Vector3 mouseScreenPosition = Input.mousePosition;
            Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, camera.nearClipPlane));
            Vector3 betweenPosition = Vector3.Lerp( transform.position, mouseWorldPosition, 0.4f);
            Vector3 smoothPosition = Vector3.Lerp( _createdCamera.transform.position, betweenPosition, cameraSmoothness);
            _createdCamera.transform.position = new Vector3(smoothPosition.x, smoothPosition.y, -10f);
        }
        
    }

    public void HandleGame(ulong currentClientId, ulong hitClientId, string hitBodyPartString , DataToSendOverNetwork data)
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
                playersAlive[AllPlayersData[i].Team] -= 1;
                performRagdollOnSelectedPlayerClientRpc(AllPlayersData[i].PlayerNetworkObject, hitBodyPartString, data);
            }
        }
        //restart game after all players are dead
        if (playersAlive[0] <= 0 || playersAlive[1] <= 0)
        {
            
            StartCoroutine(NextRoundCourtione(2, currentClientId, teamIndexOverwrite));
            
        }
    }

    private void RestartPlayersAliveList()
    {
        foreach (var Data in AllPlayersData)
        {
            if (Data is { Team: 0, Alive: true })
            {
                playersAlive[0] += 1;
            }
            if (Data is { Team: 1, Alive: true })
            {
                playersAlive[1] += 1;
            }
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
        positionsDistance = new List<int> { -3, -2, -1, 0, 1, 2 ,3 };

        _team0Spawn = GameObject.Find("Team0Spawn").transform;
        _team1Spawn = GameObject.Find("Team1Spawn").transform;
        foreach (var record in AllPlayersData)
        {
            if (AllPlayersData.FirstOrDefault(obj => obj.ClientId == record.ClientId).Team == 0)
            {
                NetworkObjectReference netObject = new NetworkObjectReference (
                    _team0Spawn.transform.GetComponent<NetworkObject>());
                SpawnPlayerOnSpawnPointClientRpc(record.PlayerNetworkObject, netObject);
            }else
            {
                NetworkObjectReference netObject = new NetworkObjectReference (
                    _team1Spawn.transform.GetComponent<NetworkObject>());
                SpawnPlayerOnSpawnPointClientRpc(record.PlayerNetworkObject, netObject);
            } 
        }
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
    
    private void UpdateWinCounter(NetworkListEvent<int> listEvent)
    {
        teamOneWinCounter.text = $"{_pointScore[0]}";
        teamTwoWinCounter.text = $"{_pointScore[1]}";
    }
    
    
    public void AddClientToTeam(ulong clientId)
    {
        if(!IsClient) return;
        AddClientToTeamServerRpc(gameObject, clientId);
    }
    [ServerRpc]
    private void AddClientToTeamServerRpc(NetworkObjectReference playerGameObject, ulong clientId, ServerRpcParams serverRpcParams = default)
    {
        _team0Spawn = GameObject.Find("Team0Spawn").transform;
        _team1Spawn = GameObject.Find("Team1Spawn").transform;
        PlayerData newUser = new PlayerData();
        newUser.ClientId = clientId;
        newUser.Team = floatIndex % 2;
        newUser.PlayerNetworkObject = playerGameObject;
        newUser.Alive = true;
        AllPlayersData.Add(newUser);
        playersAlive[floatIndex % 2] += 1;
        // teamsDictionary.Add(clientId, floatIndex%2);
        
        floatIndex++;
            if (AllPlayersData.FirstOrDefault(obj => obj.ClientId == clientId).Team == 0)
            {
                NetworkObjectReference netObject = new NetworkObjectReference (
                    _team0Spawn.transform.GetComponent<NetworkObject>());
                SpawnPlayerOnSpawnPointClientRpc(playerGameObject, netObject);
            }else
            {
                NetworkObjectReference netObject = new NetworkObjectReference (
                    _team1Spawn.transform.GetComponent<NetworkObject>());
                SpawnPlayerOnSpawnPointClientRpc(playerGameObject, netObject);
            }
    }
    public List<int> positionsDistance = new List<int> { -3, -2, -1, 0, 1, 2 ,3 };

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

    IEnumerator NextRoundCourtione(float duration, ulong clientId, int teamIndexOverwrite)
    {
        UpdatePointScoreDictionary(clientId, teamIndexOverwrite);
        yield return new WaitForSeconds(duration);
        ResetHealthMap();
        RestartPlayersAliveList();
        RestartPositions();
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
