using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class uiControler : NetworkBehaviour
{

    public AudioMixer mixer;
    public Scrollbar volumeScrollbar;
    public TextMeshProUGUI ammoCounter;
    [SerializeField] private TextMeshProUGUI hpCounter, moneyCounter, timer;
    public Transform trackingTransform;
    public static uiControler Instance;
    public GameObject canvasWorldSpace, playerNameTextMechProPrephab, tabStatisticsMenu, playerInfoTextPrephab;
    public string playerSelectedName = "";
    public TMP_InputField playerNameSelectionInputField;
    
    private void Awake()
    {
        Instance = this;
        volumeScrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
        playerNameSelectionInputField.onValueChanged.AddListener(OnplayerNameSelectionInputFieldValueChange);
    }

    private void OnplayerNameSelectionInputFieldValueChange(string value)
    {
        playerSelectedName = value;
    }
    
    private void OnScrollbarValueChanged(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 15f;
        mixer.SetFloat("MyExposedParamMaster", dB);
    }

    void Update()
    {
        if (!trackingTransform) return;
        if (!GetChildWithTag(trackingTransform, "weapon")) return;
        
        Transform weapon = trackingTransform.GetComponent<GameManager>().weapon.transform;
        float remainingBullets = weapon.GetComponent<weaponHandling>().bulletCount -
                                 weapon.GetComponent<weaponHandling>()
                                     .bulletCounter;
        ammoCounter.text = $"Bullets: {weapon.GetComponent<weaponHandling>().bulletCount} / {remainingBullets}";
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenTabStatisticsMenuServerRpc();
            // ShopUiOpen = true;
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            // ShopUiOpen = false;
            tabStatisticsMenu.SetActive(false);
            foreach (Transform child in tabStatisticsMenu.transform.Find("Team0TabKDA").Find("Viewport"))
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach (Transform child in tabStatisticsMenu.transform.Find("Team1TabKDA").Find("Viewport"))
            {
                GameObject.Destroy(child.gameObject);
            }

        } 
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void OpenTabStatisticsMenuServerRpc(ServerRpcParams rpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId } }
        };
        foreach (var data in GameManager.AllPlayersData)
        {
            OpenTabStatisticsMenuClientRpc(data.Team, data.PlayerName, data.Kda[0],data.Kda[1],data.Kda[2], clientRpcParams);
        }
    }

    [ClientRpc]
    private void OpenTabStatisticsMenuClientRpc(int team, string playerName, int kills, int deaths, int asitsts, ClientRpcParams clientRpcParams)
    {
        if (team == 0)
        {
           GameObject createdPlayerTextInfo = Instantiate(playerInfoTextPrephab, tabStatisticsMenu.transform.Find("Team0TabKDA").Find("Viewport"));
           createdPlayerTextInfo.GetComponent<TextMeshProUGUI>().text = $"{playerName} | Kills: {kills}  Deaths: {deaths}";

        }else if (team == 1)
        {
            GameObject createdPlayerTextInfo = Instantiate(playerInfoTextPrephab, tabStatisticsMenu.transform.Find("Team1TabKDA").Find("Viewport"));
            createdPlayerTextInfo.GetComponent<TextMeshProUGUI>().text = $"{playerName} | Kills: {kills}  Deaths: {deaths}";
        }
        if(!tabStatisticsMenu.activeSelf) tabStatisticsMenu.SetActive(true);
        
    }

    [ClientRpc]
    public void GetHealthForUiClientRpc(int currentHealth, ClientRpcParams clientRpcParams)
    {
        hpCounter.text = "Hp: " + currentHealth;
    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdateMoneyAmountUiServerRpc(int moneyAmount, ServerRpcParams rpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId } }
        };
        UpdateMoneyAmountUiClientRpc(moneyAmount, clientRpcParams);
    } 
    
    [ClientRpc]
    public void UpdateMoneyAmountUiClientRpc(int moneyAmount, ClientRpcParams clientRpcParams)
    {
        moneyCounter.text = "Golden Shekels: " + moneyAmount;
    }
    
    public void UpdateTimer(float time)
    {
        timer.text = "Time left: " + time;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void AddNameTagsForEachPlayerServerRpc()
    {
        //Posible bandwith improvment:
        //Instade of sending ClientRpc for every player we can send one ClientRpc with list of evry player.
        foreach (var data in GameManager.AllPlayersData)
        {
            AddNameTagsForEachPlayerClientRpc(data.PlayerNetworkObjectReference, data.PlayerName);
        }
    }
    
    [ClientRpc]
    public void AddNameTagsForEachPlayerClientRpc(NetworkObjectReference targetPlayer, string name)
    {
        //SetActive = false for player name input field.
        if (playerNameSelectionInputField.gameObject.activeSelf) playerNameSelectionInputField.gameObject.SetActive(false);
        
        if(!targetPlayer.TryGet(out NetworkObject targetPlayerNetworkObject)) return;
        GameObject newplayerNameTextMechProGameObject = Instantiate(playerNameTextMechProPrephab, canvasWorldSpace.transform);
        newplayerNameTextMechProGameObject.GetComponent<TextMeshProUGUI>().text = name;
        newplayerNameTextMechProGameObject.GetComponent<TextMeshProUGUI>().text = name;
        newplayerNameTextMechProGameObject.GetComponent<PlayerNameTagControl>().target = targetPlayerNetworkObject.gameObject;
    }
    
    Transform GetChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }
        return null;
    }
}
