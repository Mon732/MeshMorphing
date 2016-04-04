using UnityEngine;
using System.Collections;

public class crossProductTest : MonoBehaviour
{
    public int[] verts;

    Mesh cachedMesh;
    Vector3 crossProduct;
    Vector3[] vertPositions;

	// Use this for initialization
	void Start ()
    {
        try
        {
            SkinnedMeshRenderer meshFilter = GetComponent<SkinnedMeshRenderer>();
            Mesh mesh = (Mesh)Instantiate(meshFilter.sharedMesh);
            meshFilter.sharedMesh = mesh;

            cachedMesh = mesh;
        }
        catch (MissingComponentException)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            cachedMesh = meshFilter.mesh;
        }

        vertPositions = new Vector3[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            vertPositions[i] = cachedMesh.vertices[verts[i]];
        }

        for (int j = 1; j < verts.Length; j++)
        {
            for (int i = 1; i < verts.Length; i++)
            {
                if (i != j)
                {
                    Vector3 lhs = vertPositions[j] - vertPositions[0];
                    Vector3 rhs = vertPositions[i] - vertPositions[0];

                    crossProduct = Vector3.Cross(lhs, rhs);

                    Debug.Log(j + " to " + i + ": " + crossProduct);
                }
            }
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 position = transform.position;

        Debug.DrawLine(cachedMesh.vertices[verts[0]] + position, cachedMesh.vertices[verts[1]] + position, Color.red);
        Debug.DrawLine(cachedMesh.vertices[verts[0]] + position, cachedMesh.vertices[verts[2]] + position, Color.green);
        Debug.DrawLine(cachedMesh.vertices[verts[0]] + position, cachedMesh.vertices[verts[3]] + position, Color.blue);

        Debug.DrawRay(cachedMesh.vertices[verts[0]] + position, crossProduct);
	}

    float calcAngle(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        Vector3 vectorA = point1 - point0;
        Vector3 vectorB = point2 - point0;

        return Vector3.Angle(vectorA, vectorB);
    }
}
