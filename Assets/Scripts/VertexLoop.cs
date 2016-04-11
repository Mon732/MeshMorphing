using UnityEngine;
using System;
using System.Collections;


public class VertexLoop : MonoBehaviour
{
    public int[] verts;

    Mesh cachedMesh;
    Vector3[] vertPositions;
    int[] loopVerts;

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

        loopVerts = getLoop(verts, cachedMesh.vertices);
    }
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 position = transform.position;

        Debug.DrawLine(cachedMesh.vertices[verts[0]] + position, cachedMesh.vertices[verts[1]] + position, Color.red);
        Debug.DrawLine(cachedMesh.vertices[verts[0]] + position, cachedMesh.vertices[verts[2]] + position, Color.green);
        Debug.DrawLine(cachedMesh.vertices[verts[0]] + position, cachedMesh.vertices[verts[3]] + position, Color.blue);

        for (int i = 1; i < loopVerts.Length; i++)
        {
            Debug.DrawLine(vertPositions[loopVerts[i]] + position, vertPositions[loopVerts[((i + 1) % (loopVerts.Length - 1)) + 1]] + position, Color.yellow);
        }
	}

    int[] getLoop(int[] verts, Vector3[] meshVertices)
    {
        Vector3[] vertPositions = new Vector3[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            vertPositions[i] = meshVertices[verts[i]];
        }

        int[] loopVerts = new int[verts.Length];
        loopVerts[0] = verts[0];


        for (int j = 1; j < verts.Length; j++)
        {
            float[] angles = new float[verts.Length - 1];

            for (int i = 0; i < angles.Length; i++)
            {
                angles[i] = 361; //angle will never be above 180.
            }

            for (int i = 1; i < verts.Length; i++)
            {
                if (i != j)
                {
                    Vector3 lhs = vertPositions[j] - vertPositions[0];
                    Vector3 rhs = vertPositions[i] - vertPositions[0];

                    Vector3 crossProduct = Vector3.Cross(lhs, rhs);

                    Debug.Log(j + " to " + i + ": " + crossProduct);

                    float angle = calcAngle(vertPositions[0], vertPositions[j], vertPositions[i]);

                    if (crossProduct.y > 0)
                    {
                        angles[i - 1] = angle;
                    }
                    else
                    {
                        angles[i - 1] = 360 - angle;
                    }
                }
            }

            int smallestAngleTo = -1;
            float smallestAngle = 361;

            for (int i = 0; i < angles.Length; i++)
            {
                if (angles[i] < smallestAngle)
                {
                    smallestAngle = angles[i];
                    smallestAngleTo = i + 1;
                }
            }

            if (smallestAngle < 361)
            {
                Debug.Log("Next vert is " + smallestAngleTo + " at " + smallestAngle + " degrees");
                loopVerts[j] = smallestAngleTo;
            }
        }

        return loopVerts;
    }

    float calcAngle(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        Vector3 vectorA = point1 - point0;
        Vector3 vectorB = point2 - point0;

        return Vector3.Angle(vectorA, vectorB);
    }
}
