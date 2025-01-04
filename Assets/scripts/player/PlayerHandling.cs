using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.IK;


public class PlayerHhandling : NetworkBehaviour
{
    private struct BodyPartData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public string Name;
    }
    
    // NetworkBehaviourpublic NetworkList<int> playersHealth = new NetworkList<int>();
    public static Dictionary<ulong, int> clientHealthMap = new Dictionary<ulong, int>();
    private GameObject _gameManager;

    [SerializeField]
    private List<HingeJoint2D>  playerHingeJoints2d;
    private List<BodyPartData> _allBodyPartsTransformsList = new List<BodyPartData>();

    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsClient)
        {
            _gameManager = GameObject.Find("Game Manager");
        }
        SearchChildrenByTagForCurrentPlayerRagdollBodyPartsList(transform, "bodyPart");
    }
    
    void SearchChildrenByTagForCurrentPlayerRagdollBodyPartsList(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                BodyPartData Data = new BodyPartData();
                Data.Position = child.localPosition;
                Data.Rotation = child.localRotation;
                Data.Scale = child.localScale;
                Data.Name = child.name;
                _allBodyPartsTransformsList.Add(Data);
            }
            SearchChildrenByTagForCurrentPlayerRagdollBodyPartsList(child, tag);
        }
    }

    public void PlayerHit(int damageAmount,ulong clientId, string hitBodyPartName , Vector2 direction)
    {
        DataToSendOverNetwork data;
        data.Direction = direction;
        ulong currentClientId = NetworkManager.Singleton.LocalClientId;
        PlayerHitServerRpc(damageAmount, clientId, currentClientId, hitBodyPartName, data);
    }

    
    
    [ServerRpc]
    private void PlayerHitServerRpc(int damageAmount,ulong clientId , ulong currentClientId, string hitBodyPartString , DataToSendOverNetwork data, ServerRpcParams serverRpcParams = default)
    {
        clientHealthMap[clientId] -= damageAmount;
        if (clientHealthMap[clientId]<=0)
        {
            gameObject.GetComponent<GameManager>().HandleGame(currentClientId, clientId, hitBodyPartString, data);
        }
        
        // update health ui
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        GameObject.Find("UiControler").GetComponent<uiControler>()
            .GetHealthForUiClientRpc(clientHealthMap[clientId], clientRpcParams);
    }

    public void PerformRagdollOnPlayer(Transform playerTarget, string hitBodyPartString , DataToSendOverNetwork data)
    {
        Transform bodyDown = playerTarget.Find("bodyDown");
        var velocityToPass = playerTarget.GetComponent<Rigidbody2D>().linearVelocity;
        playerTarget.GetComponent<IKManager2D>().enabled = false;
        // StartCoroutine(PerformPlayerMovementStop(playerTarget));
        // playerTarget.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        // playerTarget.GetComponent<Animator>().Update(1f);
        playerTarget.GetComponent<wlakingAnimation>().enabled = false;
        playerTarget.GetComponent<crouchingAnimation>().enabled = false;
        // EditorApplication.isPaused = true;
        StartCoroutine(PerformAnimationStop(playerTarget));
        // playerTarget.GetComponent<Animator>().enabled = false;
        playerTarget.GetComponent<CapsuleCollider2D>().enabled = false;
        playerTarget.GetComponent<Rigidbody2D>().simulated = false;

        SetLayerRecursively(bodyDown.gameObject, 17);
        playerTarget.GetComponent<playerMovment>().enabled = false;

        bodyDown.GetComponent<Rigidbody2D>().simulated = true;
        
        bodyDown.Find("bodyDownCollider").GetComponent<Rigidbody2D>().simulated = false;
        // _allBodyPartsTransformsList.Add(bodyDown);
        SearchChildrenByTag(bodyDown, "bodyPart", true);
        
        bodyDown.Find("bodyUp").GetComponent<Rigidbody2D>().linearVelocity = velocityToPass * 1.5f;
        AddForceToShotObject(playerTarget, hitBodyPartString, data);
    }

    IEnumerator PerformAnimationStop(Transform playerTarget)
    {
        yield return new WaitUntil(() => playerTarget.GetComponent<crouchingAnimation>().turnToIdleInstantlyDone);
        yield return new WaitForEndOfFrame(); 
        
        playerTarget.GetComponent<Animator>().enabled = false;
        playerTarget.GetComponent<crouchingAnimation>().turnToIdleInstantlyDone = false;
    }
    IEnumerator PerformPlayerMovementStop(Transform playerTarget)
    {
        yield return new WaitForEndOfFrame(); 
        playerTarget.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }
    
    private void AddForceToShotObject(Transform playerTarget ,string hitBodyPart , DataToSendOverNetwork data)
    {
        Transform targetBodyPart = GetChildWithNameRecursively(playerTarget, hitBodyPart).parent;
        
        if (targetBodyPart.GetComponent<Rigidbody2D>())
        {
            targetBodyPart.GetComponent<Rigidbody2D>().linearVelocity = data.Direction * 20;
        }
    }
    
    public void TurnRagdollOf(Transform playerTarget)
    {
        Transform bodyDown = playerTarget.Find("bodyDown");
        bodyDown.position = _allBodyPartsTransformsList[0].Position;
        bodyDown.rotation = _allBodyPartsTransformsList[0].Rotation;
        bodyDown.localScale = _allBodyPartsTransformsList[0].Scale;
        playerTarget.GetComponent<IKManager2D>().enabled = true;
        playerTarget.GetComponent<Animator>().enabled = true;
        playerTarget.GetComponent<playerMovment>().enabled = true;
        // var velocityToPass = playerTarget.GetComponent<Rigidbody2D>().linearVelocity;
        playerTarget.GetComponent<Rigidbody2D>().simulated = true;
        playerTarget.GetComponent<CapsuleCollider2D>().enabled = true;
        playerTarget.GetComponent<wlakingAnimation>().enabled = true;
        playerTarget.GetComponent<crouchingAnimation>().enabled = true;
        SetLayerRecursively(bodyDown.gameObject, 10);
        bodyDown.GetComponent<Rigidbody2D>().simulated = false;
        
        bodyDown.Find("bodyDownCollider").GetComponent<Rigidbody2D>().simulated = true;
        
        SearchChildrenByTag(bodyDown, "bodyPart", false);
        
        // bodyDown.Find("bodyUp").GetComponent<Rigidbody2D>().linearVelocity = velocityToPass * 1.5f;
    }
    
    void SearchChildrenByTag(Transform parent, string tag, bool ragdollOn)
    {
        int index = 0;
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                if (ragdollOn)
                {
                    child.GetComponent<Joint2D>().enabled = true;
                    GetChildWithTag(child, "playerColliderDetection")
                        .GetComponent<Rigidbody2D>().simulated = false;
                    child.GetComponent<Rigidbody2D>().simulated = true;
                }
                else
                {
                    foreach (BodyPartData savedData in _allBodyPartsTransformsList)
                    {
                        if (child.name == savedData.Name)
                        {
                            child.localPosition = savedData.Position;
                            child.localRotation = savedData.Rotation;
                            child.localScale = savedData.Scale;
                        }
                    }
                    
                    child.GetComponent<Joint2D>().enabled = false;
                    GetChildWithTag(child, "playerColliderDetection")
                        .GetComponent<Rigidbody2D>().simulated = true;
                    child.GetComponent<Rigidbody2D>().simulated = false;
                    index++;
                }
            }
            SearchChildrenByTag(child, tag, ragdollOn);
        }

        if (!ragdollOn)
        {
            
        }
    }
    
    
    
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
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
    Transform GetChildWithNameRecursively(Transform parent, string nameOfGameObject)
    {
        foreach (Transform child in parent)
        {
            if (child.name == nameOfGameObject)
            {
                return child;
            }
            
            Transform foundChild = GetChildWithNameRecursively(child, nameOfGameObject);
            if (foundChild != null)
            {
                return foundChild;
            }
        }

        return null; // No match found
    }
}
