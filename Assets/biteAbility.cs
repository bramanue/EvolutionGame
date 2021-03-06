﻿using UnityEngine;
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
		cooldownTimer = 0.0f;

		blobInReach = false;
		otherBlob = null;

		abilityEnum = EAbilityType.EBiteAbility;
		abilitySuperClassEnum = EAbilityClass.EActiveAbility;

		// Put ability on blob
		transform.localScale = new Vector3 (1, 1, 1);
		transform.localPosition = new Vector3 (0, 0, 0);
	}
	
	void Update()
	{
		cooldownTimer -= Time.deltaTime;
	}

	void LateUpdate()
	{
		otherBlob = null;
		blobInReach = false;
	}

	public override void resetTransform()
	{
		transform.localScale = new Vector3 (1, 1, 1);
		transform.localPosition = new Vector3 (0, 0, 0);
		transform.localRotation = new Quaternion ();
	}
	
	public override bool useAbility() 
	{

		// TODO Cast effects / sounds
		if (cooldownTimer <= 0.0f) 
		{
			StartCoroutine (stingAnimation ());
			cooldownTimer = 0.4f;

			if(blobInReach) {
				if(isPlayer) {
					enemy enemyScript = (enemy)otherBlob.GetComponent (typeof(enemy));
					float oldSize = enemyScript.size;

					ability shieldInUse = enemyScript.shieldInUse;
					if(shieldInUse != null) {
						if(shieldInUse.abilityEnum == EAbilityType.EDustShieldAbility || shieldInUse.abilityEnum == EAbilityType.EThornShieldAbility) {
							if(shieldInUse.level == 0)
								// TODO Play blocking sound (or cracking for shield decrease)
								enemyScript.removeAndDestroyAbility(enemyScript.hasAbility(shieldInUse.abilityEnum));
							else
							{
								shieldInUse.increaseLevel((int)(-1));	// Reduce shield level by one due to impact
								// Restart cooldown timer
								cooldownTimer = cooldownTime;
								return true;
							}
						}
					} 

					enemyScript.inflictAbilityDamage(damage);
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
					damage = baseDamage + level * 0.1f;

					ability shieldInUse = playerScript.shieldInUse;
					if(shieldInUse != null) {
						if(shieldInUse.abilityEnum == EAbilityType.EDustShieldAbility || shieldInUse.abilityEnum == EAbilityType.EThornShieldAbility) {
							if(shieldInUse.level == 0)
								playerScript.removeAndDestroyAbility(playerScript.hasAbility(shieldInUse.abilityEnum));
							else
							{
								shieldInUse.increaseLevel((int)(-1));	// Reduce shield level by one due to impact
								// Restart cooldown timer
								cooldownTimer = cooldownTime;
								return true;
							}
						}
					} 

					playerScript.inflictDamage(damage);
					// Half of the inflicted damage is added to the enemy's size
					parentEnemyScript.size += 0.25f*Mathf.Min (damage, oldSize);
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

	IEnumerator stingAnimation() 
	{
		float distancePerSecond = 0.5f / 0.2f;
		for (float time = 0f; time < 0.2f; time += Time.deltaTime) {
			float y = Mathf.Min (0.5f, transform.localPosition.y + Time.deltaTime*distancePerSecond);
			transform.localPosition = new Vector3(0,y,0);
			yield return null;
		}

		for (float time = 0f; time < 0.2f; time += Time.deltaTime) {
			float y = Mathf.Max (0, transform.localPosition.y - Time.deltaTime*distancePerSecond);
			transform.localPosition = new Vector3(0,y,0);
			yield return null;
		}
		transform.localPosition = new Vector3(0,0,0);
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
		}
	}
	
	public override float calculateUseProbability(player playerScript, Vector3 toPlayer, bool attack, bool canSeePlayer) 
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