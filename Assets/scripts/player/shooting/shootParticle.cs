using Unity.Netcode;
using UnityEngine;

public class shootParticle : NetworkBehaviour
{
    private float _timer;
    void Update()
    {
        
        _timer += Time.deltaTime;
        if (_timer > 0.5f)
        {                     
            Destroy(gameObject);
        }
    }
}
