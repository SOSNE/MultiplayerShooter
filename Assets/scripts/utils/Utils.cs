using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using Random = System.Random;

public class Utils : NetworkBehaviour
{
    public static Utils Instance;
    public List<AudioClip> soundsList = new List<AudioClip>();
    public NetworkVariable<bool> allowFriendlyFire = new NetworkVariable<bool>(false);
    public AudioMixer mixer;
    public List<TextMeshProUGUI> textTypes = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> _textInstances = new List<TextMeshProUGUI>();
    
    
    [DllImport("__Internal")]
    private static extern void CopyWebGL(string str);

    private void Awake()
    {
        Instance = this;
        
        #if UNITY_EDITOR
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
        #endif
        
        _textInstances.Add(null);
        _textInstances.Add(null);
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
    
    public static void DoForSpecificChild(GameObject parent,string name, Action<GameObject> action)
    {
        if (parent == null || action == null || name == null) return;

        foreach (Transform child in parent.transform)
        {
            if(child.name == name) action(child.gameObject);

            DoForSpecificChild(child.gameObject, name, action);
        }
    }
    
    public static GameObject GetSpecificChild(GameObject parent,string name)
    {
        if (parent == null || name == null) return null;

        foreach (Transform child in parent.transform)
        {
            if(child.name == name) return child.gameObject;
            
            GameObject found = GetSpecificChild(child.gameObject, name);
            if (found != null)
                return found;
        }
        
        return null;
    }

    public static List<PlayerData> GetSelectedPlayersData(List<ulong> playerIds)
    {
        List<PlayerData> selectedPlayers = new List<PlayerData>(); 
        foreach (var data in GameManager.AllPlayersData)
        {
            if (playerIds.Contains(data.ClientId))
            {
                selectedPlayers.Add(data);
            }
        }
        return selectedPlayers;
    }
    
    [ClientRpc]
    public void SpawnPlayerOnSpawnPointClientRpc(NetworkObjectReference playerGameObject, NetworkObjectReference spawnGameObject)
    {
        List<int> positionsDistance = new List<int> { -3, -2, -1, 0, 1, 2, 3 };

        if(playerGameObject.TryGet(out NetworkObject playerNetworkObject))
        {
            if(spawnGameObject.TryGet(out NetworkObject spawnNetworkObject))
            {
                if (positionsDistance.Count != 0)
                {
                    Random random = new Random();
                    int randomIndex = random.Next(positionsDistance.Count);
                    var randomNumberDistance = positionsDistance[randomIndex];
                    playerNetworkObject.transform.position = spawnNetworkObject.transform.position +
                                                             new Vector3(randomNumberDistance, 0, 0);
                    positionsDistance.RemoveAt(randomIndex);
                }
                else
                {
                    playerNetworkObject.transform.position = spawnNetworkObject.transform.position;
                }
            }
        }
    }

    public void TextInformationSystem(string text, int typeOfTheText, float textAppearanceDelay, float textTimeToLive)
    {
        GameObject canvasParent = GameObject.Find("Canvas");
        if (_textInstances[typeOfTheText] != null)
        {
            //Potential bug when for example this object will start more coroutines.
            StopAllCoroutines(); 
            Destroy(_textInstances[typeOfTheText].gameObject);
        }
        _textInstances[typeOfTheText] = Instantiate(textTypes[typeOfTheText], canvasParent.transform).GetComponent<TextMeshProUGUI>();
        StartCoroutine(ShowText(_textInstances[typeOfTheText], text, textAppearanceDelay, textTimeToLive));
    }
    
    IEnumerator ShowText(TextMeshProUGUI textMeshPro, string fullText, float textAppearanceDelay, float textTimeToLive)
    {
        textMeshPro.text = ""; 
        
        foreach (char c in fullText)
        {
            textMeshPro.text += c; 
            yield return new WaitForSeconds(textAppearanceDelay); 
        }
        yield return new WaitForSeconds(textTimeToLive);
        Destroy(textMeshPro.gameObject);
    }
}
