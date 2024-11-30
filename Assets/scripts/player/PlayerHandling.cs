using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D.IK;


public class PlayerHhandling : NetworkBehaviour
{
    // NetworkBehaviourpublic NetworkList<int> playersHealth = new NetworkList<int>();
    public static Dictionary<ulong, int> clientHealthMap = new Dictionary<ulong, int>();
    private GameObject _gameManager;

    [SerializeField]
    private List<HingeJoint2D>  playerHingeJoints2d;
    
    
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            _gameManager = GameObject.Find("Game Manager");
            gameObject.GetComponent<GameManager>().CreateCamera();
        }
    }

    

    private void OnClientConnected(ulong clientId)
    {
        if (!IsOwner) return;
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            NewClientConnectionServerRpc(clientId);
            gameObject.GetComponent<GameManager>().AddClientToTeam(clientId);
        }
    }

    public void PlayerHit(int damageAmount,ulong clientId)
    {
        
        ulong currentClientId = NetworkManager.Singleton.LocalClientId;
        PlayerHitServerRpc(damageAmount, clientId, currentClientId);
    }

    [ServerRpc]
    private void NewClientConnectionServerRpc(ulong clientId ,ServerRpcParams serverRpcParams = default)
    {
        clientHealthMap.Add(clientId, 100);
        
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
    
    [ServerRpc]
    private void PlayerHitServerRpc(int damageAmount,ulong clientId , ulong currentClientId, ServerRpcParams serverRpcParams = default)
    {
        clientHealthMap[clientId] -= damageAmount;
        if (clientHealthMap[clientId]<=0)
        {
            gameObject.GetComponent<GameManager>().HandleGame(currentClientId, clientId);
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

    private List<Transform> _allBodyPartsTransformsList = new List<Transform>();
    public void PerformRagdollOnPlayer(Transform playerTarget)
    {
        Transform bodyDown = playerTarget.Find("bodyDown");
        playerTarget.GetComponent<IKManager2D>().enabled = false;
        playerTarget.GetComponent<Animator>().enabled = false;
        playerTarget.GetComponent<playerMovment>().enabled = false;
        var velocityToPass = playerTarget.GetComponent<Rigidbody2D>().linearVelocity;
        playerTarget.GetComponent<Rigidbody2D>().simulated = false;
        playerTarget.GetComponent<CapsuleCollider2D>().enabled = false;
        playerTarget.GetComponent<wlakingAnimation>().enabled = false;
        playerTarget.GetComponent<crouchingAnimation>().enabled = false;
        SetLayerRecursively(bodyDown.gameObject, 17);
        bodyDown.GetComponent<Rigidbody2D>().simulated = true;
        
        bodyDown.Find("bodyDownCollider").GetComponent<Rigidbody2D>().simulated = false;
        _allBodyPartsTransformsList.Add(bodyDown);
        SearchChildrenByTag(bodyDown, "bodyPart", true);
        
        bodyDown.Find("bodyUp").GetComponent<Rigidbody2D>().linearVelocity = velocityToPass * 1.5f;
    }
    
    public void TurnRagdollOf(Transform playerTarget)
    {
        Transform bodyDown = playerTarget.Find("bodyDown");
        // bodyDown.position = _allBodyPartsTransformsList[0].position;
        bodyDown.rotation = _allBodyPartsTransformsList[0].rotation;
        bodyDown.localScale = _allBodyPartsTransformsList[0].localScale;
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
                    _allBodyPartsTransformsList.Add(child);
                    child.GetComponent<Joint2D>().enabled = true;
                    GetChildWithTag(child, "playerColliderDetection")
                        .GetComponent<Rigidbody2D>().simulated = false;
                    child.GetComponent<Rigidbody2D>().simulated = true;
                }
                else
                {
                    // child.position = _allBodyPartsTransformsList[index].position;
                    child.rotation = _allBodyPartsTransformsList[index].rotation;
                    child.localScale = _allBodyPartsTransformsList[index].localScale;
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

}
