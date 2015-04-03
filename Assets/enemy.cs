using UnityEngine;
using System.Collections;

public class enemy : MonoBehaviour {

	// Start position of this enemy. Do not move away too far from this point.
	private Vector3 originalPosition;

	// Viewing direction of this enemy (normalized)
	public Vector3 viewingDirection;

	// Vector that points to the player
	public Vector3 toPlayer;

	// Defines how far this enemy can see
	public float viewingRange;

	// Defines how wide the angle of the enemy's eyes are 
	public float cosViewingAngle;

	// Stores the original cosViewingAngle for restoring default after escape procedure
	private float originalCosViewingAngle;

	// The size of this enemy
	public float size;

	// The maximum velocity of this enemy
	public float maxVelocity ;

	// Defines whether this enemy is allowed to walk
	public bool cannotMove;

	// Pointer to the player
	private GameObject player;

	// Defines whether this enemy is at idle or is hunting the player
	private bool isHuntingPlayer;

	// Defines whether this enely is running away from the player
	private bool isRunningAwayFromPlayer;
	
	// Timer used for idle waiting 
	private float idleTimer = 0.0f;

	// Timer used for hunting, running away, etc
	private float activeTimer = 0.0f;

	// Original timer value for interpolation
	private float originalActiveTimer = 0.0f;

	// The radius in which the enemy operates during idle animations
	public float idleOperationRadius;

	// The radius in which the enemy operates during hunting the player
	public float activeOperationRadius;

	// Defines by which factor velocity should be removed for idle walking actions
	private float idleWalkingSpeedReduction = 0.4f;

	// Defines by which factor velocity should be removed for idle walking actions
	private float idleRotationSpeedReduction = 0.3f;

	// The location, this enemy wants to walk to.
	private Vector3 idleWalkingTarget;

	// The target viewing  direction for some rotation
	private Vector3 idleRotationTarget;

	// The quaternion to which we want to rotate
	private Quaternion idleRotationTargetQuaternion;

	// Defines whether enemy is still in alarmed state
	private bool isAfterEscape = false;

	// Defines whether the current animation is complete
	private bool isIdleAnimationComplete = true;

	// Booleans to define the current idle action
	private bool isIdleRotating = false;
	private bool isIdleWalking = false;
	private bool isIdleWaiting = false;


	private ability[] abilities = new ability[5];
	
	private GameObject[] abilityObjects = new GameObject[5];


	// Enemy manager for performing eating, etc operations
	private enemyManager enemyMngr;


	// Use this for initialization
	void Start () 
	{
		// Get an instance of the enemy manager
		enemyMngr = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		// Get pointer to the player GameObject
		player = GameObject.Find("Blob");
		// Size should already be set by the enemyManager. Fetch it!
		size = transform.localScale.x;
		// Get the theta angle of the current rotation, corresponding to position on the unit circle
		float theta = (transform.localEulerAngles.z+90.0f)/360.0f*2.0f*Mathf.PI;
		// Get the viewing direction based on the current rotation (resp. on the previously calculated theta)
		viewingDirection = new Vector3 (Mathf.Cos (theta), Mathf.Sin (theta), 0.0f);
		// Set the idle viewing angle of this enemy
		originalCosViewingAngle = cosViewingAngle;
		// Store the starting position of this enemy
		originalPosition = transform.position;
		// Set the flags to idle
		isRunningAwayFromPlayer = false;
		isHuntingPlayer = false;
		cannotMove = false;
		unsetIdleState();
	}
	
	// Update is called once per frame
	void Update() 
	{
		// Get the theta angle of the current rotation, corresponding to position on the unit circle
		float theta = (transform.localEulerAngles.z + 90.0f) * Mathf.Deg2Rad;
		// Get the viewing direction based on the current rotation (resp. on the previously calculated theta)
		viewingDirection = new Vector3 (Mathf.Cos (theta), Mathf.Sin (theta), 0.0f);
		// Calculate vector pointing to the player
		toPlayer = player.transform.position - transform.position;

		// Reduce active timer
		activeTimer -= Time.deltaTime;
		if (isAfterEscape && activeTimer > 0) {
			// After escape from player reduce awareness(viewing angle) only with time
			cosViewingAngle += (originalCosViewingAngle + 1) * (Time.deltaTime/originalActiveTimer);
		}
		if (activeTimer <= 0) {
			if(isAfterEscape) {
				// Reset viewing angle to original state
				cosViewingAngle = originalCosViewingAngle;
				// No longer in after escape status
				isAfterEscape = false;
			}
		}

		// Check whether the player could be seen by this enemy
		bool isPlayerInViewingRange = isInViewingRange (player);

		if (isPlayerInViewingRange) 
		{
			// TODO: Camouflage, darkness, scene geometry, etc...

			// Enemy can see player

			if (size >= player.transform.localScale.x) {
				// Enemy is bigger than player (but not too big) -> attack player
				if(!isHuntingPlayer)
				{
					// When the hunt begins, mark the starting position as originalPosition
					originalPosition = transform.position;
				}
				// Only hunt, if enemy isn't too far from its original position
				if((transform.position-originalPosition).magnitude <= activeOperationRadius && size < 2.0f*player.transform.localScale.x) 
				{
					isHuntingPlayer = true;
					isAfterEscape = false;
					isRunningAwayFromPlayer = false;
					unsetIdleState();
					huntPlayer();
					return;
				}
				// Stop pursuit if too far away
			} 
			else  
			{
				// Enemy is smaller than player -> run away (forever? or get slower by exhaustion?)
				isRunningAwayFromPlayer = true;
				isAfterEscape = false;
				isHuntingPlayer = false;
				// Set viewing angle to 360° for the duration of the escape
				cosViewingAngle = -1;
				unsetIdleState();
				// TODO look for hiding spots near location
				runAwayFromPlayer();
				return;
			}
		}

		// Enemy cannot see player
		if(isHuntingPlayer) {
			// If the enemy was hunting the player, then wait a few seconds before walking back to the original position
			if(!isIdleWaiting) {
				idleTimer = Random.Range(2.0f,7.0f);
				isIdleWaiting = true;
				isIdleAnimationComplete = false;
				return;
			} else {
				performIdleBehaviour();
				if(idleTimer <= 0.0f) {
					// Idle wait is over. Walk back to original position
					isHuntingPlayer = false;
					// Set walking target to iriginal position of enemy
					idleWalkingTarget = originalPosition;
					// Calculate target viewing direction
					idleRotationTarget = (idleWalkingTarget - transform.position).normalized;
					// Calculate angle between current viewing direction and target viewing direction
					float angleBetween = Mathf.Sign (Vector3.Cross(viewingDirection, idleRotationTarget).z)*Vector3.Angle(viewingDirection, idleRotationTarget);
					// Calculate rotation target quaternion
					idleRotationTargetQuaternion = Quaternion.Euler(new Vector3(0.0f,0.0f,transform.localEulerAngles.z + angleBetween));
					// Set animation in progress
					isIdleWalking = true;
					// Set animation to incomplete
					isIdleAnimationComplete = false;
				}
			}
		}
		else if(isRunningAwayFromPlayer) {
			// Enemy can no longer see player hunting him - return to original position
			isRunningAwayFromPlayer = false;
			// Set timer until when the viewing angle is back to normal
			activeTimer = Random.Range(3.0f,6.0f);
			originalActiveTimer = activeTimer;
			originalPosition = transform.position;
		/*	// Set the walking target
			idleWalkingTarget = originalPosition;
			// Calculate target viewing direction
			idleRotationTarget = (idleWalkingTarget - transform.position).normalized;
			// Calculate angle between current viewing direction and target viewing direction
			float angleBetween = Mathf.Sign (Vector3.Cross(viewingDirection, idleRotationTarget).z)*Vector3.Angle(viewingDirection, idleRotationTarget);
			// Calculate rotation target quaternion
			idleRotationTargetQuaternion = Quaternion.Euler(new Vector3(0.0f,0.0f,transform.localEulerAngles.z + angleBetween));
			// Set animation in progress
			isIdleWalking = true;
			// Set animation to incomplete
			isIdleAnimationComplete = false; */
			// Set alarm state of enemy higher
			isAfterEscape = true;
		}
		else
		{
			performIdleBehaviour();
		}

	}

	private bool isInViewingRange(GameObject gameObject) {
		Vector3 toGameObject = gameObject.transform.position - transform.position;
		bool result = ((toGameObject.magnitude - gameObject.transform.localScale.x) < viewingRange) && (Vector3.Dot (toGameObject.normalized, viewingDirection) >= cosViewingAngle);
		return result;
	}

	private void huntPlayer() 
	{
		abilities[0].useAbility();
		if (!cannotMove) 
		{
			// Calculate rotation target (the target viewing direction, i.e. look towards player)
			Vector3 rotationTarget = toPlayer.normalized;
			// Get the angle between the target viewing direction and the current viewing direction
			float angleBetween = Mathf.Sign (Vector3.Cross (viewingDirection, rotationTarget).z) * Vector3.Angle (viewingDirection, rotationTarget);
			// Target rotation Quaternion
			Quaternion rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, transform.localEulerAngles.z + angleBetween));
			// Interpolate rotation for the current time step and apply it to the model
			transform.rotation = Quaternion.Slerp (transform.rotation, rotationTargetQuaternion, Time.deltaTime * maxVelocity);
			// Run towards player
			transform.position += viewingDirection * maxVelocity * Time.deltaTime;
		}
	}

	private void runAwayFromPlayer() 
	{
		if (!cannotMove) 
		{
			// Calculate rotation target (the target viewing direction, i.e. look away from player)
			Vector3 rotationTarget = -toPlayer.normalized;
			// Get the angle between the target viewing direction and the current viewing direction
			float angleBetween = Mathf.Sign (Vector3.Cross (viewingDirection, rotationTarget).z) * Vector3.Angle (viewingDirection, rotationTarget);
			// Target rotation Quaternion
			Quaternion rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, transform.localEulerAngles.z + angleBetween));
			// Interpolate rotation for the current time step
			Quaternion rotationMatrix = Quaternion.Slerp (transform.rotation, rotationTargetQuaternion, Time.deltaTime * maxVelocity);
			// Apply rotation to the model
			transform.rotation = rotationMatrix;
			// Walk away player
			transform.position += viewingDirection * maxVelocity * Time.deltaTime;
		}
	}

	// This function is called when the player is not in reach and simply lets the enemy move randomly around
	private void performIdleBehaviour()
	{
		if (isIdleAnimationComplete) {
			// If the idle animation is complete, then define the next random move or rotation
			float rndValue = Random.value;

			// Rotate or walk or stand still
			if (rndValue > 0.7) 
			{
				// Rotate at position
				findIdleRotationTarget();		
			//	Debug.Log("start idle rotation by " + degree + " degrees" );
			} 
			else if (rndValue > 0.2) 
			{
				// Rotate and walk towards point
				findIdleWalkingTarget();
			//	Debug.Log("Start idle walking towards " + idleWalkingTarget.x + " " + idleWalkingTarget.y + " " + idleWalkingTarget.z );
			} 
			else 
			{
				// Stand still
				idleTimer = Random.Range(1.0f,5.0f);
				isIdleWaiting = true;
			//	Debug.Log("Start idle waiting for " + idleTimer + " seconds.");

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
					transform.rotation = Quaternion.Lerp(transform.rotation, idleRotationTargetQuaternion, Time.deltaTime * maxVelocity*idleRotationSpeedReduction);
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
					// Get the angle between the target viewing direction and the current viewing direction
					float angleBetween = Mathf.Sign (Vector3.Cross(viewingDirection, rotationTarget).z)*Vector3.Angle(viewingDirection, rotationTarget);
					// Target rotation Quaternion
					Quaternion rotationTargetQuaternion = Quaternion.Euler(new Vector3(0.0f,0.0f,transform.localEulerAngles.z+angleBetween));
					// Interpolate rotation for the current time step and apply it to the model
					transform.rotation = Quaternion.Slerp(transform.rotation, rotationTargetQuaternion, Time.deltaTime * maxVelocity * idleRotationSpeedReduction);
					// Walk along viewing direction (and eventually towards target location)
					transform.position += viewingDirection * maxVelocity * idleWalkingSpeedReduction * Time.deltaTime;

				}

			}
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject == player)
			return;

		if (!isIdleAnimationComplete) {
			// If 2 enemies collide during idle action, let them search a new idle target
			unsetIdleState();
			return;
		}
		if (isHuntingPlayer) {
			if(transform.localScale.x < other.transform.localScale.x) {
				// Reposition enemy
				enemyMngr.respawnEnemy(other.gameObject);
				// Define by how much the player's blob grows
				float growFactor = other.gameObject.transform.localScale.x / transform.localScale.x;
				// Set scaling of the blob
				transform.localScale += new Vector3(0.1f,0.1f,0.1f)*growFactor*growFactor;
			}
			else 	// If the player's creature is smaller than the enemy, then reduce player's size
			{
				transform.localScale -= new Vector3(0.1f,0.1f,0.1f);
			}
		}
	}

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

	private void findIdleRotationTarget()
	{
		// Get random rotation angle
		float degree = Random.Range(30.0f,179.0f)*Mathf.Sign (Random.Range(-1,1));
		// Calculate target theta on unit circle (+90 since unit circe starts at (1,0) and Unity at (0,1))
		float targetTheta = (transform.localEulerAngles.z + 90.0f + degree)/360.0f*2.0f*Mathf.PI;
		// Calculate target viewing direction (there where the rotation will end)
		idleRotationTarget = new Vector3(Mathf.Cos(targetTheta), Mathf.Sin(targetTheta),0.0f);
		// Calculate target quaternion configuration (for Slerp)
		idleRotationTargetQuaternion = Quaternion.Euler(new Vector3(0.0f,0.0f,transform.localEulerAngles.z+degree));
		// Set animation in progress
		isIdleRotating = true;
	}

	private void unsetIdleState() {
		isIdleWaiting = false;
		isIdleWalking = false;
		isIdleRotating = false;
		isIdleAnimationComplete = true;
	}

	public void resetStates() {
		isIdleWaiting = false;
		isIdleWalking = false;
		isIdleRotating = false;
		isIdleAnimationComplete = true;
		isHuntingPlayer = false;
		isRunningAwayFromPlayer = false;
		isAfterEscape = false;
	}

	public void addAbility(GameObject ability, int slot)
	{
		abilityObjects[slot] = ability;
		abilities[slot] = (ability)abilityObjects[slot].GetComponent(typeof(ability));
	}

	
}
