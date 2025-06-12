using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class uiControler : NetworkBehaviour
{
    
    public TextMeshProUGUI ammoCounter;
    [SerializeField] private TextMeshProUGUI hpCounter, moneyCounter, timer;
    public Transform trackingTransform;
    public static uiControler Instance;
    
    private void Awake()
    {
        Instance = this;
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
