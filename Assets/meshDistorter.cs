using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class meshDistorter : MonoBehaviour {

	// Defines by how much the initial mesh is distorted
	public float intensity;

	// Defines the frequency that is used for the perlin noise during Update(), when no shield etc is used
	public float defaultWobbleFrequency = 0.9f;
	// Defines the amplitude of the wobble effect during Update(), when no shield etc is used
	public float defaultWobbleIntensity = 0.1f;
	// Let's the wobble animation go faster
	public float defaultTimeMultiplier = 1.0f;

	// Defines the frequency that is used for the perlin noise during Update()
	private float wobbleFrequency;
	// Defines the amplitude of the wobble effect during Update()
	private float wobbleIntensity;
	// Let's the wobble animation go faster
	private float timeMultiplier;


	// Reference to the parent object (does not need to be a blob)
//	public GameObject parentBlob;
	// The MeshFilter
	private MeshFilter meshFilter;
	// The mesh object
	private Mesh mesh;

	
	// The original vertices before deformation, scaling, rotation
	private Vector3[] originalVertices;
	// Stores for each vertex, which shared normal it has
	private Vector3[] vertex2NormalMap;
	// A random Vector2 initialized at Start(), that is used to give an offset for the perlin noise function in Update()
	private Vector2 initialOffset;
	// The size of the undeformed, unscaled and unrotated mesh
	private Vector3 meshExtent;


	private player parentPlayerScript;
	
	private enemy parentEnemyScript;
	
	private bool isPlayer;

	private distortionDatabase distortionDatabase;
	
	private List<List<int> > vertexConnections;

	private meshDatabase meshDatabase;
	

	// Use this for initialization
	void Start () 
	{
		meshDatabase = (meshDatabase)GameObject.Find ("MeshDatabase").GetComponent (typeof(meshDatabase));
		distortionDatabase = (distortionDatabase)GameObject.Find ("DistortionDatabase").GetComponent(typeof(distortionDatabase));

		meshFilter = this.gameObject.GetComponent<MeshFilter> ();
		mesh = meshFilter.mesh;
		mesh.MarkDynamic ();
		Bounds bounds = mesh.bounds;
		meshExtent = bounds.max - bounds.min;

		originalVertices = mesh.vertices;

		if (!meshDatabase.hasMesh (mesh))
			meshDatabase.addMesh (mesh);

		vertex2NormalMap = meshDatabase.getVertex2Normal (mesh);
		vertexConnections = meshDatabase.getVertexConnections (mesh);

	/*	vertex2NormalMap = new Vector3[originalVertices.Length];
		Vector3[] normals = mesh.normals;

		for (int i = 0; i < originalVertices.Length; i++) {
			Vector3 pos = originalVertices[i];
			Vector3 sharedNormal = new Vector3(0,0,0);
			float count = 0;
			for(int j = 0; j < originalVertices.Length; j++) {
				if(pos == originalVertices[j]) {
					sharedNormal += normals[j];
					count++;
				}
			}
			vertex2NormalMap[i] = sharedNormal/count;
			vertex2NormalMap[i].Normalize();
		}*/

		parentEnemyScript = (enemy)this.gameObject.GetComponent (typeof(enemy));
		parentPlayerScript = (player)this.gameObject.GetComponent (typeof(player));
		isPlayer = (bool)parentPlayerScript;

		initialOffset = new Vector2(Random.Range (-32000.0f, 32000.0f), Random.Range (-32000.0f, 32000.0f));
		distortMesh ();

		wobbleFrequency = defaultWobbleFrequency;
		wobbleIntensity = defaultWobbleIntensity;
		timeMultiplier = defaultTimeMultiplier;
	}
	
	// Update is called once per frame
	void Update () {

		Vector3[] vertices = mesh.vertices;

		// TODO Could put this into an IEnumerable and let it run at a lower framerate to safe ressources
		float currentTime = Time.time * timeMultiplier;
		for (int i = 0; i < vertices.Length; i++) {
			float offsetFactor = wobbleIntensity * meshExtent.x * (Mathf.PerlinNoise((originalVertices[i].x + initialOffset.x)*wobbleFrequency + currentTime, (originalVertices[i].y+initialOffset.y)*wobbleFrequency + currentTime)-0.5f);
			vertices[i] = originalVertices[i] + vertex2NormalMap[i]*offsetFactor;
		}
		wobbleFrequency = defaultWobbleFrequency;
		wobbleIntensity = defaultWobbleIntensity;
		timeMultiplier = defaultTimeMultiplier;

		mesh.vertices = vertices;
		mesh.RecalculateBounds ();
	}


	public void activateShield(EDistortionType distortionType) 
	{
		distortionDatabase.getShieldDistortionParameters (distortionType, ref wobbleFrequency, ref wobbleIntensity, ref timeMultiplier);
	}


	public void distortMesh()
	{
		Debug.Log ("distort mesh of " + this.gameObject);
		Vector3[] vertices = mesh.vertices;

		// Distort mesh if desired (i.e. when intensity != 0)
		if (intensity != 0.0f) 
		{
			for(int i = 0; i < vertexConnections.Count; i++) 
			{
				Vector3 rndDirection = Random.insideUnitSphere * intensity;
				for(int j = 0; j < vertexConnections[i].Count; j++) 
				{
					vertices [vertexConnections[i][j]] += rndDirection;
				}
			}

		/*	for (int i = 0; i < vertices.Length; i++) {
				Vector3 pos = vertices [i];
				// If this vertex has already been modified, then continue
				if (pos != originalVertices [i])
					continue;
				Vector3 rndDirection = Random.insideUnitSphere * intensity;
				for (int j = 0; j < vertices.Length; j++) {
					if (vertices [j] == pos)
						vertices [j] += rndDirection;
				}
			}*/
			mesh.vertices = vertices;
			mesh.RecalculateBounds ();

			// Make sure the mesh stays more or less the size of a unit sphere
			Bounds bounds = mesh.bounds;
			float diameter = (bounds.max - bounds.min).magnitude;
			float shrinkFactor = 4.0f / diameter;
			for (int i = 0; i < vertices.Length; i++) {
				vertices [i] *= shrinkFactor;
			}
			originalVertices = vertices;

			mesh.vertices = vertices;
			mesh.RecalculateBounds ();
			mesh.RecalculateNormals ();

			// Calculate for each vertex its shared normal
		/*	Vector3[] normals = mesh.normals;

			for(int i = 0; i < vertexConnections.Count; i++) 
			{
				Vector3 sharedNormal = new Vector3(0,0,0);
				int nofSiblings = vertexConnections[i].Count;
				for(int j = 0; j < nofSiblings; j++) {
					sharedNormal += normals[vertexConnections[i][j]];
				}
				sharedNormal /= nofSiblings;
				sharedNormal.Normalize();
				for(int j = 0; j < nofSiblings; j++) {
					vertex2NormalMap[vertexConnections[i][j]] = sharedNormal;
				}
			}
*/

		/*	for (int i = 0; i < originalVertices.Length; i++) {
				Vector3 pos = originalVertices[i];
				Vector3 sharedNormal = new Vector3(0,0,0);
				float count = 0;
				for(int j = 0; j < originalVertices.Length; j++) {
					if(pos == originalVertices[j]) {
						sharedNormal += normals[j];
						count++;
					}
				}
				vertex2NormalMap[i] = sharedNormal/count;
				vertex2NormalMap[i].Normalize();
			}*/
		}


	}

	public void replaceMesh(Mesh newMesh) {
		meshFilter.mesh = newMesh;
		distortMesh();
	}
}
