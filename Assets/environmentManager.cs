using UnityEngine;
using System.Collections;

public class environmentManager : MonoBehaviour {

	// The overall size of the map
	public Vector2 levelSize;

	// The resolution of the environment texture
	public Vector2 textureResolution;

	// reference to the player
	private GameObject player;

	private player playerScript;

	public int waterOctaves;

	public int soilOctaves = 4;

	public float maxTerrainHeight;



	public GameObject[] environmentPlanes = new GameObject[4];

	private Renderer[] environmentPlaneRenderers = new Renderer[4];

	private Mesh environmentMesh;

	private Texture2D environmentTexture;

	private Color[] textureColor;



	private Vector3 meshSize;

	// Defines which of the 4 3D planes is chosen 
	// Index 0 has lowest vertex count (17x17)
	// Index 3 has highest vertex count (132x132)
	private int planeIndex;



	// Use this for initialization
	void Start () {
		player = GameObject.Find ("Blob");
		playerScript = (player)player.GetComponent (typeof(player));
		planeIndex = 1;

		// Initialize the texture
		environmentTexture = new Texture2D ((int)textureResolution.x, (int)textureResolution.y);
		// Initialize the array, that holds the colors for the texture
		textureColor = new Color[(int)textureResolution.x * (int)textureResolution.y];

		// Instantiate all background planes
		for (int i = 0; i < environmentPlaneRenderers.Length; i++) {
			environmentPlanes[i] = (GameObject)GameObject.Instantiate(environmentPlanes[i]);
		}
		// Get the renderer of the environment planes
		for (int i = 0; i < environmentPlaneRenderers.Length; i++) {
			environmentPlaneRenderers[i] = (Renderer)environmentPlanes[i].GetComponent (typeof(Renderer));
		}
		// Bind the environment texture to the environment planes
		for (int i = 0; i < environmentPlaneRenderers.Length; i++) {
			environmentPlaneRenderers[i].material.mainTexture = environmentTexture;
		}
		// Deactivate all planes
		for (int i = 0; i < environmentPlaneRenderers.Length; i++) {
			environmentPlanes[i].SetActive(false);
		}

		// Activate current background plane
		environmentPlanes[planeIndex].SetActive(true);
		// Get the mesh of the currently active background plane
		environmentMesh = ((MeshFilter)environmentPlanes[planeIndex].GetComponent (typeof(MeshFilter))).mesh;
		// Get the extends of the currently active mesh
		meshSize = environmentPlaneRenderers[planeIndex].bounds.max - environmentPlaneRenderers[planeIndex].bounds.min;

		maxTerrainHeight = 40.0f;

	}
	
	// Update is called once per frame
	void Update () {

		// Scale the mesh according to the player's viewing range
		environmentPlanes [planeIndex].transform.localScale = new Vector3((playerScript.size + playerScript.viewingRange)*2.0f, 1, (playerScript.size + playerScript.viewingRange)*1.1f);

		// Get the current size of the mesh
		meshSize = environmentPlaneRenderers[planeIndex].bounds.max - environmentPlaneRenderers[planeIndex].bounds.min;

		// Move the environment plane along with the player
		environmentPlanes[planeIndex].transform.position = player.transform.position;

		// TODO update perlin noise parameters according to time

		// Calculate current environment texture
		calculateEnvironmentTexture ();

		calculateTerrainMap ();

		environmentTexture.SetPixels (textureColor);
		environmentTexture.Apply();
	}

	// Calculates a texture that is put over the background plane
	private void calculateEnvironmentTexture()
	{
		float xOffset = player.transform.position.x - meshSize.x * 0.5f;
		float yOffset = player.transform.position.y - meshSize.y * 0.5f;
		Vector2 distancePerPixel = new Vector2 (meshSize.x / textureResolution.x, meshSize.y / textureResolution.y);

		for (int x = 0; x < textureResolution.x; x++) 
		{
			for(int y = 0; y < textureResolution.y; y++) 
			{

				float terrainSample = addUpOctaves (3, 0.1f, 0.1f, x*distancePerPixel.x + xOffset, y*distancePerPixel.y + yOffset);

				textureColor[y*(int)textureResolution.x + x] = new Color(terrainSample,terrainSample,terrainSample);
			}
		}
	}

	// Calculates a height map for the terrain (y offset for the vertices)
	private void calculateTerrainMap()
	{
		// Get the vertices of the plane (in object space)
		Vector3[] vertices = environmentMesh.vertices;
		for (int i = 0; i < vertices.Length; i++) 
		{   
			// Transform the vertices of the plane into world space
			Vector3 vertexPoint = environmentPlanes[planeIndex].transform.TransformPoint(vertices[i]);
			float xCoord = vertexPoint.x;
			float yCoord = vertexPoint.y;
			// Generate smooth hills (z-offset)
			vertexPoint.z = addUpOctaves (4, 0.01f, 0.1f, xCoord, yCoord) * maxTerrainHeight;
			// Transform the manipulated vertex back into object space and store it
			vertices[i] = environmentPlanes[planeIndex].transform.InverseTransformPoint(vertexPoint);
		}
		// Set manipulated vertices
		environmentMesh.vertices = vertices;
		// Recalculate the boundaries
		environmentMesh.RecalculateBounds();
		// Recalculate surface normals
		environmentMesh.RecalculateNormals();
	}

	// A high persistence leads to only a small variation in amplitude, whereas a low persistence leads to high variation in amplitude
	// A low frequency leads to only few value changes over a large area, whereas a high frequency leads to lots of details over a small area
	private float addUpOctaves(int octaves, float frequency, float persistence, float x, float y)
	{
		float sampleValue = 0.0f;
		float amplitude = 1;
		float freq = frequency;
		
		for(int i = 0; i < octaves; i++) 
		{
			sampleValue += Mathf.PerlinNoise(x * freq, y * freq) * amplitude;
			amplitude *= persistence;
			freq *= 2;
		}

		return sampleValue;
	}
}
