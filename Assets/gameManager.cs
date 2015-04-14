using UnityEngine;
using System.Collections;

public class gameManager : MonoBehaviour {
	
	private GameObject player;
	
	private player playerScript;
	
	private GameObject abilityMngr;

	private abilityManager abilityManagerScript;

	private GameObject enemyMngr;

	private enemyManager enemyManagerScript;

	private GameObject environmentMngr;

	private environmentManager environmentManagerScript;

	private bool paused;

	private float timer;

	private bool bossDefeated;

	private abilityModificationPanel abilityModificationPanelScript;
	
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
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.EThornShieldAbility,4,1);
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.EElectricityShieldAbility,2,1);
		// Passive abilities
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.ERunAbility,6,4);
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.EViewAbility,7,5);

		bossDefeated = false;
		chooseNextEnvironmentalChange ();

		enemyMngr = GameObject.Find ("EnemyManager");
		enemyManagerScript = (enemyManager)enemyMngr.GetComponent (typeof(enemyManager));
		EAbilityType[] necessaryAbilities = {EAbilityType.EThornShieldAbility, EAbilityType.EWaterShieldAbility};
		enemyManagerScript.setNecessaryAbilities(necessaryAbilities);

		abilityModificationPanelScript = (abilityModificationPanel)GameObject.Find ("AbilityModificationPanel").GetComponent (typeof(abilityModificationPanel));

	}
	
	// Update is called once per frame
	void Update () {
		if (player && playerScript.size <= 0) 
		{
			print ("Game Over");
			Time.timeScale = 0.1f;
		}

		// Pause / resume game upon player input
		if (!abilityModificationPanelScript.isInChosingState && Input.GetButtonDown ("Pause"))
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

		// If the environmental change has finished, start the boss battle
		if (timer <= 0) {
			// Start boss battle
		} else {
			// Reduce timer
			timer -= Time.deltaTime;
		}

		// If the player has defeated the boss, then go to the next environmental change
		if (bossDefeated) {
			chooseNextEnvironmentalChange();
		}

	}

	private void chooseNextEnvironmentalChange()
	{
		// Choose the next environmental change at random
		int index = Random.Range (0, 9);
		timer = 6000.0f;
		bossDefeated = false;
	}

}
