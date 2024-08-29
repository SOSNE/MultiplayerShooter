using Unity.Netcode;
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
    bool _rotateFlag = true;
    
    void Update()
    {
        if (!IsOwner) return;
        
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, camera.nearClipPlane));
        
        float distance = mouseWorldPosition.x - centerOfPlayer.position.x;
        if (Mathf.Abs(distance) > 0.1f)
        {
            if (distance > 0 && _rotateFlag)
            {
                RotationData scale;
                scale.PlayerScale  = new Vector3(-1, 1, 1);
                scale.WeaponScale = new Vector3(-1, -1, 1);
                RotatePlayerAndWeaponServerRpc(gameObject, gameObject, scale);
                weapon.transform.localScale = scale.WeaponScale;

                _rotateFlag = false;
            }
            else if (distance <= 0 && !_rotateFlag)
            {
                RotationData scale;
                scale.PlayerScale  = new Vector3(1, 1, 1);
                scale.WeaponScale = new Vector3(1, 1, 1);
                RotatePlayerAndWeaponServerRpc(gameObject, gameObject, scale);
                weapon.transform.localScale = scale.WeaponScale;
                _rotateFlag = true;
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
