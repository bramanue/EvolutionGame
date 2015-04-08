using UnityEngine;
using System.Collections;

public class biteAbility : ability {

	public float baseDamage;

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
		isPlayer = (bool)parentPlayerScript;

		maxLevel = 10;
		baseDamage = 0.1f;
		damage = baseDamage + level * 0.1f;
		abilityName = "BiteAbility";
		cooldownTime = 3.0f;
		cooldownTimer = 0.0f;

		blobInReach = false;
	}
	
	void Update()
	{
		cooldownTimer -= Time.deltaTime;
		transform.localPosition = new Vector3 (0, parentBlob.transform.localScale.y, 0);
	}
	
	public override bool useAbility() 
	{
		// TODO Cast effects
		if (cooldownTimer <= 0.0f) {
			if(blobInReach) {
				if(isPlayer) {
					enemy enemyScript = (enemy)otherBlob.GetComponent (typeof(enemy));
					float oldSize = enemyScript.size;
					enemyScript.size -= damage;
					// A quarter of the inflicted damage is added to the player's size
					parentPlayerScript.size += 0.25f*Mathf.Min (damage, oldSize);
					Debug.Log (0.25f*Mathf.Min (damage, oldSize));
					// Put enemy into alert state
					enemyScript.setAlertState();
					// Restart cooldown timer
					cooldownTimer = cooldownTime;
				}
				else
				{
					player playerScript = (player)otherBlob.GetComponent (typeof(player));
					float oldSize = playerScript.size;
					playerScript.size -= damage;
					// Half of the inflicted damage is added to the enemy's size
					parentPlayerScript.size += 0.25f*Mathf.Min (damage, oldSize);
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
	void OnTriggerEnter2D(Collider2D other)
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

	// If the teeths leave the object again, remove the object from the local storage (useAbility() will no longer have any effect)
	void OnTriggerExit2D(Collider2D other)
	{
		// Check whether the teeth of the blob collided with another blob
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent (typeof(player));
		
		if (isPlayer && enemyScript != null || !isPlayer && playerScript != null)
		{
			blobInReach = false;
			otherBlob = null;
		}
	}

	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EBiteAbility;
	}
}