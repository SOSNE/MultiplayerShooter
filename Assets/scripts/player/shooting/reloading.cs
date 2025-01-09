using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;


public class reloading : NetworkBehaviour
{
    public Transform leftHandTarget, magazineSpawningTarget;
    private Transform _magazine;
    public Transform[] waypoint;
    public bool _isCoroutineRunning = false;
    public GameObject magazine;
    
    
    private GameObject _createdMagazine = null;
    void Start()
    {
        //_magazine = transform.Find("magazine");
    }

    private bool _ejectMagazine = false;
    void Update()
    {
        if(!IsOwner) return;
        float currentMagazineCount = GetComponent<weaponHandling>().bulletCounter;
        if (Input.GetKeyDown(KeyCode.R) && !_isCoroutineRunning && currentMagazineCount != 0)
        {
            _isCoroutineRunning = true;
            PerformReloadingServerRpc(transform.parent.gameObject);
        }
    }
    
    bool IsInside(Transform outer, Transform inner)
    {
        Bounds outerBounds = outer.GetComponent<Renderer>().bounds;
        Bounds innerBounds = inner.GetComponent<Renderer>().bounds;
    
        return innerBounds.Intersects(outerBounds);
    }

    [ServerRpc]
    private void PerformReloadingServerRpc(NetworkObjectReference playerGameObject)
    {
        PerformReloadingClientRpc(playerGameObject);
    }

    [ClientRpc]
    private void PerformReloadingClientRpc(NetworkObjectReference playerGameObject)
    {
        if (playerGameObject.TryGet(out NetworkObject playerNetworkObject))
        {
            if (IsOwner)
            {
                playerNetworkObject.transform.Find("pistol_0(Clone)").GetComponent<weaponHandling>().canShoot = false;
            }
            playerNetworkObject.transform.Find("pistol_0(Clone)").
                GetComponent<reloading>().
                StartCoroutine(GrabMagazine(playerNetworkObject.transform.Find("pistol_0(Clone)")));
        }
    }
    
    IEnumerator GrabMagazine(Transform weapon)
    {
        
        _magazine = Instantiate(magazine, magazineSpawningTarget.position, transform.rotation).transform;
        _magazine.SetParent(transform);
        Vector2 backDirection;
        if (transform.localScale == new Vector3(-1, -1, 1))
        {
            backDirection = transform.up;
        }
        else
        {
            backDirection = -transform.up;
        }
        
        while (IsInside(transform, _magazine))
        {
            _magazine.Translate(backDirection * 1f * Time.deltaTime, Space.World);
            
            yield return null;
        }
        
        _magazine.transform.SetParent(null);
        _magazine.GetComponent<Rigidbody2D>().simulated = true;
        _magazine.GetComponent<Rigidbody2D>().gravityScale = 2;


        leftHandTarget.SetParent(waypoint[2]);
        int index = 0;
        while (index < waypoint.Length)
        {
            while (Vector2.Distance(leftHandTarget.position, waypoint[index].position) > 0.01f)
            {
                leftHandTarget.position = Vector2.MoveTowards(leftHandTarget.position, waypoint[index].position,
                    2 * Time.deltaTime);
                yield return null;
            }
            
            if (index == 2)
            {
                yield return new WaitForSeconds(0.3f);
                _createdMagazine = Instantiate(magazine, leftHandTarget.position, magazine.transform.rotation);
                _createdMagazine.transform.SetParent(leftHandTarget);
            }

            if (index == waypoint.Length - 1)
            {
                leftHandTarget.SetParent(transform);
                Destroy(_createdMagazine);
            }
            index++;
        }
        weapon.GetComponent<weaponHandling>().bulletCounter = 0;
        if (IsOwner)
        {
            weapon.GetComponent<weaponHandling>().canShoot = true;
        }
        _isCoroutineRunning = false;
    }
}
