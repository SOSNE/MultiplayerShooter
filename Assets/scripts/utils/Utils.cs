using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

public class Utils : NetworkBehaviour
{
    public static Utils Instance;
    public List<AudioClip> soundsList = new List<AudioClip>();
    public NetworkVariable<bool> allowFriendlyFire = new NetworkVariable<bool>(false);
    public AudioMixer mixer;
    
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

    public void PlaySound(int soundListIndex, float volume, Transform soundTransform)
    {
        GameObject go = new GameObject("TempAudio");
        go.transform.position = soundTransform.position;

        AudioSource src = go.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];

        src.volume = volume;
        src.pitch = 1f;
        src.spatialBlend = 1f;
        src.minDistance = 15f;
        src.maxDistance = 100f;
        src.rolloffMode = AudioRolloffMode.Linear;

        src.PlayOneShot(soundsList[soundListIndex]);
        Destroy(go, soundsList[soundListIndex].length / src.pitch);
    }

    public PlayerData GetPlayerDataObjectFromClientIdReadOnly(ulong clientId)
    {
        //Call it only inside ServerRpc .
        foreach (var data in GameManager.AllPlayersData)
        {
            if (data.ClientId == clientId)
            {
                return data;
            }
        }
        return default;
    }
    
    public static Vector3 AngleToVector3(float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radians), Mathf.Sin(radians));
    }
    
    public static void DoForAllChildren(GameObject parent, Action<GameObject> action)
    {
        if (parent == null || action == null) return;

        foreach (Transform child in parent.transform)
        {
            // Apply the action to this child
            action(child.gameObject);

            DoForAllChildren(child.gameObject, action);
        }
    }
    
    public static float GetMiddleAngle(float a, float b)
    {
        a = (a % 360 + 360) % 360;
        b = (b % 360 + 360) % 360;

        float diff = Mathf.DeltaAngle(a, b);
        return a + diff / 2f;               
    }
}
