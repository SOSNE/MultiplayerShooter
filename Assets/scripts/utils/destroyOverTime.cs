using Unity.Netcode;
using UnityEngine;

public class destroyOverTime : NetworkBehaviour
{
    public float destroyTime;
    private float _timer;
    private void FixedUpdate()
    {
        if (!IsOwner) return;

        _timer += Time.deltaTime;
        if (_timer > destroyTime)
        {
            DestroyServerRpc();
        }
    }
    [ServerRpc]
    private void DestroyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Destroy(gameObject);
    }
}