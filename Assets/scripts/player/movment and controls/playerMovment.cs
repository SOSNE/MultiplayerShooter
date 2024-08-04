using System;
using UnityEngine;

public class playerMovment : MonoBehaviour
{
    public float movementSpeed, jumpHeight;
    public Transform weapon;
    private Rigidbody2D rb;
    private bool grounded;
    void Start()
    { 
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (weapon.position.x >= transform.position.x+0.1f)
        {
            transform.rotation = Quaternion.Euler(new Vector2(0, 180));
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector2(0, 0));
        }
        
        rb = GetComponent<Rigidbody2D>();
        
        if (Input.GetKeyDown(KeyCode.W) && grounded )
        {
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, jumpHeight);
        }
        if (Input.GetKey(KeyCode.D))
        {
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(movementSpeed, GetComponent<Rigidbody2D>().linearVelocity.y);
        }
        if (Input.GetKey(KeyCode.A))
        {
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(-movementSpeed, GetComponent<Rigidbody2D>().linearVelocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("ground"))
        {
            grounded = true;
        }
    }
    
    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("ground"))
        {
            grounded = false;
        }
    }
}
