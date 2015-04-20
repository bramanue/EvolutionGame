using UnityEngine;
using System.Collections;

public class water : hazardousEnvironment {

	// Use this for initialization
	void Start () {
		requiredAbility = EAbilityType.EWaterShieldAbility;
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
			if(playerScript.shieldInUse != null && playerScript.shieldInUse.getAbilityEnum() == EAbilityType.EWaterShieldAbility) {
				// Nothing to do, player can stay
			}
			else
			{
				// Throw back approacher
				Vector3 repulsionNormal = getRepulsionNormal(other.gameObject, null, playerScript);
				repulsionNormal = -playerScript.viewingDirection;
				playerScript.addEnvironmentPushBackForce(Time.deltaTime*playerScript.currentSpeed*slowDownFactor*repulsionNormal);
			}
			
		}
		else if (enemyScript) 
		{
			if(enemyScript.shieldInUse != null && enemyScript.shieldInUse.getAbilityEnum() == EAbilityType.EWaterShieldAbility) {
				// Nothing to do, player can stay
			}
			else
			{
				// Throw back approacher
				Vector3 repulsionNormal = getRepulsionNormal(other.gameObject, enemyScript, null);
				enemyScript.addEnvironmentPushBackForce(Time.deltaTime*enemyScript.currentSpeed*slowDownFactor*repulsionNormal);
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
		/*	if( playerScript.hasAbility(EAbilityType.EWaterShieldAbility) != -1 ) {
				// Nothing to do, player can stay
			}*/
			if(playerScript.shieldInUse != null && playerScript.shieldInUse.getAbilityEnum() == EAbilityType.EWaterShieldAbility) {
				// Nothing to do, player can stay
			}
			else
			{
				// Throw back approacher
				Vector3 repulsionNormal = getRepulsionNormal(other.gameObject, null, playerScript);
				repulsionNormal = -playerScript.viewingDirection;
				playerScript.applyEnvironmentalSlowDown(slowDownFactor);
				// Start drowning (damage dependent on frames per second)
				playerScript.inflictEnvironmentalDamage(Time.deltaTime*damagePerSecond);
			}
			
		}
		else if (enemyScript) 
		{
			enemyScript.currentEnvironment = this;
			if(enemyScript.shieldInUse != null && enemyScript.shieldInUse.getAbilityEnum() == EAbilityType.EWaterShieldAbility) {
				// Nothing to do, player can stay
			}
			else
			{
				// Throw back approacher
				Vector3 repulsionNormal = getRepulsionNormal(other.gameObject, enemyScript, null);
				enemyScript.applyEnvironmentalSlowDown(slowDownFactor);
				// Start drowning (damage dependent on frames per second)
				enemyScript.inflictEnvironmentalDamage(Time.deltaTime*damagePerSecond);
			}
		}
	}

	private Vector3 getRepulsionNormal(GameObject blob, enemy enemyScript, player playerScript)
	{
		RaycastHit hit;
		Vector3 right = blob.gameObject.transform.right;

		if (enemyScript) 
		{
			float closestColliderDistance = enemyScript.size + 10000.0f;
			Vector3 repulsionNormal = -enemyScript.viewingDirection;
			// Shoot 3 rays: one along the viewing direction, one 45° to the left and one 45° to the right
			for (int i = -1; i <= 1; i++) {
				Vector3 rayDirection = (enemyScript.viewingDirection + i * right);
				// Search close proximity for collider object
				if (Physics.Raycast (blob.gameObject.transform.position, rayDirection, out hit, enemyScript.size + 1.0f)) { 
					// Get the point on the bounding box of the hazardous environment)
					if (hit.collider.gameObject.GetComponent(typeof(hazardousEnvironment))) {
						// If this is the closest obstacle, then take the normal of this object for repulsion
						if (hit.distance < closestColliderDistance)
							repulsionNormal = hit.normal;
					}
				}
			}
			return repulsionNormal;
		} 
		else 
		{
			float closestColliderDistance = playerScript.size + 10000.0f;
			Vector3 repulsionNormal = -playerScript.viewingDirection;
			// Shoot 3 rays: one along the viewing direction, one 90° to the left and one 90° to the right
			for (int i = -1; i <= 1; i++) {
				Vector3 rayDirection = ((1-Mathf.Abs(i))*playerScript.viewingDirection + i * right).normalized;
				// Search close proximity for collider object
				if (Physics.Raycast (blob.gameObject.transform.position, rayDirection, out hit, playerScript.size*2.0f)) { 
					// Get the point on the bounding box of the hazardous environment)
					if (hit.collider.gameObject.GetComponent(typeof(hazardousEnvironment))) {
						// If this is the closest obstacle, then take the normal of this object for repulsion
						if (hit.distance < closestColliderDistance)
							repulsionNormal = hit.normal;
					}
				}
			}

			return repulsionNormal;
		}
		return new Vector3(0,0,0);
	}
	
}
