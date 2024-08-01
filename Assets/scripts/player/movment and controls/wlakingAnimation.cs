using UnityEngine;

public class wlakingAnimation : MonoBehaviour
{
    private Animator _animator;
    void Start()
    {
        _animator = GetComponent<Animator>();
    }
    
    
    void Update()
    {
        if (Mathf.Abs(GetComponent<Rigidbody2D>().linearVelocity.x) > 0.1)
        {
            _animator.SetBool("walking", true);
        }
        else
        {
            _animator.SetBool("walking", false); 
        }
        
    }
}
