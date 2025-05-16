using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class shopUi : NetworkBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private List<Button> shopButtonsList = new List<Button>();
    public Transform trackingTransform;
    [SerializeField] private GameObject moneyOperationUtilsGameObject;
    private MoneyOperationUtils _moneyOperationUtils;
    private void Start()
    {
        shopButtonsList[0].onClick.AddListener(BuyPistol);
        shopButtonsList[1].onClick.AddListener(BuyAr);
        _moneyOperationUtils = moneyOperationUtilsGameObject.GetComponent<MoneyOperationUtils>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            shopPanel.SetActive(true);
        }
        if (Input.GetKeyUp(KeyCode.B))
        {
            shopPanel.SetActive(false);
        } 
        
        // For testing shop mechanics
        if (Input.GetKeyUp(KeyCode.M))
        {
            StartCoroutine(_moneyOperationUtils.TryToBuyCoroutine("arWeapon", result =>
            {
                if (result) return;
                
                _moneyOperationUtils.UpdatePlayerMoneyAmountServerRpc(1000, NetworkManager.Singleton.LocalClientId);
            }));
        } 
    }

    void BuyPistol()
    {
        // if (!_moneyOperationUtils.TryToBuy("pistol")) return;
        StartCoroutine(trackingTransform.GetComponent<weaponSpawning>().ChangeWeaponCoroutine(0));
    }
    void BuyAr()
    {
        // if (!_moneyOperationUtils.TryToBuy("arWeapon")) return;
        StartCoroutine(trackingTransform.GetComponent<weaponSpawning>().ChangeWeaponCoroutine(1));
    }
}
