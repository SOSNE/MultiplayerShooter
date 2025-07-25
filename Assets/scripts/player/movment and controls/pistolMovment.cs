using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class pistolMovment : NetworkBehaviour
{
    public GameObject rotationCenterPointGameObject;
    public Transform positionFirstL, positionFirstR, bulletSpawn, weaponEnd;
    public double partOfTheWeaponWithMaxExtendedHands, maxExtension;
    private double _width;
    public Camera camera;
    private float _rotationRecoilAngle, _angleTarget;
    private Vector3 _currentWeaponRecoilPosition;
    public Transform[] playerTransforms;
    [SerializeField] private Transform rotationCenterPoint;
    [SerializeField] private float recoilAngleMultiplicationRate, recoilPowerValue = 0.16f;
    
    private void Start()
    {
        // _camera = Camera.main;
        // _width = GetComponent<Renderer>().bounds.size.x; 
        
        maxExtension = maxExtension - _width + 0.27;
        maxExtension = Mathf.Lerp(0, (float)maxExtension, (float)partOfTheWeaponWithMaxExtendedHands);
    }

    private float GetClosestTransform(Transform[] transforms)
    {
        float closestDistance = Mathf.Infinity;
        foreach (var tr in transforms)
        {
            float tempDistance = Vector2.Distance(weaponEnd.position, tr.position);
            
            if (tempDistance < closestDistance)
            {
                closestDistance = tempDistance;
            }
        }
        
        return closestDistance;
    }

    public float  lastTime;
    
    public void PerformRecoil()
    {
        float weaponFirerate = gameObject.GetComponent<weaponHandling>().fierRateInSeconds;
        float elapsedTime = Time.time - lastTime;
        //strengthen the value to better display it on a function.
        elapsedTime *= 7f;
        //function that calculate Angle bay time between shoots. g(x)=10^(-(0.9 (x-0.51)))+0.3
        float recoilAngleIncreaseValue = Mathf.Pow(10, -0.9f * (elapsedTime - 0.51f))+0.3f;
        lastTime = Time.time;
        //strengthen the value.
        recoilAngleIncreaseValue *= recoilAngleMultiplicationRate;
        float distance = GetClosestTransform(playerTransforms);
        
        float normalizedParam = distance / (float)maxExtension;

        // Check if normalizedParam is less than threshold.
        float recoilScaleValue = 0;
        if (normalizedParam > 0.3f)
        {
            //this function ensures that recoil is smaller when weapon is closer to a body part.
            recoilScaleValue = Mathf.Lerp(0f, recoilPowerValue, (normalizedParam - 0.3f) / (1 - 0.3f));
        }
        
        if (_weaponRecoilCoroutine != null)
        {
            StopCoroutine(_weaponRecoilCoroutine);
            _weaponRecoilCoroutine = null;
        }
        _weaponRecoilCoroutine = StartCoroutine(WeaponRecoil(weaponFirerate,weaponFirerate*2, recoilScale:  recoilScaleValue, recoilAngleIncrease: recoilAngleIncreaseValue));
    }
    
    private Vector3 _pozitionForCalculatingWeaponRotation;
    bool _wasHolding= false;
    private Coroutine _weaponRecoilCoroutine;
    private GameObject _rotationCenterPointSpawned = null;


    void Update()
    {
        if (!IsOwner) return;
        if (!camera) return;
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = camera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, camera.nearClipPlane));
        
        double leftArmExtenstion = Vector2.Distance(positionFirstL.position, mouseWorldPosition);
        double rightArmExtenstion = Vector2.Distance(positionFirstR.position, mouseWorldPosition);
        double leftArmExtenstionToWeapon = Vector2.Distance(positionFirstL.position, transform.position);
        double rightArmExtenstionToWeapon =  Vector2.Distance(positionFirstL.position, transform.position);

        
        //For rotating weapon when clicing the second mouse button.
        bool isHolding = Input.GetMouseButton(1);
        
        if (isHolding)
        {
            if (!_wasHolding)
            {
                _pozitionForCalculatingWeaponRotation = mouseWorldPosition; 
                _rotationCenterPointSpawned =Instantiate(rotationCenterPointGameObject, _pozitionForCalculatingWeaponRotation,
                    rotationCenterPointGameObject.transform.rotation);
                _wasHolding = true;
            }
        }
        else
        {
            if (_rotationCenterPointSpawned != null)
            {
                Destroy(_rotationCenterPointSpawned);
                _rotationCenterPointSpawned = null;
            }
            _pozitionForCalculatingWeaponRotation = bulletSpawn.position;
        }

        _wasHolding = isHolding;
        
        float angleTarget = Mathf.Atan2(mouseWorldPosition.y - _pozitionForCalculatingWeaponRotation.y, mouseWorldPosition.x - _pozitionForCalculatingWeaponRotation.x);
        angleTarget *= Mathf.Rad2Deg;
        Transform playerParent = transform.parent;
        float angle = angleTarget;

        if (playerParent.localScale.x < 0 || playerParent.localScale.y < 0 || playerParent.localScale.z < 0)
        {
            // If the object is mirrored, invert the target rotation to compensate for flipping
            _rotationRecoilAngle = -_rotationRecoilAngle;
            // angleCorrection = -angleCorrection;
        }
        else
        {
            angle = angleTarget + 180f;
        }
        
        angle = Mathf.LerpAngle(transform.eulerAngles.z, angle, Time.deltaTime * 10f);
        
        if (leftArmExtenstion <= maxExtension || rightArmExtenstion <= maxExtension)
        {
                // local mouse position in relation to player
                Vector3 weaponTargetPosition = transform.parent.InverseTransformPoint(new Vector3(mouseWorldPosition.x, mouseWorldPosition.y));
                
                // Vector3 counterRecoilPosition = weaponTargetPosition + _currentWeaponRecoilPosition * 0.2f;
                
                transform.localPosition = weaponTargetPosition;
                
                if (transform.localScale.x < 0 || transform.localScale.y < 0 || transform.localScale.z < 0)
                {
                    // If the object is mirrored, invert the target rotation to compensate for flipping
                    _rotationRecoilAngle = -_rotationRecoilAngle;
                }
                transform.rotation = Quaternion.Euler(new Vector3(0f,0f, transform.rotation.z + _rotationRecoilAngle));

        }
        else 
        {
            
            if (Vector2.Distance(mouseWorldPosition, transform.position) > 0.3f)
            {
                // transform.rotation = Quaternion.Euler(new Vector3(0f,0f, _angle + Convert.ToSingle(angleCorrection) + _rotationRecoilAngle));
                transform.RotateAround(rotationCenterPoint.position, Vector3.forward,
                    angle - transform.eulerAngles.z + _rotationRecoilAngle);
                // print(angleCorrection + "angle");
            }
            
            // check if we are doing recoil and if weapon is not beyond the arms reach.
            if (_currentWeaponRecoilPosition != new Vector3(0, 0, 0) && !(leftArmExtenstionToWeapon >= 1.11931 || rightArmExtenstionToWeapon >= 1.11931))
            {
                transform.localPosition = _currentWeaponRecoilPosition;
            }
        }
        if (leftArmExtenstionToWeapon >= 1.11931 || rightArmExtenstionToWeapon >= 1.11931)
        {
            Vector2 currentPosition = Vector2.Lerp(transform.position, positionFirstL.position, 0.049f);

            transform.position = currentPosition;
        }
    }
    
    IEnumerator WeaponRecoil(float durationOfRecoil, float durationOfRecoilRegression,float recoilAngleIncrease = 20, float recoilScale = 0.03f)
    {
        List<Vector3> recoilBezierCurvesList = new List<Vector3>();

        Vector3 bottomLeft = transform.localPosition; // Bottom-left 
        Vector3 topLeft = transform.localPosition + new Vector3(1.0f, 0.2f, 0) * recoilScale;  // Top-left corner
        Vector3 topRight = transform.localPosition + new Vector3(1.5f, 0.3f, 0) * recoilScale;  // Top-right corner
        Vector3 bottomRight = transform.localPosition + new Vector3(1.8f, 0.3f, 0) * recoilScale;  // Bottom-right corner
        
        
        recoilBezierCurvesList.Add(bottomLeft);
        recoilBezierCurvesList.Add(topLeft);
        recoilBezierCurvesList.Add(topRight);
        recoilBezierCurvesList.Add(bottomRight);

        float startTime = Time.time;
        while (Time.time - startTime < durationOfRecoil)
        {
            float t = (Time.time - startTime) / durationOfRecoil;

            _currentWeaponRecoilPosition = Mathf.Pow((1 - t), 3) * recoilBezierCurvesList[0] +
                                           3 * Mathf.Pow((1 - t), 2) * t * recoilBezierCurvesList[1] +
                                           3 * (1 - t) * Mathf.Pow(t, 2) * recoilBezierCurvesList[2] +
                                           Mathf.Pow(t, 3) * recoilBezierCurvesList[3];
            _rotationRecoilAngle = -Mathf.Lerp(0, recoilAngleIncrease, t);
            
            yield return null;
        }
        // return weapon position to it`s starting position
        Vector3 initialPosition = transform.localPosition;

        float startTime2 = Time.time;
        while (Time.time - startTime2 < durationOfRecoilRegression)
        {
            float t = (Time.time - startTime2) / durationOfRecoilRegression;
            
            _currentWeaponRecoilPosition = Vector3.Lerp(initialPosition, bottomLeft, t);
            
            _rotationRecoilAngle = -Mathf.Lerp(recoilAngleIncrease, 0, t);


            yield return null;
        }
        
        // StartCoroutine(WeaponRegressionAfterRecoil(0.1f, transform.localPosition, bottomLeft)); 
        _currentWeaponRecoilPosition = Vector3.zero;
        _rotationRecoilAngle = 0;
    }
}
