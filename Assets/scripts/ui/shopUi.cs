using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class shopUi : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private List<Button> shopButtonsList = new List<Button>();
    public Transform trackingTransform;

    private void Start()
    {
        shopButtonsList[0].onClick.AddListener(BuyPistol);
        shopButtonsList[1].onClick.AddListener(BuyAr);
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
    }

    void BuyPistol()
    {
        StartCoroutine(trackingTransform.GetComponent<weaponSpawning>().ChangeWeaponCoroutine(0));
    }
    void BuyAr()
    {
        StartCoroutine(trackingTransform.GetComponent<weaponSpawning>().ChangeWeaponCoroutine(1));
    }
}
