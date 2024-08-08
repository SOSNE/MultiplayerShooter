using UnityEngine;

public class bloodParticleSystem : MonoBehaviour
{
    private ParticleSystem system;
    private float time;
    void Start()
    {
        system = GetComponent<ParticleSystem>();
    }

    
    void Update()
    {
        time += Time.deltaTime;
        if (time > 0.3f)
        {
            system.Stop();
        }
    }
}
