using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class meshDatabase : MonoBehaviour 
{
	// Input from the unity editor. (Put in here all prefabs that are instantiated during the game)
	public GameObject[] inputPrefabs = new GameObject[10];
	public Mesh[] inputMeshes = new Mesh[10];

	// Contains all original meshes 
	private List<Mesh> originalMeshes = new List<Mesh> ();
	// Stores the index of each mesh (index into the other 3 datastructures)
	private Dictionary<string, int> meshIndices = new Dictionary<string, int>();
	// Stores for each mesh which vertices share the same location (for meshes with seperately stored vertices)
	private List<List<List<int> > > vertexConnectionLists = new List<List<List<int>>>();
	// Stores for each mesh the shared normals of each vertex
	private List<Vector3[]> vertex2NormalArrays = new List<Vector3[]>();
	// Stores for each mesh its original vertex locations
	private List<Vector3[]>  originalVertices = new List<Vector3[]>();

	// Use this for initialization
	void Start () 
	{
		for (int i = 0; i < inputMeshes.Length; i++) 
		{
			if(!inputMeshes[i])
				continue;
			addMesh(inputMeshes[i]);
		}

		for (int i = 0; i < inputPrefabs.Length; i++) 
		{
			if(!inputPrefabs[i])
				continue;

			Mesh mesh = inputPrefabs[i].GetComponent<MeshFilter>().mesh;
			if(!mesh) {
				Debug.Log ("No mesh found in " + inputPrefabs[i]);
				continue;
			}
			addMesh(mesh);
		}
		int a = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Puts together a "unique" string for some input mesh
	private string getHashString(Mesh mesh) {
		return mesh.name + mesh.vertexCount.ToString() + mesh.vertices[mesh.vertexCount/2].ToString() + mesh.bounds.size.ToString();
	}

	// Returns for each vertex its shared normal
	public Vector3[] getVertex2Normal(Mesh mesh) {
		int index;
		if (meshIndices.TryGetValue (getHashString(mesh), out index)) {
			return vertex2NormalArrays[index];
		} else
			return null;
	}

	// Returns a list containing at each index the indices of the vertices occupying the same 3D coordinates
	public List<List<int> > getVertexConnections(Mesh mesh) {
		int index;
		if (meshIndices.TryGetValue (getHashString(mesh), out index)) {
			return vertexConnectionLists[index];
		} else
			return null;
	}

	public Vector3[] getOriginalVertices(Mesh mesh) {
		int index;
		if (meshIndices.TryGetValue (getHashString (mesh), out index)) {
			return originalVertices [index];
		} else
			return null;
	}

	public bool hasMesh(Mesh mesh) {
		if (!mesh)
			return false;
		else
			return meshIndices.ContainsKey (getHashString(mesh));
	}

	public void addMesh(Mesh mesh) 
	{
		// Nothing to do if mesh is already in the database
		if (hasMesh (mesh))
			return;

		int index = originalMeshes.Count;
		originalMeshes.Add (mesh);

		// Create name for this mesh
		string name = getHashString (mesh);
		meshIndices.Add (name, index);

		int nofVertices = mesh.vertexCount;

		// Store the original vertices
		originalVertices.Add (mesh.vertices);

		// CALCULATE THE VERTEX_CONNECTIONS
		vertexConnectionLists.Add(new List<List<int> >());
		Vector3[] verticesCopy = mesh.vertices;
		for (int i = 0; i < nofVertices; i++) 
		{
			// If we have already stored this vertex in a previous iteration, then continue
			if(verticesCopy[i].x == float.MaxValue)
				continue;

			// Find all vertices sharing the same position 'pos' (including itself)
			Vector3 pos = verticesCopy[i];
			List<int> connectionsOf_i = new List<int>();
			for(int j = 0; j < nofVertices; j++) {
				if(pos == verticesCopy[j]) {
					connectionsOf_i.Add (j);
					// Mark the vertex such that it won't be concidered anymore in later iterations
					verticesCopy[j] = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				}
			}
			vertexConnectionLists[index].Add (connectionsOf_i);
		}
		// The number of vertices actually necessary for this mesh
		int nofSharedVertices = vertexConnectionLists [index].Count;


		// CALCULATE FOR EACH VERTEX ITS SHARED NORMAL AND STORE THEM AT THE VERTEX-INDICES OF THE ORIGINAL MESH
		vertex2NormalArrays.Add(new Vector3[nofVertices]);
		Vector3[] normals = mesh.normals;
		
		for (int i = 0; i < nofSharedVertices; i++) 
		{
			Vector3 sharedNormal = new Vector3(0,0,0);
			int nofSiblings = vertexConnectionLists[index][i].Count;
			for(int j = 0; j < nofSiblings; j++) {
				sharedNormal += normals[vertexConnectionLists[index][i][j]];
			}
			sharedNormal /= nofSiblings;
			sharedNormal.Normalize();
			for(int j = 0; j < nofSiblings; j++) {
				vertex2NormalArrays[index][vertexConnectionLists[index][i][j]] = sharedNormal;
			}
		}
		int a = 0;
	}



}
