using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class FieldOfView : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] float inverseFov;
    [SerializeField] int rayCountHalf;
    [SerializeField] float minViewDistance = 5f;
    [SerializeField] float maxViewDistance;

    private float calcInverseFOV;
    private float calcMinCircleRadius;

    private Mesh mesh;
    float angleIncrement;
    private Vector2 origin;
    float counterClockwiseAngle;
    float clockwiseAngle;
    int fovCutoff;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        fovCutoff = 0;
        angleIncrement = 180f / rayCountHalf;
        counterClockwiseAngle = 0;
        clockwiseAngle = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        float distMouseTarget = (float)Math.Sqrt(Math.Pow(mousePos.x / Screen.width - 0.5f, 2) + Math.Pow(mousePos.y / Screen.height - 0.5f, 2));
        distMouseTarget = Mathf.Max(distMouseTarget, 0.1f);
        calcInverseFOV = inverseFov + distMouseTarget*4f;
        calcMinCircleRadius = Math.Max(minViewDistance - distMouseTarget*9f, 0.6f);

        Vector3[] vertices = new Vector3[rayCountHalf * 2 + 2 + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCountHalf * 2 * 3];

        vertices[0] = Vector3.zero;
        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = rayCountHalf; i >= fovCutoff; i--)
        {
            Vector3 vertex;

            float viewDistance = Utils.Sigmoid(calcInverseFOV * i / rayCountHalf - calcInverseFOV) * maxViewDistance + calcMinCircleRadius;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, GetVectorFromAngle(counterClockwiseAngle), viewDistance, layerMask);
            if (raycastHit2D.collider == null)
            {
                // no hit
                vertex = origin + GetVectorFromAngle(counterClockwiseAngle) * viewDistance;
            }
            else
            {
                // hit object
                vertex = GetVertexFromRaycast(origin, raycastHit2D, GetVectorFromAngle(counterClockwiseAngle));
            }
            Vector3 pos = transform.InverseTransformPoint(vertex);
            vertices[vertexIndex] = pos;

            if (i < rayCountHalf)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            counterClockwiseAngle -= angleIncrement;
        }

        for (int i = rayCountHalf; i >= fovCutoff; i--)
        {
            Vector3 vertex;

            float viewDistance = Utils.Sigmoid(calcInverseFOV * i / rayCountHalf - calcInverseFOV) * maxViewDistance + calcMinCircleRadius;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, GetVectorFromAngle(clockwiseAngle), viewDistance, layerMask);
            if (raycastHit2D.collider == null)
            {
                // no hit
                vertex = origin + GetVectorFromAngle(clockwiseAngle) * viewDistance;
            }
            else
            {
                // hit object
                vertex = GetVertexFromRaycast(origin, raycastHit2D, GetVectorFromAngle(clockwiseAngle));
            }
            Vector3 pos = transform.InverseTransformPoint(vertex);
            vertices[vertexIndex] = pos;

            if (i < rayCountHalf)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex;
                triangles[triangleIndex + 2] = vertexIndex - 1;

                triangleIndex += 3;
            }

            vertexIndex++;
            clockwiseAngle += angleIncrement;
        }
        
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

    }

    public static Vector2 GetVectorFromAngle(float angle)
    {
        //angle is in [0, 360]
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static Vector2 GetVertexFromRaycast(Vector2 origin, RaycastHit2D raycastHit2D, Vector2 direction)
    {
        Vector2 vertex = raycastHit2D.point + 1.5f * direction / Mathf.Pow(Mathf.Max(Vector2.Distance(raycastHit2D.point, origin), 2f), 1f);
        return vertex;
    }

    public Vector3 GetOrigin()
    {
        return this.origin;
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimAngle(Vector3 originPos, Vector3 mouseWorldPos)
    {
        Vector2 originPos2 = originPos;
        Vector2 mouseWorldPos2 = mouseWorldPos;
        Vector2 hypotenuse = mouseWorldPos - originPos;
        
        float newAngle = Mathf.Atan(hypotenuse.y / hypotenuse.x) * Mathf.Rad2Deg;
        if (hypotenuse.x < 0)
        {
            newAngle += 180f;
        }
        this.clockwiseAngle = newAngle;
        this.counterClockwiseAngle = newAngle;

        return;
    }

}
