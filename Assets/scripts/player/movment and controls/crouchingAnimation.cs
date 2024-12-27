using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class crouchingAnimation : NetworkBehaviour
{
    [SerializeField] private GameObject _weapon;
    private bool _crouch = false;
    void OnDisable()
    {
        _crouch = false;
        TurnToIdleInstantlyServerRpc(gameObject);
        SetWalkServerRpc(_crouch, gameObject);
    }
    
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _crouch = true;
            // StartCoroutine(DrawLine(_weapon, 0.3f));
            SetWalkServerRpc(_crouch, gameObject);
            
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            _crouch = false;
            SetWalkServerRpc(_crouch, gameObject);
        }
        
    }
    [ServerRpc]
    void SetWalkServerRpc(bool value, NetworkObjectReference playerNetworkObjectReference) {
        CrouchClientRpc(value, playerNetworkObjectReference);
    }

    [ClientRpc]
    void CrouchClientRpc(bool value, NetworkObjectReference playerNetworkObjectReference) {
        if(playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            playerNetworkObject.GetComponent<Animator>().SetBool("crouching", value);
            if (value)
            {
                playerNetworkObject.GetComponent<CapsuleCollider2D>().offset = new Vector2(0.02604413f, 0.1193484f);
                playerNetworkObject.GetComponent<CapsuleCollider2D>().size = new Vector2(0.5381981f, 2.677239f);
            }
            else
            {
                playerNetworkObject.GetComponent<CapsuleCollider2D>().offset = new Vector2(0.02604413f, 0.4789118f);
                playerNetworkObject.GetComponent<CapsuleCollider2D>().size = new Vector2(0.5381981f, 3.396366f);
            }
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
            // StartCoroutine(TurnToIdleInstantlyInNextFrame(playerNetworkObject.gameObject));
        }
    }

    IEnumerator TurnToIdleInstantlyInNextFrame(GameObject player)
    {
        yield return new WaitForEndOfFrame();
        player.GetComponent<Animator>().Play("idle");
    }

    // IEnumerator DrawLine(GameObject weapon, float duration)
    // {
    //
    //     Vector2 weaponStartPosition = weapon.transform.position;
    //     float startTime = Time.time;
    //     while (Time.time - startTime < duration && weaponStartPosition.y != weaponStartPosition.y - 1)
    //     {
    //         float t = (Time.time - startTime) / duration;
    //
    //         Vector2 currentPosition = Vector2.Lerp(weaponStartPosition,
    //             new Vector2(weaponStartPosition.x, weaponStartPosition.y - 1), t);
    //         weapon.transform.position = currentPosition;
    //         yield return null;
    //     }
    // }
}
