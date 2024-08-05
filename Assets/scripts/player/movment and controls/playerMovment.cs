using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class playerMovment : NetworkBehaviour
{
    
    public float movementSpeed, jumpHeight;
    public Transform centerOfPlayer;
    private Rigidbody2D _rb;
    private bool grounded;
    private Camera _camera;

    void Start()
    { 
        _camera = Camera.main;
    }
    
    private Quaternion _targetRotation;
    private float _timer;
    
    void FixedUpdate()
    {
        if (!IsOwner) return;
        
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint(mouseScreenPosition);
        
        float distance = mouseWorldPosition.x - centerOfPlayer.position.x;
        if (Mathf.Abs(distance) > 0.1f)
        {
            if (distance > 0)
            {
                
                transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            }
            else
            {
                
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }

            
        }
        
        _rb = GetComponent<Rigidbody2D>();
        
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
