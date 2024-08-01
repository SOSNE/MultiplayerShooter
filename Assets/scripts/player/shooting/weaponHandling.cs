using UnityEngine;

public class weaponHandling : MonoBehaviour
{
    public GameObject bullet;
    public Transform bulletSpawn;
    void Start()
    {
        
    }

    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);
            
        }
    }
}
