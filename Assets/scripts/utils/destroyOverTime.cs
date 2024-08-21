using Unity.Netcode;
using UnityEngine;

public class destroyOverTime : NetworkBehaviour
{
    public float destroyTime;
    private float _timer;
    private void FixedUpdate()
    {
        

        _timer += Time.deltaTime;
        if (_timer > destroyTime)
        {
            
            if (gameObject.GetComponent<NetworkObject>() == null)
            {
                
                
                Destroy(gameObject);
            }
            else
            {
                if (!IsOwner) return;
                DestroyServerRpc();
            }
            
            
        }
    }
    [ServerRpc]
    private void DestroyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Destroy(gameObject);
    }
}