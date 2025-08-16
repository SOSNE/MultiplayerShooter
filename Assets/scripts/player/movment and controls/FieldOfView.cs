using System;
using UnityEngine;
using UnityEngine.Serialization;

public class FieldOfView : MonoBehaviour
{
    private Mesh _mesh;
    
    private static float _fov = 90f;
    private static int _reyCount = 400;
    private float _angleOfIncrease = _fov / _reyCount;
    private float _viewDistance = 10f;
    

    private Vector3[] _vertices = new Vector3[_reyCount + 2];
    private Vector2[] _uv = new Vector2[_reyCount + 2];
    private int[] _triangles = new int[_reyCount * 3];

    public LayerMask fovLayerMask;
    public static Vector3 targetFovPositionOrigin;

    private void Start()
    {
        _mesh = new Mesh();
    }

    private void Update()
    {
        transform.position = targetFovPositionOrigin;
        float angle = 90;

        // Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        // float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = mouseWorldPosition - transform.position;

        float angleTarget = Mathf.Atan2(dir.y, dir.x);
        angleTarget *= Mathf.Rad2Deg;
        
        if (transform.localScale.x >= 0)
        {
            angle = angleTarget + _fov / 2;
        }
        else
        {
            angle = angleTarget - _fov / 2;
        }
        
        _vertices[0] = Vector3.zero;

        int triangleIndex = 0;
        for (int i = 0; i <= _reyCount; i++)
        {
            //For future. There was a lot of errors here because of different world spaces.
            RaycastHit2D hit2D = Physics2D.Raycast(transform.position, Utils.AngleToVector3(angle), _viewDistance, fovLayerMask);

            if (hit2D.collider != null)
            {
                _vertices[i + 1] = transform.InverseTransformPoint(hit2D.point);
   
            }
            else
            {
                _vertices[i + 1] = transform.InverseTransformPoint(transform.position + Utils.AngleToVector3(angle) * _viewDistance);
            }

            if (i > 0)
            {
                _triangles[triangleIndex] = 0;
                _triangles[triangleIndex + 1] = i;
                _triangles[triangleIndex + 2] = i + 1;
                triangleIndex += 3;
            }
            
            if (transform.localScale.x >= 0)
            {
                angle -= _angleOfIncrease;
            }
            else
            {
                angle += _angleOfIncrease;
            }
        }
        
        _mesh.vertices = _vertices;
        _mesh.uv = _uv;
        _mesh.triangles = _triangles;
            
        GetComponent<MeshFilter>().mesh = _mesh;
    }
}
