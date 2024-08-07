using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D.IK;


public class weaponSpawning : NetworkBehaviour
{
    public Transform weapon;
    private Transform _createdWeapon, _leftHandTarget, _rightHandTarget, leftHandSolverTarget, rightHandSolverTarget;
    private bool _isParent = false;
    public LimbSolver2D leftArmSolver2D, rightArmSolver2D;
    [SerializeField] private Flare somthing;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        if (_isParent == false)
        {
            SpawnWeaponServerRpc();
            _isParent = true;
        }
    }

    void Update()
    {
        if (!IsOwner) return;
    }
    
    [ClientRpc]
    private void ClientRpcNotifyServerRpcClientRpc(ClientRpcParams clientRpcParams = default)
    {
        _createdWeapon.transform.parent = transform;
        leftHandSolverTarget = _createdWeapon.Find("left arm solver_Target");
        rightHandSolverTarget = _createdWeapon.Find("right arm solver_Target");
        
        var chainL = leftArmSolver2D.GetComponent<LimbSolver2D>().GetChain(0);
        chainL.target = leftHandSolverTarget;
        
        var chainR = rightArmSolver2D.GetComponent<LimbSolver2D>().GetChain(0);
        chainR.target = leftHandSolverTarget;
    }

    [ServerRpc]
    private void SpawnWeaponServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _createdWeapon = Instantiate(weapon, transform.position, weapon.rotation);
        NetworkObject networkObject =_createdWeapon.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(OwnerClientId);
        
        
        
        ClientRpcNotifyServerRpcClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new [] { serverRpcParams.Receive.SenderClientId } } });
    }
    private GameObject FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child.gameObject;
            }

            GameObject result = FindChildByName(child, childName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}

