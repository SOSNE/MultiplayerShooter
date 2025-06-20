using UnityEngine;

public class Utils : MonoBehaviour
{
    public static Utils Instance;
    
    private void Awake()
    {
        Instance = this;
        
        #if UNITY_EDITOR
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
        #endif

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
