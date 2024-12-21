using UnityEngine;
using Unity.Netcode;
using Vector3 = System.Numerics.Vector3;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine.Serialization;


public class weaponHandling : NetworkBehaviour
{
    public GameObject bulletTracer;
    public Transform bulletSpawn, bloodParticleSystem, shootParticleParticleSystem;
    public static readonly float  BulletCount = 10;
    [SerializeField] private float bulletSpeed, tracerLength, fierRateInSeconds;
    public LayerMask layerMask;
    public bool canShoot = true;

    [FormerlySerializedAs("BulletCounter")] public float bulletCounter = 0;
    private float _currentTime = 0;
    void Update()
    {
        if (!IsOwner) return;
        
        if (bulletCounter >= BulletCount)
        {
            return;
        }

        
        _currentTime += Time.deltaTime;
        if (_currentTime < fierRateInSeconds)
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            
            Shoot();
            GameObject.Find("Camera Control").
                GetComponent<CameraControl>().CameraShake(0.2f,0.07f);
            _currentTime = 0;
        }
    }

    private void Shoot()
    {
        Vector2 shotDirection = -bulletSpawn.right.normalized;
        ShootParticleServerRpc();
        GetComponent<pistolMovment>().PerformRecoil();    
        RaycastHit2D hit2D = Physics2D.Raycast(bulletSpawn.position, shotDirection, Mathf.Infinity, layerMask);
        if (!hit2D)
        {
            ContactData data;
            data.Position = bulletSpawn.position + (-bulletSpawn.right.normalized)*40;
            ShootHandlingBulletTracerServerRpc(data);
        }
        else if (hit2D.collider.gameObject.layer == LayerMask.NameToLayer("player body"))
        {
            ulong shooterNetworkId = hit2D.collider.transform.root.gameObject.GetComponent<NetworkObject>().OwnerClientId;
            switch (hit2D.collider.gameObject.name)
            {
                case "headCollider":
                    transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit(100, shooterNetworkId, hit2D.collider.gameObject.name, shotDirection);
                    break;
                
                case "bodyDownCollider" or "bodyUpCollider":
                    transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit(50, shooterNetworkId, hit2D.collider.gameObject.name, shotDirection);
                    break;
                
                case "rightArmStartCollider" or "rightArmEndCollider":
                    transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit(20, shooterNetworkId, hit2D.collider.gameObject.name, shotDirection);
                    break;
                
                case "leftArmStartCollider" or "leftArmEndCollider":
                    transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit(20, shooterNetworkId, hit2D.collider.gameObject.name, shotDirection);
                    break;
                
                case "leftLegStartCollider" or "leftLegEndCollider":
                    transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit(20, shooterNetworkId, hit2D.collider.gameObject.name, shotDirection);
                    break;
                
                case "rightLegStartCollider" or "rightLegEndCollider":
                    transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit(20, shooterNetworkId, hit2D.collider.gameObject.name, shotDirection);
                    break;
                
            }
            ContactData data;
            data.Position = hit2D.point;
            NetworkObjectReference netObject = new NetworkObjectReference (
                GetRootParent(hit2D.transform).GetComponent<NetworkObject>());
            if (transform.localScale.x < 0 || transform.localScale.y < 0 || transform.localScale.z < 0)
            {
                ShootHandlingBloodServerRpc(netObject, data, transform.localRotation.eulerAngles.z - 180);
            }
            else
            {
                ShootHandlingBloodServerRpc(netObject, data, transform.localRotation.eulerAngles.z);
            }
            ShootHandlingBulletTracerServerRpc(data);
        }
        else if (hit2D.collider.gameObject.layer == LayerMask.NameToLayer("ground"))
        {
            ContactData data;
            data.Position = hit2D.point;
            ShootHandlingBulletTracerServerRpc(data);
        }
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
        StartCoroutine(DrawLine(lineObject,bulletSpawn.position, contactData.Position, speed));
        ShootHandlingRpcClientRpc(contactData);
    }
    
    [ServerRpc]
    private void ShootHandlingBloodServerRpc(NetworkObjectReference playerGameObject,ContactData contactData, float transformEulerAnglesZ, ServerRpcParams serverRpcParams = default)
    {
        if (playerGameObject.TryGet(out NetworkObject networkObject))
        {
            Transform blood = Instantiate(bloodParticleSystem, contactData.Position, Quaternion.Euler(0f,0f,transformEulerAnglesZ)).transform;
            blood.GetComponent<NetworkObject>().Spawn(true);
            blood.rotation = Quaternion.Euler(0f, 0f, transformEulerAnglesZ);
            blood.SetParent(networkObject.transform);
        }
    }
    
    [ServerRpc]
    private void ShootParticleServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Transform shootParticle = Instantiate(shootParticleParticleSystem, bulletSpawn.position, Quaternion.Euler(0f,0f,bulletSpawn.eulerAngles.z));
        // Vector2 velocity = transform.parent.GetComponent<Rigidbody2D>().linearVelocity;
        // shootParticle.GetComponent<Rigidbody2D>().linearVelocity = velocity*4;
        ClientRpcNotifyClientClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new [] { serverRpcParams.Receive.SenderClientId } } });
        ShootParticleClientRpc();
    }
    
    [ClientRpc]
    private void ClientRpcNotifyClientClientRpc(ClientRpcParams clientRpcParams = default)
    {
        bulletCounter++;
    }
    
    [ClientRpc]
    private void ShootParticleClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Transform shootParticle = Instantiate(shootParticleParticleSystem, bulletSpawn.position, Quaternion.Euler(0f,0f,bulletSpawn.eulerAngles.z));
        Vector2 velocity = transform.parent.GetComponent<Rigidbody2D>().linearVelocity;
        shootParticle.GetComponent<Rigidbody2D>().linearVelocity = velocity*4;
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
    
    Transform GetRootParent(Transform obj)
    {
        while (obj.parent != null)
        {
            obj = obj.parent;
        }
        return obj;
    }
}
