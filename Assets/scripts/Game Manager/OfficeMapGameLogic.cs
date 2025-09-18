using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OfficeMapGameLogic : NetworkBehaviour
{
    public static OfficeMapGameLogic Instance;
    public static GameObject serverGameObjectReference;
    private GameObject _clientGameObject, _gameObjective;
    private bool _theMapIsOpen = false;
    
    private void Awake()
    {
        Instance = this;
        
    }
    
    private bool _isShowingTextFlag = false, _objectiveStart = false;

    private void Update()
    {
        if(!_theMapIsOpen) return;
        if (!_objectiveStart && Vector3.Distance(_clientGameObject.transform.position, _gameObjective.transform.position) <= 2f)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                _objectiveStart = true;
                PerformGameObjectiveLogicServerRpc();
            }
            Utils.Instance.TextInformationSystem("Pres E to place the drill", 1, .06f, 2f);
            _isShowingTextFlag = false;
        }
        else if (!_isShowingTextFlag)
        {
            Utils.Instance.StopTextInformationSystem(1);
            _isShowingTextFlag = true;
        }
    }
    
    
    [ServerRpc]
    private void PerformGameObjectiveLogicServerRpc()
    {
        GameManager gameManager = serverGameObjectReference.GetComponent<GameManager>();
        gameManager.StartCountdownTimerWithServerTimeClientRpc(30f);
        PerformGameObjectiveLogicClientRpc();
    }
    
    [ClientRpc]
    private void PerformGameObjectiveLogicClientRpc()
    {
        Utils.Instance.TextInformationSystem("Drill was planted", 0, .06f, 2f);
        _gameObjective.transform.Find("Drill").gameObject.SetActive(true);
        StartCoroutine(StartDrillTimerFinishVisuals(30));
    }
    

    IEnumerator StartDrillTimerFinishVisuals(int durationTime)
    {
        int remainingTime = durationTime;
        while (remainingTime >= 0)
        {
            yield return new WaitForSeconds(1f);
            remainingTime -= 1;
        }
        Utils.Instance.TextInformationSystem("Raiders won", 0, .06f, 2f);
    }
    public void OnClientSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (!IsServer) return;
        if (sceneName != "Office") return;
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        
        TeleportPlayersToSpawn(clientId);
        
        List<ulong> playerIds = new List<ulong>{clientId};
        PlayerData currentPlayerData = Utils.GetSelectedPlayersData(playerIds)[0];
        PlayStartingTextMessageClientRpc(currentPlayerData.Team, clientRpcParams);
        StartingMapSetupClientRpc(clientRpcParams);
        GameManager gameManager = serverGameObjectReference.GetComponent<GameManager>();
        gameManager.StartCountdownTimerWithServerTimeClientRpc(10f);
    }
    
    [ClientRpc]
    private void StartingMapSetupClientRpc(ClientRpcParams clientRpcParams)
    {
        _clientGameObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().gameObject;
        _gameObjective = GameObject.Find("GameObjective");
        _theMapIsOpen = true;
    }

    [ClientRpc]
    private void PlayStartingTextMessageClientRpc(int currentPlayerDataTeam, ClientRpcParams clientRpcParams)
    {
        if (currentPlayerDataTeam == 0)
        {
            Utils.Instance.TextInformationSystem("Objective: Steal the documents. If impossible — eliminate all agents.", 0, .1f, 5f);
        }
        else
        {
            Utils.Instance.TextInformationSystem("Objective: Protect the documents or eliminate all Raiders.", 0, .1f, 5f);
        }
    }
    
    private void TeleportPlayersToSpawn(ulong clientId)
    {
        List<ulong> playerIds = new List<ulong>{clientId};
        PlayerData currentPlayerData = Utils.GetSelectedPlayersData(playerIds)[0];
        
        GameObject spawnPoint = GameObject.Find($"Team{currentPlayerData.Team}Spawn");
        Utils.Instance.SpawnPlayerOnSpawnPointClientRpc(currentPlayerData.PlayerNetworkObjectReference, spawnPoint);
    }
}
