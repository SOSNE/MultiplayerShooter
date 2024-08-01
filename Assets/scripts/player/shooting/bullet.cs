using UnityEngine;

public class bullet : MonoBehaviour
{
    public float bulletSpeed = 10f;
    private Rigidbody2D rb;
    
    public GameObject GetHighestParent(GameObject child)
        {
            Transform current = child.transform;
            
            while (current.parent != null)
            {
                current = current.parent;
            }
            
            return current.gameObject;
        }

    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        Vector2 forwardDirection = transform.right;
        rb.linearVelocity += forwardDirection * -bulletSpeed;
    }
    
    private void OnTriggerEnter2D(Collider2D target)
    {
        if (GetHighestParent(target.gameObject).CompareTag("player"))
        {
            print(target.gameObject.name);
        }
    }
}
