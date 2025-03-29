using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class uiControler : NetworkBehaviour
{
    
    public TextMeshProUGUI ammoCounter;
    [SerializeField] private TextMeshProUGUI hpCounter, moneyCounter;
    public Transform trackingTransform;
    

    void Update()
    {
        if (!trackingTransform) return;
        if (!GetChildWithTag(trackingTransform, "weapon")) return;
        
        Transform weapon = trackingTransform.GetComponent<GameManager>().weapon.transform;
        float remainingBullets = weaponHandling.BulletCount -
                             weapon.GetComponent<weaponHandling>()
                                  .bulletCounter;
        ammoCounter.text = $"Bullets: {weaponHandling.BulletCount} / {remainingBullets}";
    }
    
    [ClientRpc]
    public void GetHealthForUiClientRpc(int currentHealth, ClientRpcParams clientRpcParams)
    {
        hpCounter.text = "Hp: "+ currentHealth;
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
