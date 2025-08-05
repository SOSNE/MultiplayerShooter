using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class playerMovment : NetworkBehaviour
{
    
    public float movementSpeed, jumpHeight;
    [SerializeField] private float ladderSpeed;
    public Transform centerOfPlayer, weapon;
    private Rigidbody2D _rb;
    public bool grounded, goUp;
    public Camera camera;
    public Vector3 _weaponStartScale;
    public LayerMask groundMask;


    void Start()
    {
        // _camera = Camera.main;
    }
    
    private Quaternion _targetRotation;
    private float _timer;
    bool _rotateFlag = true;
    
    void Update()
    {
        if (!IsOwner) return;
        if (!camera) return;
        if (uiControler.anyMenuIsOpen) return;
        if (weapon && _weaponStartScale == Vector3.zero)  _weaponStartScale = weapon.localScale;
        
        // check grounded
        float height = GetComponent<Collider2D>().bounds.size.y;
        Vector2 raycastTransformPosition = new Vector2(transform.position.x, transform.position.y - height / 2f); 
        grounded = Physics2D.Raycast(raycastTransformPosition, Vector2.down, 0.01f, groundMask).collider;
        
        
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, camera.nearClipPlane));
        
        float distance = mouseWorldPosition.x - centerOfPlayer.position.x;
        if (Mathf.Abs(distance) > 0.1f)
        {
            if (distance > 0 && _rotateFlag)
            {
                RotationData scale;
                scale.PlayerScale  = new Vector3(-1, 1, 1);
                scale.WeaponScale = new Vector3(_weaponStartScale.x, _weaponStartScale.y, _weaponStartScale.z);
                gameObject.transform.localScale = scale.PlayerScale;
                // RotatePlayerAndWeaponServerRpc(gameObject, gameObject, scale);
                _rotateFlag = false;
            }
            else if (distance <= 0 && !_rotateFlag)
            {
                RotationData scale;
                scale.PlayerScale  = new Vector3(1, 1, 1);
                scale.WeaponScale = new Vector3(_weaponStartScale.x, _weaponStartScale.y, _weaponStartScale.z);
                gameObject.transform.localScale = scale.PlayerScale;
                // RotatePlayerAndWeaponServerRpc(gameObject, gameObject, scale);
                _rotateFlag = true;
            }
        }
        
        _rb = GetComponent<Rigidbody2D>();
        
        if (Input.GetKeyDown(KeyCode.Space) && grounded )
        {
            _rb.linearVelocity = new Vector2(0, jumpHeight);
        }
        
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) && goUp )
        { 
            _rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            _rb.linearVelocity = new Vector2(0, ladderSpeed);
            _rb.linearDamping = 18;
        }
        else if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftControl)) && goUp && !grounded)
        {
            _rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            _rb.linearVelocity = new Vector2(0, -ladderSpeed/4);
        }
        else if(goUp)
        {
            _rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
            _rb.linearDamping = 18;
        }
        else
        {
            _rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        }
        
        if (!grounded && !goUp)
        {
            _rb.linearDamping = 0;
        }
        
        if (Input.GetKey(KeyCode.D))
        {
            _rb.linearVelocity = new Vector2(movementSpeed, GetComponent<Rigidbody2D>().linearVelocity.y);
        }
        if (Input.GetKey(KeyCode.A))
        {
            _rb.linearVelocity = new Vector2(-movementSpeed, GetComponent<Rigidbody2D>().linearVelocity.y);
        }
    }
    
    

    private void FixedUpdate()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        if (!grounded && !goUp)
        {
            _rb.linearDamping = 0;
        }
        else
        {
            _rb.linearDamping = 10; 
        }
    }

    // private void OnCollisionStay2D(Collision2D other)
    // {
    //     if (other.gameObject.CompareTag("ground"))
    //     {
    //         Grounded = true;
    //         ToggleJumpingMode(false, gameObject);
    //         ToggleJumpingModeServerRpc(false, gameObject);
    //     }
    // }
    
    // private void OnCollisionExit2D(Collision2D other)
    // {
    //     if (other.gameObject.CompareTag("ground"))
    //     {
    //         Grounded = false;
    //     }
    // }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ladder"))
        {
            goUp = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ladder"))
        {
            goUp = false;
        }
    }
    

    [ServerRpc]
    public void RotatePlayerAndWeaponServerRpc(NetworkObjectReference playerObjectReference, NetworkObjectReference weaponObjectReference, RotationData scaleVector3)
    {
        RotatePlayerAndWeaponClientRpc(playerObjectReference, weaponObjectReference, scaleVector3);
    }

    [ClientRpc]
    public void RotatePlayerAndWeaponClientRpc(NetworkObjectReference playerObjectReference, NetworkObjectReference weaponObjectReference, RotationData scaleVector3)
    {
        if(playerObjectReference.TryGet(out NetworkObject playerNetworkObject))
        { 
            playerNetworkObject.transform.localScale = scaleVector3.PlayerScale;
        }
        if(weaponObjectReference.TryGet(out NetworkObject weaponNetworkObject))
        { 
            // weaponNetworkObject.transform.localScale = scaleVector3.WeaponScale;
        }
    }
    
    public struct RotationData : INetworkSerializable
    {
        public Vector3 PlayerScale;
        public Vector3 WeaponScale;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerScale);
            serializer.SerializeValue(ref WeaponScale);
        }
    }
}
