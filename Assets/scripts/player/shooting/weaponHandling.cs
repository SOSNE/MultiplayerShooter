using UnityEngine;
using Unity.Netcode;
using Vector3 = System.Numerics.Vector3;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;

public class weaponHandling : NetworkBehaviour
{
    public GameObject bulletTracer;
    public Transform bulletSpawn, bloodParticleSystem, shootParticleParticleSystem;
    public float bulletCount = 10, fierRateInSeconds;
    [SerializeField] private float bulletSpeed, tracerLength, tracerStartWidth = 0.04f, tracerEndWidth = 0.019f;
    public LayerMask layerMask;
    public bool canShoot = true, burstMode = false;
    [SerializeField] private Gradient tracerGradientColor;
    public float bulletCounter = 0, weaponDamageMultiplier;
    private float _currentTime = 0;
    public int weaponType;
    
    
    void Update()
    {
        if (!IsOwner) return;
        
        // float shotAngle = transform.rotation.eulerAngles.z;
        if(shopUi.ShopUiOpen) return;

        if (bulletCounter >= bulletCount) return;
        
        _currentTime += Time.deltaTime;
        if (_currentTime < fierRateInSeconds) return;
        
        if (Input.GetMouseButtonDown(0) && canShoot && !burstMode)
        {
            Shoot();
            GameObject.Find("Camera Control").
                GetComponent<CameraControl>().CameraShake(0.2f,0.04f);
            _currentTime = 0;
        }
        //For burst mode
        else if (Input.GetMouseButton(0) && canShoot && burstMode)
        {
            Shoot();
            GameObject.Find("Camera Control").
                GetComponent<CameraControl>().CameraShake(0.2f,0.04f);
            _currentTime = 0;
        }
    }

    private void Shoot()
    {
        Vector2 shotDirection= -bulletSpawn.right.normalized;
        float shotAngle = transform.rotation.eulerAngles.z;    

        Transform playerParent = transform.parent;
        if (playerParent.localScale.x < 0 || playerParent.localScale.y < 0 || playerParent.localScale.z < 0)
        {
            shotDirection= bulletSpawn.right.normalized;
            shotAngle -= 180;
        }
        
        WeaponShotArtSystemServerRpc(shotAngle, gameObject);
        GetComponent<pistolMovment>().PerformRecoil();    
        RaycastHit2D[] hits2D = Physics2D.RaycastAll(bulletSpawn.position, shotDirection, Mathf.Infinity, layerMask);
        
        if (hits2D.Length == 0)
        {
            ContactData data;
            if (playerParent.localScale.x < 0 || playerParent.localScale.y < 0 || playerParent.localScale.z < 0)
            {
                data.Position = bulletSpawn.position + (bulletSpawn.right.normalized)*40;
            }
            else
            {
                data.Position = bulletSpawn.position + (-bulletSpawn.right.normalized)*40;

            }
            ShootHandlingBulletTracerServerRpc(data);
        }
        foreach (RaycastHit2D hit2D in hits2D)
        {
            GameObject target = hit2D.collider.gameObject;
            
            //Prevent hitting teammates but allow to self hit. 
            if (Utils.Instance.GetMasterParent(target.transform).layer == playerParent.gameObject.layer 
                && Utils.Instance.GetMasterParent(target.transform) != playerParent.gameObject 
                && !Utils.Instance.allowFriendlyFire.Value)
                {
                    continue;
                }
            
            if (hit2D.collider.gameObject.layer == LayerMask.NameToLayer("player body"))
            {
                ulong shooterNetworkId = hit2D.collider.transform.root.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                if (playerParent.GetComponent<GameManager>().isAlive)
                {
                    switch (hit2D.collider.gameObject.name)
                    {
                        case "headCollider":
                            transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit((int)(180*weaponDamageMultiplier), shooterNetworkId, target.name, shotDirection);
                            break;
                        
                        case "bodyDownCollider" or "bodyUpCollider":
                            transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit((int)(50*weaponDamageMultiplier), shooterNetworkId, target.name, shotDirection);
                            break;
                        
                        case "rightArmStartCollider" or "rightArmEndCollider":
                            transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit((int)(20*weaponDamageMultiplier), shooterNetworkId, target.name, shotDirection);
                            break;
                        
                        case "leftArmStartCollider" or "leftArmEndCollider":
                            transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit((int)(20*weaponDamageMultiplier), shooterNetworkId, target.name, shotDirection);
                            break;
                        
                        case "leftLegStartCollider" or "leftLegEndCollider":
                            transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit((int)(20*weaponDamageMultiplier), shooterNetworkId, target.name, shotDirection);
                            break;
                        
                        case "rightLegStartCollider" or "rightLegEndCollider":
                            transform.root.gameObject.GetComponent<PlayerHhandling>().PlayerHit((int)(20*weaponDamageMultiplier), shooterNetworkId, target.name, shotDirection);
                            break;
                    }
                }
                ContactData data;
                data.Position = hit2D.point;
                NetworkObjectReference netObject = new NetworkObjectReference (
                    GetRootParent(hit2D.transform).GetComponent<NetworkObject>());
                if (transform.parent.localScale.x < 0 || transform.parent.localScale.y < 0 || transform.parent.localScale.z < 0)
                {
                    ShootHandlingBloodServerRpc(netObject, data, transform.localRotation.eulerAngles.z);
                }
                else
                {
                    ShootHandlingBloodServerRpc(netObject, data, transform.localRotation.eulerAngles.z - 180);
                }
                ShootHandlingBulletTracerServerRpc(data);
                break;
            }
            if (target.layer == LayerMask.NameToLayer("ground"))
            {
                ContactData data;
                data.Position = hit2D.point;
                ShootHandlingBulletTracerServerRpc(data);
                break;
            }
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
    private void WeaponShotArtSystemServerRpc(float shotAngle, NetworkObjectReference weaponTargetReference, ServerRpcParams serverRpcParams = default)
    {
        // Transform shootParticle = Instantiate(shootParticleParticleSystem, bulletSpawn.position, Quaternion.Euler(0f,0f,bulletSpawn.eulerAngles.z));
        // Vector2 velocity = transform.parent.GetComponent<Rigidbody2D>().linearVelocity;
        // shootParticle.GetComponent<Rigidbody2D>().linearVelocity = velocity*4;
        ClientRpcNotifyClientClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new [] { serverRpcParams.Receive.SenderClientId } } });
        WeaponShotArtSystemClientRpc(shotAngle, weaponTargetReference);
    }
    
    [ClientRpc]
    private void ClientRpcNotifyClientClientRpc(ClientRpcParams clientRpcParams = default)
    {
        bulletCounter++;
    }
    
    [ClientRpc]
    private void WeaponShotArtSystemClientRpc(float shotAngle, NetworkObjectReference weaponTargetReference, ClientRpcParams clientRpcParams = default)
    {
        Transform shootParticle = Instantiate(shootParticleParticleSystem, bulletSpawn.position, Quaternion.Euler(0f,0f,shotAngle));
        // shootParticle.transform.SetParent(transform);
        Vector2 velocity = transform.parent.GetComponent<Rigidbody2D>().linearVelocity;
        shootParticle.GetComponent<Rigidbody2D>().linearVelocity = velocity*4;
        
        if (!weaponTargetReference.TryGet(out NetworkObject weaponGameObject)) return;
        Utils.Instance.PlaySound(weaponType,1f, weaponGameObject.transform);

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
        lineRenderer.colorGradient = tracerGradientColor;
                    
        lineRenderer.startWidth = tracerStartWidth; 
        lineRenderer.endWidth = tracerEndWidth; 
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
