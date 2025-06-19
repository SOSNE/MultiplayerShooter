using UnityEngine;

public class Utils : MonoBehaviour
{
    public static Utils Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public GameObject GetMasterParent(Transform child)
    {
        while (child.parent != null)
        {
            child = child.parent;
        }
        return child.gameObject;
    }

}
