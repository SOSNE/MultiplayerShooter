using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D.IK;


public class weaponSpawning : NetworkBehaviour
{
    public Transform weapon;
    private Transform _createdWeapon, _leftHandTarget, _rightHandTarget, leftHandSolverTarget, rightHandSolverTarget;
    private bool _isParent = false;
    public LimbSolver2D leftArmSolver2D, rightArmSolver2D;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        if (_isParent == false)
        {
            // SpawnWeaponServerRpc(gameObject);
            _isParent = true;
        }
    }

    private void Start()
    {
        if (!IsOwner) return;
        
        // SpawnWeaponServerRpc(gameObject);
        // _isParent = true;
        
    }

    void Update()
    {
        // if (!IsOwner) return;
    }

    public void SpawnWeapon()
    {
        SpawnWeaponServerRpc(gameObject);
    }
    
    [ClientRpc]
    private void ClientRpcNotifyServerRpcClientRpc(NetworkObjectReference targetPlayer)
    {
        if (targetPlayer.TryGet(out NetworkObject playerNetworkObject))
        {
            Transform targetTransform = playerNetworkObject.transform;
            _createdWeapon = Instantiate(weapon, targetTransform.position, weapon.rotation);
            
            NetworkObject networkObject = _createdWeapon.GetComponent<NetworkObject>();
            NetworkObject parentNetworkObject = transform.GetComponent<NetworkObject>();

            networkObject.SpawnWithOwnership(parentNetworkObject.OwnerClientId);
            
            _createdWeapon.transform.SetParent(targetTransform);

            targetTransform.GetComponent<GameManager>().pistol = _createdWeapon.gameObject;
            targetTransform.GetComponent<playerMovment>().weapon = _createdWeapon;

            
            Transform positionFirstL = FindChildByName(targetTransform,"rightArmStart");
            Transform positionFirstR = FindChildByName(targetTransform,"leftArmStart");
            
            _createdWeapon.GetComponent<pistolMovment>().positionFirstL = positionFirstL;
            _createdWeapon.GetComponent<pistolMovment>().positionFirstR = positionFirstR;

            List<Transform> playerTransformsList = new List<Transform>();
            playerTransformsList.Add(FindChildByName(targetTransform,"bodyDown"));
            playerTransformsList.Add(FindChildByName(targetTransform,"bodyUp"));
            playerTransformsList.Add(FindChildByName(targetTransform,"leftLegStart"));
            playerTransformsList.Add(FindChildByName(targetTransform,"rightLegStart"));
            playerTransformsList.Add(FindChildByName(targetTransform,"head"));
            _createdWeapon.GetComponent<pistolMovment>().playerTransforms = playerTransformsList.ToArray();

            
            List<Transform> waypointsList = new List<Transform>();
            waypointsList.Add(FindChildByName(targetTransform,"target (1)"));
            waypointsList.Add(FindChildByName(targetTransform,"target"));
            waypointsList.Add(FindChildByName(targetTransform,"AmmoBoxTarget"));
            waypointsList.Add(FindChildByName(_createdWeapon,"target (2)"));
            waypointsList.Add(FindChildByName(_createdWeapon,"left arm target"));

            _createdWeapon.GetComponent<reloading>().waypoint = waypointsList.ToArray();

            
            leftHandSolverTarget = _createdWeapon.Find("left arm solver_Target");
            rightHandSolverTarget = _createdWeapon.Find("right arm solver_Target");

            var chainL = leftArmSolver2D.GetComponent<LimbSolver2D>().GetChain(0);
            chainL.target = leftHandSolverTarget;
        
            var chainR = rightArmSolver2D.GetComponent<LimbSolver2D>().GetChain(0);
            chainR.target = rightHandSolverTarget;
            

        }
        
    }

    [ServerRpc]
    private void SpawnWeaponServerRpc(NetworkObjectReference targetPlayer)
    {
        ClientRpcNotifyServerRpcClientRpc(targetPlayer);
    }
    private Transform FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform result = FindChildByName(child, childName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}

