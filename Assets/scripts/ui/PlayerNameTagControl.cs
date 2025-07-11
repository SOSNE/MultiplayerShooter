using UnityEngine;

public class PlayerNameTagControl : MonoBehaviour
{
    public GameObject target;
    public float hightOverPlayer = 2;
    void Update()
    {
       if(!target) return;
       transform.position = target.transform.position + new Vector3(0, hightOverPlayer, 10);
    }
}
