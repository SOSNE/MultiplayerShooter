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
    public static bool ShopUiOpen = false;
    
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
            ShopUiOpen = true;
            uiControler.anyMenuIsOpen = true;
        }
        if (Input.GetKeyUp(KeyCode.B))
        {
            ShopUiOpen = false;
            uiControler.anyMenuIsOpen = false;
            shopPanel.SetActive(false);
        } 
        
        // For testing shop mechanics
        if (Input.GetKeyUp(KeyCode.M))
        {
            _moneyOperationUtils.UpdatePlayerMoneyAmountServerRpc(1000, NetworkManager.Singleton.LocalClientId);
            
            uiControler.Instance.OpenTabStatisticsMenuServerRpc();
        } 
    }

    void BuyPistol()
    {
        StartCoroutine(_moneyOperationUtils.TryToBuyCoroutine("pistol", result =>
        {
            if (!result) return;
                
            StartCoroutine(trackingTransform.GetComponent<weaponSpawning>().ChangeWeaponCoroutine(0));
        }));
    }
    
    void BuyAr()
    {
        StartCoroutine(_moneyOperationUtils.TryToBuyCoroutine("arWeapon", result =>
        {
            if (!result) return;
                
            StartCoroutine(trackingTransform.GetComponent<weaponSpawning>().ChangeWeaponCoroutine(1));
        }));
    }
}
