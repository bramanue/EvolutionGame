using UnityEngine;
using System.Collections;

public class ramAbility : ability {

	public bool inUse;

	public bool inTargetMode;

	public bool inChargeMode;

	public bool inAttackMode;

	private Vector3 targetDirection;

	private Quaternion rotationTargetQuaternion;

	private float rotationSpeed;

	private float ramSpeed;

	private float range;

	private float attackTimer = 0.0f;

	void Start() 
	{
		inTargetMode = true;
		inChargeMode = false;
		inAttackMode = false;
		inUse = false;
		isActiveAbility = true;
		maxLevel = 20;
		range = 5.0f;
		name = "RamAbility";
		cooldownTime = 4.0f;
		cooldownTimer = 0.0f;
		// Get the game object which has this ram ability
		parentBlob = transform.parent.gameObject;
		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent(typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent(typeof(player));
		isPlayer = (bool)parentPlayerScript;
	}

	void Update()
	{
		cooldownTimer -= Time.deltaTime;

		// Player behaviour
		if (isPlayer) 
		{
			// If target direction is not the zero vector (i.e. useAbility has been called and stick moved into a certain direction during last frame)
			if (targetDirection != new Vector3 (0, 0, 0)) 
			{
				Debug.Log (targetDirection);
				// If the player has released the button (i.e. not inUse)
				if (!inUse && inTargetMode)
				{
					inTargetMode = false;
					inChargeMode = true;
					// Ram attack is only executed after a 0.5s delay
					attackTimer = 0.5f;
					// Get the angle between the target viewing direction and the current viewing direction
					float angleBetween = Mathf.Sign (Vector3.Cross (parentPlayerScript.viewingDirection, targetDirection).z) * Vector3.Angle (parentPlayerScript.viewingDirection, targetDirection);
					// Calculate the necessary rotation speed to perform rotation within 0.5s
					rotationSpeed = Mathf.Abs(angleBetween) * 2.0f * Mathf.Deg2Rad;
					// Get the rotation target
					rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, parentBlob.transform.localEulerAngles.z + angleBetween));
					// Reset time scale to normal
					Time.timeScale = 1.0f;
				}
				else if (inChargeMode)
				{
					attackTimer -= Time.deltaTime;
					
					// Interpolate rotation for the current time step and apply it to the model
					parentBlob.transform.rotation = Quaternion.Slerp (parentBlob.transform.rotation, rotationTargetQuaternion, Time.deltaTime * rotationSpeed);

					if(attackTimer <= 0.0f)
					{
						inChargeMode = false;
						inAttackMode = true;
						attackTimer = 0.2f;
						ramSpeed = range / 0.2f;
					}
				}
				else if (inAttackMode)
				{
					// Reduce the ram-animation timer
					attackTimer -= Time.deltaTime;
					// Ram into current viewing direction
					parentBlob.transform.position += parentPlayerScript.viewingDirection * ramSpeed * Time.deltaTime;
					
					// TODO Collision
					
					// Once the timer is complete, the ram attack has ended and normal gameplay starts again
					if(attackTimer <= 0)
					{
						// Reset target direction
						targetDirection = new Vector3(0,0,0);
						// Restart the cooldown timer
						cooldownTimer = cooldownTime;
						// Reset states to idle
						inAttackMode = false;
						inChargeMode = false;
						inTargetMode = true;
					}
				}
			}
			else 
			{
				// If the ability is not used, then reset the nonmoving state of the executing blob
				if (!inUse) {
					parentPlayerScript.cannotMove = false;
					Time.timeScale = 1.0f;
				}
			}
			
		} 
		else  	// Enemy behaviour
		{
			if(targetDirection != new Vector3(0,0,0))
			{
				if (inChargeMode)
				{
					// Perform rotation within 0.5 seconds
					attackTimer -= Time.deltaTime;

					// Interpolate rotation for the current time step and apply it to the model
					parentBlob.transform.rotation = Quaternion.Slerp (parentBlob.transform.rotation, rotationTargetQuaternion, Time.deltaTime * rotationSpeed);

					if(attackTimer <= 0.0f)
					{
						inChargeMode = false;
						inAttackMode = true;
						attackTimer = 0.2f;
						ramSpeed = range / 0.2f;
					}
				}
				else if(inAttackMode)
				{
					// Reduce the ram-animation timer
					attackTimer -= Time.deltaTime;
					// Ram into current viewing direction
					parentBlob.transform.position += parentEnemyScript.viewingDirection * ramSpeed * Time.deltaTime;

					// TODO Collision

					// Once the timer is complete, the ram attack has ended and normal gameplay starts again
					if(attackTimer <= 0)
					{
						// Reset target direction
						targetDirection = new Vector3(0,0,0);
						// Restart the cooldown timer
						cooldownTimer = cooldownTime;
						// Reset states to idle
						inAttackMode = false;
						inChargeMode = false;
						inTargetMode = true;
					}
				}
			}
			else
				// If the ability is not in use, then let the enemy move normally
				if(!inUse)
					parentEnemyScript.cannotMove = false;
		}

		inUse = false;
	}

	// Called whenever the button is down
	public override bool useAbility() 
	{
		if (cooldownTimer <= 0.0f) {

			if (isPlayer) {
				if (!inUse) {
					// During targeting, the player cannot move
					parentPlayerScript.cannotMove = true;
				}

				if (inTargetMode) {
					// Slow down time while targeting
					Time.timeScale = 0.1f;
					targetDirection = new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"), 0.0f);
				}
				inUse = true;
			}
			else 	// Enemy behaviour
			{
				if(inTargetMode)
				{
					// During targeting, the enemy cannot move
					parentEnemyScript.cannotMove = true;
					// Enemy attacks current player position
					targetDirection = parentEnemyScript.toPlayer.normalized;
					// Skip targetMode in the update function. Do necessary calculation already here
					inTargetMode = false;
					inChargeMode = true;

					// Ram attack is only executed after a 0.8s delay (also used for rotation)
					attackTimer = 0.8f;
					// Get the angle between the target viewing direction and the current viewing direction
					float angleBetween = Mathf.Sign (Vector3.Cross (parentEnemyScript.viewingDirection, targetDirection).z) * Vector3.Angle (parentEnemyScript.viewingDirection, targetDirection);
					// Calculate the necessary rotation speed to perform rotation within 0.5s
					rotationSpeed = Mathf.Abs(angleBetween) * 2.0f * Mathf.Deg2Rad;
					// Get the rotation target
					rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, parentBlob.transform.localEulerAngles.z + angleBetween));
				}
			}

			return true;
		} else
			return false;
	}

}
