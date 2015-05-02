using UnityEngine;
using System.Collections;

public class meshDistorter : MonoBehaviour {

	// Defines by how much the initial mesh is distorted
	public float intensity;

	// Defines the frequency that is used for the perlin noise during update() function
	public float wobbleFrequency = 0.9f;
	// Defines the amplitude of the wobble effect during the update() function
	public float wobbleIntensity = 0.1f;
	// Let's the wobble animation go faster
	public float timeMultiplier = 1.0f;

	// SPIKE SHIELD
	// wobbleFrequency = 100.0f;  
	// wobbleIntensity 0.4 - 1.0; // Depending on level



	public GameObject parentBlob;

	private MeshFilter meshFilter;

	private Mesh mesh;

	private player parentPlayerScript;

	private enemy parentEnemyScript;

	private bool isPlayer;

	private float meshDiameter;

	private Vector3[] originalVertices;

	private Vector3[] vertex2NormalMap;
	

	// Use this for initialization
	void Start () {
		meshFilter = parentBlob.GetComponent<MeshFilter> ();
		mesh = meshFilter.mesh;
		mesh.MarkDynamic ();

		originalVertices = mesh.vertices;
		vertex2NormalMap = new Vector3[originalVertices.Length];
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
		}

		parentEnemyScript = (enemy)parentBlob.GetComponent (typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent (typeof(player));
		isPlayer = (bool)parentPlayerScript;

		distortMesh ();
	}
	
	// Update is called once per frame
	void Update () {

		Vector3[] vertices = mesh.vertices;

		// TODO Could put this into an IEnumerable and let it run at a lower framerate to safe ressources
		float currentTime = Time.time * timeMultiplier;
		for (int i = 0; i < vertices.Length; i++) {
			float offsetFactor = wobbleIntensity * parentBlob.transform.localScale.x * (Mathf.PerlinNoise(originalVertices[i].x*wobbleFrequency + currentTime, originalVertices[i].y*wobbleFrequency + currentTime)-0.5f);
			vertices[i] = originalVertices[i] + vertex2NormalMap[i]*offsetFactor;
		}
		mesh.vertices = vertices;
		mesh.RecalculateBounds ();
	}

	public void activateShield(EAbilityType abilityType) 
	{

	}


	public void distortMesh()
	{
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++) {
			Vector3 pos = vertices[i];
			// If this vertex has already been modified, then continue
			if(pos != originalVertices[i])
				continue;
			Vector3 rndDirection = Random.insideUnitSphere*intensity;
			for(int j = 0; j < vertices.Length; j++)
			{
				if(vertices[j] == pos)
					vertices[j] += rndDirection;
			}
		}
		mesh.vertices = vertices;
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
		// Make sure the mesh stays more or less the size of a unit sphere
		Bounds bounds = mesh.bounds;
		float diameter = (bounds.max - bounds.min).magnitude;
		float shrinkFactor = 4.0f / diameter;
		for (int i = 0; i < vertices.Length; i++) {
			vertices[i] *= shrinkFactor;
		}
		originalVertices = vertices;

		mesh.vertices = vertices;
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();

		// Calculate for each vertex its shared normal
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
		}
	}

	public void replaceMesh(Mesh newMesh) {
		meshFilter.mesh = newMesh;
		distortMesh();
	}
}
