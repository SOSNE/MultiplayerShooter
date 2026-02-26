using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class OfficeMapGameLogic : NetworkBehaviour
{
    //Handling of when player connect on the other map. current setup dont handle tact for example. OnClientSceneLoaded is
    //called only when player first load the lobby.
    
    
    
    public NetworkVariable<bool> objectiveStart = new NetworkVariable<bool>(false);
    
    public static OfficeMapGameLogic Instance;
    public static GameObject serverGameObjectReference;

    public GameObject skillCheckMinGame;
    
    private GameObject _clientGameObject, _gameObjective;
    private bool _theMapIsOpen = false;
    private Coroutine _objectiveDrillCoroutine;
    private PlayerData _currentPlayerData;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Instance = this;
            print("subscribed");
            //this is called when any client loads the scene. and subscribing
            //to it will invoke this function when it happens.
            //OnSceneEvent handle normal scene changes ass also scene synchronizations
            //when player joins while new scene is already loaded
            NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
        }
    }

    void OnDisable()
    {
        if (IsServer) NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
    }
    
    private bool _isShowingTextFlag = false;

    private void Update()
    {
        if(!_theMapIsOpen) return;
        float distance = Vector3.Distance(_clientGameObject.transform.position, _gameObjective.transform.position);

        if (!objectiveStart.Value) // BEFORE placing drill
        {
            if (_currentPlayerData.Team == 0) return;

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
                    SetValueToObjectiveStartServerRpc(true);
                    // objectiveStart = true;
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
            if (_currentPlayerData.Team == 1) return;
            if (distance <= 2f)
            {
                if (!_isShowingTextFlag)
                {
                    Utils.Instance.TextInformationSystem("Hold E to defuse the drill", 1, .03f, 2f);
                    _isShowingTextFlag = true;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    PerformDefuseAction();
                }
                else if (Input.GetKeyUp(KeyCode.E))
                {
                    StopDefuseActionWithoutCompletion();
                }
            }
            else if (_isShowingTextFlag) // left range
            {
                Utils.Instance.StopTextInformationSystem(1);
                _isShowingTextFlag = false;
            }
        }

    }
    
    //Raiders
    [ServerRpc(RequireOwnership = false)]
    private void PerformGameObjectiveLogicServerRpc()
    {
        GameManager gameManager = serverGameObjectReference.GetComponent<GameManager>();
        gameManager.StartCountdownTimerWithServerTimeClientRpc(30f, 1);
        PerformGameObjectiveLogicClientRpc();
    }
    
    [ClientRpc]
    private void PerformGameObjectiveLogicClientRpc()
    {
        Utils.Instance.TextInformationSystem("Drill has been planted", 0, .06f, 2f);
        _gameObjective.transform.Find("Drill").gameObject.SetActive(true);
        _objectiveDrillCoroutine = StartCoroutine(StartDrillTimerFinishVisuals(30));
        GameManager.addToGameRestartQueue += ResetObjectiveCoroutineClientRpc;
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
            RestartGameOfficeMapClientRpc();
        }
    }
    
    [ClientRpc]
    private void ResetObjectiveCoroutineClientRpc()
    {
       StopCoroutine(_objectiveDrillCoroutine);
    }
    
    
    //Agents
    private void PerformDefuseAction()
    {
        SkillCheckMInigameLogic.OnSucceedSkillCheckMiniGame += DefuseGameObjectiveLogicServerRpc;
        skillCheckMinGame.GetComponent<SkillCheckMInigameLogic>().StartSkillCheckMiniGame();
    }
    private void StopDefuseActionWithoutCompletion()
    {
        SkillCheckMInigameLogic.OnSucceedSkillCheckMiniGame -= DefuseGameObjectiveLogicServerRpc;
        skillCheckMinGame.GetComponent<SkillCheckMInigameLogic>().StopSkillCheckMiniGame();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DefuseGameObjectiveLogicServerRpc()
    {
        RestartGameOfficeMap(3, 0);
        DefuseGameObjectiveLogicClientRpc();
    }
    
    [ClientRpc]
    private void DefuseGameObjectiveLogicClientRpc()
    {
        StopCoroutine(_objectiveDrillCoroutine);
        Utils.Instance.TextInformationSystem("Drill has been disarmed", 0, .06f, 2f);
    }

    public void OnSceneEvent(SceneEvent sceneEvent)
    {
        // print(" invoce subscribed");
        if (!IsServer) return;
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete || 
            sceneEvent.SceneEventType == SceneEventType.SynchronizeComplete)
        {
            // This is your Unique ID for the client
            StartCoroutine(SetupPlayerInNewMap(sceneEvent));
        }
    }
    
    private IEnumerator SetupPlayerInNewMap(SceneEvent sceneEvent)
    {
        ulong clientId = sceneEvent.ClientId;
        print("startCoutr");

        yield return new WaitUntil(() => Utils.Instance.GetPlayerObjectUsingClientId(clientId).GetComponent<GameManager>().teamAddingSetupDone);

        string sceneName = sceneEvent.SceneName;
            
        if (sceneName != "Office") yield break;
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        
        TeleportPlayersToSpawn(clientId);
        
        _currentPlayerData = Utils.GetSelectedPlayerData(clientId);
        PlayStartingTextMessageClientRpc(_currentPlayerData.Team, clientRpcParams);
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
        PlayerData currentPlayerData = Utils.GetSelectedPlayerData(clientId);
        print(currentPlayerData.PlayerName);
        GameObject spawnPoint = GameObject.Find($"Team{currentPlayerData.Team}Spawn");
        Utils.Instance.SpawnPlayerOnSpawnPointClientRpc(currentPlayerData.PlayerNetworkObjectReference, spawnPoint);
    }

    private void RestartGameOfficeMap(float duration, int teamIndexOverwrite)
    {
        GameManager gameManager = serverGameObjectReference.GetComponent<GameManager>();
        gameManager.StartCountdownTimerWithServerTimeClientRpc(duration + 1, 1);
        StartCoroutine(gameManager.NextRoundCoroutine(duration ,teamIndexOverwrite));
        RestartGameOfficeMapClientRpc();
    }

    [ClientRpc]
    private void RestartGameOfficeMapClientRpc()
    {
        SetValueToObjectiveStartServerRpc(false);
        // objectiveStart = false;
        skillCheckMinGame.GetComponent<SkillCheckMInigameLogic>().StopSkillCheckMiniGame();
        _gameObjective.transform.Find("Drill").gameObject.SetActive(false);
    }
    
    
    [ServerRpc(RequireOwnership = false)]

    public void SetValueToObjectiveStartServerRpc(bool val)
    {
        objectiveStart.Value = val;
    }
}
