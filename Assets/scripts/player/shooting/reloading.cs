using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.Serialization;


public class reloading : NetworkBehaviour
{
    public Transform leftHandTarget, magazineSpawningTarget;
    private Transform _magazine;
    public Transform[] waypoint;
    private bool _isCoroutineRunning = false, _isMagazineStickingOut = false;
    public GameObject magazinePrefab, existingMagazine;
    [SerializeField] private float durationMagDropOveride = 0.5f;
    
    
    
    private GameObject _createdMagazine = null;
    void Start()
    {
        if (existingMagazine)
        {
            _magazine = existingMagazine.transform;
            _isMagazineStickingOut = true;
        }
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
            Transform weapon = playerNetworkObject.GetComponent<GameManager>().weapon.transform;

            if (IsOwner)
            {
                weapon.GetComponent<weaponHandling>().canShoot = false;
            }
            weapon.GetComponent<reloading>().
                StartCoroutine(GrabMagazine(weapon));
        }
    }
    
    IEnumerator GrabMagazine(Transform weapon)
    {
        if (!_isMagazineStickingOut) _magazine = Instantiate(magazinePrefab, magazineSpawningTarget.position, transform.rotation).transform;
        _magazine.SetParent(transform);
        Vector2 backDirection;
        if (transform.localScale.x < 0 || transform.localScale.y < 0 || transform.localScale.z < 0)
        {
            backDirection = transform.up;
        }
        else
        {
            backDirection = -transform.up;
        }

        float elapsed = 0f;
        
        bool dropMag = false;
        while (!dropMag)
        {
            if (!IsInside(transform, _magazine) || elapsed >= durationMagDropOveride) dropMag = true;
            _magazine.Translate(backDirection * 1f * Time.deltaTime, Space.World);
            elapsed += Time.deltaTime;
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
                _createdMagazine = Instantiate(magazinePrefab, leftHandTarget.position, magazinePrefab.transform.rotation);
                _createdMagazine.transform.SetParent(leftHandTarget);
            }

            if (index == waypoint.Length - 1)
            {
                leftHandTarget.SetParent(transform);
                if (!_isMagazineStickingOut)
                {
                    Destroy(_createdMagazine);
                }
                else
                {
                    Destroy(_createdMagazine);
                    _magazine = Instantiate(magazinePrefab, magazineSpawningTarget.position, transform.rotation).transform;
                    _magazine.transform.SetParent(transform);
                }
                
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
