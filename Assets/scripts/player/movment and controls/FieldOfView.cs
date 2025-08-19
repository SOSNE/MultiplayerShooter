using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class FieldOfView : MonoBehaviour
{
    private Mesh _mesh;
    
    private static float _fov = 90f;
    public int reyCount = 80;
    
    private float _viewDistance = 20f;
    // private float _distanceThreshold = 2f;


    private List<Vector3> _vertices = new List<Vector3>();
    private List<Vector2> _uv = new List<Vector2>();
    private List<int> _triangles = new List<int>();

    public LayerMask fovLayerMask;
    public static Vector3 targetFovPositionOrigin;

    private void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

    }

    private void LateUpdate()
    {
        transform.position = targetFovPositionOrigin;
        _vertices.Clear();
        _triangles.Clear();
        _uv.Clear();
        float angle;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = mouseWorldPosition - targetFovPositionOrigin;
        float angleOfIncrease = _fov / reyCount;
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

        _vertices.Add(Vector3.zero);
        _uv.Add(Vector2.zero);

        // float previousAngle = 0;
        // float previousDistance = 0f; 
        //
        // bool isEdgeFlag = false;
        // float triangleIndex = 0;
        for (int i = 0; i <= reyCount; i++)
        {
            //For future. There was a lot of errors here because of different world spaces.
            RaycastHit2D hit2D = Physics2D.Raycast(targetFovPositionOrigin, Utils.AngleToVector3(angle), _viewDistance,
                fovLayerMask);
            // float currentDistance = Vector3.Distance(targetFovPositionOrigin, hit2D.point);

            if (hit2D.collider != null)
            {
                _vertices.Add(transform.InverseTransformPoint(hit2D.point));
                // Debug.DrawLine(targetFovPositionOrigin, transform.InverseTransformPoint(hit2D.point), Color.blue, 2.5f);
                // Debug.DrawLine(targetFovPositionOrigin, hit2D.point, Color.red, 2f);

                // if ((i > 0 && !isEdgeFlag) ||
                //     (i > 0 && Mathf.Abs(currentDistance - previousDistance) > _distanceThreshold))
                // {
                    // Debug.DrawLine(targetFovPositionOrigin, hit2D.point, Color.red, 2f);

                    // Vector3 vertices = FindEdgeVerticesLinearSearch(previousAngle, angle);
                    // Debug.DrawLine(targetFovPositionOrigin, transform.InverseTransformPoint(vertices), Color.red, 0.5f);
                    // if (vertices != Vector3.zero)
                    // {
                    //     _vertices.Add(transform.InverseTransformPoint(vertices));
                    //     _uv.Add(Vector2.zero);
                    // }

                    // isEdgeFlag = true;
                // }
            }
            else
            {
                _vertices.Add(transform.InverseTransformPoint(targetFovPositionOrigin +
                                                              Utils.AngleToVector3(angle) * _viewDistance));
                // if ((i > 0 && isEdgeFlag) ||
                //     (i > 0 && Mathf.Abs(currentDistance - previousDistance) > _distanceThreshold))
                // {
                //     Vector3 vertices = FindEdgeVerticesLinearSearch(previousAngle, angle);
                //     if (vertices != Vector3.zero)
                //     {
                //         _vertices.Add(transform.InverseTransformPoint(vertices));
                //         _uv.Add(Vector2.zero);
                //     }
                // }
                //
                // isEdgeFlag = false;
            }
            // previousDistance = currentDistance;

            _uv.Add(Vector2.zero);

            if (i > 0)
            {
                _triangles.Add(0);
                _triangles.Add(i);
                _triangles.Add(i + 1);
            }

            // previousAngle = angle;

            if (transform.localScale.x >= 0)
            {
                angle -= angleOfIncrease;
            }
            else
            {
                angle += angleOfIncrease;
            }
        }

            _mesh.Clear();
            _mesh.vertices = _vertices.ToArray();
            _mesh.uv = _uv.ToArray();
            _mesh.triangles = _triangles.ToArray();

        
    }

    // private Vector3 FindEdgeVerticesLinearSearch(float angleA, float angleB)
    // {
    //     Vector3 edgeVertex = Vector3.zero;
    //     bool edgeFound = false;
    //
    //     int iterations = 30;
    //     float distanceThreshold = 0.1f;
    //     float previousDistance = _viewDistance;
    //     bool previousWasHit = false;
    //     RaycastHit2D previousHit = default;
    //
    //     for (int i = 0; i <= iterations; i++)
    //     {
    //         float t = i / (float)iterations;
    //         float angleToCheck = Mathf.LerpAngle(angleA, angleB, t);
    //         Vector3 dir = Utils.AngleToVector3(angleToCheck);
    //
    //         RaycastHit2D hit = Physics2D.Raycast(targetFovPositionOrigin, dir, _viewDistance, fovLayerMask);
    //
    //         bool hitValid = hit.collider != null;
    //         float currentDistance = hitValid
    //             ? Vector3.Distance(targetFovPositionOrigin, hit.point)
    //             : _viewDistance;
    //
    //         if (!edgeFound && i > 0)
    //         {
    //             bool distanceJump = Mathf.Abs(currentDistance - previousDistance) > distanceThreshold;
    //
    //             if (hitValid && !previousWasHit) // miss → hit
    //             {
    //                 edgeVertex = hit.point;
    //                 edgeFound = true;
    //             }
    //             else if (!hitValid && previousWasHit) // hit → miss
    //             {
    //                 edgeVertex = hit.point;
    //             }
    //             else if (distanceJump)
    //             {
    //                 edgeVertex = previousHit.point;                   
    //                 edgeFound = true;
    //             }
    //         }
    //
    //         previousDistance = currentDistance;
    //         previousWasHit = hitValid;
    //         previousHit = hit;
    //         
    //         if (edgeFound)
    //         {
    //             Debug.DrawLine(targetFovPositionOrigin, edgeVertex, Color.red, 0.1f);
    //         }
    //     }
    //
    //     // if (edgeFound)
    //     // {
    //     //     Debug.DrawLine(targetFovPositionOrigin, edgeVertex, Color.red, 0.1f);
    //     // }
    //
    //     return edgeVertex;
    // }

    
    // private Vector3 FindEdgeVertices(float angleA, float angleB)
    // {
    //     Vector3 edgeVertex = Vector3.zero;
    //     float minDistance = 0f;
    //     // float maxDistance = _viewDistance;
    //
    //     for (int i = 0; i < 10; i++)
    //     {
    //         float middleAngle = Utils.GetMiddleAngle(angleA, angleB);
    //         Vector3 dir = Utils.AngleToVector3(middleAngle);
    //         RaycastHit2D hit = Physics2D.Raycast(targetFovPositionOrigin, dir, _viewDistance, fovLayerMask);
    //
    //         float distance = hit.collider != null ? Vector3.Distance(targetFovPositionOrigin, hit.point) : _viewDistance;
    //
    //         bool validHit = hit.collider != null && distance < _viewDistance - 0.01f;
    //
    //         // If hit and distance makes sense, treat it as a hit
    //         if (validHit)
    //         {
    //             // Check if the distance difference is small enough (real edge, not random far object)
    //             if (Mathf.Abs(distance - minDistance) <= _distanceThreshold || minDistance == 0f)
    //             {
    //                 edgeVertex = hit.point;
    //                 angleA = middleAngle;
    //                 minDistance = distance;
    //             }
    //             else
    //             {
    //                 // Too far away, likely another object behind → treat as miss
    //                 edgeVertex = targetFovPositionOrigin + dir * _viewDistance;
    //                 angleB = middleAngle;
    //                 // maxDistance = distance;
    //             }
    //         }
    //         else
    //         {
    //             // No hit → miss
    //             edgeVertex = targetFovPositionOrigin + dir * _viewDistance;
    //             angleB = middleAngle;
    //             // maxDistance = distance;
    //         }
    //     }
    //
    //     Debug.DrawLine(targetFovPositionOrigin, edgeVertex, Color.red, 0.1f);
    //     return edgeVertex;
    // }

}
