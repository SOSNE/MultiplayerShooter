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
        TeleportPlayersToSpawn(clientId);
        gameManager.StartCountdownTimerWithServerTimeClientRpc(10f);
    }

    private void TeleportPlayersToSpawn(ulong clientId)
    {
        List<ulong> playerIds = new List<ulong>{clientId};
        PlayerData currentPlayerData = Utils.GetSelectedPlayersData(playerIds)[0];
        
        GameObject spawnPoint = GameObject.Find($"Team{currentPlayerData.Team}Spawn");
        Utils.Instance.SpawnPlayerOnSpawnPointClientRpc(currentPlayerData.PlayerNetworkObjectReference, spawnPoint);
    }
}
