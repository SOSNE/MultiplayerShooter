using System;
using UnityEngine;
using Unity.Netcode;

public class pistolMovment : NetworkBehaviour
{
    public Transform targetL, targetR, positionL, positionR, positionFirstL, positionFirstR;
    public double maxExtension = 1.11931;
    private Camera _camera;
    
    private void Start()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        if (!IsOwner) return;
        
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint(mouseScreenPosition);
        
        double leftArmExtenstion = Vector2.Distance(positionFirstL.position, mouseWorldPosition);
        double rightArmExtenstion = Vector2.Distance(positionFirstR.position, mouseWorldPosition);
        float pistolToMouseDistance = Vector2.Distance(transform.position, mouseWorldPosition);
        float correctionValue = 0.1f;

        float sin = correctionValue / pistolToMouseDistance;

        float angleCorrection = Mathf.Atan(sin);
        angleCorrection = (180 / Mathf.PI) * angleCorrection - 180;
        
        
        float angle = Mathf.Atan2(mouseWorldPosition.y - transform.position.y, mouseWorldPosition.x - transform.position.x);
        angle = (180 / Mathf.PI) * angle;
        
        
        
        if (leftArmExtenstion <= maxExtension || rightArmExtenstion <= maxExtension)
        {
            transform.position =new Vector2(mouseWorldPosition.x ,mouseWorldPosition.y) ;
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(0f,0f, angle + Convert.ToSingle(angleCorrection)));
        }
    }
}
