using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Flatten : MonoBehaviour
{
    public bool skinnedMesh = false;

    Mesh cachedMesh;
    float averageTriangleArea;

    [SerializeField]
    List<int> vertsToDisplay;

    [SerializeField]
    Vector3[] verts;

    [SerializeField]
    int[] triangles;

	// Use this for initialization
	void Start ()
    {
        float startTime = Time.realtimeSinceStartup;

        if (!skinnedMesh)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            cachedMesh = meshFilter.mesh;
        }
        else
        {
            SkinnedMeshRenderer meshFilter = GetComponent<SkinnedMeshRenderer>();
            Mesh mesh = (Mesh)Instantiate(meshFilter.sharedMesh);
            meshFilter.sharedMesh = mesh;

            cachedMesh = mesh;
        }

        averageTriangleArea = calcAverageTriangleArea(cachedMesh.vertices, cachedMesh.triangles);

        int vert = 0;
        int[] neighbours = findNeighbours(vert, cachedMesh.vertices, cachedMesh.triangles);

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

        calcCurvature(cachedMesh.vertices[0], neighbourVerts.ToArray());

        verts = cachedMesh.vertices;
        triangles = cachedMesh.triangles;

        float endTime = Time.realtimeSinceStartup;
        float deltaTime = (endTime - startTime);

        Debug.Log("Startup took " + deltaTime + "seconds.");
	}
	
	// Update is called once per frame
	void Update ()
    {
        for (int i = 0; i < vertsToDisplay.Count; i++)
        {
            Debug.DrawLine(cachedMesh.vertices[vertsToDisplay[0]] + transform.position, cachedMesh.vertices[vertsToDisplay[i]] + transform.position);
        }
	}

    //point p, neighbours pᵢ
    float calcCurvature(Vector3 point, Vector3[] neighbours)
    {
        float angleSum = 0; //∑ᵢαᵢ

        for (int i = 0; i < neighbours.Length; i++)
        {
            angleSum += calcAngle(point, neighbours[i], neighbours[i + 1]);
        }

        float areaSum = 0; //∑ᵢAᵢ(p)

        for (int i = 0; i < neighbours.Length; i++)
        {
            areaSum += calcTriangleArea(point, neighbours[i], neighbours[i + 1]);
        }

        float curvature = ((2*Mathf.PI) - angleSum)/(averageTriangleArea + (areaSum/3)); //Kp

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

        return neighbours.ToArray();
    }
}
