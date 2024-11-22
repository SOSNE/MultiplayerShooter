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

    private void Update()
    {
        if (Input.GetKey(KeyCode.Y))
        {
            PerformRagdollOnPlayer(transform);
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
            PerformRagdollOnPlayer(transform);
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

    private void PerformRagdollOnPlayer(Transform playerTarget)
    {
        Transform bodyDown = transform.Find("bodyDown");
        playerTarget.GetComponent<IKManager2D>().enabled = false;
        playerTarget.GetComponent<Animator>().enabled = false;
        playerTarget.GetComponent<playerMovment>().enabled = false;
        var velocityToPass = playerTarget.GetComponent<Rigidbody2D>().linearVelocity;
        playerTarget.GetComponent<Rigidbody2D>().simulated = false;
        playerTarget.GetComponent<CapsuleCollider2D>().enabled = false;
        playerTarget.GetComponent<wlakingAnimation>().enabled = false;
        playerTarget.GetComponent<crouchingAnimation>().enabled = false;
        SetLayerRecursively(transform.Find("bodyDown").gameObject, 17);
        bodyDown.GetComponent<Rigidbody2D>().simulated = true;
        
        bodyDown.Find("bodyDownCollider").GetComponent<Rigidbody2D>().simulated = false;

        foreach (var joint2D in playerHingeJoints2d)
        {
            joint2D.enabled = true;
            GetChildWithTag(joint2D.gameObject.transform, "playerColliderDetection")
                .GetComponent<Rigidbody2D>().simulated = false;
            joint2D.gameObject.GetComponent<Rigidbody2D>().simulated = true;
            print(joint2D.gameObject.name);
        }
        bodyDown.Find("bodyUp").GetComponent<Rigidbody2D>().linearVelocity = velocityToPass * 1.5f;
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
