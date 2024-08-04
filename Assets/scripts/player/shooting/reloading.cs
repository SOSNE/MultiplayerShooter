using System;
using System.Collections;
using UnityEngine;

public class reloading : MonoBehaviour
{
    public Transform leftHandTarget, magazineSpawningTarget;
    private Transform _magazine;
    public Transform[] waypoint;
    private int _index = 0;
    private bool _isCoroutineRunning = false;
    public GameObject magazine;
    
    
    GameObject _createdMagazine = null;
    void Start()
    {
        _magazine = transform.Find("magazine");
    }

    private bool _ejectMagazine = false;
    void Update()
    {
        if (_createdMagazine != null)
        {
            _magazine = _createdMagazine.transform;
        }
        if (Input.GetKey(KeyCode.R))
        {
            _ejectMagazine = true;
        }
        if (IsInside(transform, _magazine) && _ejectMagazine)
        {
            Vector2 backDirection = -transform.up;
            _magazine.Translate(backDirection * 1f * Time.deltaTime, Space.World);
            
        }else if(!IsInside(transform, _magazine) && _ejectMagazine)
        {
            _ejectMagazine = false;
            _magazine.transform.SetParent(null);
            _magazine.GetComponent<Rigidbody2D>().simulated = true;
            _magazine.GetComponent<Rigidbody2D>().gravityScale = 2;
            if (!_isCoroutineRunning)
            {
                StartCoroutine(GrabMagazine());
            }
        }
    }
    
    bool IsInside(Transform outer, Transform inner)
    {
        Bounds outerBounds = outer.GetComponent<Renderer>().bounds;
        Bounds innerBounds = inner.GetComponent<Renderer>().bounds;

        return innerBounds.Intersects(outerBounds);
    }
    float _timer;
    
    
    IEnumerator GrabMagazine()
    {
        _isCoroutineRunning = true;
        leftHandTarget.SetParent(waypoint[2]);
        while (_index < waypoint.Length)
        {
            while (Vector2.Distance(leftHandTarget.position, waypoint[_index].position) > 0.01f)
            {
                leftHandTarget.position = Vector2.MoveTowards(leftHandTarget.position, waypoint[_index].position, 7 * Time.deltaTime);
                yield return null; 
            }
            
            if (_index == 2)
            {
                yield return new WaitForSeconds(2f);
                _createdMagazine = Instantiate(magazine, leftHandTarget.position, magazine.transform.rotation);
                _createdMagazine.transform.SetParent(leftHandTarget);
            }

            if (_index == waypoint.Length - 1)
            {
                if (_createdMagazine != null)
                {
                    Destroy(_createdMagazine);
                    _createdMagazine = Instantiate(magazine, magazineSpawningTarget.position, transform.rotation);
                    _createdMagazine.transform.SetParent(transform);
                    leftHandTarget.SetParent(transform);
                }
            }
            _index++;
        }

        _index = 0;
        _isCoroutineRunning = false;   
    }
}
