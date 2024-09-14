using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class pistolMovment : NetworkBehaviour
{
    public Transform targetL, targetR, positionL, positionR, positionFirstL, positionFirstR;
    public double maxExtension = 1.11931;
    public Camera camera;
    private float width, _rotationRecoilAngle, _angle;
    private Vector3 _currentWeaponRecoilPosition;
    
    
    private void Start()
    {
        // _camera = Camera.main;
        width = GetComponent<Renderer>().bounds.size.x;
        maxExtension = maxExtension - width + 0.27;
    }

    public void PerformRecoil()
    {
        StartCoroutine(WeaponRecoil(transform, 2f));
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
                
                Vector3 counterRecoilPosition = weaponTargetPosition + _currentWeaponRecoilPosition * 0.2f;

                transform.localPosition = counterRecoilPosition;
        }
        else 
        {
            
            if (Vector2.Distance(mouseWorldPosition, transform.position) > 0.3f)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0f,0f, _angle + Convert.ToSingle(angleCorrection) + _rotationRecoilAngle));
            }
            
            if (_currentWeaponRecoilPosition != new Vector3(0, 0, 0))
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
    
    IEnumerator WeaponRecoil(Transform weapon,float duration,float recoilAngleIncrease = 40, float recoilScale = 0.4f)
    {
        List<Vector3> recoilBezierCurvesList = new List<Vector3>();
        
        float distance = 0.3f;
        Vector3 bottomLeft = transform.localPosition; // Bottom-left corner
        Vector3 topLeft = transform.localPosition + new Vector3(0.38f, 0.34f, 0) * recoilScale;     // Top-left corner
        Vector3 topRight = transform.localPosition + new Vector3(1.3f, 0.56f, 0) * recoilScale;     // Top-right corner
        Vector3 bottomRight = transform.localPosition + new Vector3(1.5f, 0.57f, 0) * recoilScale; // Bottom-right corner

        recoilBezierCurvesList.Add(bottomLeft);
        recoilBezierCurvesList.Add(topLeft);
        recoilBezierCurvesList.Add(topRight);
        recoilBezierCurvesList.Add(bottomRight);

        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;

            _currentWeaponRecoilPosition = Mathf.Pow((1 - t), 3) * recoilBezierCurvesList[0] +
                                           3 * Mathf.Pow((1 - t), 2) * t * recoilBezierCurvesList[1] +
                                           3 * (1 - t) * Mathf.Pow(t, 2) * recoilBezierCurvesList[2] +
                                           Mathf.Pow(t, 3) * recoilBezierCurvesList[3];
            _rotationRecoilAngle = -Mathf.Lerp(0, recoilAngleIncrease, t);
            
            
            print(_rotationRecoilAngle);
            yield return null;
        }
        // return weapon position to its starting position
        transform.localPosition = bottomLeft;
        _currentWeaponRecoilPosition = Vector3.zero;
        _rotationRecoilAngle = 0;
    }
}
