using UnityEngine;
using System.Collections;

public class enemy : MonoBehaviour {

	// Start position of this enemy. Do not move away too far from this point.
	private Vector3 originalPosition;

	// Viewing direction of this enemy (normalized)
	private Vector3 viewingDirection;

	// Defines how far this enemy can see
	public float viewingRange = 5.0f;

	// Defines how wide the angle of the enemy's eyes are 
	public float cosViewingAngle;

	// Stores the original cosViewingAngle for restoring default after escape procedure
	private float originalCosViewingAngle;

	// The size of this enemy
	public float size;

	// The maximum velocity of this enemy
	public float maxVelocity = 4.0f;

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

	// Defines by which factor velocity should be removed for idle actions
	private float idleSpeedReduction = 0.3f;

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


	// Use this for initialization
	void Start () {
		player = GameObject.Find("Blob");
		size = transform.localScale.x;
		// Get the theta angle of the current rotation, corresponding to position on the unit circle
		float theta = (transform.localEulerAngles.z+90.0f)/360.0f*2.0f*Mathf.PI;
		// Get the viewing direction based on the current rotation (resp. on the previously calculated theta)
		viewingDirection = new Vector3 (Mathf.Cos (theta), Mathf.Sin (theta), 0.0f);
	//	idleOperationRadius = 10.0f;
		originalPosition = transform.position;
		unsetIdleState();
		isHuntingPlayer = false;
		isRunningAwayFromPlayer = false;
	//	cosViewingAngle = 0.5f;
		originalCosViewingAngle = cosViewingAngle;
	//	viewingRange = 10.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Get the theta angle of the current rotation, corresponding to position on the unit circle
		float theta = (transform.localEulerAngles.z+90.0f)/360.0f*2.0f*Mathf.PI;
		// Get the viewing direction based on the current rotation (resp. on the previously calculated theta)
		viewingDirection = new Vector3 (Mathf.Cos (theta), Mathf.Sin (theta), 0.0f);
		// Calculate vector pointing to the player
		Vector3 toPlayer = player.transform.position - transform.position;

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

		// Check whether player is in viewing range and viewing angle
		if (toPlayer.magnitude - 0.5*player.transform.localScale.x < viewingRange && Vector3.Dot(toPlayer.normalized, viewingDirection) >= cosViewingAngle) {

			// TODO: Camouflage, darkness, scene geometry, etc...

			// Enemy can see player

			if (size >= player.transform.localScale.x) {
				// Enemy is bigger than player (but not too big) -> attack player

				// Only hunt, if enemy isn't too far from its original position
				if((transform.position-originalPosition).magnitude <= 2.0f*idleOperationRadius && size < 2*player.transform.localScale.x) {
					isHuntingPlayer = true;
					isAfterEscape = false;
					isRunningAwayFromPlayer = false;
					unsetIdleState();
					huntPlayer(toPlayer);
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
				runAwayFromPlayer(toPlayer);
				return;
			}
		}

		// Enemy cannot see player
		if(isHuntingPlayer) {
			// If the enemy was hunting the player, then wait a few seconds before walking back to the original position
			if(!isIdleWaiting) {
				idleTimer = Random.Range(3.0f,8.0f);
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
			activeTimer = Random.Range(2.0f,5.0f);
			originalActiveTimer = activeTimer;
			//cosViewingAngle = originalCosViewingAngle;
			// Set the walking target
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
			// Set alarm state of enemy higher
			isAfterEscape = true;
		}
		else
		{
			performIdleBehaviour();
		}

	}

	private void huntPlayer(Vector3 toPlayer) 
	{
		// Calculate rotation target (the target viewing direction, i.e. look towards player)
		Vector3 rotationTarget = toPlayer.normalized;
		// Get the angle between the target viewing direction and the current viewing direction
		float angleBetween = Mathf.Sign (Vector3.Cross(viewingDirection, rotationTarget).z)*Vector3.Angle(viewingDirection, rotationTarget);
		// Target rotation Quaternion
		Quaternion rotationTargetQuaternion = Quaternion.Euler(new Vector3(0.0f,0.0f,transform.localEulerAngles.z+angleBetween));
		// Interpolate rotation for the current time step
		Quaternion rotationMatrix = Quaternion.Slerp(transform.rotation, rotationTargetQuaternion, Time.deltaTime * maxVelocity);
		// Apply rotation to the model
		transform.rotation = rotationMatrix;
		// Walk towards player
		transform.position += viewingDirection * maxVelocity * Time.deltaTime;
	}

	private void runAwayFromPlayer(Vector3 toPlayer) 
	{
		// Calculate rotation target (the target viewing direction, i.e. look away from player)
		Vector3 rotationTarget = -toPlayer.normalized;
		// Get the angle between the target viewing direction and the current viewing direction
		float angleBetween = Mathf.Sign (Vector3.Cross(viewingDirection, rotationTarget).z)*Vector3.Angle(viewingDirection, rotationTarget);
		// Target rotation Quaternion
		Quaternion rotationTargetQuaternion = Quaternion.Euler(new Vector3(0.0f,0.0f,transform.localEulerAngles.z+angleBetween));
		// Interpolate rotation for the current time step
		Quaternion rotationMatrix = Quaternion.Slerp(transform.rotation, rotationTargetQuaternion, Time.deltaTime * maxVelocity);
		// Apply rotation to the model
		transform.rotation = rotationMatrix;
		// Walk away player
		transform.position += viewingDirection * maxVelocity * Time.deltaTime;
	}

	// This function is called when the player is not in reach and simply lets the enemy move randomly around
	private void performIdleBehaviour()
	{
		if (isIdleAnimationComplete) {
			// If the idle animation is complete, then define the next random move or rotation
			float rndValue = Random.value;

			// Rotate or walk or stand still
			if (rndValue > 0.7) {
				// Rotate at position

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
			
			//	Debug.Log("start idle rotation by " + degree + " degrees" );
			} else if (rndValue > 0.2) {
				// Rotate and walk towards point

				// Get random point in unit circle
				Vector2 rnd2D = Random.insideUnitCircle;
				// Transform random value into point within the idle operation radius
				idleWalkingTarget = originalPosition + new Vector3(rnd2D.x, rnd2D.y, 0.0f)*idleOperationRadius;
				// Calculate target viewing direction
				idleRotationTarget = (idleWalkingTarget - transform.position).normalized;
				// Calculate angle between current viewing direction and target viewing direction
				float angleBetween = Mathf.Sign (Vector3.Cross(viewingDirection, idleRotationTarget).z)*Vector3.Angle(viewingDirection, idleRotationTarget);
				// Calculate rotation target quaternion
				idleRotationTargetQuaternion = Quaternion.Euler(new Vector3(0.0f,0.0f,transform.localEulerAngles.z + angleBetween));
				// Set animation in progress
				isIdleWalking = true;

			//	Debug.Log("Start idle walking towards " + idleWalkingTarget.x + " " + idleWalkingTarget.y + " " + idleWalkingTarget.z );
			} else {
				// stand still

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
					// Calculate the rotation matrix for current timestep
					Quaternion rotationMatrix = Quaternion.Lerp(transform.rotation, idleRotationTargetQuaternion, Time.deltaTime * maxVelocity*idleSpeedReduction);
					// Apply rotation to the model
					transform.rotation = rotationMatrix;
					return;
				}

			}
			else if(isIdleWalking) {

				// Get the vector towards the target location
				Vector3 toTargetLocation = idleWalkingTarget - transform.position;
				// Stop walking, if we have reached the target location (up to a certain precision)
				if(toTargetLocation.magnitude <= 0.1f ) {
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
					transform.rotation = Quaternion.Slerp(transform.rotation, rotationTargetQuaternion, Time.deltaTime * maxVelocity);
					// Walk along viewing direction (and eventually towards target location)
					transform.position += viewingDirection * maxVelocity*idleSpeedReduction * Time.deltaTime;

				}

			}
		}
	}

	private void unsetIdleState() {
		isIdleWaiting = false;
		isIdleWalking = false;
		isIdleRotating = false;
		isIdleAnimationComplete = true;
	}

	public void setRandomParameters()
	{
		Debug.Log ("set parameters");
		size = player.transform.localScale.x + Random.Range(-1.0f,1.0f);
		transform.localScale = new Vector3 (size, size, size);
	}
}
