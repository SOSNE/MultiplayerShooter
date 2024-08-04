using System;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkMenagerUi : MonoBehaviour
{
    public Button host, client;

    private void Awake()
    {
        host.onClick.AddListener((() => 
            NetworkManager.Singleton.StartHost()));
        
        client.onClick.AddListener((() => 
            NetworkManager.Singleton.StartClient()));
    }
}
