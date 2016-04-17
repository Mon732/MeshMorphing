using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Flatten : MonoBehaviour
{
    Mesh cachedMesh;
    float averageTriangleArea;

    [SerializeField]
    List<int> vertsToDisplay;

    [SerializeField]
    Vector3[] verts;

    [SerializeField]
    int[] triangles;

    [SerializeField]
    int[] loopVerts;

	// Use this for initialization
	void Start ()
    {
        float startTime = Time.realtimeSinceStartup;

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

        averageTriangleArea = calcAverageTriangleArea(cachedMesh.vertices, cachedMesh.triangles);

        int vert = 12;
        int[] neighbours = findNeighbours(vert, cachedMesh.vertices, cachedMesh.triangles);
        neighbours = renderedVertsToMeshVerts(neighbours, cachedMesh.vertices);

        neighbours = getLoop(neighbours, cachedMesh.vertices);

        loopVerts = neighbours;

        vertsToDisplay = new List<int>();
        vertsToDisplay.Add(vert);

        string output = gameObject.name + ", neighbours of " + vert + ": ";

        List<Vector3> neighbourVerts = new List<Vector3>();

        foreach (int neighbour in neighbours)
        {
            output += neighbour + ", ";
            vertsToDisplay.Add(neighbour);
            neighbourVerts.Add(cachedMesh.vertices[neighbour]);
        }

        Debug.Log(output);

        float curvature = calcCurvature(cachedMesh.vertices[vert], neighbourVerts.ToArray());

        Debug.Log(gameObject.name + ", curvature of " + vert + ": " + curvature);

        verts = cachedMesh.vertices;
        triangles = cachedMesh.triangles;

        float endTime = Time.realtimeSinceStartup;
        float deltaTime = (endTime - startTime);

        Debug.Log("Startup took " + (deltaTime * 1000) + "ms.");
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 position = transform.position;

        for (int i = 0; i < vertsToDisplay.Count; i++)
        {
            Debug.DrawLine(cachedMesh.vertices[vertsToDisplay[0]] + transform.position, cachedMesh.vertices[vertsToDisplay[i]] + transform.position);
        }

        for (int i = 0; i < (loopVerts.Length - 1); i++)
        {
            int j = ((i + 1) % (loopVerts.Length - 1));

            Debug.DrawLine(cachedMesh.vertices[loopVerts[i + 1]] + position, cachedMesh.vertices[loopVerts[j + 1]] + position, Color.yellow);
        }
	}

    //point p, neighbours pᵢ
    float calcCurvature(Vector3 point, Vector3[] neighbours)
    {
        float angleSum = 0; //∑ᵢαᵢ

        for (int i = 0; i < (neighbours.Length - 1); i++)
        {
            angleSum += calcAngle(point, neighbours[i], neighbours[(i + 1)]);
        }

        float areaSum = 0; //∑ᵢAᵢ(p)

        for (int i = 0; i < (neighbours.Length - 1); i++)
        {
            areaSum += calcTriangleArea(point, neighbours[i], neighbours[(i + 1)]);
        }

        float curvature = ((2*Mathf.PI) - (Mathf.Deg2Rad*angleSum))/(averageTriangleArea + (areaSum/3)); //Kp

        return curvature;
    }

    //Calculates angle between three points.
    //Lines used are point 0 to point 1 and point 0 to point 2.
    float calcAngle(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        Vector3 vectorA = point1 - point0;
        Vector3 vectorB = point2 - point0;

        return Vector3.Angle(vectorA, vectorB);
    }

    float calcTriangleArea(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        Vector3 vectorA = point1 - point0;
        Vector3 vectorB = point2 - point0;
        float angle = calcAngle(point0, point1, point2);

        float area = 0.5f * vectorA.magnitude * vectorB.magnitude * Mathf.Sin(Mathf.Deg2Rad * angle);

        return area;
    }

    float calcAverageTriangleArea(Vector3[] vertices, int[] triangles)
    {
        float areaSum = 0;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 point0 = vertices[triangles[i    ]];
            Vector3 point1 = vertices[triangles[i + 1]];
            Vector3 point2 = vertices[triangles[i + 2]];

            areaSum += calcTriangleArea(point0, point1, point2);
        }

        return areaSum / (cachedMesh.triangles.Length / 3);
    }

    int[] findNeighbours(int index, Vector3[] vertices, int[] triangles)
    {
        List<int> neighbours = new List<int>();
        List<int> verts = Enumerable.Range(0, vertices.Length).Where(i => vertices[i] == vertices[index]).ToList();

        foreach (int vert in verts)
        {
            List<int> positions = Enumerable.Range(0, triangles.Length).Where(i => triangles[i] == vert).ToList();

            foreach (int position in positions)
            {
                int triNumber = (position / 3) * 3; //Which triangle

                neighbours.Add(triangles[triNumber]);
                neighbours.Add(triangles[triNumber + 1]);
                neighbours.Add(triangles[triNumber + 2]);
            }
        }

        neighbours = neighbours.Distinct().ToList();
        neighbours.Remove(index);
        neighbours.Insert(0, index);

        return neighbours.ToArray();
    }

    int[] renderedVertsToMeshVerts(int[] vertIndexes, Vector3[] meshVerts)
    {
        List<Vector3> verts = new List<Vector3>();

        foreach (int vertIndex in vertIndexes)
        {
            verts.Add(meshVerts[vertIndex]);
        }

        verts = verts.Distinct().ToList();
        List<int> newVerts = new List<int>();

        foreach (Vector3 vert in verts)
        {
            newVerts.Add(Array.IndexOf(meshVerts, vert));
        }

        return newVerts.ToArray();
    }

    int[] meshVertsToRenderedVerts(int[] vertIndexes, Vector3[] meshVerts)
    {
        List<int> indexes = vertIndexes.ToList();

        //indexes.Sort();

        foreach (int index in indexes)
        {
            List<int> renderedVerts = Enumerable.Range(0, meshVerts.Length).Where(i => meshVerts[i] == meshVerts[index]).ToList();

            foreach (int renderedVert in renderedVerts)
            {
                indexes.Add(renderedVert);
            }
        }

        return indexes.Distinct().ToArray();
    }

    int[] getLoop(int[] verts, Vector3[] meshVertices)
    {
        Vector3[] vertPositions = new Vector3[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            vertPositions[i] = meshVertices[verts[i]];
        }

        int[] loopVerts = new int[verts.Length];
        loopVerts[0] = 0;


        for (int j = 1; j < verts.Length; j++)
        {
            float[] angles = new float[verts.Length - 1];

            for (int i = 0; i < angles.Length; i++)
            {
                angles[i] = 361; //angle will never be above 360.
            }

            for (int i = 1; i < verts.Length; i++)
            {
                if (i != j)
                {
                    Vector3 lhs = vertPositions[j] - vertPositions[0];
                    Vector3 rhs = vertPositions[i] - vertPositions[0];

                    Vector3 crossProduct = Vector3.Cross(lhs, rhs);

                    //Debug.Log(verts[j] + " to " + verts[i] + ": " + crossProduct);

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
                //Debug.Log("Next vert is " + verts[smallestAngleTo] + " at " + smallestAngle + " degrees");
                loopVerts[j] = smallestAngleTo;
            }
        }

        int[] sortedVerts = new int[verts.Length];
        sortedVerts[0] = verts[0];

        int currentVert = 1;

        for (int i = 1; i < sortedVerts.Length; i++)
        {
            sortedVerts[i] = verts[currentVert];
            currentVert = loopVerts[currentVert];
        }

        return sortedVerts;
    }
}
