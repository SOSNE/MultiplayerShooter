using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class pistolMovment : NetworkBehaviour
{
    public Transform positionFirstL, positionFirstR;
    public double maxExtension, width;
    public Camera camera;
    private float _rotationRecoilAngle, _angle;
    private Vector3 _currentWeaponRecoilPosition;
    public Transform[] playerTransforms;
    
    
    private void Start()
    {
        // _camera = Camera.main;
        // width = GetComponent<Renderer>().bounds.size.x;
        maxExtension = maxExtension - width + 0.27;
    }

    private float GetClosestTransform(Transform[] transforms)
    {
        float closestDistance = Mathf.Infinity;
        foreach (var tr in transforms)
        {
            float tempDistance = Vector2.Distance(transform.position, tr.position);
            
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
        float elapsedTime = Time.time - lastTime;
        //strengthen the value to better display it on a function.
        elapsedTime *= 7f;
        //function that calculate Angle bay time between shoots. g(x)=10^(-(0.9 (x-0.51)))+0.3
        float recoilAngleIncreaseValue = Mathf.Pow(10, -0.9f * (elapsedTime - 0.51f))+0.3f;
        lastTime = Time.time;
        //strengthen the value.
        recoilAngleIncreaseValue *= 15;
        float distance = GetClosestTransform(playerTransforms);
        
        float normalizedParam = distance / (float)maxExtension;

        // Check if normalizedParam is less than threshold.
        float recoilScaleValue = 0;
        if (normalizedParam > 0.3f)
        {
            //this function ensures that recoil is smaller when weapon is closer to a body part.
            recoilScaleValue = Mathf.Lerp(0f, 0.06f, (normalizedParam - 0.3f) / (1 - 0.3f));
        }
        StartCoroutine(WeaponRecoil(0.2f,0.1f, recoilScale:  recoilScaleValue, recoilAngleIncrease: recoilAngleIncreaseValue));
    }
    
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
        float pistolToMouseDistance = Vector2.Distance(transform.position, mouseWorldPosition);
        float correctionValue = 0.1f;

        float sin = correctionValue / pistolToMouseDistance;

        float angleCorrection = Mathf.Atan(sin);
        angleCorrection = (180 / Mathf.PI) * angleCorrection - 180;
        
        
        _angle = Mathf.Atan2(mouseWorldPosition.y - transform.position.y, mouseWorldPosition.x - transform.position.x);
        _angle = (180 / Mathf.PI) * _angle;
        
        
        
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
                if (transform.localScale.x < 0 || transform.localScale.y < 0 || transform.localScale.z < 0)
                {
                    // If the object is mirrored, invert the target rotation to compensate for flipping
                    _rotationRecoilAngle = -_rotationRecoilAngle;
                    angleCorrection = -angleCorrection;
                }
                transform.rotation = Quaternion.Euler(new Vector3(0f,0f, _angle + Convert.ToSingle(angleCorrection) + _rotationRecoilAngle));
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
    
    IEnumerator WeaponRecoil(float durationOfRagdoll, float durationOfRagdollRegression,float recoilAngleIncrease = 20, float recoilScale = 0.03f)
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
        while (Time.time - startTime < durationOfRagdoll)
        {
            float t = (Time.time - startTime) / durationOfRagdoll;

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
        while (Time.time - startTime2 < durationOfRagdollRegression)
        {
            float t = (Time.time - startTime2) / durationOfRagdollRegression;
            
            _currentWeaponRecoilPosition = Vector3.Lerp(initialPosition, bottomLeft, t);
            
            _rotationRecoilAngle = -Mathf.Lerp(recoilAngleIncrease, 0, t);


            yield return null;
        }
        
        // StartCoroutine(WeaponRegressionAfterRecoil(0.1f, transform.localPosition, bottomLeft)); 
        _currentWeaponRecoilPosition = Vector3.zero;
        _rotationRecoilAngle = 0;
    }
}
