using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Animations : NetworkBehaviour
{
    private bool _walk = false;
    private Vector3 lastPosition;
    private bool _crouch = false;
    public bool turnToIdleInstantlyDone = false;

    void OnDisable()
    {
        if (!IsOwner) return;
        if(gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("crouching")) return;
        _crouch = false;
        SetCrouchServerRpc(_crouch, gameObject);
        TurnToIdleInstantlyServerRpc(gameObject);
    }

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (!IsOwner) return;
        // if (uiControler.anyMenuIsOpen) return;
        
        if (!GetComponent<playerMovment>().grounded)
        {
            ToggleAnimationMode("jump", gameObject);
            ToggleAnimationModeServerRpc("jump", gameObject);
        }
        else if(Input.GetKey(KeyCode.LeftShift))
        { 
            //"crouch"
            ToggleCrouchingMode(true, gameObject);
            SetCrouchServerRpc(true, gameObject);
        }
        else if (Mathf.Abs(GetComponent<Rigidbody2D>().linearVelocity.x) > 0.01f)
        {
            ToggleAnimationMode("walk", gameObject);
            ToggleAnimationModeServerRpc("walk", gameObject);
        }
        else
        {
            ToggleAnimationMode("idle", gameObject);
            ToggleAnimationModeServerRpc("idle", gameObject);
        }
    }

    private void ResetAllAnimationTriggers(string resetException)
    {
        foreach (AnimatorControllerParameter param in GetComponent<Animator>().parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger && resetException != param.name)
            {
                GetComponent<Animator>().ResetTrigger(param.name);
            }
        }
    }
    
     [ServerRpc]
     void SetCrouchServerRpc(bool value, NetworkObjectReference playerNetworkObjectReference, ServerRpcParams serverrpcParams = default) {
         
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
         ResetAllAnimationTriggers("crouch");
         GetComponent<Animator>().SetTrigger("crouch");
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
             playerNetworkObject.GetComponent<Animations>().turnToIdleInstantlyDone = true;
         }
     }
     
     [ServerRpc]
     void ToggleAnimationModeServerRpc(string value, NetworkObjectReference playerNetworkObjectReference, ServerRpcParams serverRpcParams = default) {
         
         // Exclude the sender from the ClientRpc
         ClientRpcParams clientRpcParams = new ClientRpcParams
         {
             Send = new ClientRpcSendParams
             {
                 TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds
                     .Where(id => id != serverRpcParams.Receive.SenderClientId)
                     .ToList()
             }
         };
         ToggleAnimationModeClientRpc(value, playerNetworkObjectReference, clientRpcParams);
     }
    
     [ClientRpc]
     void ToggleAnimationModeClientRpc(string value, NetworkObjectReference playerNetworkObjectReference, ClientRpcParams clientRpcParams = default) {
         if(playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
         {
             ToggleAnimationMode(value, playerNetworkObject.gameObject);
         }
     }
    
     private void ToggleAnimationMode(string value, GameObject target)
     {
         ResetAllAnimationTriggers(value);
         GetComponent<Animator>().SetTrigger(value);
     }
}
