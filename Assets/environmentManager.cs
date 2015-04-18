using UnityEngine;
using System.Collections;

public class environmentManager : MonoBehaviour {

	// The overall size of the map
	public Vector2 levelSize;

	// The resolution of the environment texture
	public Vector2 mainTextureResolution;

	// The resolution of the environment bump map texture
	public Vector2 bumpMapTextureResolution;

	// The resolution for the single environmental hazard blocks
	public Vector2 environmentalHazardResolution;

	// The resolution of the environment reflection cube map
	public Vector2 cubeMapTextureResolution;

	// Stores reference to the player game object	
	private GameObject player;

	// Stores reference to the player script
	private player playerScript;

	public float randomInitialValue;

	private GameObject[] environmentalHazards;

	private float[] environmentalHazardMask;


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
		// Get a random initial start point on the map
		randomInitialValue = Random.Range ((float)int.MinValue/10000.0f,(float)int.MaxValue/10000.0f);

		environmentalHazards = new GameObject[(int)(environmentalHazardResolution.x*environmentalHazardResolution.y*0.5f)];
		for (int i = 0; i < environmentalHazards.Length; i++) {
			environmentalHazards[i] = new GameObject();
			environmentalHazards[i].AddComponent<MeshFilter>();
			environmentalHazards[i].GetComponent<MeshFilter>().mesh.MarkDynamic();
			environmentalHazards[i].AddComponent<MeshRenderer>();
			environmentalHazards[i].AddComponent<MeshCollider>();
			environmentalHazards[i].GetComponent<MeshCollider>().convex = true;
			environmentalHazards[i].GetComponent<MeshCollider>().isTrigger = true;
			environmentalHazards[i].AddComponent(typeof(thornBush));
			environmentalHazards[i].SetActive(false);
		}

		environmentalHazardMask = new float[(int)(mainTextureResolution.x * mainTextureResolution.y)];
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
		float xOffset = player.transform.position.x + randomInitialValue;
		float yOffset = player.transform.position.y + randomInitialValue;
		// Calculate for each texture what distance each pixel covers in world coordinates
		Vector2 distancePerPixel = new Vector2 (meshSize.x / mainTextureResolution.x, meshSize.y / mainTextureResolution.y);
		// Get the current time for time dependent perlin noise
		float currentTime = Time.time;

		// Generate thread array to store the working threads
		System.Threading.Thread[] threads = new System.Threading.Thread[nofThreads];
		// Calculate on how many rows of the texture each thread has to work on.
		int rowsPerThread = (int)Mathf.Floor(mainTextureResolution.y / nofThreads);


		// Start working threads
		for (int threadIndex = 0; threadIndex < nofThreads-1; threadIndex++)
		{
			int startRow = threadIndex*rowsPerThread;
			// Give the thread the function it needs to work on
			threads[threadIndex] = new System.Threading.Thread(() => parallelCalculation(startRow, startRow + rowsPerThread, xOffset, yOffset, distancePerPixel, currentTime));
			// Start the working thread
			threads[threadIndex].Start();
		}

		// Main thread calculates rest of the texture
		parallelCalculation((nofThreads-1)*rowsPerThread, (int)mainTextureResolution.y, xOffset, yOffset, distancePerPixel, currentTime);

		// Main thread waits for the other threads to finish
		for (int threadIndex = 0; threadIndex < nofThreads-1; threadIndex++)
			threads [threadIndex].Join();

	//	generateEnvironmentalHazards(0.8f,environmentalHazardMask);

		// Apply the texture to the mesh renderer
		environmentTexture.SetPixels (mainTextureColor);
		environmentTexture.Apply();

		Debug.Log ("Total time for tex calculation " + (System.DateTime.Now.Millisecond - t0) + "ms");
		return;

	}

	private void parallelCalculation(int startRow, int endRow, float xOffset, float yOffset, Vector2 distancePerPixel, float currentTime)
	{
		for (int y = startRow; y < endRow; y++) 
		{
			for(int x = 0; x < mainTextureResolution.x; x++) 
			{
				float terrainSample = addUpOctaves (3, 0.1f, 0.1f, x*distancePerPixel.x + xOffset, y*distancePerPixel.y + yOffset);
				// Make it a water surface
			/*	if(terrainSample < 0.8)
				{
					mainTextureColor[y*(int)mainTextureResolution.x + x] = new Color(0,0,1)*(1.0f - terrainSample);
					float waveHeight = generateWaterSurface(x*distancePerPixel.x + xOffset, y*distancePerPixel.y + yOffset, currentTime);
					bumpMapColor[y*(int)bumpMapTextureResolution.x + x] = new Color(0,0,waveHeight);
				}  */
				
				mainTextureColor[y*(int)mainTextureResolution.x + x] = new Color(terrainSample,terrainSample,terrainSample);
				environmentalHazardMask[y*(int)mainTextureResolution.x + x] = terrainSample;
			}
		}
	}
	
	private void generateEnvironmentalHazards (float threshold1, float[] mask1)
	{
		Vector2[] pointGrid = new Vector2[(int)(environmentalHazardResolution.x * environmentalHazardResolution.y)];
		Vector3 lowerLeftPoint = new Vector3 (player.transform.position.x - 0.5f * meshSize.x, player.transform.position.y - 0.5f * meshSize.y, 0);
		Vector3 stepSize = new Vector3 (meshSize.x / (environmentalHazardResolution.x - 1), meshSize.y / (environmentalHazardResolution.y - 1), 0);
		// Make sure the sampling grid stays the same for the same background plane size
		float offsetX = lowerLeftPoint.x % stepSize.x;
		float offsetY = lowerLeftPoint.y % stepSize.y;
		lowerLeftPoint += new Vector3 (offsetX, offsetY, 0);	
		offsetX /= stepSize.x;
		offsetY /= stepSize.y;
		offsetX = 0;
		offsetY = 0;
		// We assume that all textures are squares
		int maskRes = (int)Mathf.Sqrt (mask1.Length);
		float stepSizeOnMask = maskRes / environmentalHazardResolution.x;

		for (int y = 0; y < environmentalHazardResolution.y; y++) {
			for (int x = 0; x < environmentalHazardResolution.x; x++) {
				// Get the sample point in space of the mask
				int samplePoint = (int)(((y+offsetY) * environmentalHazardResolution.x + (x+offsetX)) * stepSizeOnMask);
				if (mask1 [samplePoint] > threshold1) {
					pointGrid [(int)(y * environmentalHazardResolution.x + x)] = new Vector2 (1, mask1 [samplePoint]);
				} else {
					pointGrid [(int)(y * environmentalHazardResolution.x + x)] = new Vector2 (0, mask1 [samplePoint]);
				}
			}
		}



		Debug.Log ("lowerLeftPoint = " + lowerLeftPoint);
		Debug.Log ("stepSizeOnMesh = " + stepSize);
		Vector3[] vertices = new Vector3[(int)(2 * environmentalHazardResolution.x)];
		int[] faces = new int[(int)(2 * 3 * environmentalHazardResolution.x)];
		int vertexCount = 0;
		int faceCount = 0;

		int gameObjectIndex = 0;

		for (int y = 0; y < environmentalHazardResolution.y - 1; y++) {
			bool connected = true;
			for (int x = 0; x < environmentalHazardResolution.x - 1; x++) {
				int index = (int)(y * environmentalHazardResolution.x + x);
				if (pointGrid [index].x == 1) {
					if (true/*y == 0*/) {  // Don't look down
						bool right = (pointGrid [index + 1].x == 1) ? true : false;
						float rightValue = pointGrid [index + 1].y;

						bool top = (pointGrid [(int)(index + environmentalHazardResolution.x)].x == 1) ? true : false;
						float topValue = pointGrid [(int)(index + environmentalHazardResolution.x)].y;

						bool topRight = (pointGrid [(int)(index + environmentalHazardResolution.x + 1)].x == 1) ? true : false;
						float topRightValue = pointGrid [(int)(index + environmentalHazardResolution.x + 1)].y;

						// Check whether we have a trivial solution
						if (top && right && topRight) {
							// Quad ::
							if (vertexCount == 0) {
								vertices [vertexCount] = lowerLeftPoint + new Vector3 (x * stepSize.x, y * stepSize.y, 0);	// point
								vertices [vertexCount + 1] = lowerLeftPoint + new Vector3 (x * stepSize.x, (y + 1) * stepSize.y, 0);	// top
								vertexCount += 2;
							}
							vertices [vertexCount] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, y * stepSize.y, 0);	// right
							vertices [vertexCount + 1] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, (y + 1) * stepSize.y, 0);	// top right
							vertexCount += 2;
							// Top left triangle :'
							faces [(faceCount * 3)] = vertexCount - 4; // point
							faces [(faceCount * 3) + 1] = vertexCount - 3; // top
							faces [(faceCount * 3) + 2] = vertexCount - 1; // top right
							faceCount++;
							// Bottom right triangle .:
							faces [(faceCount * 3)] = vertexCount - 4; // point
							faces [(faceCount * 3) + 1] = vertexCount - 1; // top right
							faces [(faceCount * 3) + 2] = vertexCount - 2; // right
							faceCount++;

						} else if (top && topRight) {
							// Triangle :'
							// End of convex shape
							if (vertexCount == 0) {
								vertices [vertexCount] = lowerLeftPoint + new Vector3 (x * stepSize.x, y * stepSize.y, 0);	// point
								vertices [vertexCount + 1] = lowerLeftPoint + new Vector3 (x * stepSize.x, (y + 1) * stepSize.y, 0);	// top
								vertexCount += 2;
							}
							vertices [vertexCount] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, (y + 1) * stepSize.y, 0);	// top right
							vertexCount++;
							// Top left triangle :'
							faces [(faceCount * 3)] = vertexCount - 3; // point
							faces [(faceCount * 3) + 1] = vertexCount - 2; // top
							faces [(faceCount * 3) + 2] = vertexCount - 1; // top right
							faceCount++;
							// End of convex shape
							Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
							mesh.Clear();
							Vector3[] newVertices = new Vector3[vertexCount];
							for(int i = 0; i < vertexCount; i++){
								newVertices[i] = vertices[i];
							}
							int[] newFaces = new int[faceCount*3];
							for(int i = 0; i < faceCount*3; i++){
								newFaces[i] = faces[i];
							}
							mesh.vertices = newVertices;
							mesh.triangles = newFaces;
							mesh.RecalculateBounds();
							mesh.RecalculateNormals();
							MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
							// meshCollider.sharedMesh = null;
							meshCollider.sharedMesh = mesh;
							Debug.Log (meshCollider.contactOffset);
							environmentalHazards [gameObjectIndex].SetActive (true);
							gameObjectIndex++;
							// Reset datastructures
							vertexCount = 0;
							faceCount = 0;
						} else if (top && right) {
							// Triangle :.
							// End of convex shape
							if (vertexCount == 0) {
								vertices [vertexCount] = lowerLeftPoint + new Vector3 (x * stepSize.x, y * stepSize.y, 0);	// point
								vertices [vertexCount + 1] = lowerLeftPoint + new Vector3 (x * stepSize.x, (y + 1) * stepSize.y, 0);	// top
								vertexCount += 2;
							}
							vertices [vertexCount] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, y*stepSize.y, 0);	// right
							vertexCount++;
							// Bottom left triangle
							faces [(faceCount * 3)] = vertexCount - 3; // point
							faces [(faceCount * 3) + 1] = vertexCount - 2; // top
							faces [(faceCount * 3) + 2] = vertexCount - 1; // right
							faceCount++;
							// End of convex shape
							Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
							mesh.Clear();
							Vector3[] newVertices = new Vector3[vertexCount];
							for(int i = 0; i < vertexCount; i++){
								newVertices[i] = vertices[i];
							}
							int[] newFaces = new int[faceCount*3];
							for(int i = 0; i < faceCount*3; i++){
								newFaces[i] = faces[i];
							}
							mesh.vertices = newVertices;
							mesh.triangles = newFaces;
							MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
							// meshCollider.sharedMesh = null;
							meshCollider.sharedMesh = mesh;
							meshCollider.sharedMesh.RecalculateBounds();
							meshCollider.sharedMesh.RecalculateNormals();
							environmentalHazards [gameObjectIndex].SetActive (true);
							gameObjectIndex++;
							// Reset datastructures
							vertexCount = 0;
							faceCount = 0;
						} else if (topRight && right) {
							// Triangle .:
							// Begin of convex shape
							if (vertexCount != 0) {
								Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
								mesh.Clear();
								Vector3[] newVertices = new Vector3[vertexCount];
								for(int i = 0; i < vertexCount; i++){
									newVertices[i] = vertices[i];
								}
								int[] newFaces = new int[faceCount*3];
								for(int i = 0; i < faceCount*3; i++){
									newFaces[i] = faces[i];
								}
								mesh.vertices = newVertices;
								mesh.triangles = newFaces;
								MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
								// meshCollider.sharedMesh = null;
								meshCollider.sharedMesh = mesh;
								meshCollider.sharedMesh.RecalculateBounds();
								meshCollider.sharedMesh.RecalculateNormals();
								environmentalHazards [gameObjectIndex].SetActive (true);
								gameObjectIndex++;
								// Reset datastructures
								vertexCount = 0;
								faceCount = 0;
							}
							if (vertexCount == 0) {
								vertices [vertexCount] = lowerLeftPoint + new Vector3 (x * stepSize.x, y * stepSize.y, 0);	// point
								vertexCount++;
							}
							vertices [vertexCount] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, y * stepSize.y, 0);	// right
							vertices [vertexCount + 1] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, (y + 1) * stepSize.y, 0);	// top right
							vertexCount += 2;
							// Bottom right triangle
							faces [(faceCount * 3)] = vertexCount - 3; // point
							faces [(faceCount * 3) + 1] = vertexCount - 1; // top right
							faces [(faceCount * 3) + 2] = vertexCount - 2; // right
							faceCount++;
						} else if (top) {
							if (topRightValue > rightValue) {
								// If top right point is closer to the threshold value than the top point, then include top right
								if (threshold1 - topRightValue <= topValue - threshold1) {
									pointGrid [(int)(index + environmentalHazardResolution.x + 1)].x = 1;
									// Triangle ;'
									// End of convex shape
									// copied
									if (vertexCount == 0) {
										vertices [vertexCount] = lowerLeftPoint + new Vector3 (x * stepSize.x, y * stepSize.y, 0);	// point
										vertices [vertexCount + 1] = lowerLeftPoint + new Vector3 (x * stepSize.x, (y + 1) * stepSize.y, 0);	// top
										vertexCount += 2;
									}
									vertices [vertexCount] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, (y + 1) * stepSize.y, 0);	// top right
									vertexCount++;
									// Top left triangle
									faces [(faceCount * 3)] = vertexCount - 3; // point
									faces [(faceCount * 3) + 1] = vertexCount - 2; // top
									faces [(faceCount * 3) + 2] = vertexCount - 1; // top right
 									faceCount++;
									// End of convex shape
									Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
									mesh.Clear();
									Vector3[] newVertices = new Vector3[vertexCount];
									for(int i = 0; i < vertexCount; i++){
										newVertices[i] = vertices[i];
									}
									int[] newFaces = new int[faceCount*3];
									for(int i = 0; i < faceCount*3; i++){
										newFaces[i] = faces[i];
									}
									mesh.vertices = newVertices;
									mesh.triangles = newFaces;
									MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
									// meshCollider.sharedMesh = null;
									meshCollider.sharedMesh = mesh;
									meshCollider.sharedMesh.RecalculateBounds();
									meshCollider.sharedMesh.RecalculateNormals();
									environmentalHazards [gameObjectIndex].SetActive (true);
									gameObjectIndex++;
									// Reset datastructures
									vertexCount = 0;
									faceCount = 0;
								} else {
									// pointGrid[(int)(index + environmentalHazardResolution.x + 1)].x = 0;
									// No Triangle
									if (vertexCount != 0) {
										Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
										mesh.Clear();
										Vector3[] newVertices = new Vector3[vertexCount];
										for(int i = 0; i < vertexCount; i++){
											newVertices[i] = vertices[i];
										}
										int[] newFaces = new int[faceCount*3];
										for(int i = 0; i < faceCount*3; i++){
											newFaces[i] = faces[i];
										}
										mesh.vertices = newVertices;
										mesh.triangles = newFaces;
										MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
										// meshCollider.sharedMesh = null;
										meshCollider.sharedMesh = mesh;
										meshCollider.sharedMesh.RecalculateBounds();
										meshCollider.sharedMesh.RecalculateNormals();
										environmentalHazards [gameObjectIndex].SetActive (true);
										gameObjectIndex++;
										// Reset datastructures
										vertexCount = 0;
										faceCount = 0;
									}
								}
							} else {
								// If top right point is closer to the threshold value than the top point, then include top right
								if (threshold1 - rightValue <= topValue - threshold1) {
									pointGrid [(int)(index + 1)].x = 1;
									// Triangle :.
									// End of convex shape 
									// copied
									if (vertexCount == 0) {
										vertices [vertexCount] = lowerLeftPoint + new Vector3 (x * stepSize.x, y * stepSize.y, 0);	// point
										vertices [vertexCount + 1] = lowerLeftPoint + new Vector3 (x * stepSize.x, (y + 1) * stepSize.y, 0);	// top
										vertexCount += 2;
									}
									vertices [vertexCount] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, y*stepSize.y, 0);	// right
									vertexCount++;
									// Bottom left triangle
									faces [(faceCount * 3)] = vertexCount - 3; // point
									faces [(faceCount * 3) + 1] = vertexCount - 2; // top
									faces [(faceCount * 3) + 2] = vertexCount - 1; // right
									faceCount++;
									// End of convex shape
									Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
									mesh.Clear();
									Vector3[] newVertices = new Vector3[vertexCount];
									for(int i = 0; i < vertexCount; i++){
										newVertices[i] = vertices[i];
									}
									int[] newFaces = new int[faceCount*3];
									for(int i = 0; i < faceCount*3; i++){
										newFaces[i] = faces[i];
									}
									mesh.vertices = newVertices;
									mesh.triangles = newFaces;
									MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
									// meshCollider.sharedMesh = null;
									meshCollider.sharedMesh = mesh;
									meshCollider.sharedMesh.RecalculateBounds();
									meshCollider.sharedMesh.RecalculateNormals();
									environmentalHazards [gameObjectIndex].SetActive (true);
									gameObjectIndex++;
									// Reset datastructures
									vertexCount = 0;
									faceCount = 0;
								} else {
									// pointGrid[(int)(index + environmentalHazardResolution.x + 1)].x = 0;
									// No Triangle
									if (vertexCount != 0) {
										Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
										mesh.Clear();
										Vector3[] newVertices = new Vector3[vertexCount];
										for(int i = 0; i < vertexCount; i++){
											newVertices[i] = vertices[i];
										}
										int[] newFaces = new int[faceCount*3];
										for(int i = 0; i < faceCount*3; i++){
											newFaces[i] = faces[i];
										}
										mesh.vertices = newVertices;
										mesh.triangles = newFaces;
										MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
										// meshCollider.sharedMesh = null;
										meshCollider.sharedMesh = mesh;
										meshCollider.sharedMesh.RecalculateBounds();
										meshCollider.sharedMesh.RecalculateNormals();
										environmentalHazards [gameObjectIndex].SetActive (true);
										gameObjectIndex++;
										// Reset datastructures
										vertexCount = 0;
										faceCount = 0;
									}
								}
							}

						} else if (right) {
							if (topRightValue > topValue) {
								// If top right point is closer to the threshold value than the top point, then include top right
								if (threshold1 - topRightValue <= rightValue - threshold1) {
									pointGrid [(int)(index + environmentalHazardResolution.x + 1)].x = 1;
									// Triangle .:
									// Begin of convex shape
									if (vertexCount != 0) {
										Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
										mesh.Clear();
										Vector3[] newVertices = new Vector3[vertexCount];
										for(int i = 0; i < vertexCount; i++){
											newVertices[i] = vertices[i];
										}
										int[] newFaces = new int[faceCount*3];
										for(int i = 0; i < faceCount*3; i++){
											newFaces[i] = faces[i];
										}
										mesh.vertices = newVertices;
										mesh.triangles = newFaces;
										MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
										// meshCollider.sharedMesh = null;
										meshCollider.sharedMesh = mesh;
										meshCollider.sharedMesh.RecalculateBounds();
										meshCollider.sharedMesh.RecalculateNormals();
										environmentalHazards [gameObjectIndex].SetActive (true);
										gameObjectIndex++;
										// Reset datastructures
										vertexCount = 0;
										faceCount = 0;
									}
									// copied
									if (vertexCount == 0) {
										vertices [vertexCount] = lowerLeftPoint + new Vector3 (x * stepSize.x, y * stepSize.y, 0);	// point
										vertexCount++;
									}
									vertices [vertexCount] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, y * stepSize.y, 0);	// right
									vertices [vertexCount + 1] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, (y + 1) * stepSize.y, 0);	// top right
									vertexCount += 2;
									// Bottom right triangle
									faces [(faceCount * 3)] = vertexCount - 3; // point
									faces [(faceCount * 3) + 1] = vertexCount - 2; // right
									faces [(faceCount * 3) + 2] = vertexCount - 1; // top right
									faceCount++;
								} else {
									// pointGrid[(int)(index + 1)].x = 0;
									// No Triangle
									if (vertexCount != 0) {
										Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
										mesh.Clear();
										Vector3[] newVertices = new Vector3[vertexCount];
										for(int i = 0; i < vertexCount; i++){
											newVertices[i] = vertices[i];
										}
										Debug.Log ("nofVertices = " + vertexCount);
										int[] newFaces = new int[faceCount*3];
										for(int i = 0; i < faceCount*3; i++){
											newFaces[i] = faces[i];
											Debug.Log ("referenced vertex = " + faces[i]);
										}
										mesh.vertices = newVertices;
										mesh.triangles = newFaces;
										MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
										// meshCollider.sharedMesh = null;
										meshCollider.sharedMesh = mesh;
										meshCollider.sharedMesh.RecalculateBounds();
										meshCollider.sharedMesh.RecalculateNormals();
										environmentalHazards [gameObjectIndex].SetActive (true);
										gameObjectIndex++;
										// Reset datastructures
										vertexCount = 0;
										faceCount = 0;
									}
								}
							} else {
								// If top right point is closer to the threshold value than the top point, then include top right
								if (threshold1 - topValue <= rightValue - threshold1) {
									pointGrid [(int)(index + environmentalHazardResolution.x)].x = 1;
									// Triangle :.
									// End of convex shape 
									// copied
									if (vertexCount == 0) {
										vertices [vertexCount] = lowerLeftPoint + new Vector3 (x * stepSize.x, y * stepSize.y, 0);	// point
										vertices [vertexCount + 1] = lowerLeftPoint + new Vector3 (x * stepSize.x, (y + 1) * stepSize.y, 0);	// top
										vertexCount += 2;
									}
									vertices [vertexCount] = lowerLeftPoint + new Vector3 ((x + 1) * stepSize.x, y*stepSize.y, 0);	// right
									vertexCount++;
									// Bottom left triangle
									faces [(faceCount * 3)] = vertexCount - 3; // point
									faces [(faceCount * 3) + 1] = vertexCount - 2; // right
									faces [(faceCount * 3) + 2] = vertexCount - 1; // top
									faceCount++;
									// End of convex shape
									Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
									mesh.Clear();
									Vector3[] newVertices = new Vector3[vertexCount];
									for(int i = 0; i < vertexCount; i++){
										newVertices[i] = vertices[i];
									}
									int[] newFaces = new int[faceCount*3];
									for(int i = 0; i < faceCount*3; i++){
										newFaces[i] = faces[i];
									}
									mesh.vertices = newVertices;
									mesh.triangles = newFaces;
									MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
									// meshCollider.sharedMesh = null;
									meshCollider.sharedMesh = mesh;
									meshCollider.sharedMesh.RecalculateBounds();
									meshCollider.sharedMesh.RecalculateNormals();
									environmentalHazards [gameObjectIndex].SetActive (true);
									gameObjectIndex++;
									// Reset datastructures
									vertexCount = 0;
									faceCount = 0;
								} else {
									// pointGrid[(int)(index + environmentalHazardResolution.x + 1)].x = 0;
									// No Triangle
									if (vertexCount != 0) {
										Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
										mesh.Clear();
										Vector3[] newVertices = new Vector3[vertexCount];
										for(int i = 0; i < vertexCount; i++){
											newVertices[i] = vertices[i];
										}
										int[] newFaces = new int[faceCount*3];
										for(int i = 0; i < faceCount*3; i++){
											newFaces[i] = faces[i];
										}
										mesh.vertices = newVertices;
										mesh.triangles = newFaces;
										MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
										// meshCollider.sharedMesh = null;
										meshCollider.sharedMesh = mesh;
										meshCollider.sharedMesh.RecalculateBounds();
										meshCollider.sharedMesh.RecalculateNormals();
										environmentalHazards [gameObjectIndex].SetActive (true);
										gameObjectIndex++;
										// Reset datastructures
										vertexCount = 0;
										faceCount = 0;
									}
								}
							}
						} else {
							if (vertexCount != 0) {
								Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
								mesh.Clear();
								Vector3[] newVertices = new Vector3[vertexCount];
								for(int i = 0; i < vertexCount; i++){
									newVertices[i] = vertices[i];
								}
								int[] newFaces = new int[faceCount*3];
								for(int i = 0; i < faceCount*3; i++){
									newFaces[i] = faces[i];
								}
								mesh.vertices = newVertices;
								mesh.triangles = newFaces;
								MeshCollider meshCollider = environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>();
								// meshCollider.sharedMesh = null;
								meshCollider.sharedMesh = mesh;
								meshCollider.sharedMesh.RecalculateBounds();
								meshCollider.sharedMesh.RecalculateNormals();
								environmentalHazards [gameObjectIndex].SetActive (true);
								gameObjectIndex++;
								// Reset datastructures
								vertexCount = 0;
								faceCount = 0;
							}
						}
					} else { 	// Look down for empty points
						bool right = (pointGrid [index + 1].x == 1) ? true : false;
						float rightValue = pointGrid [index + 1].y;

						bool top = (pointGrid [(int)(index + environmentalHazardResolution.x)].x == 1) ? true : false;
						float topValue = pointGrid [(int)(index + environmentalHazardResolution.x)].y;

						bool topRight = (pointGrid [(int)(index + environmentalHazardResolution.x + 1)].x == 1) ? true : false;
						float topRightValue = pointGrid [(int)(index + environmentalHazardResolution.x + 1)].y;

						bool bottom = (pointGrid [(int)(index - environmentalHazardResolution.x)].x == 1) ? true : false;
						float bottomValue = pointGrid [(int)(index - environmentalHazardResolution.x)].y;

						bool bottomRight = (pointGrid [(int)(index - environmentalHazardResolution.x + 1)].x == 1) ? true : false;
						float bottomRightValue = pointGrid [(int)(index - environmentalHazardResolution.x + 1)].y;
					}
				
				}
				//if(gameObjectIndex == 1)
				//	break;
			}

			// if there still is a mesh in construction, then construct it
			if (vertexCount != 0) {
				Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
				mesh.Clear();
				Vector3[] newVertices = new Vector3[vertexCount];
				for(int i = 0; i < vertexCount; i++){
					newVertices[i] = vertices[i];
				}
				int[] newFaces = new int[faceCount*3];
				for(int i = 0; i < faceCount*3; i++){
					newFaces[i] = faces[i];
				}
				mesh.vertices = newVertices;
				mesh.triangles = newFaces;
				environmentalHazards [gameObjectIndex].SetActive (true);
				gameObjectIndex++;
				// Reset datastructures
				vertexCount = 0;
				faceCount = 0;
			}


		}
		// Deactivate all remaining prepared gameobjects
		for (int i = gameObjectIndex; i < environmentalHazards.Length; i++) {
			if(environmentalHazards[i].activeSelf)
				environmentalHazards[i].SetActive(false);
			else
				break;
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
