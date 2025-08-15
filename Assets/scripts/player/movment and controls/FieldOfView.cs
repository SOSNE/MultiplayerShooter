using System;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    private Mesh _mesh;
    
    private static float fov = 90f;
    private static int reyCount = 50;
    private float angleOfIncrease = fov / reyCount;
    private float viewDistance = 10f;
    

    private Vector3[] vertices = new Vector3[reyCount + 2];
    private Vector2[] uv = new Vector2[reyCount + 2];
    private int[] triangles = new int[reyCount * 3];

    public LayerMask fovLayerMask;

    private void Start()
    {
        _mesh = new Mesh();
    }

    private void Update()
    {
        float angle = 90;

        // Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        // float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = mouseWorldPosition - transform.position;

        float angleTarget = Mathf.Atan2(dir.y, dir.x);
        angleTarget *= Mathf.Rad2Deg;
        
        if (transform.localScale.x >= 0)
        {
            angle = angleTarget + fov / 2;
        }
        else
        {
            angle = angleTarget - fov / 2;
        }
        
        vertices[0] = Vector3.zero;

        int triangleIndex = 0;
        for (int i = 0; i <= reyCount; i++)
        {
            //For future. There was a lot of errors here because of different world spaces.
            RaycastHit2D hit2D = Physics2D.Raycast(transform.position, Utils.AngleToVector3(angle), viewDistance, fovLayerMask);

            if (hit2D.collider != null)
            {
                vertices[i + 1] = transform.InverseTransformPoint(hit2D.point);
   
            }
            else
            {
                vertices[i + 1] = transform.InverseTransformPoint(transform.position + Utils.AngleToVector3(angle) * viewDistance);
            }

            if (i > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = i;
                triangles[triangleIndex + 2] = i + 1;
                triangleIndex += 3;
            }
            
            if (transform.localScale.x >= 0)
            {
                angle -= angleOfIncrease;
            }
            else
            {
                angle += angleOfIncrease;
            }
        }


        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
            
        GetComponent<MeshFilter>().mesh = _mesh;
    }
}
