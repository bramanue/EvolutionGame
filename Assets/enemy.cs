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

	// Defines how far this enemy can see
	public float viewingRange;

	// Stores the original viewing range
	public float originalViewingRange;

	// Defines how wide the angle of the enemy's eyes are 
	public float cosViewingAngle;
	
	// Stores the original cosViewingAngle for restoring default after escape procedure
	private float originalCosViewingAngle;


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

	// Defines whether this enemy is at idle or is hunting the player
	public bool isHuntingPlayer;
	
	// Defines whether this enely is running away from the player
	private bool isRunningAwayFromPlayer;

	// Defines whether enemy is still in alarmed state
	private bool isAfterEscape;

	// Timer used to keep awareness of enemy blob up
	private float isInAlertStateTimer = 0.0f;
	
	// Original timer value for interpolation
	private float originalActiveTimer = 0.0f;


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
		// Store the starting position of this enemy
		originalPosition = transform.position;
		// Store the original viewing range
		originalViewingRange = viewingRange;
		// Set the flags to idle
		resetAllStates ();
	}
	
	// Update is called once per frame
	void Update() 
	{
		// Get the viewing direction
		viewingDirection = transform.up;

		// Calculate vector pointing to the player
		toPlayer = player.transform.position - transform.position;

		// Reduce active timer
		isInAlertStateTimer -= Time.deltaTime;
		if (isAfterEscape && isInAlertStateTimer > 0) {
			// After escape from player reduce awareness(viewing angle) only with time
			cosViewingAngle += (originalCosViewingAngle + 1) * (Time.deltaTime/originalActiveTimer);
		}
		if (isInAlertStateTimer <= 0) {
			if(isAfterEscape) {
				// Reset viewing angle to original state
				cosViewingAngle = originalCosViewingAngle;
				// No longer in after escape status
				isAfterEscape = false;
			}
		}

		// If blinded, then slowly restore viewing range
		if (blinded && blindedTimer > 0) {
			float diff = originalViewingRange - viewingRange;
			viewingRange += 0.5f*diff/(blindedTimer/Time.deltaTime);	// Restore only until have original viewing range
			blindedTimer -= Time.deltaTime;
		}
		else {
			viewingRange = originalViewingRange;	// Restore viewing range
			blinded = false;
		}

		// Move and use abilities if not stunned
		if (!stunned) {

			// Use running
			if(abilities[6] != null)
				abilities[6].useAbility();

			// Calculate the current speed
			currentSpeed = baseVelocity + runVelocityBoost;

			// Check whether the player could be seen by this enemy
			bool isPlayerInViewingRange = isInViewingRange (player);

			if (isPlayerInViewingRange) {
				// TODO: Camouflage, darkness, scene geometry, etc...

				// Enemy can see player

				if (size > playerScript.size) {
					// Enemy is bigger than player (but not too big) -> attack player
					if (!isHuntingPlayer) {
						// When the hunt begins, mark the starting position as originalPosition
						originalPosition = transform.position;
					}
					// Only hunt, if enemy isn't too far from its original position
					if ((transform.position - originalPosition).magnitude <= activeOperationRadius && size < 2.0f * playerScript.size) {
						cosViewingAngle = -1;
						isHuntingPlayer = true;
						isAfterEscape = false;
						isRunningAwayFromPlayer = false;
						resetIdleStates ();
						huntPlayer ();
						return;
					}
					if(size < 2.0f * playerScript.size) {
						isHuntingPlayer = false;
						isAfterEscape = false;
						isRunningAwayFromPlayer = false;
						resetIdleStates ();
						// Proceed with idle behaviour
					}
					// Stop pursuit if too far away
				} else {
					// Enemy is smaller than player -> run away (forever? or get slower by exhaustion?)
					isRunningAwayFromPlayer = true;
					isAfterEscape = false;
					isHuntingPlayer = false;
					// Set viewing angle to 360° for the duration of the escape
					cosViewingAngle = -1;
					resetIdleStates ();
					// TODO look for hiding spots near location
					runAwayFromPlayer ();
					return;
				}
			}

			// Enemy cannot see player

			// Reduce the blob's speed for idle behaviour
			currentSpeed *= idleSpeedReduction;

			// If the enemy was hunting the player, then wait a few seconds before walking back to the original position
			if (isHuntingPlayer) {
				if (!isAfterEscape) {
					isAfterEscape = true;
					// Define for how long the awareness of the blob remains sharpend
					isInAlertStateTimer = Random.Range (3.0f, 6.0f);
					return;
				} else {
					performIdleBehaviour ();
					if (isInAlertStateTimer <= 0.0f) {
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
				// Set timer until when the viewing angle is back to normal
				isInAlertStateTimer = Random.Range (3.0f, 6.0f);
				originalActiveTimer = isInAlertStateTimer;
				originalPosition = transform.position;
				// Set alarm state of enemy higher
				isAfterEscape = true;
			} else {
				// In case that setAlertstate() was called but player was not spotted
				if (cosViewingAngle < originalCosViewingAngle && isInAlertStateTimer <= 0) {
					isAfterEscape = true;
					isInAlertStateTimer = Random.Range (3.0f, 6.0f);
				}
				performIdleBehaviour ();
			}

		} else {
			stunnedTimer -= Time.deltaTime;
			if(stunnedTimer <= 0.0f)
				stunned = false;
		}


		environmentProximityData = null;

		// Apply environmental factors
		transform.position += environmentalPushBack;
		environmentalPushBack = new Vector3 (0, 0, 0);

		transform.position += environmentalCourseCorrection;
		environmentalCourseCorrection = new Vector3 (0, 0, 0);

		size -= environmentalDamage;
		environmentalDamage = 0.0f;
		// Adapt visuals to the actual size
		grow ();
	}

	// Calculates whether player blob can be seen by this enemy or not
	private bool isInViewingRange(GameObject gameObject) {
		Vector3 toGameObject = gameObject.transform.position - transform.position;
		bool result = ((toGameObject.magnitude - gameObject.transform.localScale.x) < viewingRange) && (Vector3.Dot (toGameObject.normalized, viewingDirection) >= cosViewingAngle);
		return result;
	}

	// Function called, when the blob is hunting the player
	private void huntPlayer() 
	{
		abilities[0].useAbility();
		if (canMove) 
		{
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
		if (canMove)
		{
			if (environmentProximityData != null) {
				avoidEnvironmentalHazard();
			}
			else
			{
				// Calculate rotation target (the target viewing direction, i.e. look away from player)
				Vector3 rotationTarget = -toPlayer.normalized;
				// Perform rotation away from the player
				performRotation(rotationTarget);
			}
			// Run away from player
			transform.position += viewingDirection * currentSpeed * Time.deltaTime;
		}
	}

	// Function called to evade environmental hazards
	private void avoidEnvironmentalHazard()
	{
		// Get a vector that points away from the dangerous object
		Vector3 rotationTarget = environmentProximityData.getSafestDirection();
		Debug.Log ("Towards" + rotationTarget);

		// If the safest direction is along the viewingDirection, then no special rotation is required - simply walk on
		if(1.0f - Vector3.Dot (rotationTarget, viewingDirection) <= 0.01f) {
			transform.position += Time.deltaTime*currentSpeed*viewingDirection;
			return;
		}
		// Perform the rotation
		performRotation (rotationTarget);
		return;
	}

	// Performs a rotation around the z-axis, such that the blob will eventually look into 'targetViewingDirection'
	private void performRotation(Vector3 targetViewingDirection)
	{
		// Get the angle between the current viewing direction and the target viewing direction
		float angleBetween = Mathf.Sign (Vector3.Cross (viewingDirection, targetViewingDirection).z) * Vector3.Angle (viewingDirection, targetViewingDirection);
		// Target rotation Quaternion
		Quaternion rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, transform.localEulerAngles.z + angleBetween));
		// Interpolate rotation for the current time step
		transform.rotation = Quaternion.Slerp (transform.rotation, rotationTargetQuaternion, Time.deltaTime * currentSpeed);
	}


	// This function is called when the player is not in reach and lets the enemy move around randomly
	private void performIdleBehaviour()
	{
		// If enemy has walked/turned close to a dangerous environment, then turn away from this object
		if (environmentProximityData != null) {
			resetIdleStates();
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
			} 
			else if (rndValue > 0.2) 
			{
				// Rotate and walk towards point
				findIdleWalkingTarget();
			} 
			else 
			{
				// Stand still
				idleTimer = Random.Range(0.5f,3.5f);
				isIdleWaiting = true;

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
					transform.rotation = Quaternion.Slerp(transform.rotation, idleRotationTargetQuaternion, Time.deltaTime * currentSpeed);
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
		// This is handled in the player script
		if (other.gameObject == player) { 
			// TODO Maybe set alerted state if player bounces in with force
			return;
		}

		// If 2 enemies collide during idle action, let them search a new idle target
		if (!isIdleAnimationComplete) {
			resetIdleStates();
			return;
		}

		// If the enemy is hunting the player and collides with a different enemy, then the smaller enemy
		// gets eaten
		if (isHuntingPlayer) 
		{
			enemy enemyScript = (enemy)other.gameObject.GetComponent(typeof(enemy));
			if(enemyScript && size > enemyScript.size) {
				// Define by how much the player's blob grows
				float growFactor = enemyScript.size / size;
				// Set scaling of the blob
				size += 0.1f*growFactor*growFactor;
				// Reposition enemy
				enemyMngr.respawnEnemy(other.gameObject);
			}
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
		isAfterEscape = false;
		canMove = true;
	}

	// Adds an ability into index 'slot' of the ability array of this enemy
	public void addAbility(GameObject ability, int slot)
	{
		nofAbilities++;
		abilityObjects[slot] = ability;
		abilities[slot] = (ability)abilityObjects[slot].GetComponent(typeof(ability));
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
			return abilities [Random.Range (0, nofAbilities - 1)];
		} else
			return null;
	}

	// Returns -1 when the player does not have this ability and otherwise the index to where this ability resides in the ability array
	public int hasAbility(EAbilityType abilityType)
	{
		for (int i = 0; i < abilities.Length; i++) {
			if(abilities[i] != null && abilities[i].getAbilityEnum() == abilityType)
				return i;
		}
		return -1;
	}

	// Sets this enemy into alerted state
	public void setAlertState() {
		cosViewingAngle = -1;
		isInAlertStateTimer = Random.Range (3.0f, 8.0f);
	}

	// Sets this enemy to blinded, which reduces the enemy's viewing range considerably
	public void setBlinded(float time) {
		if (!blinded) 
		{
			viewingRange = 0;
			blindedTimer = time;
			blinded = true;
		}
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
