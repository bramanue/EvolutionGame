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

	// PERLIN NOISE VARIABLES FOR THE ENVIRONMENTAL OBSTACLES

	public int environmentOctaves = 2;

	public float environmentPersistence = 0.2f;

	public float environmentFrequency = 0.05f;


	// PERLIN NOISE VARIABLES FOR THE 3D TERRAIN FORMING

	// The higher, the more details but slower performance (low value recommended, since mesh cannot represent higher detail anyway)
	public int terrainFormingOctaves = 2;
	// The higher, the flatter, the terrain will be. The lower the more steep hills/cliffs there will be
	public float terrainFormingPersistence = 0.1f;
	// The higher, the shorter the distance between hills/cliffs
	public float terrainFormingFrequency = 0.42f;
	// Maximum height of the terrain
	public float maxTerrainHeight = 8.0f;


	// Defines how many threads are involved in calculating the perlin noise textures
	public int nofThreads = 3;


	// Datastructures

	private Texture2D environmentTexture;

	private Texture2D environmentBumpMap;

	private Texture2D environmentCubeMap;

	private static Color[] mainTextureColor;

	private Color[] bumpMapColor;

	private Color[] cubeMapColor;

	public GameObject environmentPlaneTilePrefab;

	private GameObject[] environmentPlaneTiles = new GameObject[9];

	private Renderer[] environmentPlaneTileRenderers = new Renderer[9];

	private Mesh[] environmentPlaneTileMeshes = new Mesh[9];

	private Bounds[] environmentPlaneTileBounds = new Bounds[9];

	private Texture2D[] environmentPlaneTileTextures = new Texture2D[9];

	private int[] planeTileOrdering = new int[9];
	// Stores the extent of all combined active tiles Vector4(Min_x, Min_y, Max_x, Max_y)
	private Vector4 currentBackgroundExtent;


	public float tileSize = 20;
	
	// Defines which of the 4 3D planes is chosen 
	// Index 0 has lowest quads count (16x16)
	// Index 3 has highest quads count (132x132)
	public int planeIndex = 2;

	private Vector3 meshSize;

	private Vector3 tileExtent;

	public bool moveBackground = true;

	public float backgroundMultiplierX = 1.0f;

	public float backgroundMultiplierY = 1.0f;

	public float backgroundTimeMultiplier = 0.3f;

	// The size of the environment texture in global space(should cover viewing range)
	public Vector2 mainTextureSize;


	// ENVIRONMENTAL OBJECT PREFABS
	public GameObject waterPrefab;

	public GameObject iceFieldPrefab;

	public GameObject thornBushPrefab;

	public GameObject lavaFieldPrefab;

	public GameObject dustStormPrefab;

	public GameObject lightningStormPrefab;


	// An array contianing all environmental obstacles on the field
	private GameObject[] environmentalObstacles = new GameObject[1000];
	// The next index into environmentalObstacles[] where a null object is
	private int environmentalObstacleIndex;
	// Defines from which index of environmentalObjects[] the current Update() iteration has to loop from
	private int previousPatchEndIndex;
	// Defines until which index of environmentalObjects[] the current Update() iteration has to loop
	private int[] patchSizes = new int[10];
	// The current Update() iteration
	private int currentPatchIndex;
	// How many Update() iterations are granted to update the whole environmentalObstacles[] array
	private int nofPatches;

	// Stores an index to the object of environmentalObstacles that is at that location of the texture
	private int[] textureQuadIsOccupiedBy;



	// Nof quads of the background tiles in x and y direction (must be a square)
	public float meshResolution = 64;


	// Current viewing range of the player
	private float playerViewingRange;


	private int frameCounter = 0;



	// Use this for initialization
	void Start () 
	{
		player = GameObject.Find ("Blob");
		playerScript = (player)player.GetComponent (typeof(player));

		// Instantiate all background planes
		for (int i = 0; i < environmentPlaneTiles.Length; i++) {
			environmentPlaneTiles[i] = (GameObject)GameObject.Instantiate(environmentPlaneTilePrefab);
			environmentPlaneTiles[i].transform.localScale = new Vector3(tileSize, 1, tileSize);
		}

		Bounds meshBounds = environmentPlaneTiles[0].GetComponent<MeshFilter>().mesh.bounds;
		tileExtent = (meshBounds.max - meshBounds.min)*tileSize*(meshResolution-1)/meshResolution;
		tileExtent.y = tileExtent.z;
		tileExtent.z = 0;
		Vector3 bottomLeft = player.transform.position - new Vector3 (1.0f * tileExtent.x, 1.0f * tileExtent.y, 0);

		// Position background planes
		Vector3 playerPos = player.transform.position;
		for (int y = 0; y < 3; y++) {
			for (int x = 0; x < 3; x++) {
				environmentPlaneTiles[y*3 + x].transform.position = bottomLeft + new Vector3(x*tileExtent.x, y*tileExtent.y, 0);
				planeTileOrdering[y*3 + x] = y*3 + x;
			}
		}

		// Get the renderer of the environment plane tiles
		for (int i = 0; i < environmentPlaneTileRenderers.Length; i++) {
			environmentPlaneTileRenderers[i] = (Renderer)environmentPlaneTiles[i].GetComponent (typeof(Renderer));
		}
		// Get the bounds of the scaled meshes from the renderers
		for (int i = 0; i < environmentPlaneTileBounds.Length; i++) {
			environmentPlaneTileBounds[i] = environmentPlaneTileRenderers[i].bounds;
		}
		// Create textures for all environment tile planes
		for (int i = 0; i < environmentPlaneTileTextures.Length; i++) {
			// Initialize the main texture
			environmentPlaneTileTextures[i] = new Texture2D ((int)mainTextureResolution.x, (int)mainTextureResolution.y);
			environmentPlaneTileRenderers[i].material.SetTexture("_MainTex", environmentPlaneTileTextures[i]);
		}
		for (int i = 0; i < environmentPlaneTileMeshes.Length; i++) {
			environmentPlaneTileMeshes[i] = ((MeshFilter)environmentPlaneTiles[i].GetComponent (typeof(MeshFilter))).mesh;
			environmentPlaneTileMeshes[i].MarkDynamic ();
		}


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

	
		randomInitialValue = Random.Range ((float)int.MinValue/10000.0f,(float)int.MaxValue/10000.0f);

		environmentalHazards = new GameObject[(int)(environmentalHazardResolution.x*environmentalHazardResolution.y*0.5f)];
		for (int i = 0; i < environmentalHazards.Length; i++) {
			environmentalHazards[i] = new GameObject();
			environmentalHazards[i].AddComponent<MeshFilter>();
			environmentalHazards[i].GetComponent<MeshFilter>().mesh.MarkDynamic();
			environmentalHazards[i].AddComponent<MeshRenderer>();
			environmentalHazards[i].AddComponent<BoxCollider>();
			environmentalHazards[i].GetComponent<BoxCollider>().isTrigger = true;
			environmentalHazards[i].AddComponent(typeof(thornBush));
			thornBush t = (thornBush)environmentalHazards[i].GetComponent(typeof(thornBush));
			t.damagePerSecond = 0.1f;
			t.slowDownFactor = 0.6f;
			environmentalHazards[i].SetActive(false);
		}

		environmentalHazardMask = new float[(int)(mainTextureResolution.x * mainTextureResolution.y)];
		textureQuadIsOccupiedBy = new int[(int)(mainTextureResolution.x * mainTextureResolution.y)];
		// Set all spaces to unoccupied
		for (int i = 0; i < textureQuadIsOccupiedBy.Length; i++) {
			textureQuadIsOccupiedBy[i] = -1;
		}

		nofPatches = 10;
		int nofStoredObstacles = environmentalObstacles.Length;
		int patchSize = nofStoredObstacles / nofPatches;
		for (int i = 1; i < nofPatches; i++) {
			patchSizes[i-1] = i*patchSize;
		}
		environmentalObstacleIndex = 0;
	//	environmentalObstacles[environmentalObstacleIndex] = GameObject.Instantiate(thornBushPrefab);
		patchSizes[nofPatches-1] = nofStoredObstacles;
	}
	
	// Update is called once per frame
	void Update () 
	{
		float t0 = System.DateTime.Now.Millisecond;

		// TODO Could put this into an IEnumerable and let it run at a lower framerate to safe ressources
		float currentTime = Time.time;

		playerViewingRange = playerScript.currentViewingRange + player.transform.localScale.x;
		Vector3 playerPos = player.transform.position;
		mainTextureSize = new Vector2 (2.1f*(playerViewingRange), 2.1f*(playerViewingRange));

		// Loop over the 9 plane tiles from the bottom left row-wise to the top right
		for (int i = 0; i < 9; i++) {
			Vector3 distance = playerPos - environmentPlaneTileBounds[planeTileOrdering[i]].center;
			// Set the plane active if player is within its bounds 
			if(Mathf.Abs(distance.x) < environmentPlaneTileBounds[planeTileOrdering[i]].extents.x && Mathf.Abs(distance.y) < environmentPlaneTileBounds[planeTileOrdering[i]].extents.y) {
				environmentPlaneTiles[planeTileOrdering[i]].SetActive(true);
				// If player has entered a new tile, reposition the planes, such that the current tile is the center of the 9 tiles
				if(i != 4) {
					setNewCenterTile(i);
				}
				// Calculate terrain elevation for all active planes
				distortBackgroundPlane(planeTileOrdering[planeTileOrdering[i]],moveBackground,backgroundTimeMultiplier,backgroundMultiplierX,backgroundMultiplierY,currentTime);
			}
			// Set the plane active if player's viewingRange reaches the closest point of this plane tile's boundaries
			else if( (environmentPlaneTileBounds[planeTileOrdering[i]].ClosestPoint(playerPos) - playerPos).magnitude <= 4.0f*playerViewingRange)
			{
				if(!environmentPlaneTiles[planeTileOrdering[i]].activeSelf) 
				{
					environmentPlaneTiles[planeTileOrdering[i]].SetActive(true);
				}
				// Calculate terrain elevation for all active planes
				distortBackgroundPlane(planeTileOrdering[planeTileOrdering[i]],moveBackground,backgroundTimeMultiplier,backgroundMultiplierX,backgroundMultiplierY,currentTime);
			}
			// Otherwise set it inactive
			else 
			{
				if(environmentPlaneTiles[planeTileOrdering[i]].activeSelf) 
				{
					environmentPlaneTiles[planeTileOrdering[i]].SetActive(false);
				}
			}
		}
		// Calculate the bottom left and top right point of the combination of all active plane tiles
		currentBackgroundExtent = new Vector4 (float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
		for (int i = 0; i < 9; i++) {
			if (environmentPlaneTiles[i].activeSelf) {
				currentBackgroundExtent.x = Mathf.Min (currentBackgroundExtent.x, environmentPlaneTileBounds[i].min.x);
				currentBackgroundExtent.y = Mathf.Min (currentBackgroundExtent.y, environmentPlaneTileBounds[i].min.y);
				currentBackgroundExtent.z = Mathf.Max (currentBackgroundExtent.z, environmentPlaneTileBounds[i].max.x);
				currentBackgroundExtent.w = Mathf.Max (currentBackgroundExtent.w, environmentPlaneTileBounds[i].max.y);
			}
		}

		// Loop over the current patch of environmental obstacles
		int obstacleIndex = previousPatchEndIndex;
		for (; obstacleIndex < patchSizes [currentPatchIndex]; obstacleIndex++) 
		{
			if(environmentalObstacles[obstacleIndex] == null)
				continue;

			Bounds obstacleBounds = environmentalObstacles[obstacleIndex].GetComponent<MeshRenderer>().bounds;
			Vector3 distance = playerPos - obstacleBounds.center;
			// Set the obstacle to active if player's viewingRange reaches the closest point of this obstacle's boundaries
			if( (obstacleBounds.ClosestPoint(playerPos) - playerPos).magnitude <= 4.0f*playerViewingRange)
			{
				environmentalObstacles[obstacleIndex].SetActive(true);
			}
			// Otherwise set it inactive
			else 
			{
				environmentalObstacles[obstacleIndex].SetActive(false);
			}
		}
		// Prepare next iteration over environmental obstacles
		if (obstacleIndex >= environmentalObstacles.Length) {
			currentPatchIndex = 0;
			previousPatchEndIndex = 0;
		} else {
			currentPatchIndex++;
			previousPatchEndIndex = obstacleIndex;
		}


		// TODO update perlin noise parameters according to time and plyer's size

		// Calculate current environment texture
		calculateEnvironmentTextureNew ();

	//	Debug.Log ("Total time for environment update on CPU : " + (System.DateTime.Now.Millisecond - t0) + "ms");
		frameCounter++;
	}

	IEnumerator removeEnvironmentalObstacle(int index, float destructionTime = 1.0f) 
	{
		Vector3 initialSize = environmentalObstacles [index].transform.localScale;
		Vector3 sizeReductionPerSecond = initialSize / destructionTime;

		for (float time = 0.0f; time < destructionTime; time += Time.deltaTime) {
			environmentalObstacles [index].transform.localScale -= sizeReductionPerSecond*Time.deltaTime;
			yield return null;
		}
		environmentalObstacles [index].transform.localScale = new Vector3 (0, 0, 0);
		GameObject.Destroy(environmentalObstacles [index]);
	}


	private void calculateEnvironmentTextureNew()
	{
		Vector2 bottomLeftOfBackground = new Vector2 (currentBackgroundExtent.x, currentBackgroundExtent.y);
		Vector2 currentExtents = new Vector2 (currentBackgroundExtent.z, currentBackgroundExtent.w) - bottomLeftOfBackground;
		Vector2 topRightOfBackground = bottomLeftOfBackground + new Vector2 (Mathf.Max (currentExtents.x, currentExtents.y), Mathf.Max (currentExtents.x, currentExtents.y));
		Vector2 textureExtent = topRightOfBackground - bottomLeftOfBackground;

		Vector2 bottomLeftOfViewingRange = new Vector2 (player.transform.position.x - playerViewingRange, player.transform.position.y - playerViewingRange);
		Vector2 viewingRangeExtent = 8.0f * new Vector2(playerViewingRange, playerViewingRange);

		// Calculate what distance each pixel of the texture covers in world space
		Vector2 worldDistancePerPixel = new Vector2 (textureExtent.x / mainTextureResolution.x, textureExtent.y / mainTextureResolution.y);
		// Get the current time for time dependent perlin noise
		float currentTime = Time.time;

		int startRow = (int)(mainTextureResolution.y - (textureExtent.y - (bottomLeftOfViewingRange.y - bottomLeftOfBackground.y)) / worldDistancePerPixel.y);
		startRow = (int)Mathf.Max (0, startRow - 1);
		int endRow = (int)(startRow + viewingRangeExtent.y / worldDistancePerPixel.y + 1);
		endRow = (int)Mathf.Min (endRow + 1, mainTextureResolution.y);

		int startColumn = (int)(mainTextureResolution.x - (textureExtent.x - (bottomLeftOfViewingRange.x - bottomLeftOfBackground.x)) / worldDistancePerPixel.x);
		startColumn = (int)Mathf.Max (0, startColumn - 1);
		int endColumn = (int)(startColumn + viewingRangeExtent.x / worldDistancePerPixel.x + 1);
		endColumn = (int)Mathf.Min (endColumn + 1, mainTextureResolution.y);

		calculateTextureMaskNew(startRow, endRow, startColumn, endColumn, bottomLeftOfBackground.x, bottomLeftOfBackground.y, worldDistancePerPixel, currentTime);

		float threshold = 0.75f;
		generateEnvironmentalHazardsNew (startRow, endRow, startColumn, endColumn, bottomLeftOfBackground, worldDistancePerPixel, threshold);
	}

	private void calculateTextureMaskNew(int startRow, int endRow, int startColumn, int endColumn, float xOffset, float yOffset, Vector2 distancePerPixel, float currentTime)
	{
		for (int y = startRow; y < endRow; y++) 
		{
			for(int x = startColumn; x < endColumn; x++) 
			{
				float terrainSample = addUpOctaves (environmentOctaves, environmentFrequency, environmentPersistence, x*distancePerPixel.x + xOffset, y*distancePerPixel.y + yOffset);
				environmentalHazardMask[y*(int)mainTextureResolution.x + x] = terrainSample;
			}
		}
	}

	private void generateEnvironmentalHazardsNew (int startRow, int endRow, int startColumn, int endColumn, Vector2 bottomLeftOfBackground, Vector2 worldDistancePerPixel, float threshold)
	{
		Vector2 extentToCenterOfPixel = worldDistancePerPixel * 0.5f;
		int count = 0;
		for (int y = startRow; y < endRow; y++) 
		{
			for(int x = startColumn; x < endColumn; x++) 
			{
				// Continue only if this texture quad is not yet occupied by some structure
				if(textureQuadIsOccupiedBy[y*(int)mainTextureResolution.x + x] == -1)
				{
					if(environmentalHazardMask[y*(int)mainTextureResolution.x + x] > threshold) 
					{
						environmentalObstacles[environmentalObstacleIndex] = instantiateRandomPrefab();

						float rndValue = Random.Range(0.5f,1.0f);
						float size = rndValue * extentToCenterOfPixel.x;
						environmentalObstacles[environmentalObstacleIndex].transform.localScale *= size;

						Vector2 displacement = Random.insideUnitCircle * 0.5f;
						displacement.x *= extentToCenterOfPixel.x;
						displacement.y *= extentToCenterOfPixel.y;
						Vector3 position = new Vector3(bottomLeftOfBackground.x + x*worldDistancePerPixel.x + extentToCenterOfPixel.x + displacement.x, 
						                               bottomLeftOfBackground.y + y*worldDistancePerPixel.y + extentToCenterOfPixel.y + displacement.y, 
						                               0);

						environmentalObstacles[environmentalObstacleIndex].transform.position = position;
						textureQuadIsOccupiedBy[y*(int)mainTextureResolution.x + x] = environmentalObstacleIndex;

						environmentalObstacleIndex++;
						count++;
					}
				}
				else
				{
					// TODO Maybe check whether it can be combined with neighboring obstacles
				}
			}
		}
	}

	private GameObject instantiateRandomPrefab()
	{
		int rndValue = (int)Random.Range (0, 7);

		switch (rndValue) {
		case (0) :
			return GameObject.Instantiate(waterPrefab);
			break;
		case (1) :
			return GameObject.Instantiate(iceFieldPrefab);
			break;
		case (2) :
			return GameObject.Instantiate(thornBushPrefab);
			break;
		case (3) :
			return GameObject.Instantiate(lavaFieldPrefab);
			break;
		case (4) :
			return GameObject.Instantiate(dustStormPrefab);
			break;
		case (5) :
			return GameObject.Instantiate(lightningStormPrefab);
			break;
		default:
			return GameObject.Instantiate(thornBushPrefab);
		}
	}

	// Calculates a texture for each environment component that acts as mask for high resolution textures
	private void calculateEnvironmentTexture()
	{
		float t0 = System.DateTime.Now.Millisecond;
		
		// Calculate the offsets induced by the current player position for the perlin noise calculation
		float xOffset = player.transform.position.x + randomInitialValue;
		float yOffset = player.transform.position.y + randomInitialValue;
		
		// Calculate for each texture what distance each pixel covers in world coordinates
		Vector2 distancePerPixel = new Vector2 (mainTextureSize.x / mainTextureResolution.x, mainTextureSize.y / mainTextureResolution.y);
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
			threads[threadIndex] = new System.Threading.Thread(() => calculateTextureMask(startRow, startRow + rowsPerThread, xOffset, yOffset, distancePerPixel, currentTime));
			// Start the working thread
			threads[threadIndex].Start();
		}
		
		// Main thread calculates rest of the texture
		calculateTextureMask((nofThreads-1)*rowsPerThread, (int)mainTextureResolution.y, xOffset, yOffset, distancePerPixel, currentTime);
		
		// Main thread waits for the other threads to finish
		for (int threadIndex = 0; threadIndex < nofThreads-1; threadIndex++)
			threads [threadIndex].Join();
		
		generateEnvironmentalHazards(0.8f,environmentalHazardMask);
		
		// Apply the texture to the mesh renderer
		environmentTexture.SetPixels (mainTextureColor);
		environmentTexture.Apply();
		
		Debug.Log ("Total time for tex calculation " + (System.DateTime.Now.Millisecond - t0) + "ms");
		return;
		
	}

	private void calculateTextureMask(int startRow, int endRow, float xOffset, float yOffset, Vector2 distancePerPixel, float currentTime)
	{
		for (int y = startRow; y < endRow; y++) 
		{
			for(int x = 0; x < mainTextureResolution.x; x++) 
			{
				float terrainSample = addUpOctaves (environmentOctaves, environmentFrequency, environmentPersistence, x*distancePerPixel.x + xOffset, y*distancePerPixel.y + yOffset);
				mainTextureColor[y*(int)mainTextureResolution.x + x] = new Color(terrainSample,terrainSample,terrainSample);
				environmentalHazardMask[y*(int)mainTextureResolution.x + x] = terrainSample;
			}
		}
	}
	
	private void generateEnvironmentalHazards (float threshold1, float[] mask1)
	{
		Vector2[] pointGrid = new Vector2[(int)(environmentalHazardResolution.x * environmentalHazardResolution.y)];
		Vector3 lowerLeftPoint = new Vector3 (player.transform.position.x - 0.5f * mainTextureSize.x, player.transform.position.y - 0.5f * mainTextureSize.y, 0);
		Vector3 originalLowerLeftPoint = new Vector3 (player.transform.position.x - 0.5f * mainTextureSize.x, player.transform.position.y - 0.5f * mainTextureSize.y, 0);
		Vector3 stepSize = new Vector3 (mainTextureSize.x / (environmentalHazardResolution.x - 1), mainTextureSize.y / (environmentalHazardResolution.y - 1), 0);
		// Make sure the sampling grid stays the same for the same background plane size
		// Calculate the offset between sampling grid and lower left point of the plane (in world space)
		float offsetX = lowerLeftPoint.x % stepSize.x;
		float offsetY = lowerLeftPoint.y % stepSize.y;
		// Set bottom left of the plane on a point belonging to the sampling grid
		lowerLeftPoint -= new Vector3 (offsetX, offsetY, 0);	
		// Convert offsets to local space of the textures
		float unitOffsetX = offsetX/stepSize.x;
		float unitOffsetY = offsetY/stepSize.y;

		// We assume that all textures are squares
		int maskRes = (int)Mathf.Sqrt (mask1.Length);
		float stepSizeOnMask = maskRes / environmentalHazardResolution.x;

		// Used to convert from global coordinates to coordinates on the environmentMask Texture
		Vector3 global2Mask = new Vector3(maskRes/mainTextureSize.x, maskRes/mainTextureSize.y, 0);

		for (int y = 0; y < environmentalHazardResolution.y; y++) {
			for (int x = 0; x < environmentalHazardResolution.x; x++) {
				// Get the sample point in space of the mask
				int samplePoint = (int)((x*stepSize.x - offsetX)*global2Mask.x) + (int)(((int)((y*stepSize.y - offsetY)*global2Mask.y))*maskRes);
				//	int samplePoint = (int)((((int)(y-unitOffsetY)) * environmentalHazardResolution.x + ((int)(x-unitOffsetX))) * stepSizeOnMask);
				// If the sampled point lies outside the mask, then set this point's value to -1
				if(samplePoint < 0 || samplePoint >= mask1.Length) {
					pointGrid [(int)(y * environmentalHazardResolution.x + x)] = new Vector2 (0, -1);
					continue;
				}

				if (mask1 [samplePoint] > threshold1) {
					pointGrid [(int)(y * environmentalHazardResolution.x + x)] = new Vector2 (1, mask1 [samplePoint]);
				} else {
					pointGrid [(int)(y * environmentalHazardResolution.x + x)] = new Vector2 (0, mask1 [samplePoint]);
				}
			}
		}

		Vector3[] vertices = new Vector3[(int)(2 * environmentalHazardResolution.x)];
		int[] faces = new int[(int)(2 * 3 * environmentalHazardResolution.x)];
		int vertexCount = 0;
		int faceCount = 0;

		int gameObjectIndex = 0;

		for (int y = 0; y < environmentalHazardResolution.y - 1; y++) {
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
							createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);

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
							createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
						
						} else if (topRight && right) {
							// Triangle .:
							// Begin of convex shape
							if (vertexCount != 0) {
								createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
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
									createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
								
								} else {
									// pointGrid[(int)(index + environmentalHazardResolution.x + 1)].x = 0;
									// No Triangle
									if (vertexCount != 0) {
										createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
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
									createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
								
								} else {
									// pointGrid[(int)(index + environmentalHazardResolution.x + 1)].x = 0;
									// No Triangle
									if (vertexCount != 0) {
										createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
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
										createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
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
										createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
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
									createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
								
								} else {
									// pointGrid[(int)(index + environmentalHazardResolution.x + 1)].x = 0;
									// No Triangle
									if (vertexCount != 0) {
										createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
									}
								}
							}
						} else {
							if (vertexCount != 0) {
								createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
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
			}

			// if there still is a mesh in construction, then construct it
			if (vertexCount != 0) {
				createObstacleColliderMesh(ref gameObjectIndex, ref vertexCount, ref faceCount, vertices, faces);
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

	// Creates a 3D Mesh with box collider and resets the passed integer references for reuse
	private void createObstacleColliderMesh(ref int gameObjectIndex, ref int vertexCount, ref int faceCount, Vector3[] vertices, int[] faces)
	{
		Mesh mesh = environmentalHazards [gameObjectIndex].GetComponent<MeshFilter> ().mesh;
		// environmentalHazards[gameObjectIndex].GetComponent<MeshCollider>().sharedMesh = null;
		mesh.Clear();
		Vector3[] newVertices = new Vector3[vertexCount];
		for(int i = 0; i < vertexCount; i++){
			newVertices[i] = vertices[i];
		}
		int[] newFaces = new int[faceCount*3];
		for(int i = 0; i < faceCount*3; i++){
			newFaces[i] = faces[i];
		}
		// Set the vertices of the mesh
		mesh.vertices = newVertices;
		// Set the triangles of the mesh
		mesh.triangles = newFaces;
		// Recalculate bounds and normals of the newly formed mesh
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		// Remove the previous box collider
		Destroy (environmentalHazards[gameObjectIndex].GetComponent<BoxCollider>());
		// Add a new box collider (with the correct coordinates)
		environmentalHazards[gameObjectIndex].AddComponent<BoxCollider>();
		environmentalHazards[gameObjectIndex].GetComponent<BoxCollider>().isTrigger = true;
		// Make the collider 3D (and not just flat)
		Vector3 boxSize = environmentalHazards[gameObjectIndex].GetComponent<BoxCollider>().size;
		environmentalHazards[gameObjectIndex].GetComponent<BoxCollider>().size = new Vector3(boxSize.x, boxSize.y, player.transform.localScale.x*2.0f);
		// Activate the collider
		environmentalHazards [gameObjectIndex].SetActive (true);
		gameObjectIndex++;
		// Reset datastructures
		vertexCount = 0;
		faceCount = 0;
	}


	private void distortBackgroundPlane(int tileIndex, bool moveOverTime, float timeMultiplier, float xMultiplier, float yMultiplier, float currentTime) 
	{
		// Get the vertices of the plane (in object space)
		Vector3[] vertices = environmentPlaneTileMeshes[tileIndex].vertices;
		float timeOffset = 0.0f;
		if (moveOverTime) {
			timeOffset = currentTime*timeMultiplier;
		}
		for (int i = 0; i < vertices.Length; i++)
		{   
			// Transform the vertices of the plane into world space
			Vector3 vertexPoint = environmentPlaneTiles[tileIndex].transform.TransformPoint(vertices[i]);

			float xCoord = (vertexPoint.x + timeOffset)*xMultiplier;
			float yCoord = (vertexPoint.y + timeOffset)*yMultiplier;
			// Generate smooth hills (z-offset)
			vertexPoint.z = addUpOctaves (terrainFormingOctaves, terrainFormingFrequency, terrainFormingPersistence, xCoord, yCoord) * maxTerrainHeight;
			// Transform the manipulated vertex back into object space and store it
			vertices[i] = environmentPlaneTiles[tileIndex].transform.InverseTransformPoint(vertexPoint);
		}
		// Set manipulated vertices
		environmentPlaneTileMeshes[tileIndex].vertices = vertices;
		// Recalculate the boundaries
		environmentPlaneTileMeshes[tileIndex].RecalculateBounds();
		// Recalculate surface normals
		environmentPlaneTileMeshes[tileIndex].RecalculateNormals();
	}


	// Is called, when the player enters a new plane tile. This function reorders the plane tiles such that 'index' is the new central tile
	// Following are the indices of the tiles (4 is always the central plane)
	// 6 7 8
	// 3 4 5
	// 0 1 2
	private void setNewCenterTile(int index) 
	{
		Vector3 centralPlaneTilePos = environmentPlaneTiles[planeTileOrdering[index]].transform.position;
		int old0, old1, old2, old3, old4, old5, old6, old7, old8;
		old0 = planeTileOrdering[0];
		old1 = planeTileOrdering[1];
		old2 = planeTileOrdering[2];
		old3 = planeTileOrdering[3];
		old4 = planeTileOrdering[4];
		old5 = planeTileOrdering[5];
		old6 = planeTileOrdering[6];
		old7 = planeTileOrdering[7];
		old8 = planeTileOrdering[8];
		Debug.Log ("Switched to plane" + index);
		
		switch(index) {
		case (0) :
			// reposition tiles 2,5,6,7,8 to positions 0,1,2,3,6
			
			// Move 2 to 2
			environmentPlaneTiles[planeTileOrdering[2]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[2]] = environmentPlaneTileRenderers[planeTileOrdering[2]].bounds;
			// Move 5 to 3
			environmentPlaneTiles[planeTileOrdering[5]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, 0, 0);
			environmentPlaneTileBounds[planeTileOrdering[5]] = environmentPlaneTileRenderers[planeTileOrdering[5]].bounds;
			planeTileOrdering[3] = planeTileOrdering[5];
			// Move 6 to 6
			environmentPlaneTiles[planeTileOrdering[6]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[6]] = environmentPlaneTileRenderers[planeTileOrdering[6]].bounds;
			// Move 7 to 0
			environmentPlaneTiles[planeTileOrdering[7]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[7]] = environmentPlaneTileRenderers[planeTileOrdering[7]].bounds;
			planeTileOrdering[0] = planeTileOrdering[7];
			// Move 8 to 1
			environmentPlaneTiles[planeTileOrdering[8]].transform.position = centralPlaneTilePos + new Vector3(0, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[8]] = environmentPlaneTileRenderers[planeTileOrdering[8]].bounds;
			planeTileOrdering[1] = planeTileOrdering[8];
			
			planeTileOrdering[4] = old0;
			planeTileOrdering[5] = old1;
			planeTileOrdering[7] = old3;
			planeTileOrdering[8] = old4;
			break;
		case (1) :
			// reposition tiles 6,7,8 to positions 0,1,2
			
			// Move 6 to 0
			environmentPlaneTiles[planeTileOrdering[6]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[6]] = environmentPlaneTileRenderers[planeTileOrdering[6]].bounds;
			planeTileOrdering[0] = planeTileOrdering[6];
			// Move 7 to 1
			environmentPlaneTiles[planeTileOrdering[7]].transform.position = centralPlaneTilePos + new Vector3(0, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[7]] = environmentPlaneTileRenderers[planeTileOrdering[7]].bounds;
			planeTileOrdering[1] = planeTileOrdering[7];
			// Move 8 to 2
			environmentPlaneTiles[planeTileOrdering[8]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[8]] = environmentPlaneTileRenderers[planeTileOrdering[8]].bounds;
			planeTileOrdering[2] = planeTileOrdering[8];
			
			planeTileOrdering[3] = old0;
			planeTileOrdering[4] = old1;
			planeTileOrdering[5] = old2;
			planeTileOrdering[6] = old3;
			planeTileOrdering[7] = old4;
			planeTileOrdering[8] = old5;
			break;
		case (2) :
			// reposition tiles 0,3,6,7,8 to positions 0,1,2,5,8
			
			// Move 0 to 0
			environmentPlaneTiles[planeTileOrdering[0]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[0]] = environmentPlaneTileRenderers[planeTileOrdering[0]].bounds;
			// Move 3 to 1
			environmentPlaneTiles[planeTileOrdering[3]].transform.position = centralPlaneTilePos + new Vector3(0, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[3]] = environmentPlaneTileRenderers[planeTileOrdering[3]].bounds;
			planeTileOrdering[1] = planeTileOrdering[3];
			// Move 6 to 2
			environmentPlaneTiles[planeTileOrdering[6]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[6]] = environmentPlaneTileRenderers[planeTileOrdering[6]].bounds;
			planeTileOrdering[2] = planeTileOrdering[6];
			// Move 7 to 5
			environmentPlaneTiles[planeTileOrdering[7]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, 0, 0);
			environmentPlaneTileBounds[planeTileOrdering[7]] = environmentPlaneTileRenderers[planeTileOrdering[7]].bounds;
			planeTileOrdering[5] = planeTileOrdering[7];
			// Move 8 to 8
			environmentPlaneTiles[planeTileOrdering[8]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[8]] = environmentPlaneTileRenderers[planeTileOrdering[8]].bounds;
			
			planeTileOrdering[3] = old1;
			planeTileOrdering[4] = old2;
			planeTileOrdering[6] = old4;
			planeTileOrdering[7] = old5;
			break;
		case (3) :
			// reposition tiles 2,5,8 to positions 0,3,6
			
			// Move 2 to 0
			environmentPlaneTiles[planeTileOrdering[2]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[2]] = environmentPlaneTileRenderers[planeTileOrdering[2]].bounds;
			planeTileOrdering[0] = planeTileOrdering[2];
			// Move 5 to 3
			environmentPlaneTiles[planeTileOrdering[5]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, 0, 0);
			environmentPlaneTileBounds[planeTileOrdering[5]] = environmentPlaneTileRenderers[planeTileOrdering[5]].bounds;
			planeTileOrdering[3] = planeTileOrdering[5];
			// Move 8 to 6
			environmentPlaneTiles[planeTileOrdering[8]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[8]] = environmentPlaneTileRenderers[planeTileOrdering[8]].bounds;
			planeTileOrdering[6] = planeTileOrdering[8];
			
			planeTileOrdering[1] = old0;
			planeTileOrdering[2] = old1;
			planeTileOrdering[4] = old3;
			planeTileOrdering[5] = old4;
			planeTileOrdering[7] = old6;
			planeTileOrdering[8] = old7;
			break;
		case (5) :
			// reposition tiles 0,3,6 to positions 2,5,8
			
			// Move 0 to 2
			environmentPlaneTiles[planeTileOrdering[0]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[0]] = environmentPlaneTileRenderers[planeTileOrdering[0]].bounds;
			planeTileOrdering[2] = planeTileOrdering[0];
			// Move 3 to 5
			environmentPlaneTiles[planeTileOrdering[3]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, 0, 0);
			environmentPlaneTileBounds[planeTileOrdering[3]] = environmentPlaneTileRenderers[planeTileOrdering[3]].bounds;
			planeTileOrdering[5] = planeTileOrdering[3];
			// Move 6 to 8
			environmentPlaneTiles[planeTileOrdering[6]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[6]] = environmentPlaneTileRenderers[planeTileOrdering[6]].bounds;
			planeTileOrdering[8] = planeTileOrdering[6];
			
			planeTileOrdering[0] = old1;
			planeTileOrdering[1] = old2;
			planeTileOrdering[3] = old4;
			planeTileOrdering[4] = old5;
			planeTileOrdering[6] = old7;
			planeTileOrdering[7] = old8;
			break;
		case (6) :
			// reposition tiles 0,1,2,5,8 to positions 0,3,6,7,8
			
			// Move 0 to 0
			environmentPlaneTiles[planeTileOrdering[0]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[0]] = environmentPlaneTileRenderers[planeTileOrdering[0]].bounds;
			// Move 1 to 3
			environmentPlaneTiles[planeTileOrdering[1]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, 0, 0);
			environmentPlaneTileBounds[planeTileOrdering[1]] = environmentPlaneTileRenderers[planeTileOrdering[1]].bounds;
			planeTileOrdering[3] = planeTileOrdering[1];
			// Move 2 to 6
			environmentPlaneTiles[planeTileOrdering[2]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[2]] = environmentPlaneTileRenderers[planeTileOrdering[2]].bounds;
			planeTileOrdering[6] = planeTileOrdering[2];
			// Move 5 to 7
			environmentPlaneTiles[planeTileOrdering[5]].transform.position = centralPlaneTilePos + new Vector3(0, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[5]] = environmentPlaneTileRenderers[planeTileOrdering[5]].bounds;
			planeTileOrdering[7] = planeTileOrdering[5];
			// Move 8 to 8
			environmentPlaneTiles[planeTileOrdering[8]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[8]] = environmentPlaneTileRenderers[planeTileOrdering[8]].bounds;
			
			planeTileOrdering[1] = old3;
			planeTileOrdering[2] = old4;
			planeTileOrdering[4] = old6;
			planeTileOrdering[5] = old7;
			break;
		case (7) :
			// reposition tiles 0,1,2 to positions 6,7,8
			
			// Move 0 to 6
			environmentPlaneTiles[planeTileOrdering[0]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[0]] = environmentPlaneTileRenderers[planeTileOrdering[0]].bounds;
			planeTileOrdering[6] = planeTileOrdering[0];
			// Move 1 to 7
			environmentPlaneTiles[planeTileOrdering[1]].transform.position = centralPlaneTilePos + new Vector3(0, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[1]] = environmentPlaneTileRenderers[planeTileOrdering[1]].bounds;
			planeTileOrdering[7] = planeTileOrdering[1];
			// Move 2 to 8
			environmentPlaneTiles[planeTileOrdering[2]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[2]] = environmentPlaneTileRenderers[planeTileOrdering[2]].bounds;
			planeTileOrdering[8] = planeTileOrdering[2];
			
			planeTileOrdering[0] = old3;
			planeTileOrdering[1] = old4;
			planeTileOrdering[2] = old5;
			planeTileOrdering[3] = old6;
			planeTileOrdering[4] = old7;
			planeTileOrdering[5] = old8;
			break;
		case (8) :
			// reposition tiles 0,1,2,3,6 to positions 2,5,6,7,8
			
			// Move 0 to 8
			environmentPlaneTiles[planeTileOrdering[0]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[0]] = environmentPlaneTileRenderers[planeTileOrdering[0]].bounds;
			planeTileOrdering[8] = planeTileOrdering[0];
			// Move 1 to 7
			environmentPlaneTiles[planeTileOrdering[1]].transform.position = centralPlaneTilePos + new Vector3(0, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[1]] = environmentPlaneTileRenderers[planeTileOrdering[1]].bounds;
			planeTileOrdering[7] = planeTileOrdering[1];
			// Move 2 to 2
			environmentPlaneTiles[planeTileOrdering[2]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, -tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[2]] = environmentPlaneTileRenderers[planeTileOrdering[2]].bounds;
			// Move 3 to 5
			environmentPlaneTiles[planeTileOrdering[3]].transform.position = centralPlaneTilePos + new Vector3(tileExtent.x, 0, 0);
			environmentPlaneTileBounds[planeTileOrdering[3]] = environmentPlaneTileRenderers[planeTileOrdering[3]].bounds;
			planeTileOrdering[5] = planeTileOrdering[3];
			// Move 6 to 6
			environmentPlaneTiles[planeTileOrdering[6]].transform.position = centralPlaneTilePos + new Vector3(-tileExtent.x, tileExtent.y, 0);
			environmentPlaneTileBounds[planeTileOrdering[6]] = environmentPlaneTileRenderers[planeTileOrdering[6]].bounds;
			
			planeTileOrdering[0] = old4;
			planeTileOrdering[1] = old5;
			planeTileOrdering[3] = old7;
			planeTileOrdering[4] = old8;
			break;
		default :
			break;
		};
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
