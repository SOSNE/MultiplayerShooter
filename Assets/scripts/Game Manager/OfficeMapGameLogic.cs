using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OfficeMapGameLogic : NetworkBehaviour
{
    public static OfficeMapGameLogic Instance;
    public static GameObject serverGameObjectReference;

    public GameObject skillCheckMinGame;
    
    private GameObject _clientGameObject, _gameObjective;
    private bool _theMapIsOpen = false;
    private Coroutine _objcetiveDrillCoroutine;
    
    private void Awake()
    {
        Instance = this;
        
    }
    
    private bool _isShowingTextFlag = false, _objectiveStart = false;

    private void Update()
    {
        if(!_theMapIsOpen) return;
        float distance = Vector3.Distance(_clientGameObject.transform.position, _gameObjective.transform.position);

        if (!_objectiveStart) // BEFORE placing drill
        {
            if (distance <= 2f)
            {
                if (!_isShowingTextFlag)
                {
                    Utils.Instance.TextInformationSystem("Press E to place the drill", 1, .06f, 2f);
                    _isShowingTextFlag = true;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    PerformGameObjectiveLogicServerRpc();
                }

                if (Input.GetKeyUp(KeyCode.E))
                {
                    _objectiveStart = true;
                    Utils.Instance.StopTextInformationSystem(1); // clear old text
                    _isShowingTextFlag = false;
                }
            }
            else if (_isShowingTextFlag) // left range
            {
                Utils.Instance.StopTextInformationSystem(1);
                _isShowingTextFlag = false;
            }
        }
        else // AFTER drill placed → defuse mode
        {
            if (distance <= 2f)
            {
                if (!_isShowingTextFlag)
                {
                    Utils.Instance.TextInformationSystem("Press E to defuse the drill", 1, .06f, 2f);
                    _isShowingTextFlag = true;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    PerformDefuseAction();
                }
            }
            else if (_isShowingTextFlag) // left range
            {
                Utils.Instance.StopTextInformationSystem(1);
                _isShowingTextFlag = false;
            }
        }

    }
    
    
    [ServerRpc]
    private void PerformGameObjectiveLogicServerRpc()
    {
        GameManager gameManager = serverGameObjectReference.GetComponent<GameManager>();
        gameManager.StartCountdownTimerWithServerTimeClientRpc(30f);
        PerformGameObjectiveLogicClientRpc();
    }
    
    [ClientRpc]
    private void PerformGameObjectiveLogicClientRpc()
    {
        Utils.Instance.TextInformationSystem("Drill has been planted", 0, .06f, 2f);
        _gameObjective.transform.Find("Drill").gameObject.SetActive(true);
        _objcetiveDrillCoroutine = StartCoroutine(StartDrillTimerFinishVisuals(30));
    }
    
    private void PerformDefuseAction()
    {
        SkillCheckMInigameLogic.OnSucceedSkillCheckMiniGame += DefuseGameObjectiveLogicServerRpc;
        skillCheckMinGame.GetComponent<SkillCheckMInigameLogic>().StartSkillCheckMiniGame();
    }
    
    [ServerRpc]
    private void DefuseGameObjectiveLogicServerRpc()
    {
        RestartGameOfficeMap(3, 1);
        DefuseGameObjectiveLogicClientRpc();
    }
    
    [ClientRpc]
    private void DefuseGameObjectiveLogicClientRpc()
    {
        StopCoroutine(_objcetiveDrillCoroutine);
        Utils.Instance.TextInformationSystem("Drill has been disarmed", 0, .06f, 2f);
    }

    IEnumerator StartDrillTimerFinishVisuals(int durationTime)
    {
        int remainingTime = durationTime;
        while (remainingTime >= 0)
        {
            yield return new WaitForSeconds(1f);
            remainingTime -= 1;
        }
        Utils.Instance.TextInformationSystem("Raiders won", 0, .06f, 2f);
        if (IsServer)
        {
            RestartGameOfficeMap(3, 0);
        }
    }
    public void OnClientSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (!IsServer) return;
        if (sceneName != "Office") return;
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        
        TeleportPlayersToSpawn(clientId);
        
        List<ulong> playerIds = new List<ulong>{clientId};
        PlayerData currentPlayerData = Utils.GetSelectedPlayersData(playerIds)[0];
        PlayStartingTextMessageClientRpc(currentPlayerData.Team, clientRpcParams);
        StartingMapSetupClientRpc(clientRpcParams);
        // GameManager gameManager = serverGameObjectReference.GetComponent<GameManager>();
        // gameManager.StartCountdownTimerWithServerTimeClientRpc(10f);
    }
    
    [ClientRpc]
    private void StartingMapSetupClientRpc(ClientRpcParams clientRpcParams)
    {
        _clientGameObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().gameObject;
        _gameObjective = GameObject.Find("GameObjective");
        _theMapIsOpen = true;
    }

    [ClientRpc]
    private void PlayStartingTextMessageClientRpc(int currentPlayerDataTeam, ClientRpcParams clientRpcParams)
    {
        if (currentPlayerDataTeam == 0)
        {
            Utils.Instance.TextInformationSystem("Objective: Steal the documents. If impossible — eliminate all agents.", 0, .1f, 5f);
        }
        else
        {
            Utils.Instance.TextInformationSystem("Objective: Protect the documents or eliminate all Raiders.", 0, .1f, 5f);
        }
    }
    
    private void TeleportPlayersToSpawn(ulong clientId)
    {
        List<ulong> playerIds = new List<ulong>{clientId};
        PlayerData currentPlayerData = Utils.GetSelectedPlayersData(playerIds)[0];
        
        GameObject spawnPoint = GameObject.Find($"Team{currentPlayerData.Team}Spawn");
        Utils.Instance.SpawnPlayerOnSpawnPointClientRpc(currentPlayerData.PlayerNetworkObjectReference, spawnPoint);
    }

    private void RestartGameOfficeMap(float duration, int teamIndexOverwrite)
    {
        GameManager gameManager = serverGameObjectReference.GetComponent<GameManager>();
        gameManager.StartCountdownTimerWithServerTimeClientRpc(duration + 1);
        StartCoroutine(gameManager.NextRoundCoroutine(duration ,teamIndexOverwrite));
        _objectiveStart = false;
        _gameObjective.transform.Find("Drill").gameObject.SetActive(false);
    }
}
