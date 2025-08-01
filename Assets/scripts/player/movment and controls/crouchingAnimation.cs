using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class crouchingAnimation : NetworkBehaviour
{
    [SerializeField] private GameObject _weapon;
    private bool _crouch = false;
    public bool turnToIdleInstantlyDone = false;
    void OnDisable()
    {
        if(!IsOwner) return;
        _crouch = false;
        SetWalkServerRpc(_crouch, gameObject);
        TurnToIdleInstantlyServerRpc(gameObject);
    }
    
    void Update()
    {
        if (!IsOwner) return;
        if (uiControler.anyMenuIsOpen) return;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _crouch = true;
            ToggleCrouchingMode(_crouch, gameObject);
            // StartCoroutine(DrawLine(_weapon, 0.3f));
            SetWalkServerRpc(_crouch, gameObject);
            
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _crouch = false;
            ToggleCrouchingMode(_crouch, gameObject);
            SetWalkServerRpc(_crouch, gameObject);
        }
        
    }
    [ServerRpc]
    void SetWalkServerRpc(bool value, NetworkObjectReference playerNetworkObjectReference, ServerRpcParams serverrpcParams = default) {
        
        // Exclude the sender from the ClientRpc
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                    .Where(id => id != serverrpcParams.Receive.SenderClientId)
                    .ToList()
            }
        };
        
        CrouchClientRpc(value, playerNetworkObjectReference, clientRpcParams);
    }

    [ClientRpc]
    void CrouchClientRpc(bool value, NetworkObjectReference playerNetworkObjectReference, ClientRpcParams clientRpcParams = default) {
        if(playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            ToggleCrouchingMode(value, playerNetworkObject.gameObject);
        }
    }

    private void ToggleCrouchingMode(bool value, GameObject target)
    {
        target.GetComponent<Animator>().SetBool("crouching", value);
        if (value)
        {
            target.GetComponent<CapsuleCollider2D>().offset = new Vector2(0.02604413f, 0.1193484f);
            target.GetComponent<CapsuleCollider2D>().size = new Vector2(0.5381981f, 2.677239f);
        }
        else
        {
            target.GetComponent<CapsuleCollider2D>().offset = new Vector2(0.02604413f, 0.4789118f);
            target.GetComponent<CapsuleCollider2D>().size = new Vector2(0.5381981f, 3.396366f);
        }
    }
    
    [ServerRpc]
    void TurnToIdleInstantlyServerRpc(NetworkObjectReference playerNetworkObjectReference) {
        TurnToIdleInstantlyClientRpc(playerNetworkObjectReference);
    }

    [ClientRpc]
    void TurnToIdleInstantlyClientRpc(NetworkObjectReference playerNetworkObjectReference) {
        if(playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            playerNetworkObject.GetComponent<Animator>().Play("idle");
            playerNetworkObject.GetComponent<crouchingAnimation>().turnToIdleInstantlyDone = true;
        }
    }
}
