using System;
using Unity.Netcode;
using UnityEngine;

public class playerMovment : NetworkBehaviour
{
    
    public float movementSpeed, jumpHeight;
    [SerializeField] private float ladderSpeed;
    public Transform centerOfPlayer, weapon;
    private Rigidbody2D _rb;
    private bool _grounded, _goUp;
    public Camera camera;
    public Vector3 _weaponStartScale;

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
        if (weapon && _weaponStartScale == Vector3.zero)  _weaponStartScale = weapon.localScale;
        
        
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, camera.nearClipPlane));
        
        float distance = mouseWorldPosition.x - centerOfPlayer.position.x;
        if (Mathf.Abs(distance) > 0.1f)
        {
            if (distance > 0 && _rotateFlag)
            {
                RotationData scale;
                scale.PlayerScale  = new Vector3(-1, 1, 1);
                scale.WeaponScale = new Vector3(-_weaponStartScale.x, -_weaponStartScale.y, _weaponStartScale.z);
                RotatePlayerAndWeaponServerRpc(gameObject, gameObject, scale);
                weapon.transform.localScale = scale.WeaponScale;

                _rotateFlag = false;
            }
            else if (distance <= 0 && !_rotateFlag)
            {
                RotationData scale;
                scale.PlayerScale  = new Vector3(1, 1, 1);
                scale.WeaponScale = new Vector3(_weaponStartScale.x, _weaponStartScale.y, _weaponStartScale.z);
                RotatePlayerAndWeaponServerRpc(gameObject, gameObject, scale);
                weapon.transform.localScale = scale.WeaponScale;
                _rotateFlag = true;
            }
        }
        
        _rb = GetComponent<Rigidbody2D>();
        
        if (Input.GetKeyDown(KeyCode.Space) && _grounded )
        {
            _rb.linearVelocity = new Vector2(0, jumpHeight);
        }
        
        
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) && _goUp )
        { 
            _rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            _rb.linearVelocity = new Vector2(0, ladderSpeed);
            _rb.linearDamping = 18;
        }
        else if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftControl)) && _goUp && !_grounded)
        {
            _rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            _rb.linearVelocity = new Vector2(0, -ladderSpeed/4);
        }
        else if(_goUp)
        {
            _rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
            _rb.linearDamping = 18;
        }
        else
        {
            _rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        }
        
        if (!_grounded && !_goUp)
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
        
        if (!_grounded && !_goUp)
        {
            _rb.linearDamping = 0;
        }
        else
        {
            _rb.linearDamping = 10; 
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("ground"))
        {
            _grounded = true;
        }
    }
    
    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("ground"))
        {
            _grounded = false;
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ladder"))
        {
            _goUp = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ladder"))
        {
            _goUp = false;
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
            // TODO create weapon spawning system with weapon that have network
            // object component.
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
