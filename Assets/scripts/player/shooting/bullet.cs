using Unity.Netcode;
using UnityEngine;

public class bullet : NetworkBehaviour
{
    public float bulletSpeed = 10f;
    private Rigidbody2D rb;
    public GameObject bloodParticleSystem;
    
    // public GameObject GetHighestParent(GameObject child)
    //     {
    //         Transform current = child.transform;
    //         
    //         while (current.parent != null)
    //         {
    //             current = current.parent;
    //         }
    //         
    //         return current.gameObject;
    //     }

    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        Vector2 forwardDirection = transform.right;
        rb.linearVelocity = forwardDirection * -bulletSpeed;
    }
    
    // private void OnTriggerEnter2D(Collider2D target)
    // {
    //     if (!IsOwner)return;
    //     if (GetHighestParent(target.gameObject).CompareTag("player"))
    //     {
    //         SpawnBloodServerRpc();
    //         Destroy(gameObject);
    //     }
    // }
    //
    // [ServerRpc]
    // private void SpawnBloodServerRpc()
    // {
    //     Transform blood = Instantiate(bloodParticleSystem, transform.position, Quaternion.Euler(0f,0f,transform.eulerAngles.z +180)).transform;
    //     blood.GetComponent<NetworkObject>().Spawn(true);
    // }
}
