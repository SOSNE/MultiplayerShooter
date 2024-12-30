using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D.IK;


public class weaponSpawning : NetworkBehaviour
{
    public Transform weapon;
    private Transform _leftHandTarget, _rightHandTarget, leftHandSolverTarget, rightHandSolverTarget;
    public bool isWeaponSpawned = false;
    // public LimbSolver2D leftArmSolver2D, rightArmSolver2D;
    public bool _weaponSeatUpDone = false;
    

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
    private void PerformWeaponSetupClientRpc(NetworkObjectReference targetPlayer)
    {
        if (_weaponSeatUpDone) return;
        if (targetPlayer.TryGet(out NetworkObject playerNetworkObject))
        {
            
                Transform targetTransform = playerNetworkObject.transform;
                Transform createdWeapon = targetTransform.Find("pistol_0(Clone)");
                // if(FindChildByName(targetTransform, "pistol_0(Clone)") != null) return;
                //
                // _createdWeapon = Instantiate(weapon, targetTransform.position, weapon.rotation);
                //
                // NetworkObject networkObject = _createdWeapon.GetComponent<NetworkObject>();
                // NetworkObject parentNetworkObject = targetTransform.GetComponent<NetworkObject>();
                // print("test");
                //
                // networkObject.SpawnWithOwnership(parentNetworkObject.OwnerClientId);
                //
                // _createdWeapon.transform.SetParent(targetTransform);

                targetTransform.GetComponent<GameManager>().pistol = createdWeapon.gameObject;
                targetTransform.GetComponent<playerMovment>().weapon = createdWeapon;


                Transform positionFirstL = FindChildByName(targetTransform, "rightArmStart");
                Transform positionFirstR = FindChildByName(targetTransform, "leftArmStart");

                createdWeapon.GetComponent<pistolMovment>().positionFirstL = positionFirstL;
                createdWeapon.GetComponent<pistolMovment>().positionFirstR = positionFirstR;

                List<Transform> playerTransformsList = new List<Transform>();
                playerTransformsList.Add(FindChildByName(targetTransform, "bodyDown"));
                playerTransformsList.Add(FindChildByName(targetTransform, "bodyUp"));
                playerTransformsList.Add(FindChildByName(targetTransform, "leftLegStart"));
                playerTransformsList.Add(FindChildByName(targetTransform, "rightLegStart"));
                playerTransformsList.Add(FindChildByName(targetTransform, "head"));
                createdWeapon.GetComponent<pistolMovment>().playerTransforms = playerTransformsList.ToArray();


                List<Transform> waypointsList = new List<Transform>();
                waypointsList.Add(FindChildByName(targetTransform, "target (1)"));
                waypointsList.Add(FindChildByName(targetTransform, "target"));
                waypointsList.Add(FindChildByName(targetTransform, "AmmoBoxTarget"));
                waypointsList.Add(FindChildByName(createdWeapon, "target (2)"));
                waypointsList.Add(FindChildByName(createdWeapon, "left arm target"));

                createdWeapon.GetComponent<reloading>().waypoint = waypointsList.ToArray();


                leftHandSolverTarget = createdWeapon.Find("left arm solver_Target");
                rightHandSolverTarget = createdWeapon.Find("right arm solver_Target");

                LimbSolver2D leftArmSolver2D = targetTransform.Find("right arm solver").GetComponent<LimbSolver2D>();
                LimbSolver2D rightArmSolver2D = targetTransform.Find("left arm solver").GetComponent<LimbSolver2D>();

                
                var chainL = leftArmSolver2D.GetComponent<LimbSolver2D>().GetChain(0);
                chainL.target = leftHandSolverTarget;

                var chainR = rightArmSolver2D.GetComponent<LimbSolver2D>().GetChain(0);
                chainR.target = rightHandSolverTarget;
                isWeaponSpawned = true;
                targetTransform.GetComponent<weaponSpawning>()._weaponSeatUpDone = true;
                // Debug.Break();
        }
    }
    

    [ServerRpc]
    private void SpawnWeaponServerRpc(NetworkObjectReference targetPlayer)
    {
        if (targetPlayer.TryGet(out NetworkObject playerNetworkObject))
        {
            Transform targetTransform = playerNetworkObject.transform;
            Transform createdWeapon = Instantiate(weapon, targetTransform.position, weapon.rotation);
            
            NetworkObject networkObject = createdWeapon.GetComponent<NetworkObject>();
            NetworkObject parentNetworkObject = targetTransform.GetComponent<NetworkObject>();
        
            networkObject.SpawnWithOwnership(parentNetworkObject.OwnerClientId);
            createdWeapon.transform.SetParent(targetTransform);
            foreach (var data in GameManager.AllPlayersData)
            {
                PerformWeaponSetupClientRpc(data.PlayerNetworkObject);
            }
            
        }
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

