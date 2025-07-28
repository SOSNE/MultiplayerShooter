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
            //This canot be destroyed becouse it is spawned over network.
            system.Stop();
        }
    }
}
