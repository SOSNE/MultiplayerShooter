using UnityEngine;

public class weaponHandling : MonoBehaviour
{
    public GameObject bullet;
    public Transform bulletSpawn;
    public static readonly float  BulletCount = 10;
    
    void Start()
    {
        
    }

    public static float BulletCounter = 0;
    void Update()
    {
        if (BulletCounter < BulletCount)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);
                BulletCounter++;
            }
        }
    }
}
