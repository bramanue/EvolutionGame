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


	// Use this for initialization
	void Start () 
	{
		// Get an instance of the enemy manager
		enemyMngr = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		// Get pointer to the player GameObject
		player = GameObject.Find("Blob");
		// Get access to the player script
		playerScript = (player)player.GetComponent(typeof(player));
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
	}
	
	// Update is called once per frame
	void Update() 
	{
		// If this blob is dead, then don't move anymore
		if (size <= 0)
			return;

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
			// Reduce active timer
			isInAlertedStateTimer -= Time.deltaTime;
			// After escape from player reduce awareness(viewing angle) only with time
			cosViewingAngle += (originalCosViewingAngle + 1) * (Time.deltaTime/originalInAlertedStateTimer);
		}
		else {
			cosViewingAngle = originalCosViewingAngle;
			isInAlertedState = false;
		}

		// If blinded, then slowly restore viewing range
		if (blinded && blindedTimer > 0) {
			blindedTimer -= Time.deltaTime;
			// Completely blinded for the first half of the blinded duration
			if(blindedTimer <= originalBlindedTimer*0.5)
			{
				// Then restore until full viewing range
				float diff = originalViewingRange - viewingRange;
				viewingRange += 2.0f * diff * (Time.deltaTime/originalBlindedTimer);
			}
		}
		else {
			viewingRange = originalViewingRange;	// Restore viewing range completely
			blinded = false;
		}

		// Calculate the current maximally achievable speed
		currentSpeed = (baseVelocity + runVelocityBoost)*environmentalSlowDown;

		// Move and use abilities if not stunned
		if (!stunned) {

			// Check whether the player can be seen by this enemy
			canSeePlayer = isInViewingRange (player);

			if (canSeePlayer) {
				// TODO: Camouflage, darkness, scene geometry, etc...

				// Enemy can see player
				Debug.Log ("Player spotted");
				if (size > playerScript.size) {

					if (!isHuntingPlayer) {
						Debug.Log ("Begin hunt -> set new originalPosition");
						// When the hunt begins, mark the starting position as originalPosition
						originalPosition = transform.position;
					}
					// Only hunt, if enemy isn't too far from its original position
					if ((transform.position - originalPosition).magnitude <= activeOperationRadius && size < 2.0f * player.transform.localScale.x) {

						// Do only hunt, if player is not in an environment, that this enemy cannot enter
						// TODO aggressivity level -> some may take the pursuit never the less
						if(playerScript.currentEnvironment == null || hasAbility(playerScript.currentEnvironment.requiredAbility) != -1)
						{
							Debug.Log ("Player is in empty environment or enemy has required ability");
							// Use running
							if(abilities[6] != null)
								abilities[6].useAbility();

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
						isHuntingPlayer = false;
						isRunningAwayFromPlayer = false;
						resetIdleStates ();
						// Proceed with idle behaviour
					}
					// Stop pursuit
				} 
				else  // Run away
				{
					// Use running
					if(abilities[6] != null)
						abilities[6].useAbility();
					// Enemy is smaller than player -> run away (forever? or get slower by exhaustion?)
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

		size -= environmentalDamage;
		environmentalDamage = 0.0f;

		environmentalSlowDown = 1.0f;

		if (currentEnvironment == null)
			lastSecureSpot = transform.position;
	}

	void LateUpdate() {
		// Adapt visuals to the actual size
		grow ();

		previousEnvironment = currentEnvironment;
		currentEnvironment = null;
		environmentProximityData = null;
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

			// Calculate rotation target (the viewing direction this enemy strives for, i.e. towards player)
			Vector3 rotationTarget = toPlayer.normalized;
			// Perform rotation towards player
			performRotation(rotationTarget);
			// Run towards player
			transform.position += viewingDirection * currentSpeed * Time.deltaTime;
			Debug.Log ("Run towards player");
		}
	}

	// Function called when this blob is running away from the player
	private void runAwayFromPlayer() 
	{
		if (canMove) {
			if (environmentProximityData != null) {
				avoidEnvironmentalHazard ();
			} else {
				// Calculate rotation target (the target viewing direction, i.e. look away from player)
				Vector3 rotationTarget = -toPlayer.normalized;
				// Perform rotation away from the player
				performRotation (rotationTarget);
				// Run away from player
				transform.position += viewingDirection * currentSpeed * Time.deltaTime;
			}

		} else if (!stunned) {

		}
	}

	// Function called to evade environmental hazards
	private void avoidEnvironmentalHazard()
	{
		// If we have just entered a dangerous environment try to get out again
		if (closeToEnvironmentBoundary || (currentEnvironment != null && previousEnvironment == null)) 
		{
			Debug.Log ("Just entered hazardous environment. Turn around");

			if(currentEnvironment != null)
			{
				int abilityIndex = hasAbility(currentEnvironment.requiredAbility);
				if(abilityIndex != -1)
					abilities[abilityIndex].useAbility();
			}

			if (!closeToEnvironmentBoundary) {
				// Initialize the rotation
				idleRotationTarget = -viewingDirection;
				closeToEnvironmentBoundary = true;
				return;
			} 
			else 
			{
				if (currentEnvironment == null){
					// We safely got out
					closeToEnvironmentBoundary = false;
					return;	
				} else {
					// Perform rotation and moving with max speed
					currentSpeed = (baseVelocity + runVelocityBoost)*environmentalSlowDown;
					performRotation (idleRotationTarget);
					transform.position += viewingDirection * currentSpeed * Time.deltaTime;
					return;
				}
			}
		} 
		// If we are stuck in a hazardous environment, then try to get our with max speed
		else if (currentEnvironment != null && previousEnvironment != null) 
		{
			int abilityIndex = hasAbility(currentEnvironment.requiredAbility);
			if(abilityIndex != -1)
				abilities[abilityIndex].useAbility();

			Debug.Log ("Inside hazardous environment. Get out straight on");
			// Perform rotation and moving with max speed
			currentSpeed = (baseVelocity + runVelocityBoost)*environmentalSlowDown;
			transform.position += viewingDirection * currentSpeed * Time.deltaTime;
			return;
		}
		else  // If we are close to a boundary, but not inside, then:
			  // Idle: try to avoid the environmental hazard
			  // Hunting: enter, if ability available, stop if not available
			  // Running: enter, if ability available, evade if not available
		{
			if(isRunningAwayFromPlayer || isHuntingPlayer)
			{
				int abilityIndex = hasAbility(environmentProximityData.requiredAbility);
				if(abilityIndex != -1) {
					abilities[abilityIndex].useAbility();
					return;
				}
			}
			if(!isHuntingPlayer)
			{
				// Get a vector that points away from the dangerous object
				Vector3 rotationTarget = environmentProximityData.getSafestDirection();
				Vector3 closestObstacle = environmentProximityData.getClosestDirection();
				float distance = closestObstacle.magnitude;
				float rotationDampening = Mathf.Min(1.0f, 1.0f - distance/(2.0f*currentSpeed));
				Debug.Log ("Rotate towards " + rotationTarget);
				//	Debug.Log ("Towards" + rotationTarget);
				transform.position += Time.deltaTime * currentSpeed * viewingDirection;
				// If the safest direction is along the viewingDirection, then no special rotation is required - simply walk on
				if (Mathf.Abs (Vector3.Dot (rotationTarget, viewingDirection)) <= 0.01f /*1.0f - Vector3.Dot (rotationTarget, viewingDirection) <= 0.01f*/) {
					return;
				}
				else
				{
					// Perform the rotation
					performRotation (rotationTarget,rotationDampening);
					return;
				}
			}
			else  // If is hunting player
			{


			}
		}
	}

	// Performs a rotation around the z-axis, such that the blob will eventually look into 'targetViewingDirection'
	private void performRotation(Vector3 targetViewingDirection, float rotationDampener = 1.0f)
	{
		// Get the angle between the current viewing direction and the target viewing direction
		float angleBetween = Mathf.Sign (Vector3.Cross (viewingDirection, targetViewingDirection).z) * Vector3.Angle (viewingDirection, targetViewingDirection);
		// Target rotation Quaternion
		Quaternion rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, transform.localEulerAngles.z + angleBetween));
		// Interpolate rotation for the current time step
		transform.rotation = Quaternion.Slerp (transform.rotation, rotationTargetQuaternion, Time.deltaTime * Mathf.Min (currentSpeed,1.0f) * rotationDampener);
	}


	// This function is called when the player is not in reach and lets the enemy move around randomly
	private void performIdleBehaviour()
	{
		// Reduce the blob's speed for idle behaviour
		currentSpeed *= idleSpeedReduction;

		// If enemy has walked/turned close to a dangerous environment, then turn away from this object
		if (environmentProximityData != null) {
		//	resetIdleStates();
			avoidEnvironmentalHazard();
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
				Debug.Log ("Rotate");
			} 
			else if (rndValue > 0.2) 
			{
				// Rotate and walk towards point
				findIdleWalkingTarget();
				Debug.Log ("Walk to " + idleWalkingTarget);
			} 
			else 
			{
				// Stand still
				idleTimer = Random.Range(0.5f,3.5f);
				isIdleWaiting = true;
				Debug.Log ("Wait");
			}
			isIdleAnimationComplete = false;
		} 
		else 	// Continue animation
		{
			if(isIdleWaiting) {

				// Stop standing still animation after timer runs out.
				idleTimer -= Time.deltaTime;
				if(idleTimer <= 0.0f) {
					isIdleAnimationComplete = true;
					isIdleWaiting = false;
				}
				return;
			}
			else if(isIdleRotating) {

				// Stop rotation animation, if we have reached the target value
				if(Mathf.Abs(Vector3.Dot(viewingDirection, idleRotationTarget) - 1.0f) <= 0.01) {
					isIdleRotating = false;
					isIdleAnimationComplete = true;
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

				// Get the vector towards the target location
				Vector3 toTargetLocation = idleWalkingTarget - transform.position;
				// Stop walking, if we have reached the target location (up to a certain precision)
				if(toTargetLocation.magnitude <= size ) {
					isIdleWalking = false;
					isIdleAnimationComplete = true;
					Debug.Log ("Target reached");
					return;
				} else {
					// Calculate rotation target (the target viewing direction, s.t. enemy looks towards target location)
					Vector3 rotationTarget = toTargetLocation.normalized;
					// Perform the rotation
					performRotation(rotationTarget);
					// Walk along viewing direction (and eventually towards target location)
					transform.position += viewingDirection * currentSpeed * Time.deltaTime;
				}
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
		// Set animation in progress
		isIdleWalking = true;
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
		// Set animation in progress
		isIdleRotating = true;
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
	}

	public void addAbility(GameObject ability, int slot)
	{
		nofAbilities++;
		if (abilities [slot] != null) {
			removeAndDestroyAbility(slot);
		}
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
		if (nofAbilities > 0) {
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

}
