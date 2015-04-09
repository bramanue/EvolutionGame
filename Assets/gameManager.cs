using UnityEngine;
using System.Collections;

public class gameManager : MonoBehaviour {
	
	private GameObject player;
	
	private player playerScript;
	
	private GameObject abilityMngr;

	private abilityManager abilityManagerScript;

	private bool paused;
	
	// Use this for initialization
	void Start () 
	{
		player = GameObject.Find("Blob");
		playerScript = (player)player.GetComponent(typeof(player));
		abilityMngr = GameObject.Find("AbilityManager");
		abilityManagerScript = (abilityManager)abilityMngr.GetComponent (typeof(abilityManager));
		// Acive Abilities
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.ERamAbility,0,4);
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.EBiteAbility,1,1);
		// Shield abilities
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.EThornShieldAbility,2,1);
		// Passive abilities
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.ERunAbility,6,4);
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.EViewAbility,7,5);
	}
	
	// Update is called once per frame
	void Update () {
		if (player && playerScript.size <= 0) 
		{
			print ("Game Over");
			Time.timeScale = 0.1f;
		}

		if (Input.GetButtonDown ("Pause"))
		{ 
			if (paused) {
				playerScript.setStunned(0.0f);
				Time.timeScale = 1.0f;
				paused = false;
			}
			else
			{
				playerScript.setStunned(9999999999999.0f);
				Time.timeScale = 0.0f;
				paused = true;
			}
		}

		if (paused) {
			Time.timeScale = 0.0f;
			playerScript.setStunned (9999999999999.0f);
		}

	}

}
