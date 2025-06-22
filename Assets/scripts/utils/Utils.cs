using System.Runtime.InteropServices;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static Utils Instance;
    [DllImport("__Internal")]
    private static extern void CopyWebGL(string str);

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
    public void CopyText(string text) {
        #if UNITY_WEBGL && !UNITY_EDITOR
            CopyWebGL(text);
        #else
            GUIUtility.systemCopyBuffer = text;
        #endif
    }

}
