// using Unity.Netcode;
// using UnityEngine;
//
// public class wlakingAnimation : NetworkBehaviour
// {
//     private Animator _animator;
//     private bool _walk;
//     private Vector3 lastPosition;
//
//     void Start()
//     {
//         _animator = GetComponent<Animator>();
//         _walk = false;
//         lastPosition = transform.position;
//     }
//     
//     
//     //Temp fix because it wont work if someone will have different fps except of 60. (:
//     void FixedUpdate()
//     {
//         if (playerMovment.Grounded && Vector3.Distance(transform.position, lastPosition) > 0.01)
//         {
//             gameObject.GetComponent<Animator>().SetBool("walking", true);
//         }
//         else
//         {
//             gameObject.GetComponent<Animator>().SetBool("walking", false);
//         }
//         lastPosition = transform.position;
//         // SetWalkServerRpc(_walk, gameObject);
//     }
//     
//     [ServerRpc]
//     void SetWalkServerRpc(bool value, NetworkObjectReference playerNetworkObjectReference) {
//         SetWalkClientRpc(value, playerNetworkObjectReference);
//     }
//
//     [ClientRpc]
//     void SetWalkClientRpc(bool value, NetworkObjectReference playerNetworkObjectReference) {
//         if(playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
//         {
//             playerNetworkObject.GetComponent<Animator>().SetBool("walking", value);
//         }
//     }
// }
