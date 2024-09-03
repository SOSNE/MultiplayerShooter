using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class pistolMovment : NetworkBehaviour
{
    public Transform targetL, targetR, positionL, positionR, positionFirstL, positionFirstR;
    public double maxExtension = 1.11931;
    public Camera camera;
    private float width;
    
    private void Start()
    {
        // _camera = Camera.main;
        width = GetComponent<Renderer>().bounds.size.x;
        maxExtension = maxExtension - width + 0.27;
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
            transform.position =new Vector2(mouseWorldPosition.x ,mouseWorldPosition.y) ;
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(0f,0f, angle + Convert.ToSingle(angleCorrection)));
        }
        if (leftArmExtenstionToWeapon >= 1.11931 || rightArmExtenstionToWeapon >= 1.11931)
        {
            Vector2 currentPosition = Vector2.Lerp(transform.position, positionFirstL.position, 0.049f);

            transform.position = currentPosition;
            // StartCoroutine(MoveWeaponToHand(gameObject, transform.position, targetR.position, 0.1f, leftArmExtenstionToWeapon, rightArmExtenstionToWeapon));
        }
    }
    
    // IEnumerator MoveWeaponToHand(GameObject weapon ,Vector2 startPoint, Vector2 endPoint, float duration, double leftArmExtenstionToWeapon, double rightArmExtenstionToWeapon)
    // {
    //     
    //     float startTime = Time.time;
    //     while (Time.time - startTime < duration || leftArmExtenstionToWeapon <= 1.11931 || rightArmExtenstionToWeapon <= 1.11931)
    //     {
    //         float t = (Time.time - startTime) / duration;
    //         
    //         Vector2 currentPosition = Vector2.Lerp(startPoint, endPoint, t);
    //
    //         weapon.transform.position = currentPosition;
    //         
    //         yield return null;
    //     }
    // }
}
