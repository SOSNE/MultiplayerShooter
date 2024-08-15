using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkList<int> result;
    [SerializeField] private TextMeshProUGUI teamOneWinCounter, teamTwoWinCounter;
    
    private void Awake()
    {
        result = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        result.OnListChanged += UpdateWinCounter;
        base.OnNetworkSpawn();
    }
    
    
    private void UpdateWinCounter(NetworkListEvent<int> changeEvent)
    {
        teamOneWinCounter.text = $"{result[0]}";
        teamTwoWinCounter.text = $"{result[1]}";
    }

    private void OnConnectedToServer()
    {
        
    }
}
