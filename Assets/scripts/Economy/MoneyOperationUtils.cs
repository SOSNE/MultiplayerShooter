using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MoneyOperationUtils : NetworkBehaviour
{
    private int _moneyAmount = 0;
    private bool _doneFlag = false;
    private static Dictionary<string, int> CostsDictionary = new Dictionary<string, int>();

    private void Start()
    {
        CostsDictionary["pistol"] = 50;
        CostsDictionary["arWeapon"] = 100;
    }
    
    public bool TryToBuy(string productString)
    {
        StartCoroutine(BuyCoroutine());
        if (CostsDictionary[productString] <= _moneyAmount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private IEnumerator BuyCoroutine()
    {
        CheckMoneyAmountForServerRpc(NetworkManager.Singleton.LocalClientId);
        yield return new WaitUntil(() => _doneFlag);
        _doneFlag = false;
    }

    [ServerRpc]
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
}
