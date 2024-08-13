using Unity.Netcode;
using UnityEngine;

public class shootParticle : NetworkBehaviour
{
    private float _timer;
    void FixedUpdate()
    {
        if (!IsServer) return;
        _timer += Time.deltaTime;
        if (_timer > 0.5f)
        {                                                                                                                                         
            ShootParticleDestroyServerRpc();
        }
    }
    
    [ServerRpc]
    private void ShootParticleDestroyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Destroy(gameObject);
    }
}
