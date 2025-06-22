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
    public Button host, client, copyCodeButton;
    public Toggle conectionMod, allowFriendlyFireMod;
    public TextMeshProUGUI codeTextMeshPro;
    private NetworkManager networkManager;
    private string _ip = "127.0.0.1", _code;
    private Action _startActionHost, _startActionClient;
    
    public void OnInputIp(string ipAddress)
    {
        _ip = ipAddress;
    }
    
    private void Awake()
    {
        _startActionHost = () => StartHostWithRelay();
        _startActionClient = () => StartClientWithRelay(_ip);
        
        host.onClick.AddListener(() =>
        {
            _startActionHost?.Invoke();
        });
        
        client.onClick.AddListener(() =>
        {
            _startActionClient?.Invoke();
        });
        copyCodeButton.onClick.AddListener(() =>
        {
            Utils.Instance.CopyText(_code);
        });
        
        conectionMod.onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                _startActionHost = () =>
                {
                    UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                    unityTransport.SetConnectionData("127.0.0.1", 7777);
                };
                _startActionHost += () => NetworkManager.Singleton.StartHost();
                
                _startActionClient = () =>
                {
                    UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                    unityTransport.SetConnectionData(_ip, 7777);
                };
                _startActionClient += () => NetworkManager.Singleton.StartClient();
                
            }
            else
            {
                _startActionHost = () => StartHostWithRelay();
                _startActionClient = () => StartClientWithRelay(_ip);
            }
        });
        allowFriendlyFireMod.onValueChanged.AddListener((bool isOn) =>
        {
            Utils.Instance.allowFriendlyFire.Value = isOn;
        });
    }
    
    public async Task<string> StartHostWithRelay(int maxConnections = 4)
    {
        await Unity.Services.Core.UnityServices.InitializeAsync();

        if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();

        var allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(new RelayServerData(allocation, "wss"));

        _code = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        print(_code);
        codeTextMeshPro.text = _code;
        codeTextMeshPro.gameObject.SetActive(true);
        Utils.Instance.CopyText(_code);
        copyCodeButton.gameObject.SetActive(true);
        allowFriendlyFireMod.gameObject.SetActive(true);
        return NetworkManager.Singleton.StartHost() ? _code : null;
    }
    
    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        await Unity.Services.Core.UnityServices.InitializeAsync();

        if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();

        var allocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(new RelayServerData(allocation, "wss"));

        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
