using System.Runtime.InteropServices;
using UnityEngine;

public class BrowserInputFix : MonoBehaviour
{
    // [DllImport("__Internal")]
    // private static extern void BlockBrowserShortcuts();
    //
    // void Start()
    // {
    //     #if !UNITY_EDITOR && UNITY_WEBGL
    //         BlockBrowserShortcuts();
    //     #endif
    // }
}