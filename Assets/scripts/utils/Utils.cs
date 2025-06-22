using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

public class Utils : NetworkBehaviour
{
    public static Utils Instance;
    public NetworkVariable<bool> allowFriendlyFire = new NetworkVariable<bool>(false);

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
