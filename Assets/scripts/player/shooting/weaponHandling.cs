using UnityEngine;
using Unity.Netcode;
using Vector3 = System.Numerics.Vector3;

public class weaponHandling : NetworkBehaviour
{
    public GameObject bullet;
    public Transform bulletSpawn, bloodParticleSystem, shootParticleParticleSystem;
    public static readonly float  BulletCount = 10;
    public LayerMask layerMask;
    
    void Start()
    {
        
    }

    public static float BulletCounter = 0;
    void Update()
    {
        if (!IsOwner) return;
        if (BulletCounter < BulletCount)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ShootParticleServerRpc();
                RaycastHit2D hit2D = Physics2D.Raycast(bulletSpawn.position, -bulletSpawn.right, Mathf.Infinity, layerMask);
                if (hit2D)
                {
                    ContactData data;
                    data.Position = hit2D.point;
                    NetworkObjectReference   netObject = new NetworkObjectReference (
                        hit2D.transform.GetComponent<NetworkObject>());
                    ShootBloodServerRpc(netObject,data);
                }
                
            }
        }
    }
    
    [ClientRpc]
    private void ClientRpcNotifyServerRpcClientRpc(ClientRpcParams clientRpcParams = default)
    {
        BulletCounter++;
    }
    
    [ServerRpc]
    private void ShootBloodServerRpc(NetworkObjectReference  playerGameObject,ContactData contactData,ServerRpcParams serverRpcParams = default)
    {
        Transform blood = Instantiate(bloodParticleSystem, contactData.Position, Quaternion.Euler(0f,0f,transform.eulerAngles.z +180)).transform;
        blood.GetComponent<NetworkObject>().Spawn(true);
        if (playerGameObject.TryGet(out NetworkObject networkObject))
        {
            blood.SetParent(networkObject.transform);
        }
    }
    
    [ServerRpc]
    private void ShootParticleServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Transform shootParticle = Instantiate(shootParticleParticleSystem, bulletSpawn.position, Quaternion.Euler(0f,0f,bulletSpawn.eulerAngles.z));
        shootParticle.GetComponent<NetworkObject>().Spawn(true);
        Vector2 velocity = transform.parent.GetComponent<Rigidbody2D>().linearVelocity;
        shootParticle.GetComponent<Rigidbody2D>().linearVelocity = velocity;
        ClientRpcNotifyServerRpcClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new [] { serverRpcParams.Receive.SenderClientId } } });
    }
    
    struct ContactData : INetworkSerializable
    {
        public Vector2 Position;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
        }
    }
}
