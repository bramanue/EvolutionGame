using UnityEngine;
using System.Collections;

public class ramAbility : ability {

	// Defines whether the useAbility() function has just been called
	public bool inUse;

	private bool slowed;


	public bool inTargetMode;

	public bool inChargeMode;

	public bool inAttackMode;


	private float attackTimer = 0.0f;

	private Vector3 targetDirection;

	private Quaternion rotationTargetQuaternion;

	private float rotationSpeed;

	private float ramSpeed;


	public float damage;

	private float internalDamage;

	public float baseDamage = 0.1f;


	public float range;

	public float baseRange = 3.0f;

	private float internalRange;


	void Start() 
	{
		// Get the game object which has this ram ability
		parentBlob = transform.parent.gameObject;
		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent(typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent(typeof(player));
		isPlayer = (bool)parentPlayerScript;

		inTargetMode = true;
		inChargeMode = false;
		inAttackMode = false;
		inUse = false;

		internalDamage = baseDamage + level * 0.1f;
		damage = internalDamage*parentBlob.transform.localScale.x;
		internalRange = baseRange + 0.5f*level;
		range = internalRange*parentBlob.transform.localScale.x;

		abilityName = "Ram Attack";
		abilitySuperClassEnum = EAbilityClass.EActiveAbility;
		cooldownTimer = 0.0f;

		increaseLevel (0);

		abilityEnum = EAbilityType.ERamAbility;

		// Put ability on blob
		transform.localPosition = new Vector3 (0, 0, 0);
	}

	void Update()
	{
		cooldownTimer -= Time.deltaTime;
		transform.localPosition = new Vector3 (0, 0, 0);
		transform.localScale = new Vector3 (1, 1, 1);
		transform.rotation = new Quaternion ();

		// Update range and damage
		damage = internalDamage*parentBlob.transform.localScale.x;
		range = internalRange*parentBlob.transform.localScale.x;

		// Player behaviour
		if (isPlayer) 
		{
			// If target direction is not the zero vector (i.e. useAbility has been called and stick moved into a certain direction during last frame)
			if (targetDirection != new Vector3 (0, 0, 0)) 
			{
				// If the player has released the button (i.e. not inUse), then initialize rotation
				if (!inUse && inTargetMode)
				{
					inTargetMode = false;
					inChargeMode = true;
					// Ram attack is only executed after a 0.5s delay
					attackTimer = 0.3f;
					// Get the angle between the target viewing direction and the current viewing direction
					float angleBetween = Mathf.Sign (Vector3.Cross (parentPlayerScript.viewingDirection, targetDirection).z) * Vector3.Angle (parentPlayerScript.viewingDirection, targetDirection);
					// Calculate the necessary rotation speed to perform rotation within 0.5s
					rotationSpeed = Mathf.Abs(angleBetween) * 8.0f * Mathf.Deg2Rad;
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
				else
				{
					// Reset states if game was paused
					if(Time.timeScale == 0.0f)
					{
						// Reset target direction
						targetDirection = new Vector3(0,0,0);
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
					parentPlayerScript.canMove = true;
					targetDirection = new Vector3(0,0,0);
					// Make sure time normalization is only called once and not all the time (could not pause game otherwise)
					if(slowed) {
						Time.timeScale = 1.0f;
						slowed = false;
					}
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

					// TODO Collision with other objects

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
					parentEnemyScript.canMove = true;
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
					parentPlayerScript.canMove = false;
				}

				if (inTargetMode) {
					// Slow down time while targeting
					slowed = true;
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
					parentEnemyScript.canMove = false;
					// Enemy attacks current player position
					targetDirection = parentEnemyScript.toPlayer.normalized;
					// Skip targetMode in the update function. Do necessary calculation already here
					inTargetMode = false;
					inChargeMode = true;

					// Ram attack is only executed after a 0.8s delay (also used for rotation)
					attackTimer = 1.0f;
					// Get the angle between the target viewing direction and the current viewing direction
					float angleBetween = Mathf.Sign (Vector3.Cross (parentEnemyScript.viewingDirection, targetDirection).z) * Vector3.Angle (parentEnemyScript.viewingDirection, targetDirection);
					// Calculate the necessary rotation speed to perform rotation within 0.5s
					rotationSpeed = Mathf.Abs(angleBetween) * 8.0f * Mathf.Deg2Rad;
					// Get the rotation target
					rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, parentBlob.transform.localEulerAngles.z + angleBetween));
				}
			}

			return true;
		} else
			return false;
	}

	// If the ram ability touches an object perform the following
	void OnTriggerEnter(Collider other)
	{
		// If collision with own blob, do nothing
		if (other.gameObject == parentBlob)
			return;
		
		// Check whether the teeth of the blob collided with another blob
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent (typeof(player));

		// If we are in attack mode and hit an enemy/player, then put damage to that blob
		if (inAttackMode) {
			// TODO play sound

			// TODO Collision with environment
			bool inpenetratableObject = false;
			if (inpenetratableObject) {
				attackTimer = 0.0f;
				// Reset target direction
				targetDirection = new Vector3(0,0,0);
				// Restart the cooldown timer
				cooldownTimer = cooldownTime;
				// Reset states to idle
				inAttackMode = false;
				inChargeMode = false;
				inTargetMode = true;
				// Hit had hard -> set stunned for a few seconds TODO duration dependent on object(?)
				if(isPlayer)
					parentPlayerScript.setStunned(2.0f);
				else
					parentEnemyScript.setStunned(2.0f);
				return;
			}

			bool destructibleObject = false;
			if(destructibleObject) {
				// TODO destruct object
			}

			if (enemyScript != null || playerScript != null) {
				if (isPlayer) {
					// TODO reduce damage if dust shield / thorn shield used
					enemyScript.inflictAbilityDamage(damage);
				} else {
					if (playerScript) {

						Debug.Log ("Level = " + level + " Damage to player = " + damage);
						ability shieldInUse = playerScript.shieldInUse;
						if(shieldInUse != null) {
							if(shieldInUse.getAbilityEnum() == EAbilityType.EDustShieldAbility || shieldInUse.getAbilityEnum() == EAbilityType.EThornShieldAbility) {
								shieldInUse.increaseLevel(-1);	// Reduce shield level by one due to impact
								return;
							}
						} 
						playerScript.size -= damage;

					} else {
						ability shieldInUse = enemyScript.shieldInUse;
						if(shieldInUse != null) {
							if(shieldInUse.getAbilityEnum() == EAbilityType.EDustShieldAbility || shieldInUse.getAbilityEnum() == EAbilityType.EThornShieldAbility) {
								shieldInUse.increaseLevel(-1);	// Reduce shield level by one due to impact
								return;
							}
						} 
						enemyScript.inflictAbilityDamage(damage);
					}
				}
			}
		}
	}

	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		internalDamage = baseDamage + 0.1f * level;
		internalRange = baseRange + 0.5f * level;
		return level - previousLevel;
		
		// TODO Change appearance of ability sprite
	}

	public override float calculateUseProbability(player playerScript, Vector3 toPlayer, bool attack, bool canSeePlayer) 
	{
		if (cooldownTimer > 0 || !canSeePlayer)
			return 0.0f;

		if (attack) 
		{
			float distance = toPlayer.magnitude;
			if (distance < range) {
				// Make it more likely to use, if player is behind you (due to fast rotation)
				float multiplier = 1.0f + Mathf.Abs (Vector3.Dot (parentEnemyScript.viewingDirection, toPlayer) - 1) * 0.3f;
				return (1.0f - distance / range) * multiplier;
			} else
				// For fast rotation and approach
				return Mathf.Abs (Vector3.Dot (parentEnemyScript.viewingDirection, toPlayer) - 1) * 0.2f;
		} 
		else 
		{
			return 0.0f;
		}
	}

	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.ERamAbility;
	}

}
