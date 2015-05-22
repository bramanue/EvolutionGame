using UnityEngine;
using System.Collections;

public class enemy : MonoBehaviour {

	// Start position of this enemy. Do not move away too far from this point.
	private Vector3 originalPosition;
	
	// Vector that points to the player
	public Vector3 toPlayer;

	// Pointer to the player
	private GameObject player;
	
	// The player script
	private player playerScript;

	// Enemy manager for performing eating, etc operations
	private enemyManager enemyMngr;


	// Viewing direction of this enemy (normalized)
	public Vector3 viewingDirection;

	// Used to detect course corrections done by the envrionmentEvader
	private Vector3 previousViewingDirection;

	// The base viewing range without any eye ability
	public float baseViewingRange;
	
	// The viewing range boost given by the eye ability
	public float viewingRangeBoost;
	
	// Viewing Range of the player (depends on ability "Eyes")
	public float viewingRange;

	// Stores the original viewing range
	public float originalViewingRange;

	// Defines how wide the angle of the enemy's eyes are 
	public float cosViewingAngle;
	
	// Stores the original cosViewingAngle for restoring default after escape procedure
	public float originalCosViewingAngle;

	// Flag set, when enemy can see player
	private bool canSeePlayer;


	// The base velocity of this blob (without any running ability)
	public float baseVelocity = 5.0f;

	// Max velocity of the player (depends on ability "Speed")
	public float runVelocityBoost;

	// The current pace of movement
	public float currentSpeed;

	// The maximum velocity of this enemy
	public float maxVelocity ;
	
	// Defines whether this enemy is allowed to walk
	public bool canMove;


	// The size of this enemy
	public float size;

	// Defines by how much this blob can grow per second
	public float growSpeed = 0.1f;

	// Defines by how much this blob can shrink per second
	public float shrinkSpeed = 1.0f;
	

	// Defines whether this enemy can act at all
	public bool stunned;

	// Timer which starts running, when this enemy blob gets stunned
	public float stunnedTimer;

	// Defines whether this enemy is currently blinded
	public bool blinded;
	
	// Timer that starts when this blob is blinded
	private float blindedTimer;

	// The total blinded time
	private float originalBlindedTimer;

	// Defines whether this enemy is at idle or is hunting the player
	public bool isHuntingPlayer;
	
	// Defines whether this enely is running away from the player
	private bool isRunningAwayFromPlayer;

	// Defines whether enemy is still in alarmed state
	private bool isInAlertedState;

	// Timer used to keep awareness of enemy blob up
	private float isInAlertedStateTimer = 0.0f;
	
	// Original timer value for interpolation
	private float originalInAlertedStateTimer = 0.0f;


	// Damage inflicted by touching or residing inside a certain environment
	private float environmentalDamage;

	// Force induced by touching or residing inside a certain environment
	private Vector3 environmentalPushBack;

	// Course correction induced by nearby hazardous environments TODO remove
	private Vector3 environmentalCourseCorrection;

	// Data, that tells this blob about nearby hazardous environments
	public dangerProximity environmentProximityData;
	

	// Timer used for idle waiting 
	private float idleTimer = 0.0f;

	// The radius in which the enemy operates during idle animations
	public float idleOperationRadius;

	// The radius in which the enemy operates during hunting the player
	public float activeOperationRadius;

	// Defines by which factor velocity should be removed for idle walking actions
	private float idleSpeedReduction = 0.4f;

	private float environmentalSlowDown = 1.0f;


	// The location, this enemy wants to walk to.
	private Vector3 idleWalkingTarget;

	// The target viewing  direction for some rotation
	private Vector3 idleRotationTarget;

	// The quaternion to which we want to rotate
	private Quaternion idleRotationTargetQuaternion;
	
	// Defines whether the current animation is complete
	private bool isIdleAnimationComplete = true;

	// Booleans to define the current idle action
	private bool isIdleRotating = false;
	private bool isIdleWalking = false;
	private bool isIdleWaiting = false;


	// The number of abilities of this enemy
	private int nofAbilities;

	// Stores all abilitiy scripts
	private ability[] abilities = new ability[8];

	// Stores the ability GameObjects
	private GameObject[] abilityObjects = new GameObject[8];

	// Stores a reference to the currently active shield
	public ability shieldInUse;

	// Stores the environment the enemy blob currently resides in
	public hazardousEnvironment currentEnvironment;

	// Stores the environment, the enemy blob was previously in
	private hazardousEnvironment previousEnvironment;

	// Saves the last known spot on the map without environmental hazards
	private Vector3 lastSecureSpot;

	private bool closeToEnvironmentBoundary;

	public Color defaultColor = new Color(0.75f,0.0f,0.0f);

	public Material defaultMaterial;

	public bool dead;
	// Defines how much of the damage is due to the player
	public float scoreFraction;

	private float originalSize;

	private float totalDamageByPlayer;

	private bool didSeePlayer;

	public bool hostile;

	private dangerLight dangerLight;



	private lootManager lootManager;


	// Use this for initialization
	void Start () 
	{
		// Get an instance of the enemy manager
		enemyMngr = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		// Get pointer to the player GameObject
		player = GameObject.Find("Blob");
		// Get access to the player script
		playerScript = (player)player.GetComponent(typeof(player));
		// Get the loot manager
		lootManager = (lootManager)GameObject.Find ("LootManager").GetComponent (typeof(lootManager));
		// Get the current viewing direction
		viewingDirection = transform.up;
		// Set the idle viewing angle of this enemy
		originalCosViewingAngle = cosViewingAngle;
		Debug.Log ("originalCosViewingAngle = " + originalCosViewingAngle);
		// Store the starting position of this enemy
		originalPosition = transform.position;
		// Store the original viewing range
		originalViewingRange = baseViewingRange + viewingRangeBoost;
		// Set the flags to idle
		resetAllStates ();
		// Set flag to allow movement
		canMove = true;
		// Set the original size
		originalSize = size;

		dangerLight = (dangerLight)this.gameObject.GetComponentInChildren (typeof(dangerLight));

		shrinkSpeed = size;

		Debug.Log ("originalsize = " + originalSize);
	}
	
	// Update is called once per frame
	void Update() 
	{
		// If dead, then don't move anymore
		if (dead)
			return;

		// If enemy has just died, then throw some ability loot
		if (size <= 0) 
		{
			if (hasAbilities()) {
				// Get a random ability
				ability rndAbility = getRandomAbility();
				// Throw it in front of the dying enemy
				Vector3 throwTo = transform.position + viewingDirection*transform.localScale.x;
				lootManager.throwAbilityLoot(rndAbility, 1, transform.position, throwTo);
			}
			// Calculate how much damage was inflicted by player
			scoreFraction = totalDamageByPlayer / originalSize;
			dead = true;
			return;
		}

		shrinkSpeed = Mathf.Max (1.0f,size);
		
		growSpeed = Mathf.Max (0.4f, 0.4f * size);

		// Get the viewing direction
		viewingDirection = transform.up;

		// Calculate vector pointing to the player
		toPlayer = player.transform.position - transform.position;

		// Use eye ability
		if(abilities[7] != null)
			abilities[7].useAbility();
		// Set the original viewing range
		originalViewingRange = baseViewingRange + viewingRangeBoost;

		// Modify viewing angle
		if (isInAlertedStateTimer > 0) {
			if(!isRunningAwayFromPlayer)
			{
				// Reduce active timer
				isInAlertedStateTimer -= Time.deltaTime;
				// After escape from player reduce awareness(viewing angle) only with time
				cosViewingAngle += 0.2f*(originalCosViewingAngle + 1) * (Time.deltaTime/originalInAlertedStateTimer);
			}
		}
		else {
			cosViewingAngle = originalCosViewingAngle;
			isInAlertedState = false;
		}

		// If blinded, then slowly restore viewing range
		if (blinded && blindedTimer > 0) 
		{
			blindedTimer -= Time.deltaTime;
			// Completely blinded for the first half of the blinded duration
			if(blindedTimer <= originalBlindedTimer*0.5)
			{
				// Then restore until full viewing range
				float diff = originalViewingRange - viewingRange;
				viewingRange += 2.0f * diff * (Time.deltaTime/originalBlindedTimer);
			}
		}
		else 
		{
			viewingRange = originalViewingRange;	// Restore viewing range completely
			blinded = false;
		}

		if (transform.localScale.x > player.transform.localScale.x) {
			dangerLight.setToAttack ();
		} else {
			dangerLight.setToFlee ();
		}

		// Move and use abilities if not stunned
		if (!stunned) 
		{
			// Check whether the player can be seen by this enemy
			canSeePlayer = isInViewingRange (player);
			didSeePlayer = canSeePlayer;

			if (canSeePlayer && hostile) {
				// TODO: Camouflage, darkness, scene geometry, etc...

				// Enemy can see player
				if (size > playerScript.size) {

					if (!isHuntingPlayer) {
						// When the hunt begins, mark the starting position as originalPosition
						originalPosition = transform.position;
					}
					// Only hunt, if enemy isn't too far from its original position
					if ((transform.position - originalPosition).magnitude <= activeOperationRadius && size < 2.0f * player.transform.localScale.x) {

						// Do only hunt, if player is not in an environment, that this enemy cannot enter
						// TODO aggressivity level -> some may take the pursuit never the less
						if(playerScript.currentEnvironment == null || hasAbility(playerScript.currentEnvironment.requiredAbility) != -1)
						{
							dangerLight.frequency = 1.0f;
							isHuntingPlayer = true;
							// Increase awareness
							setAlertedState();
							isRunningAwayFromPlayer = false;
							resetIdleStates ();
							huntPlayer ();
							return;
						}
						else
						{

						}
					}
					if(size < 2.0f * playerScript.size) {
						dangerLight.frequency = 0.5f;
						isHuntingPlayer = false;
						isRunningAwayFromPlayer = false;
						// Proceed with idle behaviour
					}
					// Stop pursuit
				} 
				else  // Run away
				{
					// Enemy is smaller than player -> run away (forever? or get slower by exhaustion?)
					dangerLight.frequency = 1.0f;
					isRunningAwayFromPlayer = true;
					isInAlertedState = false;
					isHuntingPlayer = false;
					// Set viewing angle to 360° for the duration of the escape
					cosViewingAngle = -1;
					resetIdleStates ();
					runAwayFromPlayer ();
					return;
				}
			}

			// Enemy cannot see player


			// If the enemy was hunting the player, then wait a few seconds before walking back to the original position
			if (isHuntingPlayer) 
			{
				// If we have just lost sight of the player in the previous frame, then keep the awareness up for some time
				if (!isInAlertedState) {
					setAlertedState();
					return;
				} else {
					// In the next frame, we continue idle behaviour (but still in alerted state)
					performIdleBehaviour ();
					if (isInAlertedStateTimer <= 0.0f) {
						// Idle wait is over. Walk back to original position
						isHuntingPlayer = false;
						// Set walking target to iriginal position of enemy
						idleWalkingTarget = originalPosition;
						// Calculate target viewing direction
						idleRotationTarget = (idleWalkingTarget - transform.position).normalized;
						// Calculate angle between current viewing direction and target viewing direction
						float angleBetween = Mathf.Sign (Vector3.Cross (viewingDirection, idleRotationTarget).z) * Vector3.Angle (viewingDirection, idleRotationTarget);
						// Calculate rotation target quaternion
						idleRotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, transform.localEulerAngles.z + angleBetween));
						// Set animation in progress
						isIdleWalking = true;
						// Set animation to incomplete
						isIdleAnimationComplete = false;
					}
				}
			} else if (isRunningAwayFromPlayer) {
				// Enemy can no longer see player hunting him - go back to idle but remain alert for a certain time
				isRunningAwayFromPlayer = false;
				// Increase enemy's awareness
				setAlertedState();
				// End of pursuit is enemy's new home
				originalPosition = transform.position;
			} else {
				performIdleBehaviour ();
			}

		} else {
			stunnedTimer -= Time.deltaTime;
			if(stunnedTimer <= 0.0f)
				stunned = false;
		}

		// Apply environmental factors
		transform.position += environmentalPushBack;
		environmentalPushBack = new Vector3 (0, 0, 0);

		transform.position += environmentalCourseCorrection;
		environmentalCourseCorrection = new Vector3 (0, 0, 0);

		if (isRunningAwayFromPlayer || isHuntingPlayer) {
			totalDamageByPlayer += environmentalDamage;
		}
		size -= environmentalDamage;
		environmentalDamage = 0.0f;

		environmentalSlowDown = 1.0f;

		if (currentEnvironment == null)
			lastSecureSpot = transform.position;
	}

	void LateUpdate() {
		// Adapt visuals to the actual size
		size = Mathf.Max (0.0f, size);
		grow ();

		previousEnvironment = currentEnvironment;
		currentEnvironment = null;
		environmentProximityData = null;
		previousViewingDirection = viewingDirection;
	}

	// Calculates whether player blob can be seen by this enemy or not TODO for 3D we need to give the rayDirection a value for the 3rd component (height)
	private bool isInViewingRange(GameObject gameObject) 
	{
		Vector3 toGameObject = gameObject.transform.position - transform.position;
		if (toGameObject.magnitude - gameObject.transform.localScale.x < transform.localScale.x + viewingRange) {
			bool inViewingAngle = Vector3.Dot (toGameObject.normalized, viewingDirection) >= cosViewingAngle;
			if(!inViewingAngle) {
				RaycastHit hit;
				// Find vector pointing along the maxViewingAngle cone
				Quaternion quat = Quaternion.AngleAxis(Mathf.Sign (Vector3.Cross(toGameObject,viewingDirection).z) * Mathf.Acos(cosViewingAngle)*Mathf.Rad2Deg, new Vector3(0,0,-1));
				Vector3 rayDirection = quat * viewingDirection;
				// Check whether the ray hits an object
				if(Physics.Raycast(transform.position, rayDirection, out hit, toGameObject.magnitude - gameObject.transform.localScale.x))
				{
					if(hit.collider.gameObject == gameObject) {
						return true;
					}
				}
			}
			return inViewingAngle;
		} else {
			return false;
		}
	}

	// Function called, when the blob is hunting the player
	private void huntPlayer() 
	{
		// Decide on a shield to use if any
		float[] useShieldProbabilities = new float[2];
		for (int i = 4; i < 6; i++) {
			useShieldProbabilities[i-4] = (abilities[i] != null) ? abilities[i].calculateUseProbability(playerScript, toPlayer, isHuntingPlayer, canSeePlayer) : 0.0f;
		}
		int mostProbableShieldIndex = -1;
		float maxShieldUseProbability = 0.0f;
		for (int i = 0; i < 2; i++) {
			if(useShieldProbabilities[i] > maxShieldUseProbability) {
				mostProbableShieldIndex = i+4;
				maxShieldUseProbability = useShieldProbabilities[i];
			}
		}
		if (mostProbableShieldIndex != -1) {
			Debug.Log ("Use Shield " + abilities[mostProbableShieldIndex].abilityName);
			abilities [mostProbableShieldIndex].useAbility ();
		}


		if (canMove) 
		{
			// Use running
			useRunning();

			// Decide on an attack ability to use if any
			float[] useProbabilities = new float[4];
			for (int i = 0; i < 4; i++) {
				useProbabilities[i] = (abilities[i] != null) ? abilities[i].calculateUseProbability(playerScript, toPlayer, isHuntingPlayer, canSeePlayer) : -1.0f;
			}
			int mostProbableIndex = -1;
			float maxUseProbability = 0.0f;
			for (int i = 0; i < 4; i++) {
				if(useProbabilities[i] > maxUseProbability) {
					mostProbableIndex = i;
					maxUseProbability = useProbabilities[i];
				}
			}
			if(mostProbableIndex != -1) {
				Debug.Log ("Use Ability " + abilities[mostProbableIndex].abilityName);
				abilities[mostProbableIndex].useAbility();
			}

			if (environmentProximityData != null || currentEnvironment != null) {
				bool carryOnAsUsual = avoidEnvironmentalHazard();
				if(!carryOnAsUsual)
					return;
			}

			// Calculate rotation target (the viewing direction this enemy strives for, i.e. towards player)
			Vector3 rotationTarget = toPlayer.normalized;
			// Perform rotation towards player
			performRotation(rotationTarget);
			// Run towards player
			transform.position += viewingDirection * currentSpeed * Time.deltaTime;
		}
	}

	// Function called when this blob is running away from the player
	private void runAwayFromPlayer() 
	{
		if (stunned)
			return;

		if (canMove) 
		{
			// Use running
			useRunning();

			if (environmentProximityData != null || currentEnvironment != null) {
				bool carryOnAsUsual = avoidEnvironmentalHazard();
				Debug.Log ("carryOnAsUsual = " + carryOnAsUsual);
				if(!carryOnAsUsual)
					return;
			}

			// Calculate rotation target (the target viewing direction, i.e. look away from player)
			Vector3 rotationTarget = -toPlayer.normalized;
			// Perform rotation away from the player
			performRotation (rotationTarget);
			// Run away from player
			transform.position += viewingDirection * currentSpeed * Time.deltaTime;


		} else if (!stunned) {

		}
	}

	// Function called to evade environmental hazards
	// Returns true, if calling procedure can act as usual. Returns false, if calling procedure should not perform any additional movements.
	private bool avoidEnvironmentalHazard()
	{
		// If we have just entered a dangerous environment try to get out again unless you're on the run
		if (closeToEnvironmentBoundary || (currentEnvironment != null && previousEnvironment == null)) {
			Debug.Log ("Just entered hazardous environment. Turn around");

			if (currentEnvironment != null) {
				int abilityIndex = hasAbility (currentEnvironment.requiredAbility);
				if (abilityIndex != -1) {
					abilities [abilityIndex].useAbility ();
					// If we have the ability and are hunted, then don't turn around!
					if (isRunningAwayFromPlayer || isHuntingPlayer)
						return true;
				}

			}

			if (!closeToEnvironmentBoundary) {
				// Initialize the rotation
				idleRotationTarget = -viewingDirection;
				closeToEnvironmentBoundary = true;
				return false;
			} else {
				if (currentEnvironment == null) {
					// We safely got out
					closeToEnvironmentBoundary = false;
					resetIdleStates();
					return true;	
				} else {
					// Perform rotation and moving with max speed
					useRunning();
					performRotation (idleRotationTarget);
					transform.position += viewingDirection * currentSpeed * Time.deltaTime;
					return false;
				}
			}
		} 
		// If we are stuck in a hazardous environment, then try to get our with max speed
		else if (currentEnvironment != null && previousEnvironment != null) {
			int abilityIndex = hasAbility (currentEnvironment.requiredAbility);
			if (abilityIndex != -1)
				abilities [abilityIndex].useAbility ();

			// If we are hunting or running away, then simply carry on as usual
			if (isHuntingPlayer || isRunningAwayFromPlayer) {
				return true;
				// Or give up hunt
			}

			// If we are in idle, then move straight on to get out
			Debug.Log ("Inside hazardous environment. Get out straight on");
			// Perform rotation and moving with max speed
			// Perform rotation and moving with max speed
			useRunning();
			transform.position += viewingDirection * currentSpeed * Time.deltaTime;
			return false;
		} else if (currentEnvironment == null && previousEnvironment != null) {
			// We got out safely
			resetIdleStates();
			return true;
		}
		else  // If we are close to a boundary, but not inside, then:
		{
			if(isRunningAwayFromPlayer || isHuntingPlayer)
			{
				int abilityIndex = hasAbility(environmentProximityData.requiredAbility);
				if(abilityIndex != -1) 
				{
					abilities[abilityIndex].useAbility();
					if(isRunningAwayFromPlayer) 
					{
						// turn towards environment to hide in there
						transform.position += Time.deltaTime * currentSpeed * viewingDirection;
						performRotation (environmentProximityData.getClosestDirection().normalized);
						return false;
					}
					else // If is hunting player, then carry on as usual
					{
						return true;
					}
				}

			}

			// Get a vector that points away from the dangerous object
			Vector3 safestDirection = environmentProximityData.getSafestDirection();
			Vector3 rotationTarget = (safestDirection + viewingDirection).normalized;
			Vector3 closestObstacle = environmentProximityData.getClosestDirection();
			float distance = closestObstacle.magnitude;
			// We should be able to perform half the rotation (i.e. 90°) within one second
			float angleBetween = Mathf.Sign (Vector3.Cross (viewingDirection, rotationTarget).z) * Vector3.Angle (viewingDirection, rotationTarget);
			float rotationDampening = Mathf.Deg2Rad*(angleBetween / currentSpeed);

			Debug.Log ("Rotate towards " + rotationTarget);

			// If the safest direction is along the viewingDirection, then no special rotation is required - simply walk on
			if (Vector3.Dot (closestObstacle, viewingDirection) <= 0.1f /*1.0f - Vector3.Dot (rotationTarget, viewingDirection) <= 0.01f*/) {
				transform.position += Time.deltaTime * currentSpeed * viewingDirection;
				if(closestObstacle.magnitude < 0.5*transform.localScale.x)
					// Allow free movement if not too close to the object
					return false;
				else
					return true;
			}
			else
			{
				// Perform the rotation
				transform.position += Time.deltaTime * currentSpeed * viewingDirection;
			//	transform.Rotate (new Vector3(0,0,angleBetween)*Time.deltaTime);
				performRotation (rotationTarget);
				return false;
			}

		}
	}

	// Performs a rotation around the z-axis, such that the blob will eventually look into 'targetViewingDirection'
	private void performRotation(Vector3 targetViewingDirection, float rotationDampener = 1.0f)
	{
		// Get the angle between the current viewing direction and the target viewing direction
		float angleBetween = Mathf.Sign (Vector3.Cross (viewingDirection, targetViewingDirection).z) * Vector3.Angle (viewingDirection, targetViewingDirection);

		if (isHuntingPlayer || isRunningAwayFromPlayer) {
			// Perform a quicker rotation in active mode

			// Target rotation Quaternion
			Quaternion rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, transform.localEulerAngles.z + angleBetween));
			// Interpolate rotation for the current time step
			transform.rotation = Quaternion.Slerp (transform.rotation, rotationTargetQuaternion, Time.deltaTime * currentSpeed * rotationDampener);
		} 
		else 
		{
			// Perform rotation within 1 second
			transform.Rotate (new Vector3(0,0,angleBetween)*Time.deltaTime);
		}
	}



	// This function is called when the player is not in reach and lets the enemy move around randomly
	private void performIdleBehaviour()
	{
		// Reduce the blob's speed for idle behaviour
		currentSpeed = (baseVelocity + runVelocityBoost)*idleSpeedReduction*environmentalSlowDown;

		dangerLight.frequency = 0.5f;

		// If enemy has walked/turned close to a dangerous environment, then turn away from this object
		if (environmentProximityData != null  || currentEnvironment != null) {
			bool carryOnAsUsual = avoidEnvironmentalHazard();
			Debug.Log ("carryOnAsUsual = " + carryOnAsUsual);
			if(!carryOnAsUsual)
				return;
		}

		if (isIdleAnimationComplete) {
			// If the idle animation is complete, then define the next random move or rotation
			float rndValue = Random.value;

			// Rotate or walk or stand still
			if (rndValue > 0.7) 
			{
				// Rotate at position
				findIdleRotationTarget();	
				isIdleRotating = true;
			} 
			else if (rndValue > 0.2) 
			{
				// Walk and rotate randomly
				findIdleWalkingRotation();
				idleTimer = Random.Range (2.0f,4.0f);
				isIdleWalking = true;
			} 
			else 
			{
				// Stand still
				idleTimer = Random.Range(0.5f,1.0f);
				isIdleWaiting = true;
			}
			isIdleAnimationComplete = false;
		} 
		else 	// Continue animation
		{
			if(isIdleWaiting) 
			{
				// Stop standing still animation after timer runs out.
				idleTimer -= Time.deltaTime;
				if(idleTimer <= 0.0f) {
					resetIdleStates();
				}
				return;
			}
			else if(isIdleRotating) {

				// Stop rotation animation, if we have reached the target value
				if(Mathf.Abs(Vector3.Dot(viewingDirection, idleRotationTarget) - 1.0f) <= 0.01) {
					resetIdleStates();
					return;
				} 
				else 	// Continue rotation animation
				{
					// Calculate the rotation matrix for current timestep and apply it to the model
					transform.rotation = Quaternion.Slerp(transform.rotation, idleRotationTargetQuaternion, Time.deltaTime * Mathf.Min(1.0f,currentSpeed));
					return;
				}

			}
			else if(isIdleWalking) {

				idleTimer -= Time.deltaTime;
				if(idleTimer <= 0) {
					resetIdleStates();
					return;
				}
				else
				{
					transform.position += viewingDirection * currentSpeed * Time.deltaTime;
					if(Mathf.Abs (Vector3.Dot (viewingDirection,idleRotationTarget) - 1.0f) > Mathf.Abs (Vector3.Dot (previousViewingDirection,idleRotationTarget) - 1.0f)) {
					    findIdleWalkingRotation();
						Debug.Log ("New idle walking rotation due to correction");
					}
					else if(Mathf.Abs (Vector3.Dot (viewingDirection,idleRotationTarget) - 1.0f) <= 0.01)
						findIdleWalkingRotation();
					else
						transform.rotation = Quaternion.Slerp(transform.rotation, idleRotationTargetQuaternion, Time.deltaTime * currentSpeed *0.5f);
						
				}
			}
			else
			{
				// Something went wrong
				resetIdleStates();
			}
		}
	}

	// Decide what happens when this blob collides with another structure
	void OnTriggerEnter(Collider other)
	{
		// If 2 enemies collide during idle action, let them search a new idle target
		if(other.GetComponent(typeof(enemy)))
			if (!isIdleAnimationComplete) {
			//	resetIdleStates();
				return;
			}
	}

	// Sets a random walking target for this enemy
	private void findIdleWalkingTarget()
	{
		// Get random point in unit circle
		Vector2 rnd2D = Random.insideUnitCircle;
		// Transform random value into point within the idle operation radius
		idleWalkingTarget = originalPosition + new Vector3(rnd2D.x, rnd2D.y, 0.0f)*idleOperationRadius;
		// TODO Check that it is sufficiently far away

		// Calculate target viewing direction
		idleRotationTarget = (idleWalkingTarget - transform.position).normalized;
		// Calculate angle between current viewing direction and target viewing direction
		float angleBetween = Mathf.Sign (Vector3.Cross(viewingDirection, idleRotationTarget).z)*Vector3.Angle(viewingDirection, idleRotationTarget);
		// Calculate rotation target quaternion
		idleRotationTargetQuaternion = Quaternion.Euler(new Vector3(0.0f,0.0f,transform.localEulerAngles.z + angleBetween));
	}

	private void findIdleWalkingRotation()
	{
		// Get random rotation angle
		float degree = Random.Range(10.0f,90.0f)*Mathf.Sign (Random.Range(-1,1));
		// Calculate target theta on unit circle (+90 since unit circe starts at (1,0) and Unity at (0,1))
		float targetTheta = (transform.localEulerAngles.z + 90.0f + degree)*Mathf.Deg2Rad;
		// Calculate target viewing direction (there where the rotation will end)
		idleRotationTarget = new Vector3(Mathf.Cos(targetTheta), Mathf.Sin(targetTheta),0.0f);
		// Calculate target quaternion configuration (for Slerp)
		idleRotationTargetQuaternion = Quaternion.Euler(new Vector3(0.0f,0.0f,transform.localEulerAngles.z+degree));
	}

	// Sets a random rotation target for this enemy
	private void findIdleRotationTarget()
	{
		// Get random rotation angle
		float degree = Random.Range(30.0f,179.0f)*Mathf.Sign (Random.Range(-1,1));
		// Calculate target theta on unit circle (+90 since unit circe starts at (1,0) and Unity at (0,1))
		float targetTheta = (transform.localEulerAngles.z + 90.0f + degree)*Mathf.Deg2Rad;
		// Calculate target viewing direction (there where the rotation will end)
		idleRotationTarget = new Vector3(Mathf.Cos(targetTheta), Mathf.Sin(targetTheta),0.0f);
		// Calculate target quaternion configuration (for Slerp)
		idleRotationTargetQuaternion = Quaternion.Euler(new Vector3(0.0f,0.0f,transform.localEulerAngles.z+degree));
	}

	// Resets all idle states of this enemy to default
	private void resetIdleStates() {
		isIdleWaiting = false;
		isIdleWalking = false;
		isIdleRotating = false;
		isIdleAnimationComplete = true;
	}

	// Resets all states of this enemy to default
	public void resetAllStates() {
		isIdleWaiting = false;
		isIdleWalking = false;
		isIdleRotating = false;
		isIdleAnimationComplete = true;
		isHuntingPlayer = false;
		isRunningAwayFromPlayer = false;
		isInAlertedState = false;
		canMove = true;
		stunned = false;
		dead = false;
		totalDamageByPlayer = 0;
		originalSize = size;
		// Make sure ability meshes have the correct size
		for (int i = 0; i < 8; i++) {
			if(abilities[i]) {
				abilities[i].resetTransform();
			}
		}
	}

	private void useRunning() {
		if (abilities [6]) {
			abilities[6].useAbility();
		}
		currentSpeed = (baseVelocity + runVelocityBoost)*environmentalSlowDown;
	}

	public void addAbility(GameObject ability, int slot)
	{
		if (abilities [slot] != null) {
			removeAndDestroyAbility(slot);
		}
		nofAbilities++;
		abilityObjects [slot] = ability;
		abilities[slot] = (ability)abilityObjects [slot].GetComponent (typeof(ability));
	}
	
	public void removeAndDestroyAbility(int slot) {
		if (abilities [slot] != null) {
			nofAbilities--;
			abilities [slot] = null;
			GameObject.Destroy (abilityObjects [slot].gameObject);
		}
	}

	public void removeAbility(int slot) {
		if (abilities [slot] != null) {
			nofAbilities--;
			abilities [slot] = null;
			abilityObjects [slot] = null;
		}
	}

	// Returns a bool indicating whether this enemy has any abilities
	public bool hasAbilities()
	{
		return nofAbilities != 0;
	}

	// Returns a random ability of this enemy (for the case when the player eats this enemy)
	public ability getRandomAbility()
	{
		if (nofAbilities > 0) 
		{
			Debug.Log (nofAbilities);
			int index = Random.Range (0, nofAbilities);
			int counter = 0;
			for(int i = 0; i < 8; i++) {
				if(abilities[i] != null) {
					if(counter == index)
						return abilities[i];
					else 
						counter++;
				}
			}
		} 
		// else
		return null;
	}

	// Returns -1 when the player does not have this ability and otherwise the index to where this ability resides in the ability array
	public int hasAbility(EAbilityType abilityType)
	{
		if (nofAbilities == 0)
			return -1;

		for (int i = 0; i < abilities.Length; i++) {
			if(abilities[i] != null && abilities[i].getAbilityEnum() == abilityType)
				return i;
		}
		return -1;
	}

	public void getShieldAbilities(out ability shield0, out ability shield1 ) {
		shield0 = abilities[4];
		shield1 = abilities[5];
	}

	// Sets this enemy into alerted state
	public void setAlertedState() 
	{
		cosViewingAngle = -1;
		isInAlertedStateTimer = Random.Range (3.0f, 8.0f);
		originalInAlertedStateTimer = isInAlertedStateTimer;
		isInAlertedState = true;
	}

	// Sets this enemy to blinded, which reduces the enemy's viewing range considerably
	public void setBlinded(float time) 
	{
		viewingRange = 0;
		blindedTimer = time;
		originalBlindedTimer = time;
		blinded = true;
	}

	// Makes this enemy blob unable to act for the specified time
	public void setStunned(float time) {
		// TODO make stun visible
		stunned = true;
		stunnedTimer = time;
	}

	// Makes the blob grow smoothly
	private void grow()
	{
		float currentSize = transform.localScale.x;
		float diff = size - currentSize;
		if (diff < 0) 
		{
			// Shrinking
			float nextSize = Mathf.Max(size,currentSize - Time.deltaTime*shrinkSpeed*Mathf.Max (1.0f,-diff));
			transform.localScale = new Vector3(nextSize, nextSize, nextSize);
		}
		else
		{
			// Growing
			float nextSize = Mathf.Min(size,currentSize + Time.deltaTime*growSpeed);
			transform.localScale = new Vector3(nextSize, nextSize, nextSize);
		}
	}

	// Forces this blob, to use this ability
	public void activateAbility(int abilityIndex) 
	{
		abilities [abilityIndex].useAbility ();
	}

	// Inflict damage induced by the current environment
	public void inflictEnvironmentalDamage(float damage)
	{
		environmentalDamage = Mathf.Max (environmentalDamage,damage);
	}

	public void applyEnvironmentalSlowDown(float slowDown)
	{
		environmentalSlowDown = slowDown;
	}

	// Add force induced by hazardous environment
	public void addEnvironmentPushBackForce(Vector3 force)
	{
		if (force.magnitude > environmentalPushBack.magnitude)
			environmentalPushBack = force;
	}

	// Add an external force to the enemy position
	public void addCourseCorrection(Vector3 correction)
	{
		environmentalCourseCorrection += correction;
	}

	// Called when the player eats this enemy blob
	public void eat()
	{
		totalDamageByPlayer += size;
		size = 0;
	}

	// Called when player's attacks hit this enemy
	public void inflictAbilityDamage(float damage) 
	{
		if (damage > 0) 
		{
			totalDamageByPlayer += damage;

			setAlertedState();
			// We can only inflict as much damage as the enemy has health
			damage = Mathf.Min (damage,size);
			// For each 10% of size lost through the attack, there is some chance that the enmy drops loot
			int nofLootChances = (int)(damage / (0.1f*size));
			// Each loot would have the following size
			float sizePerLoot = damage / nofLootChances;

			for(int i = 0; i < nofLootChances; i++)
			{
				float rnd = Random.value;
				// 50% chance for loot
				if(rnd > 0.5f) 
				{
					if(rnd > 0.75 || !hasAbilities()) 
					{
						// Throw sizeLoot
						float radius = Random.Range(1.0f,2.5f)*transform.localScale.x;
						float theta = Random.Range(0.0f,2.0f*Mathf.PI);
						Vector3 throwTo = transform.position + new Vector3(radius*Mathf.Cos(theta), radius*Mathf.Sin (theta), 0);
						lootManager.throwSizeLoot(sizePerLoot*Random.Range (0.2f,0.8f),transform.position,throwTo);
					}
					else
					{
						// Throw ability loot
						ability rndAbility = getRandomAbility();
						float radius = Random.Range(1.0f,2.5f)*transform.localScale.x;
						float theta = Random.Range(0.0f,2.0f*Mathf.PI);
						Vector3 throwTo = transform.position + new Vector3(radius*Mathf.Cos(theta), radius*Mathf.Sin (theta), 0);

						lootManager.throwAbilityLoot(rndAbility, 1, transform.position, throwTo);
						rndAbility.increaseLevel(-(int)(0.1f*rndAbility.maxLevel));

						// Probability that enemy looses that ability
						float loseChance = Random.value;
						if(loseChance > 0.9) {
							// TODO Play sound or something
							removeAndDestroyAbility(hasAbility(rndAbility.abilityEnum));
						}
					}
				}
			}
		}
		size -= damage;
	}

}
