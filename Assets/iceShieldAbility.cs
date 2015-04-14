﻿using UnityEngine;
using System.Collections;

public class iceShieldAbility : ability {
	
	private float timer;
	
	private float maxTimeInIce = 30.0f;
	
	private float damage = 0.1f;
	
	private bool inUse;

	private bool deactivateInNextFrame;
	
	// Use this for initialization
	void Start () {
		// Get the game object which has this ram ability
		parentBlob = transform.parent.gameObject;
		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent(typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent(typeof(player));
		isPlayer = (bool)parentPlayerScript;
		
		cooldownTimer = 0.0f;

		damage = 0.1f + level * 0.1f;
		maxTimeInIce = 30.0f + 30.0f * level;
		timer = maxTimeInIce;

		abilitySuperClassEnum = EAbilityClass.EShieldAbility;
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = new Vector3 (0, 0, 0);
		transform.localScale = new Vector3 (1, 1, 1);
	}

	void LateUpdate() {
		// Deactivate shield if requested in previous frame
		if (deactivateInNextFrame)
			inUse = false;

		if (inUse) {
			// TODO change visuals
			((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = new Color(0.5f,0.5f,1,0.9f);
			
			// Make sure ability is not able to be used forever unless the ability is at its max level
			if(level < maxLevel)
				timer -= Time.deltaTime;
		} 
		else 
		{
			cooldownTimer -= Time.deltaTime;
			
			if (timer <= 0 && cooldownTimer <= 0) {
				// Once the ability has been used to its end, give it a 5 second cooldown, befor it can be used again
				cooldownTimer = 5.0f;
			} 
			
			// Restore timer, when ability is not in use
			timer = Mathf.Min (maxTimeInIce, timer + Time.deltaTime);
			
			// Reset to default sprite if no other shield is active
			if(isPlayer) {
				if(parentPlayerScript.shieldInUse == null)
					((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = parentPlayerScript.defaultColor;
			} else {
				if(parentEnemyScript.shieldInUse == null)
					((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = parentEnemyScript.defaultColor;
			}
		}
		deactivateInNextFrame = true;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (!inUse)
			return;

		// If collision with own blob, do nothing
		if (other.gameObject == parentBlob)
			return;
		
		// Check whether the teeth of the blob collided with another blob
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent (typeof(player));
		
		if (isPlayer && enemyScript) {
			// Enemy is hurt by player's ice shield if enemy does not have a lava shield, dust shield or thorn shield
		/*	if(enemyScript.hasAbility(EAbilityType.ELavaShieldAbility) == -1 && 
			   enemyScript.hasAbility(EAbilityType.EDustShieldAbility) == -1 && 
			   enemyScript.hasAbility(EAbilityType.EThornShieldAbility) == -1  )
			{
				enemyScript.size -= damage;
				enemyScript.setAlertState();
			}*/

			if(enemyScript.shieldInUse == null || ( 
			   enemyScript.shieldInUse.getAbilityEnum() != EAbilityType.ELavaShieldAbility && 
			   enemyScript.shieldInUse.getAbilityEnum() != EAbilityType.EDustShieldAbility && 
			   enemyScript.shieldInUse.getAbilityEnum() != EAbilityType.EThornShieldAbility ) )
			{
				Debug.Log ("Enemy hurt by ice shield: Damage = " + damage);
				enemyScript.size -= damage;
				enemyScript.setAlertState();
			}
		} else if (!isPlayer && playerScript) {
			// Player is hurt by enemy's thorn shield if player does not have a thorn shield or dust shield
		/*	if(playerScript.hasAbility(EAbilityType.ELavaShieldAbility) == -1 && 
			   playerScript.hasAbility(EAbilityType.EDustShieldAbility) == -1 && 
			   playerScript.hasAbility(EAbilityType.EThornShieldAbility) == -1 )
			{
				playerScript.size -= damage;
			}
*/
			// Player is hurt by enemy's thorn shield if player does not have a thorn shield or dust shield
			if(playerScript.shieldInUse == null || ( 
			   playerScript.shieldInUse.getAbilityEnum() != EAbilityType.ELavaShieldAbility && 
			   playerScript.shieldInUse.getAbilityEnum() != EAbilityType.EDustShieldAbility && 
			   playerScript.shieldInUse.getAbilityEnum() != EAbilityType.EThornShieldAbility ) )
			{
				playerScript.size -= damage;
			}
		}

	}
	
	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		int increase = previousLevel - level;
		timer += increase*30.0f;
		damage = 0.1f + level * 0.1f;
		maxTimeInIce = 30.0f + 30.0f * level;
		return increase;
	}
	
	public override bool useAbility() 
	{
		if (timer > 0) {
			inUse = true;
			deactivateInNextFrame = false;
			return true;
		} else {
			return false;
		}
	}
	
	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EIceShieldAbility;
	}
	
}
