using Unity.Netcode;
using UnityEngine;

public class wlakingAnimation : NetworkBehaviour
{
    private Animator _animator;
    private bool _walk;
    void Start()
    {
        _animator = GetComponent<Animator>();
        _walk = false;
    }
    
    
    void Update()
    {
        if (!IsOwner) return;
        if (Mathf.Abs(GetComponent<Rigidbody2D>().linearVelocity.x) > 0.1)
        {
            _walk = true;
        }
        else
        {
            _walk = false;
        }
        SetWalkServerRpc(_walk, gameObject);
    }
    
    [ServerRpc]
    void SetWalkServerRpc(bool value, NetworkObjectReference playerNetworkObjectReference) {
        SetWalkClientRpc(value, playerNetworkObjectReference);
    }

    [ClientRpc]
    void SetWalkClientRpc(bool value, NetworkObjectReference playerNetworkObjectReference) {
        if(playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            playerNetworkObject.GetComponent<Animator>().SetBool("walking", value);
        }
    }

}
