using TMPro;
using Unity.Netcode;
using UnityEngine;

public class uiControler : NetworkBehaviour
{
    public TextMeshProUGUI ammoCounter;
    [SerializeField] private TextMeshProUGUI hpCounter;
    
    void Update()
    {
        float remainingBullets = weaponHandling.BulletCount - weaponHandling.BulletCounter;
        ammoCounter.text = $"Bulets: {weaponHandling.BulletCount} / {remainingBullets}";
    }
    
    [ClientRpc]
    public void GetHealthForUiClientRpc(int currentHealth, ClientRpcParams clientRpcParams)
    {
        hpCounter.text = "Hp: "+ currentHealth;
    }
}
