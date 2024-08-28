using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class playerMovment : NetworkBehaviour
{
    
    public float movementSpeed, jumpHeight;
    public Transform centerOfPlayer, weapon;
    private Rigidbody2D _rb;
    private bool grounded;
    public Camera camera;

    void Start()
    { 
        // _camera = Camera.main;
    }
    
    private Quaternion _targetRotation;
    private float _timer;
    
    void Update()
    {
        if (!IsOwner) return;
        
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, camera.nearClipPlane));
        
        float distance = mouseWorldPosition.x - centerOfPlayer.position.x;
        if (Mathf.Abs(distance) > 0.1f)
        {
            if (distance > 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                weapon.localScale = new Vector3(-1, -1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
                weapon.localScale = new Vector3(1, 1, 1);
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
