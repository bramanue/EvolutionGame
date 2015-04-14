using UnityEngine;
using System.Collections;

public class environmentManager : MonoBehaviour {

	// The overall size of the map
	public Vector2 levelSize;

	// The resolution of the environment texture
	public Vector2 mainTextureResolution;

	// The resolution of the environment bump map texture
	public Vector2 bumpMapTextureResolution;

	// The resolution of the environment reflection cube map
	public Vector2 cubeMapTextureResolution;

	// Stores reference to the player game object	
	private GameObject player;

	// Stores reference to the player script
	private player playerScript;


	// PERLIN NOISE VARIABLES FOR THE WATER BUMP MAP

	// The higher, the more details, but slower performance
	public int waterBumpMapOctaves = 3;
	// The higher, the flater the water surface, the smaller the bigger waves
	public float waterBumpMapPersistence = 0.2f;
	// The higher, the shorter the distance between the waves waves
	public float waterBumpMapFrequency = 3.0f;
	// Defines at which height the water surface is
	public float liquidLevel = 0.2f;

	// PERLIN NOISE VARIABLES FOR THE SOIL BUMP MAP


	// PERLIN NOISE VARIABLES FOR THE 3D TERRAIN FORMING

	// The higher, the more details but slower performance (low value recommended, since mesh cannot represent higher detail anyway)
	public int terrainFormingOctaves = 2;
	// The higher, the flatter, the terrain will be. The lower the more steep hills/cliffs there will be
	public float terrainFormingPersistence = 0.1f;
	// The higher, the shorter the distance between hills/cliffs
	public float terrainFormingFrequency = 0.03f;
	// Maximum height of the terrain
	public float maxTerrainHeight = 10.0f;


	// Defines how many threads are involved in calculating the perlin noise textures
	public int nofThreads = 3;


	// Datastructures
	public GameObject[] environmentPlanes = new GameObject[4];

	private Renderer[] environmentPlaneRenderers = new Renderer[4];

	private Mesh currentEnvironmentMesh;

	private Texture2D environmentTexture;

	private Texture2D environmentBumpMap;

	private Texture2D environmentCubeMap;

	private static Color[] mainTextureColor;

	private Color[] bumpMapColor;

	private Color[] cubeMapColor;



	// Defines which of the 4 3D planes is chosen 
	// Index 0 has lowest quads count (16x16)
	// Index 3 has highest quads count (132x132)
	public int planeIndex = 2;

	private Vector3 meshSize;


	// Use this for initialization
	void Start () {
		player = GameObject.Find ("Blob");
		playerScript = (player)player.GetComponent (typeof(player));

		// Initialize the main texture
		environmentTexture = new Texture2D ((int)mainTextureResolution.x, (int)mainTextureResolution.y);
		// Initialize the array, that holds the colors for the texture
		mainTextureColor = new Color[(int)mainTextureResolution.x * (int)mainTextureResolution.y];

		// Initialize the bump map texture
		environmentBumpMap = new Texture2D ((int)bumpMapTextureResolution.x, (int)bumpMapTextureResolution.y);
		// Initialize the array, that holds the colors for the bump map
		bumpMapColor = new Color[(int)bumpMapTextureResolution.x * (int)bumpMapTextureResolution.y];

		// Initialize the cube map texture
		environmentCubeMap = new Texture2D ((int)cubeMapTextureResolution.x, (int)cubeMapTextureResolution.y);
		// Initialize the array, that holds the colors for the cube map
		cubeMapColor = new Color[(int)cubeMapTextureResolution.x * (int)cubeMapTextureResolution.y];

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
			environmentPlaneRenderers[i].material.SetTexture("_MainTex", environmentTexture);
			environmentPlaneRenderers[i].material.SetTextureScale("_MainTex", new Vector2(-1,-1));	// Invert texture due to uv mapping of the plane
			environmentPlaneRenderers[i].material.SetTexture("_BumpMap", environmentBumpMap);
			environmentPlaneRenderers[i].material.SetTextureScale("_MainTex", new Vector2(-1,-1));	// Invert texture due to uv mapping of the plane
		}
		// Deactivate all planes
		for (int i = 0; i < environmentPlaneRenderers.Length; i++) {
			environmentPlanes[i].SetActive(false);
		}

		// Activate current background plane
		environmentPlanes[planeIndex].SetActive(true);
		// Get the mesh of the currently active background plane
		currentEnvironmentMesh = ((MeshFilter)environmentPlanes[planeIndex].GetComponent (typeof(MeshFilter))).mesh;
		// Mark the mesh as dynamic, since it will be changed in every frame
		currentEnvironmentMesh.MarkDynamic ();
		// Get the extends of the currently active mesh
		meshSize = environmentPlaneRenderers[planeIndex].bounds.max - environmentPlaneRenderers[planeIndex].bounds.min;

	}
	
	// Update is called once per frame
	void Update () {

		float t0 = System.DateTime.Now.Millisecond;

		// Scale the mesh according to the player's viewing range
		environmentPlanes [planeIndex].transform.localScale = new Vector3((playerScript.transform.localScale.x + playerScript.viewingRange)*2.5f, 1, (playerScript.size + playerScript.viewingRange)*1.1f);

		// Get the current size of the mesh (we need to take it from the renderer in order to get world coordinates - the mesh has bounds in untransformed coordinates)
		meshSize = environmentPlaneRenderers[planeIndex].bounds.max - environmentPlaneRenderers[planeIndex].bounds.min;


		// TODO update perlin noise parameters according to time

		// Calculate current environment texture
		calculateEnvironmentTexture ();

		// Calculate 3D terrain
		calculateTerrainMap ();

	//	Debug.Log ("Total time for environment update on CPU : " + (System.DateTime.Now.Millisecond - t0) + "ms");

	}

	void LateUpdate() {
		// Move the environment plane along with the player
		environmentPlanes[planeIndex].transform.position = player.transform.position;
	}

	// Calculates a texture for each environment component that acts as mask for high resolution textures
	private void calculateEnvironmentTexture()
	{
		float t0 = System.DateTime.Now.Millisecond;

		// Calculate the offsets induced by the current player position for the perlin noise calculation
		float xOffset = player.transform.position.x;
		float yOffset = player.transform.position.y;
		// Calculate for each texture what distance each pixel covers in world coordinates
		Vector2 distancePerPixel = new Vector2 (meshSize.x / mainTextureResolution.x, meshSize.y / mainTextureResolution.y);
		// Get the current time for time dependent perlin noise
		float currentTime = Time.time;

		// Generate thread array to store the working threads
		System.Threading.Thread[] threads = new System.Threading.Thread[nofThreads];
		// Calculate on how many rows of the texture each thread has to work on.
		int rowsPerThread = (int)Mathf.Floor(mainTextureResolution.y / nofThreads);
		// Row at which the next thread has to start
	//	int startRow = 0;


		// Start working threads
		for (int threadIndex = 0; threadIndex < nofThreads-1; threadIndex++)
		{
			int startRow = threadIndex*rowsPerThread;
		//	Debug.Log (startRow);
			// Give the thread the function it needs to work on
			threads[threadIndex] = new System.Threading.Thread(() => parallelCalculation(startRow, startRow + rowsPerThread, xOffset, yOffset, distancePerPixel, currentTime));
			// Start the working thread
			threads[threadIndex].Start();
			// Increase the start index for the next thread
			// startRow += rowsPerThread;

		}

		// Main thread calculates rest of the texture
		parallelCalculation((nofThreads-1)*rowsPerThread, (int)mainTextureResolution.y, xOffset, yOffset, distancePerPixel, currentTime);

		// Main thread waits for the other threads to finish
		for (int threadIndex = 0; threadIndex < nofThreads-1; threadIndex++)
			threads [threadIndex].Join();

		// Apply the texture to the mesh renderer
		environmentTexture.SetPixels (mainTextureColor);
		environmentTexture.Apply();

	//	Debug.Log ("Total time for tex calculation " + (System.DateTime.Now.Millisecond - t0) + "ms");
		return;

	}

	private void parallelCalculation(int startRow, int endRow, float xOffset, float yOffset, Vector2 distancePerPixel, float currentTime)
	{
	//	Debug.Log (System.Threading.Thread.CurrentThread.ManagedThreadId + " From " + startRow + " to " + endRow);
		for (int y = startRow; y < endRow; y++) 
		{
			for(int x = 0; x < mainTextureResolution.x; x++) 
			{
				float terrainSample = addUpOctaves (3, 0.1f, 0.1f, x*distancePerPixel.x + xOffset, y*distancePerPixel.y + yOffset);
				// Make it a water surface
				if(terrainSample < 0.8)
				{
					mainTextureColor[y*(int)mainTextureResolution.x + x] = new Color(0,0,1)*(1.0f - terrainSample);
					float waveHeight = generateWaterSurface(x*distancePerPixel.x + xOffset, y*distancePerPixel.y + yOffset, currentTime);
					bumpMapColor[y*(int)bumpMapTextureResolution.x + x] = new Color(0,0,waveHeight);
				}
				
				mainTextureColor[y*(int)mainTextureResolution.x + x] = new Color(terrainSample,terrainSample,terrainSample);
			}
		}
	}

	// Calculates a height map for the terrain (y offset for the vertices)
	private void calculateTerrainMap()
	{
		// Get the vertices of the plane (in object space)
		Vector3[] vertices = currentEnvironmentMesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{   
			// Transform the vertices of the plane into world space
			Vector3 vertexPoint = environmentPlanes[planeIndex].transform.TransformPoint(vertices[i]);
			float xCoord = vertexPoint.x;
			float yCoord = vertexPoint.y;
			// Generate smooth hills (z-offset)
			vertexPoint.z = addUpOctaves (terrainFormingOctaves, terrainFormingFrequency, terrainFormingPersistence, xCoord, yCoord) * maxTerrainHeight;
			// Transform the manipulated vertex back into object space and store it
			vertices[i] = environmentPlanes[planeIndex].transform.InverseTransformPoint(vertexPoint);
		}
		// Set manipulated vertices
		currentEnvironmentMesh.vertices = vertices;
		// Recalculate the boundaries
		currentEnvironmentMesh.RecalculateBounds();
		// Recalculate surface normals
		currentEnvironmentMesh.RecalculateNormals();
	}

	private float generateWaterSurface(float x, float y, float currentTime)
	{
		return addUpOctaves(waterBumpMapOctaves, waterBumpMapFrequency, waterBumpMapPersistence, x + currentTime, y + currentTime);
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

	// Cross product for the case, when a has x-value of 0 and b has y-value of 0
	// Used for normal calculation
	private Vector3 simplyfiedCrossProduct(Vector3 a, Vector3 b) 
	{
		return new Vector3 (a.y*b.z, a.z*b.x, -b.x*a.y);
	}
}
