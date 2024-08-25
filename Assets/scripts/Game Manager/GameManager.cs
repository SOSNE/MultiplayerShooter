using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private static Dictionary<ulong, int> teamsDictionary = new Dictionary<ulong, int>();
    private TextMeshProUGUI teamOneWinCounter, teamTwoWinCounter;
    private static NetworkList<int> _pointScore  = new NetworkList<int>();
    private static int floatIndex;
    private Transform _team0Spawn, _team1Spawn;
    [SerializeField] private GameObject camera, pistol;
    public float cameraSmoothness ;


    private void Start()
    {
        teamOneWinCounter = FindObjectInHierarchy("Team 0").GetComponent<TextMeshProUGUI>();
        teamTwoWinCounter = FindObjectInHierarchy("Team 1").GetComponent<TextMeshProUGUI>();
        // Cursor.lockState = CursorLockMode.Confined;
        // Cursor.visible = true;
        
    }

    private GameObject _createdCamera;
    private static bool _pointScoreInitialize = true;
    public override void OnNetworkSpawn()
    {
        if (IsServer && _pointScoreInitialize)
        {
            _pointScore.Add(0);
            _pointScore.Add(0);
            _pointScoreInitialize = false;
        }
        base.OnNetworkSpawn();
        _pointScore.OnListChanged += UpdateWinCounter;
    }

    public void CreateCamera()
    {
        if(!IsOwner) return;
        _createdCamera = Instantiate(camera, transform.position, transform.rotation);
        pistol.GetComponent<pistolMovment>().camera = _createdCamera.GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        if(!IsOwner) return;
        Camera camera = _createdCamera.GetComponent<Camera>();
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, camera.nearClipPlane));
        Vector3 betweenPosition = Vector3.Lerp( transform.position, mouseWorldPosition, 0.4f);
        Vector3 smoothPosition = Vector3.Lerp( _createdCamera.transform.position, betweenPosition, cameraSmoothness);
        print(Vector3.Distance(_createdCamera.transform.position, transform.position));
        _createdCamera.transform.position = new Vector3(smoothPosition.x, smoothPosition.y, -10f);
    }

    public void UpdatePointScoreDictionary(ulong clientId)
    {
        int teamIndex = teamsDictionary[clientId];
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
        teamsDictionary.Add(clientId, floatIndex%2);
        floatIndex++;
            if (teamsDictionary[clientId] == 0)
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
        
        foreach (var player in teamsDictionary)
        {
            print(player);
        }
    }

    [ClientRpc]
    private void SpawnPlayerOnSpawnPointClientRpc(NetworkObjectReference playerGameObject, NetworkObjectReference spawnGameObject)
    {
        if(playerGameObject.TryGet(out NetworkObject playerNetworkObject))
        {
            if(spawnGameObject.TryGet(out NetworkObject spawnNetworkObject))
            {
                playerNetworkObject.transform.position = spawnNetworkObject.transform.position;
            }
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
