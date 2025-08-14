using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MoneyOperationUtils : NetworkBehaviour
{
    public int _moneyAmount = 0;
    private bool _doneFlag = false;
    private static Dictionary<string, int> CostsDictionary = new Dictionary<string, int>();
    public static MoneyOperationUtils Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        CostsDictionary["pistol"] = 850;
        CostsDictionary["arWeapon"] = 2000;
    }
    
    // public bool TryToBuy(string productString)
    // {
    // StartCoroutine(TryToBuyCoroutine(productString, result =>
    // {
    //     return result;
    // }));
    //     
    // }
    public IEnumerator TryToBuyCoroutine(string productString, System.Action<bool> callback)
    {
        _doneFlag = false;
        CheckMoneyAmountForServerRpc(NetworkManager.Singleton.LocalClientId);

        yield return new WaitUntil(() => _doneFlag);

        
        if (CostsDictionary[productString] <= _moneyAmount)
        {
            UpdatePlayerMoneyAmountServerRpc(-CostsDictionary[productString], NetworkManager.Singleton.LocalClientId);
            callback(true); 
        }
        else
        {
            callback(false);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void CheckMoneyAmountForServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId } }
        };

        foreach (var data in  GameManager.AllPlayersData)
        {
            
            if (data.ClientId == clientId)
            {
                CallbackAckClientRpc(data.MoneyAmount, clientRpcParams);
            }
        }
    }

    [ClientRpc]
    private void CallbackAckClientRpc(int moneyAmount, ClientRpcParams serverRpcParams = default)
    {
        _moneyAmount = moneyAmount;
        _doneFlag = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerMoneyAmountServerRpc(int moneyAmountToAdd, ulong clientId)
    {
        for (int i = 0; i < GameManager.AllPlayersData.Count; i++)
        {
            if (GameManager.AllPlayersData[i].ClientId == clientId)
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new List<ulong> { clientId }
                    }                
                };
                
                var data = GameManager.AllPlayersData[i];
                data.MoneyAmount += moneyAmountToAdd;
                GameManager.AllPlayersData[i] = data;
                GameObject.Find("UiControler").GetComponent<uiControler>()
                    .UpdateMoneyAmountUiClientRpc(data.MoneyAmount, clientRpcParams);
                uiControler.Instance.UpdateTabStatisticsMenuClientRpc(clientId, GameManager.AllPlayersData[i].Team, GameManager.AllPlayersData[i].PlayerName, GameManager.AllPlayersData[i].Kda[0], GameManager.AllPlayersData[i].Kda[1],GameManager.AllPlayersData[i].Kda[2],GameManager.AllPlayersData[i].MoneyAmount, GameManager.AllPlayersData[i].Alive);
                break;
            }
            
        }
    }
}
