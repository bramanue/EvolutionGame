using UnityEngine;
using System.Collections;

public class biteAbility : ability {

	public float baseDamage = 0.1f;

	public float damage;

	private bool blobInReach;

	private GameObject otherBlob;

	void Start() 
	{
		// Get the game object which has this ram ability
		parentBlob = transform.parent.gameObject;
		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent(typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent(typeof(player));
		// Check whether it is a player or AI blob
		isPlayer = (bool)parentPlayerScript;

		maxLevel = 10;
		damage = baseDamage + level * 0.1f;
		abilityName = "BiteAbility";
		cooldownTime = 3.0f;
		cooldownTimer = 0.0f;

		blobInReach = false;
		otherBlob = null;

		abilityEnum = EAbilityType.EBiteAbility;
		abilitySuperClassEnum = EAbilityClass.EActiveAbility;

		// Put ability on blob
		transform.localPosition = new Vector3 (0, 1.3f, 0);
	}
	
	void Update()
	{
		cooldownTimer -= Time.deltaTime;
	}

	void LateUpdate()
	{
		transform.localPosition = new Vector3(0,1.28f,0);
		otherBlob = null;
		blobInReach = false;
	}
	
	public override bool useAbility() 
	{
		// TODO Cast effects / sounds
		if (cooldownTimer <= 0.0f) {
			if(blobInReach) {
				if(isPlayer) {
					enemy enemyScript = (enemy)otherBlob.GetComponent (typeof(enemy));
					float oldSize = enemyScript.size;
					enemyScript.size -= damage;
					// A quarter of the inflicted damage is added to the player's size
					parentPlayerScript.size += 0.25f*Mathf.Min (damage, oldSize);
					Debug.Log ("Bite ability inflicted " + 0.25f*Mathf.Min (damage, oldSize) + " damage.");
					// Put enemy into alert state
					enemyScript.setAlertedState();
					// Restart cooldown timer
					cooldownTimer = cooldownTime;
				}
				else
				{
					player playerScript = (player)otherBlob.GetComponent (typeof(player));
					float oldSize = playerScript.size;
					playerScript.size -= damage;
					// Half of the inflicted damage is added to the enemy's size
					playerScript.size += 0.25f*Mathf.Min (damage, oldSize);
					// Restart cooldown timer
					cooldownTimer = cooldownTime;
				}

			} else {
				return false;
			}
			
			cooldownTimer = cooldownTime;
			return true;
		} 
		else 
		{
			return false;
		}
	}

	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		damage = baseDamage + level * 0.1f;
		return level - previousLevel;
	}

	// If the teeths enter an object, store that object for the useAbility() function (in case it is an enemy/player)
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == parentBlob)
			return;

		// Check whether the teeth of the blob collided with another blob
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent (typeof(player));

		if (isPlayer && enemyScript != null || !isPlayer && playerScript != null)
		{
			blobInReach = true;
			otherBlob = other.gameObject;
			if(!isPlayer) {
				parentEnemyScript.activateAbility(parentEnemyScript.hasAbility(EAbilityType.EBiteAbility));
			}
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (other.gameObject == parentBlob)
			return;
		
		// Check whether the teeth of the blob collided with another blob
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent (typeof(player));
		
		if (isPlayer && enemyScript != null || !isPlayer && playerScript != null)
		{
			blobInReach = true;
			otherBlob = other.gameObject;
			if(!isPlayer) {
				parentEnemyScript.activateAbility(parentEnemyScript.hasAbility(EAbilityType.EBiteAbility));
			}
		}
	}
	
	public override float calculateUseProbability(player playerScript, bool attack) 
	{
		if (cooldownTimer > 0)
			return 0.0f;
		
		if (blobInReach) {
			return 0.9f;
		} else {
			return 0.0f;
		}
	}


	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EBiteAbility;
	}
}