using UnityEngine;
using Unity.Netcode;
using Vector3 = System.Numerics.Vector3;
using System.Collections;


public class weaponHandling : NetworkBehaviour
{
    public GameObject bulletTracer;
    public Transform bulletSpawn, bloodParticleSystem, shootParticleParticleSystem;
    public static readonly float  BulletCount = 10;
    [SerializeField] private float bulletSpeed, tracerLength;
    public LayerMask layerMask;
    [SerializeField] private 
    
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
                 
                RaycastHit2D hit2D = Physics2D.Raycast(bulletSpawn.position, -bulletSpawn.right.normalized, Mathf.Infinity, layerMask);
                if (!hit2D)
                {
                    ContactData data;
                    data.Position = bulletSpawn.position+ (-bulletSpawn.right.normalized)*40;
                    ShootHandlingBulletTracerServerRpc(data);
                }
                else if (hit2D.collider.gameObject.layer == LayerMask.NameToLayer("player body"))
                {
                    ulong shooterNetworkId = hit2D.collider.transform.root.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                    transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit(5, shooterNetworkId);
                    ContactData data;
                    data.Position = hit2D.point;
                    NetworkObjectReference netObject = new NetworkObjectReference (
                        hit2D.transform.GetComponent<NetworkObject>());
                    ShootHandlingBloodServerRpc(netObject,data);
                    ShootHandlingBulletTracerServerRpc(data);
                }
                else if (hit2D.collider.gameObject.layer == LayerMask.NameToLayer("ground"))
                {
                    ContactData data;
                    data.Position = hit2D.point;
                    ShootHandlingBulletTracerServerRpc(data);
                }
            }
        }
    }
    
    [ClientRpc]
    private void ClientRpcNotifyServerRpcClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Transform shootParticle = Instantiate(shootParticleParticleSystem, bulletSpawn.position, Quaternion.Euler(0f,0f,bulletSpawn.eulerAngles.z));
        Vector2 velocity = transform.parent.GetComponent<Rigidbody2D>().linearVelocity;
        shootParticle.GetComponent<Rigidbody2D>().linearVelocity = velocity*4;
        BulletCounter++;
    }
    
    [ClientRpc]
    private void ShootHandlingRpcClientRpc(ContactData contactData, ClientRpcParams clientRpcParams = default)
    {
        if (IsHost) return;
        float speed = Vector2.Distance(bulletSpawn.position, contactData.Position) / bulletSpeed;
        GameObject lineObject = Instantiate(bulletTracer);
        StartCoroutine(DrawLine(lineObject,bulletSpawn.position, contactData.Position, speed));
    }
    
    [ServerRpc]
    private void ShootHandlingBulletTracerServerRpc(ContactData contactData,ServerRpcParams serverRpcParams = default)
    {
        GameObject lineObject = Instantiate(bulletTracer);
        float speed = Vector2.Distance(bulletSpawn.position, contactData.Position) / bulletSpeed;
        print(speed);
        StartCoroutine(DrawLine(lineObject,bulletSpawn.position, contactData.Position, speed));
        ShootHandlingRpcClientRpc(contactData);
    }
    
    [ServerRpc]
    private void ShootHandlingBloodServerRpc(NetworkObjectReference playerGameObject,ContactData contactData,ServerRpcParams serverRpcParams = default)
    {
        if (playerGameObject.TryGet(out NetworkObject networkObject))
        {
            Transform blood = Instantiate(bloodParticleSystem, contactData.Position, Quaternion.Euler(0f,0f,transform.eulerAngles.z +180)).transform;
            blood.GetComponent<NetworkObject>().Spawn(true);
            blood.SetParent(networkObject.transform);
        }
    }
    
    
    
    [ServerRpc]
    private void ShootParticleServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Transform shootParticle = Instantiate(shootParticleParticleSystem, bulletSpawn.position, Quaternion.Euler(0f,0f,bulletSpawn.eulerAngles.z));
        Vector2 velocity = transform.parent.GetComponent<Rigidbody2D>().linearVelocity;
        shootParticle.GetComponent<Rigidbody2D>().linearVelocity = velocity*4;
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
    
    IEnumerator DrawLine(GameObject lineObject ,Vector2 startPoint, Vector2 endPoint, float duration)
    {
        
                    
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineObject.GetComponent<destroyOverTime>().destroyTime = duration;
                    
        lineRenderer.positionCount = 2;
                    
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint); 
                    
        lineRenderer.startWidth = 0.02f; 
        lineRenderer.endWidth = 0.009f; 
        lineRenderer.useWorldSpace = true; 

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, startPoint);
        float startTime = Time.time;
        while (Time.time - startTime < duration && lineRenderer != null)
        {
            float t = (Time.time - startTime) / duration;
            
            Vector2 currentPosition = Vector2.Lerp(startPoint, endPoint, t);
    
            
            Vector2 direction = (endPoint - startPoint).normalized;
    
            
            Vector2 currentStartPosition = currentPosition - direction * (tracerLength);
            Vector2 currentEndPosition = currentPosition + direction * (tracerLength);
            lineRenderer.SetPosition(0, currentStartPosition);
            lineRenderer.SetPosition(1, currentEndPosition);
            
            yield return null;
        }

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(1, endPoint);
        }
        
    }

}
