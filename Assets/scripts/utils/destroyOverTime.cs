using UnityEngine;

public class destroyOverTime : MonoBehaviour
{
    public float destroyTime;
    private float _timer;
    private void FixedUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer > destroyTime)
        {
            Destroy(gameObject);
        }
    }
}