using System;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;

public class NetworkMenagerUi : MonoBehaviour
{
    public Button host, client;
    private NetworkManager networkManager;
    
    public void OnInputIp(string ipAddress)
    {
        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData(ipAddress, 7777);
    }
    
    private void Awake()
    {
        host.onClick.AddListener((() => 
            NetworkManager.Singleton.StartHost()));
        
        client.onClick.AddListener((() => 
            NetworkManager.Singleton.StartClient()));
    }
}
