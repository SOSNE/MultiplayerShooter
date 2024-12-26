using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : NetworkBehaviour
{
    private void Start()
    {
        string ip = PlayerPrefs.GetString("ip");
        print(ip);
        if (ip == "HOSTDONOTPASSASSIP")
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            unityTransport.SetConnectionData(ip, 7777);
            NetworkManager.Singleton.StartClient();
        }
    }

}
