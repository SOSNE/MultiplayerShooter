using TMPro;
using Unity.Netcode;
using UnityEngine;

public class uiControler : NetworkBehaviour
{
    public TextMeshProUGUI ammoCounter;
    [SerializeField] private TextMeshProUGUI hpCounter;
    public Transform trackingTransform;
    private 
    void Update()
    {
        if (trackingTransform)
        {
            Transform weapon = trackingTransform.GetComponent<GameManager>().weapon.transform;
            float remainingBullets = weaponHandling.BulletCount -
                                 weapon.GetComponent<weaponHandling>()
                                      .bulletCounter;
            ammoCounter.text = $"Bullets: {weaponHandling.BulletCount} / {remainingBullets}";
        }
    }
    
    [ClientRpc]
    public void GetHealthForUiClientRpc(int currentHealth, ClientRpcParams clientRpcParams)
    {
        hpCounter.text = "Hp: "+ currentHealth;
    }
}
