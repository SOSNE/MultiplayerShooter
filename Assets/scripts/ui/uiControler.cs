using TMPro;
using Unity.Netcode;
using UnityEngine;

public class uiControler : NetworkBehaviour
{
    public TextMeshProUGUI ammoCounter;
    [SerializeField] private TextMeshProUGUI hpCounter;
    void Start()
    {
        
    }
    
    void Update()
    {
        float remainingBullets = weaponHandling.BulletCount - weaponHandling.BulletCounter;
        ammoCounter.text = $"Bulets: {weaponHandling.BulletCount} / {remainingBullets}";
        if (PlayerHhandling.clientHealthMap.ContainsKey(NetworkManager.Singleton.LocalClientId))
        {
            hpCounter.text = "Hp: "+ PlayerHhandling.clientHealthMap[NetworkManager.Singleton.LocalClientId];
        }
    }
}
