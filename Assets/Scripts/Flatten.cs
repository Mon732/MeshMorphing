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

    int highestVert;
    float highestCurvature;

    struct edgeStruct
    {
        public int vertexA;
        public int vertexB;
    }

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

        //int vert = 12;

        for (int i = 0; i < cachedMesh.vertices.Length; i++)
        {
            //int i = 12;
            //int i = 260;

            int[] neighbours = findNeighbours(i, cachedMesh.vertices, cachedMesh.triangles);
            neighbours = renderedVertsToMeshVerts(neighbours, cachedMesh.vertices);

            List<int> vertsList = neighbours.ToList();
            vertsList.RemoveAt(0);
            neighbours = vertsList.ToArray();

            List<int> loops = new List<int>();
            
            neighbours = getLoop(neighbours[0], neighbours, loops, cachedMesh.vertices, cachedMesh.triangles);

            List<int> l = neighbours.ToList();
            l.Insert(0, i);
            neighbours = l.ToArray();
            
            loopVerts = neighbours;

            vertsToDisplay = new List<int>();
            vertsToDisplay.Add(i);

            string output = gameObject.name + ", neighbours of " + i + ": ";

            List<Vector3> neighbourVerts = new List<Vector3>();

            foreach (int neighbour in neighbours)
            {
                if (neighbour != i)
                {
                    output += neighbour + ", ";
                    vertsToDisplay.Add(neighbour);
                    neighbourVerts.Add(cachedMesh.vertices[neighbour]);
                }
            }

            Debug.Log(output);

            float curvature = calcCurvature(cachedMesh.vertices[i], neighbourVerts.ToArray());

            Debug.Log(gameObject.name + ", curvature of " + i + ": " + curvature);

            if (curvature > highestCurvature)
            {
                highestCurvature = curvature;
                highestVert = i;
            }
        }

        List<int> loopList = new List<int>();

        vertsToDisplay = findNeighbours(highestVert, cachedMesh.vertices, cachedMesh.triangles).ToList();
        loopVerts = getLoop(vertsToDisplay[0], vertsToDisplay.ToArray(), loopList, cachedMesh.vertices, cachedMesh.triangles);

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

        for (int i = 0; i < neighbours.Length; i++)
        {
            angleSum += calcAngle(point, neighbours[i], neighbours[(i + 1) % neighbours.Length]);
        }

        float areaSum = 0; //∑ᵢAᵢ(p)

        for (int i = 0; i < neighbours.Length; i++)
        {
            areaSum += calcTriangleArea(point, neighbours[i], neighbours[(i + 1) % neighbours.Length]);
        }

        Debug.Log("angleSum: " + (angleSum * Mathf.Rad2Deg));
        Debug.Log("2Pi - angleSum:" + ((2 * Mathf.PI) - (angleSum)));

        float curvature = ((2*Mathf.PI) - (angleSum))/(averageTriangleArea + (areaSum/3)); //Kp

        return curvature;
    }

    //Calculates angle between three points.
    //Lines used are point 0 to point 1 and point 0 to point 2.
    float calcAngle(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        Vector3 vectorA = point1 - point0;
        Vector3 vectorB = point2 - point0;

        float angle = Vector3.Angle(vectorA, vectorB);

        angle = Mathf.Deg2Rad * angle;

        return angle;
    }

    float calcTriangleArea(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        Vector3 vectorA = point1 - point0;
        Vector3 vectorB = point2 - point0;
        float angle = calcAngle(point0, point1, point2);

        float area = 0.5f * vectorA.magnitude * vectorB.magnitude * Mathf.Sin(angle);

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

        float areaAverage = areaSum / (cachedMesh.triangles.Length / 3);

        Debug.Log("Average Area: " + areaAverage);

        return areaAverage;
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

        foreach (int index in indexes.ToList())
        {
            List<int> renderedVerts = Enumerable.Range(0, meshVerts.Length).Where(i => meshVerts[i] == meshVerts[index]).ToList();

            foreach (int renderedVert in renderedVerts.ToList())
            {
                indexes.Add(renderedVert);
            }
        }

        return indexes.Distinct().ToArray();
    }

    int[] getLoop(int startVert, int[] verts, List<int> loopVerts, Vector3[] vertices, int[] triangles)
    {
        loopVerts.Add(startVert);

        int[] neighbours = findNeighbours(startVert, vertices, triangles);

        foreach (int neighbour in neighbours)
        {
            if (verts.Contains(neighbour) && !loopVerts.Contains(neighbour))
            {
                loopVerts = getLoop(neighbour, verts, loopVerts, vertices, triangles).ToList();
            }
        }

        return loopVerts.ToArray();
    }
}
