using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OfficeMapGameLogic : NetworkBehaviour
{
    public static OfficeMapGameLogic Instance;
    public static GameObject serverGameObjectReference;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void OnClientSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        GameManager gameManager = serverGameObjectReference.GetComponent<GameManager>();
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        
        TeleportPlayersToSpawn(clientId);
        PlayStartingTextMessageClientRpc(clientId, clientRpcParams);
        gameManager.StartCountdownTimerWithServerTimeClientRpc(10f);
    }

    [ClientRpc]
    private void PlayStartingTextMessageClientRpc(ulong clientId, ClientRpcParams clientRpcParams)
    {
        List<ulong> playerIds = new List<ulong>{clientId};
        PlayerData currentPlayerData = Utils.GetSelectedPlayersData(playerIds)[0];
        if (currentPlayerData.Team == 0)
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
