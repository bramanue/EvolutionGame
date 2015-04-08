using UnityEngine;
using System.Collections;

public class water : hazardousEnvironment {
	
	private float throwBackTimer;
	
	// Use this for initialization
	void Start () {
		requiredAbility = EAbilityType.EWaterShieldAbility;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log ("Collision detected");
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent(typeof(player));
		
		if (playerScript) 
		{
			if( playerScript.hasAbility(EAbilityType.EWaterShieldAbility) != -1 ) {
				// Nothing to do, player can enter
				// TODO play sound or such
			}
			else
			{
				Debug.Log ("Player cannot swim!");
				// Throw back approacher
				// TODO throw back in normal direction of the water
				other.gameObject.transform.position -= Time.deltaTime*playerScript.currentSpeed*playerScript.viewingDirection;
				// Disable player for a short time
				playerScript.setStunned(0.1f);
			}
			
		}
		else if (enemyScript) 
		{
			if( enemyScript.hasAbility(EAbilityType.EWaterShieldAbility) != -1 ) {
				// Nothing to do, enemy can enter
				// TODO play sound or such
			}
			else
			{
				Debug.Log ("Enemy cannot swim!");
				// Throw back approacher
				// TODO throw back in normal direction of the water
				other.gameObject.transform.position -= enemyScript.viewingDirection;
				// Disable enemy for a short time
				enemyScript.setStunned(0.2f);
			}
		}
	}

	void OnTriggerStay2D(Collider2D other) {
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent(typeof(player));
		
		if (playerScript) 
		{
			if( playerScript.hasAbility(EAbilityType.EWaterShieldAbility) != -1 ) {
				// Nothing to do, player can enter
				// TODO play sound or such
			}
			else
			{
				Debug.Log ("Player cannot swim!");
				// Throw back approacher
				// TODO throw back in normal direction of the water
				other.gameObject.transform.position -= Time.deltaTime*playerScript.currentSpeed*playerScript.viewingDirection;
				// Disable player for a short time
			}
			
		}
		else if (enemyScript) 
		{
			if( enemyScript.hasAbility(EAbilityType.EWaterShieldAbility) != -1 ) {
				// Nothing to do, enemy can enter
				// TODO play sound or such
			}
			else
			{
				Debug.Log ("Enemy cannot swim!");
				// Throw back approacher
				// TODO throw back in normal direction of the water
				other.gameObject.transform.position -= Time.deltaTime*enemyScript.currentSpeed*playerScript.viewingDirection;
				// Disable enemy for a short time
			}
		}
	}
	
}
