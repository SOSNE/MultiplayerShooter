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
        if (Mathf.Abs(GetComponent<Rigidbody2D>().linearVelocity.x) > 0.1)
        {
            gameObject.GetComponent<Animator>().SetBool("walking", true);
        }
        else
        {
            gameObject.GetComponent<Animator>().SetBool("walking", false);
        }
        // SetWalkServerRpc(_walk, gameObject);
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
