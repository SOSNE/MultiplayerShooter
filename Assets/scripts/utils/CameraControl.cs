using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraControl : NetworkBehaviour
{
    public Transform currentPlayer;
    [SerializeField] private float cameraSmoothness;
    void Start()
    {
        float screenAspect = (float)Screen.width / Screen.height;

        // Calculate the scaling needed to adjust the viewport
        float scale = screenAspect /(16f / 9f);
        
        if (scale < 1.0f)
        {
            //when screen is higher then wider
            Camera.main.rect = new Rect(0, (1f - scale) / 2f, 1, scale);
        }
        else
        {
            //when screen is wider then higher
            float widthScale = (16f / 9f) / screenAspect;
            Camera.main.rect = new Rect((1f - widthScale) / 2f, 0f, widthScale, 1f);
        }
    }

    private Vector3 _smoothPosition;
    private void FixedUpdate()
    {
        // if(!IsOwner) return;
        // This part cause bug. I think this it should be in ServerRcp because AllPlayersData list is local for host.
        // if (AllPlayersData.FirstOrDefault(obj => obj.ClientId == NetworkManager.Singleton.LocalClientId).Alive)
        if (currentPlayer) 
        {
            
            Camera camera = Camera.main.GetComponent<Camera>();
            Vector3 mouseScreenPosition = Input.mousePosition;
            Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, camera.nearClipPlane));
            Vector3 betweenPosition = Vector3.Lerp( currentPlayer.transform.position, mouseWorldPosition, 0.4f);
            _smoothPosition = Vector3.Lerp( Camera.main.transform.position, betweenPosition, cameraSmoothness);
            Camera.main.transform.position = new Vector3(_smoothPosition.x, _smoothPosition.y, -10f);
        }
    }

    public void CameraShake(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        // Vector3 start = Camera.main.transform.position
        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            float currentMagnitude = Mathf.Lerp(magnitude, 0, t);
            float x = Random.Range(-1f, 1f) * currentMagnitude;
            float y = Random.Range(-1f, 1f) * currentMagnitude;
            Camera.main.transform.localPosition = new Vector3(_smoothPosition.x + x, _smoothPosition.y + y, -10f);
            yield return null;
        }
    }
}
