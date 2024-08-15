using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHhandling : NetworkBehaviour
{
    private NetworkList<int> _playersHealth;
    
    [SerializeField] private GameObject gameManager;
    
    private void Awake()
    {
        _playersHealth = new NetworkList<int>();
    }
    
    public override void OnNetworkSpawn()
    {
        
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    private void OnClientConnected(ulong clientId)
    {
        print("connected with id:" +  clientId);
    }

    public void PlayerHit()
    {
        
    }
}
