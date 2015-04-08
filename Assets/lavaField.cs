using UnityEngine;
using System.Collections;

public class lavaField : hazardousEnvironment {
	
	private float throwBackTimer;
	
	// Use this for initialization
	void Start () {
		requiredAbility = EAbilityType.ELavaShieldAbility;
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
			if( playerScript.hasAbility(EAbilityType.ELavaShieldAbility) != -1 ) {
				// Nothing to do, player can enter
				// TODO play sound or such
			}
			else
			{
				Debug.Log ("Player has no thorn shield!");
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
			if( enemyScript.hasAbility(EAbilityType.ELavaShieldAbility) != -1 ) {
				// Nothing to do, enemy can enter
				// TODO play sound or such
			}
			else
			{
				Debug.Log ("Enemy has no thorn shield!");
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
