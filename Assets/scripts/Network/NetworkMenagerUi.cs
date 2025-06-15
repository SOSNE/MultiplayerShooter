using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
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
            StartHostWithRelay();
            // NetworkManager.Singleton.StartHost();
            // PlayerPrefs.SetString("ip", "HOSTDONOTPASSASSIP");
            // SceneManager.LoadScene("SampleScene");
        }));
        
        client.onClick.AddListener((() =>
        {
            StartClientWithRelay(_ip);
            // NetworkManager.Singleton.StartClient();
            // PlayerPrefs.SetString("ip", _ip);
            // SceneManager.LoadScene("SampleScene");
        }));
    }
    
    public async Task<string> StartHostWithRelay(int maxConnections = 4)
    {
        await Unity.Services.Core.UnityServices.InitializeAsync();

        if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();

        var allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(new RelayServerData(allocation, "wss"));

        var joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        print(joinCode);
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }
    
    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        await Unity.Services.Core.UnityServices.InitializeAsync();

        if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();

        print(joinCode);
        var allocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(new RelayServerData(allocation, "wss"));

        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
