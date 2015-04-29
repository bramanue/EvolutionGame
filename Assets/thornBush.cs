using UnityEngine;
using System.Collections;

public class thornBush : hazardousEnvironment {

	private float throwBackTimer;

	// Use this for initialization
	void Start () {
		requiredAbility = EAbilityType.EThornShieldAbility;
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
		
			if( playerScript.shieldInUse != null && playerScript.shieldInUse.abilityEnum == EAbilityType.EThornShieldAbility ) {
				// Nothing to do, player can enter
				// TODO play sound or such
			}
			else
			{
				// Player takes damager
				playerScript.inflictEnvironmentalDamage(0.1f);
				// Throw back approacher
				// TODO throw back in normal direction of the thorn bush
				playerScript.addEnvironmentPushBackForce(playerScript.viewingDirection);
				// Disable player for a short time
				playerScript.setStunned(0.3f);
			}
				
		}
		else if (enemyScript) 
		{

			if( enemyScript.shieldInUse != null && enemyScript.shieldInUse.abilityEnum == EAbilityType.EThornShieldAbility ) {
				// Nothing to do, enemy can enter
				// TODO play sound or such
			}
			else
			{
				// Enemy takes damager
				enemyScript.inflictEnvironmentalDamage(0.1f);
				// Throw back approacher
				// TODO throw back in normal direction of the thorn bush
				enemyScript.addEnvironmentPushBackForce(enemyScript.viewingDirection);
				// Disable enemy for a short time
				enemyScript.setStunned(0.3f);
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

			if( playerScript.shieldInUse != null && playerScript.shieldInUse.abilityEnum == EAbilityType.EThornShieldAbility ) {
				// Nothing to do, player can enter
				// TODO play sound or such
			}
			else
			{
				// Player takes damager
				playerScript.size -= 0.1f;
				// Throw back approacher
				// TODO throw back in normal direction of the thorn bush
				other.gameObject.transform.position -= playerScript.viewingDirection;
				// Disable player for a short time
				playerScript.setStunned(0.3f);
			}
			
		}
		else if (enemyScript) 
		{
			enemyScript.currentEnvironment = this;

			if( enemyScript.shieldInUse != null && enemyScript.shieldInUse.abilityEnum == EAbilityType.EThornShieldAbility ) {
				// Nothing to do, enemy can enter
				// TODO play sound or such
			}
			else
			{
				// Enemy takes damager
				enemyScript.size -= 0.1f;
				// Throw back approacher
				// TODO throw back in normal direction of the thorn bush
				other.gameObject.transform.position -= enemyScript.viewingDirection;
				// Disable enemy for a short time
				enemyScript.setStunned(0.3f);
			}
		}
	}


}
