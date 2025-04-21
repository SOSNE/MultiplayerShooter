using UnityEngine;

public class shopUi : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel; 

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
}
