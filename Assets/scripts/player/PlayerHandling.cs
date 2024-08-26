using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHhandling : NetworkBehaviour
{
    // NetworkBehaviourpublic NetworkList<int> playersHealth = new NetworkList<int>();
    private static Dictionary<ulong, int> _clientHealthMap = new Dictionary<ulong, int>();
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
        _clientHealthMap.Add(clientId, 10);
        foreach (var value  in _clientHealthMap)
        {
            print(value);
        }
    }
    
    [ServerRpc]
    private void PlayerHitServerRpc(int damageAmount,ulong clientId , ulong currentClientId, ServerRpcParams serverRpcParams = default)
    {
        print("palyer hit hp: " + _clientHealthMap[clientId]);
        _clientHealthMap[clientId] -= damageAmount;
        foreach (var value  in _clientHealthMap)
        {
            print(value);
        }
        if (_clientHealthMap[clientId]<=0)
        {
            print("player of id: " + _clientHealthMap[clientId] + "died");
            gameObject.GetComponent<GameManager>().UpdatePointScoreDictionary(currentClientId);
            gameObject.GetComponent<GameManager>().RestartPositions();
        }
    }
}
