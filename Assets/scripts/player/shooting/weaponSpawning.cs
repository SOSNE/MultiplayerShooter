using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D.IK;


public class weaponSpawning : NetworkBehaviour
{
    public List<Transform> weapons;
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
    private bool _isItOnFlag = true;
    void Update()
    {
        if (!IsOwner) return;
        
        if( Input.GetKeyDown(KeyCode.Y) && _isItOnFlag)
        {
            _isItOnFlag = false;
            StartCoroutine(ChangeWeaponCoroutine());
        }
    }

    private bool _acknowledgmentFlag = false;
    private IEnumerator ChangeWeaponCoroutine()
    {
        gameObject.GetComponent<weaponSpawning>()._weaponSeatUpDone = false;
        PreparesAllClientsForWeaponChangeServerRpc(gameObject);
        yield return new WaitUntil(() => _acknowledgmentFlag);
        SpawnWeaponServerRpc(gameObject, 1);
        _acknowledgmentFlag = false;
    }
    
    [ServerRpc]
    private void PreparesAllClientsForWeaponChangeServerRpc(NetworkObjectReference targetPlayer, ServerRpcParams rpcParams = default)
    {
        if (!targetPlayer.TryGet(out NetworkObject playerNetworkObject)) return;
        GetChildWithTag(playerNetworkObject.transform, "weapon").GetComponent<NetworkObject>().Despawn();
       
        ResetIsSetupDoneForEveryClientRpc();
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId }
            }
        };
        AcknowledgeChangeWeaponClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void ResetIsSetupDoneForEveryClientRpc()
    {
        gameObject.GetComponent<weaponSpawning>()._weaponSeatUpDone = false;
    }
    [ClientRpc]
    private void AcknowledgeChangeWeaponClientRpc(ClientRpcParams clientRpcParams)
    {
        Debug.Log("Acknowledgment received: Server processed your request!");
        _acknowledgmentFlag = true;
    }

    public void SpawnWeapon()
    {
        print("spawn weapon for: " + gameObject.name);
        SpawnWeaponServerRpc(gameObject, 0);
    }
    
    [ClientRpc]
    private void PerformWeaponSetupClientRpc(NetworkObjectReference targetPlayer, NetworkObjectReference weaponNetworkObjectReference)
    {
        
        if (targetPlayer.TryGet(out NetworkObject playerNetworkObject))
        {
            if (weaponNetworkObjectReference.TryGet(out NetworkObject networkObjectWeapon))
            {
                Transform targetTransform = playerNetworkObject.transform;
                // if(targetTransform.name == "player: 1") Debug.Break();
                if (targetTransform.GetComponent<weaponSpawning>()._weaponSeatUpDone) return;
                Transform createdWeapon = networkObjectWeapon.transform;
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

                targetTransform.GetComponent<GameManager>().weapon = createdWeapon.gameObject;
                targetTransform.GetComponent<playerMovment>().weapon = createdWeapon;
                
                createdWeapon.GetComponent<pistolMovment>().camera = Camera.main;
                
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
                playerTransformsList.Add(FindChildByName(targetTransform, "rightArmStart"));
                playerTransformsList.Add(FindChildByName(targetTransform, "leftArmStart"));
                createdWeapon.GetComponent<pistolMovment>().playerTransforms = playerTransformsList.ToArray();


                List<Transform> waypointsList = new List<Transform>();
                waypointsList.Add(FindChildByName(targetTransform, "target (1)"));
                waypointsList.Add(FindChildByName(targetTransform, "target"));
                waypointsList.Add(FindChildByName(targetTransform, "AmmoBoxTarget"));
                waypointsList.Add(FindChildByName(createdWeapon, "ReloadingAnimTarget 2"));
                waypointsList.Add(FindChildByName(createdWeapon, "LeftArmTargetReloadTarget"));

                createdWeapon.GetComponent<reloading>().waypoint = waypointsList.ToArray();


                leftHandSolverTarget = createdWeapon.Find("LeftArmTarget");
                rightHandSolverTarget = createdWeapon.Find("RightArmTarget");

                LimbSolver2D leftArmSolver2D = targetTransform.Find("right arm solver").GetComponent<LimbSolver2D>();
                LimbSolver2D rightArmSolver2D = targetTransform.Find("left arm solver").GetComponent<LimbSolver2D>();


                var chainL = leftArmSolver2D.GetComponent<LimbSolver2D>().GetChain(0);
                chainL.target = leftHandSolverTarget;

                var chainR = rightArmSolver2D.GetComponent<LimbSolver2D>().GetChain(0);
                
                chainR.target = rightHandSolverTarget;
                isWeaponSpawned = true;
                targetTransform.GetComponent<weaponSpawning>()._weaponSeatUpDone = true;
            }
        }
    }


    [ServerRpc]
    private void SpawnWeaponServerRpc(NetworkObjectReference targetPlayer, int weaponToSpawnIndex)
    {
        if (targetPlayer.TryGet(out NetworkObject playerNetworkObject))
        {
            Transform targetTransform = playerNetworkObject.transform;
            Transform weapon = weapons[weaponToSpawnIndex];
            Transform createdWeapon = Instantiate(weapon, targetTransform.position, weapon.rotation);
            
            NetworkObject networkObject = createdWeapon.GetComponent<NetworkObject>();
            NetworkObject parentNetworkObject = targetTransform.GetComponent<NetworkObject>();
        
            networkObject.SpawnWithOwnership(parentNetworkObject.OwnerClientId);
            createdWeapon.transform.SetParent(targetTransform);
            PerformWeaponSetupClientRpc(targetPlayer, createdWeapon.gameObject);
            foreach (var data in GameManager.AllPlayersData)
            {
                //Find weapon that is already created at clint.
                if (!data.PlayerNetworkObject.TryGet(out NetworkObject playerNetworkObjectForEachPlayer)) return;
                if (playerNetworkObjectForEachPlayer.gameObject == gameObject) return;
                GameObject weaponOfThisPlayer = GetChildWithTag(playerNetworkObjectForEachPlayer.transform, "weapon").gameObject;
                PerformWeaponSetupClientRpc(data.PlayerNetworkObject, weaponOfThisPlayer);
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
    
    Transform GetChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }
        return null;
    }
}

