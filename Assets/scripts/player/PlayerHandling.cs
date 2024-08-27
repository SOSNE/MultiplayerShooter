using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHhandling : NetworkBehaviour
{
    // NetworkBehaviourpublic NetworkList<int> playersHealth = new NetworkList<int>();
    public static Dictionary<ulong, int> clientHealthMap = new Dictionary<ulong, int>();
    private GameObject _gameManager;
    
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            _gameManager = GameObject.Find("Game Manager");
            gameObject.GetComponent<GameManager>().CreateCamera();
        }
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

    public void PlayerHit(int damageAmount,ulong clientId)
    {
        
        ulong currentClientId = NetworkManager.Singleton.LocalClientId;
        PlayerHitServerRpc(damageAmount, clientId, currentClientId);
    }

    [ServerRpc]
    private void NewClientConnectionServerRpc(ulong clientId ,ServerRpcParams serverRpcParams = default)
    {
        clientHealthMap.Add(clientId, 10);
    }
    
    [ServerRpc]
    private void PlayerHitServerRpc(int damageAmount,ulong clientId , ulong currentClientId, ServerRpcParams serverRpcParams = default)
    {
        clientHealthMap[clientId] -= damageAmount;
        if (clientHealthMap[clientId]<=0)
        {
            gameObject.GetComponent<GameManager>().HandleGame(currentClientId, clientId);
        }
    }
}
