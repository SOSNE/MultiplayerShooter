using System;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class NetworkMenagerUi : MonoBehaviour
{
    public Button host, client;
    private NetworkManager networkManager;
    private string _ip;
    
    public void OnInputIp(string ipAddress)
    {
        // UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        // unityTransport.SetConnectionData(ipAddress, 7777);
        _ip = ipAddress;
    }
    
    private void Awake()
    {
        host.onClick.AddListener((() =>
        {
            // NetworkManager.Singleton.StartHost();
            PlayerPrefs.SetString("ip", "HOSTDONOTPASSASSIP");
            SceneManager.LoadScene("SampleScene");
        }));
        
        client.onClick.AddListener((() =>
        {
            // NetworkManager.Singleton.StartClient();
            PlayerPrefs.SetString("ip", _ip);
            SceneManager.LoadScene("SampleScene");
        }));
    }
}
