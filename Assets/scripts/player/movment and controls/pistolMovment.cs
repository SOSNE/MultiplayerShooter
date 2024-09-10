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
    private float width;
    [SerializeField] private List<Vector3> recoilBezierCurvesList = new List<Vector3>();
    private Vector3 _currentWeaponRecoilPosition;
    
    
    private void Start()
    {
        // _camera = Camera.main;
        width = GetComponent<Renderer>().bounds.size.x;
        maxExtension = maxExtension - width + 0.27;
        float distance = 1;
        Vector3 bottomLeft = transform.position + new Vector3(-distance, -distance, 0); // Bottom-left corner
        Vector3 topLeft = transform.position + new Vector3(-distance, distance, 0);     // Top-left corner
        Vector3 topRight = transform.position + new Vector3(distance, distance, 0);     // Top-right corner
        Vector3 bottomRight = transform.position + new Vector3(distance, -distance, 0); // Bottom-right corner

        recoilBezierCurvesList.Add(bottomLeft);
        recoilBezierCurvesList.Add(topLeft);
        recoilBezierCurvesList.Add(topRight);
        recoilBezierCurvesList.Add(bottomRight);
    }

    public void PerformRecoil()
    {
        StartCoroutine(WeaponRecoil(transform, 0.5f));
    }
    
    void Update()
    {
        if (!IsOwner) return;
        
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
        
        
        float angle = Mathf.Atan2(mouseWorldPosition.y - transform.position.y, mouseWorldPosition.x - transform.position.x);
        angle = (180 / Mathf.PI) * angle;
        
        
        
        if (leftArmExtenstion <= maxExtension || rightArmExtenstion <= maxExtension)
        {
            transform.position = new Vector3(mouseWorldPosition.x ,mouseWorldPosition.y) + _currentWeaponRecoilPosition;
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(0f,0f, angle + Convert.ToSingle(angleCorrection)));
            if (_currentWeaponRecoilPosition != new Vector3(0, 0, 0))
            {
                transform.position =  _currentWeaponRecoilPosition;
            }
        }
        if (leftArmExtenstionToWeapon >= 1.11931 || rightArmExtenstionToWeapon >= 1.11931)
        {
            Vector2 currentPosition = Vector2.Lerp(transform.position, positionFirstL.position, 0.049f);

            transform.position = currentPosition;
        }
    }
    
    IEnumerator WeaponRecoil(Transform weapon,float duration)
    {
        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            
            _currentWeaponRecoilPosition = Mathf.Pow((1 - t), 3) * recoilBezierCurvesList[0] +
                                     3 * Mathf.Pow((1 - t), 2) * t * recoilBezierCurvesList[1] +
                                     3 * (1 - t) * Mathf.Pow(t, 2) * recoilBezierCurvesList[2] +
                                     Mathf.Pow(t, 3) * recoilBezierCurvesList[3];
            yield return null;
        }

        _currentWeaponRecoilPosition = new Vector3(0, 0, 0);
    }
}
