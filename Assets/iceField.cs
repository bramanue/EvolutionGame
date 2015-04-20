﻿using UnityEngine;
using System.Collections;

public class iceField : hazardousEnvironment {
	
	private float throwBackTimer;
	
	// Use this for initialization
	void Start () {
		requiredAbility = EAbilityType.EIceShieldAbility;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnTriggerEnter(Collider other)
	{
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent(typeof(player));
		
		if (playerScript) 
		{

			if( playerScript.shieldInUse != null && playerScript.shieldInUse.abilityEnum == EAbilityType.EIceShieldAbility ) {
				// Nothing to do, player can enter
				// TODO play sound or such
			}
			else
			{
				// Enemy takes damager
				playerScript.inflictEnvironmentalDamage(damagePerSecond*Time.deltaTime);
				// Throw back approacher
				// TODO throw back in normal direction of the thorn bush
				playerScript.applyEnvironmentalSlowDown(slowDownFactor);
			}
			
		}
		else if (enemyScript) 
		{

			if( enemyScript.shieldInUse != null && enemyScript.shieldInUse.abilityEnum != EAbilityType.EIceShieldAbility ) {
				// Nothing to do, enemy can enter
				// TODO play sound or such
			}
			else
			{
				// Enemy takes damager
				enemyScript.inflictEnvironmentalDamage(damagePerSecond*Time.deltaTime);
				// Throw back approacher
				// TODO throw back in normal direction of the thorn bush
				enemyScript.applyEnvironmentalSlowDown(slowDownFactor);
			}
		}
	}

	void OnTriggerStay(Collider other)
	{
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent(typeof(player));
		
		if (playerScript) 
		{
			playerScript.currentEnvironment = this;

			if( playerScript.shieldInUse != null && playerScript.shieldInUse.abilityEnum == EAbilityType.EIceShieldAbility ) {
				// Nothing to do, player can enter
				// TODO play sound or such
			}
			else
			{
				// Enemy takes damager
				playerScript.inflictEnvironmentalDamage(damagePerSecond*Time.deltaTime);
				// Throw back approacher
				// TODO throw back in normal direction of the thorn bush
				playerScript.applyEnvironmentalSlowDown(slowDownFactor);
			}
			
		}
		else if (enemyScript) 
		{
			enemyScript.currentEnvironment = this;

			if( enemyScript.shieldInUse != null && enemyScript.shieldInUse.abilityEnum != EAbilityType.EIceShieldAbility ) {
				// Nothing to do, enemy can enter
				// TODO play sound or such
			}
			else
			{
				// Enemy takes damager
				enemyScript.inflictEnvironmentalDamage(damagePerSecond*Time.deltaTime);
				// Throw back approacher
				// TODO throw back in normal direction of the thorn bush
				enemyScript.applyEnvironmentalSlowDown(slowDownFactor);
			}
		}
	}
}
