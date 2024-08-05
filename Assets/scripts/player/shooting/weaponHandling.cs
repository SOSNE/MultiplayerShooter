using UnityEngine;
using Unity.Netcode;

public class weaponHandling : NetworkBehaviour
{
    public GameObject bullet;
    public Transform bulletSpawn;
    public static readonly float  BulletCount = 10;
    
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
                ShootServerRpc();
            }
        }
    }
    
    [ClientRpc]
    private void ClientRpcNotifyServerRpcClientRpc(ClientRpcParams clientRpcParams = default)
    {
        BulletCounter++;
    }
    
    [ServerRpc]
    private void ShootServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Transform spawnedBullet = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation).transform;
        spawnedBullet.GetComponent<NetworkObject>().Spawn(true);
        ClientRpcNotifyServerRpcClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new [] { serverRpcParams.Receive.SenderClientId } } });
    }
}
