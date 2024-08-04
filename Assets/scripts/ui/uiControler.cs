using TMPro;
using UnityEngine;

public class uiControler : MonoBehaviour
{
    public TextMeshProUGUI ammoCounter;
    void Start()
    {
        
    }
    
    void Update()
    {
        float remainingBullets = weaponHandling.BulletCount - weaponHandling.BulletCounter;
        ammoCounter.text = $"Bulets: {weaponHandling.BulletCount} / {remainingBullets}";
    }
}
