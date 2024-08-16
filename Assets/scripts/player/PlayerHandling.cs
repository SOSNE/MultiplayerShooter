using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHhandling : NetworkBehaviour
{
    // NetworkBehaviourpublic NetworkList<int> playersHealth = new NetworkList<int>();
    private static Dictionary<ulong, int> _clientHealthMap = new Dictionary<ulong, int>();
    
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    
    
    private void OnClientConnected(ulong clientId)
    {
        if (!IsOwner) return;
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            NewClientConnectionServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    public void PlayerHit(int damageAmount,ulong clientId)
    {
        PlayerHitServerRpc(damageAmount, clientId);
    }

    [ServerRpc]
    private void NewClientConnectionServerRpc(ulong clientId ,ServerRpcParams serverRpcParams = default)
    {
        _clientHealthMap.Add(clientId, 10);
        foreach (var value  in _clientHealthMap)
        {
            print(value);
        }
    }
    
    [ServerRpc]
    private void PlayerHitServerRpc(int damageAmount,ulong clientId , ServerRpcParams serverRpcParams = default)
    {
        print(clientId);
        print("palyer hit hp: " + _clientHealthMap[clientId]);
        _clientHealthMap[clientId] -= damageAmount;
        foreach (var value  in _clientHealthMap)
        {
            print(value);
        }
        if (_clientHealthMap[clientId]<=0)
        {
            print("player of id: " + _clientHealthMap[clientId] + "died");
        }
        
    }
}
