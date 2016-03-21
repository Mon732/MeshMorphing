using UnityEngine;
using System.Collections;

public class vertexSphereProjection : MonoBehaviour
{
    public float radius = 1;

    [Range(0,1)]
    public float t = 1;

    public bool skinnedMesh = false;

    public bool drawLines = true;
    public Color lineColour = Color.green;

    Vector3[] cachedVertices;

	// Use this for initialization
	void Start ()
    {
        if (!skinnedMesh)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            cachedVertices = meshFilter.mesh.vertices;
        }
        else
        {
            SkinnedMeshRenderer meshFilter = GetComponent<SkinnedMeshRenderer>();
            Mesh mesh = (Mesh)Instantiate(meshFilter.sharedMesh);
            meshFilter.sharedMesh = mesh;

            cachedVertices = mesh.vertices;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3[] vertices = (Vector3[])cachedVertices.Clone();

        //Debug.Log(gameObject.name);
        //Debug.Log(vertices[0]);
        //Debug.Log(vertices[0].normalized);
        //Debug.Log(vertices[0].normalized.magnitude);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 pos = (vertices[i].normalized * radius);
            Vector3 vert = (pos * t) + (cachedVertices[i] * (1 - t));
            vertices[i] = vert;

            if (drawLines)
            {
                //Debug.DrawLine(transform.position, transform.position + pos, lineColour);
                Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + vert, lineColour);
            }
        }

        if (!skinnedMesh)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh.vertices = vertices;
        }
        else
        {
            SkinnedMeshRenderer meshFilter = GetComponent<SkinnedMeshRenderer>();
            meshFilter.sharedMesh.vertices = vertices;
        }
	}
}
