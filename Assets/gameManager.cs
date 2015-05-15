using UnityEngine;
using System.Collections;

public class gameManager : MonoBehaviour {
	
	private GameObject player;
	
	private player playerScript;

	private abilityManager abilityManager;

	private enemyManager enemyManager;

	private environmentManager environmentManager;

	private highscoreManager highscoreManager;

	private tutorialManager tutorialManager;

	private bool paused;

	private float timer;

	private bool bossDefeated;

	private abilityModificationPanel abilityModificationPanelScript;

	private GameObject mainMenu;

	private bool gameStarted;

	private GUIStyle textStyle = new GUIStyle();
	
	// Use this for initialization
	void Start () 
	{
		player = GameObject.Find("Blob");
		playerScript = (player)player.GetComponent(typeof(player));
		abilityManager = (abilityManager)GameObject.Find("AbilityManager").GetComponent (typeof(abilityManager));
		environmentManager = (environmentManager)GameObject.Find ("EnvironmentManager").GetComponent (typeof(environmentManager));
		highscoreManager = (highscoreManager)GameObject.Find ("HighscoreManager").GetComponent (typeof(highscoreManager));
		mainMenu = GameObject.Find ("MainMenu");

		// Active Abilities
		abilityManager.addAbilityToPlayer(player,EAbilityType.ERamAbility,0,4);
		abilityManager.addAbilityToPlayer(player,EAbilityType.EBiteAbility,1,1);
		// Shield abilities
		abilityManager.addAbilityToPlayer(player,EAbilityType.EThornShieldAbility,4,1);
		abilityManager.addAbilityToPlayer(player,EAbilityType.EElectricityShieldAbility,5,1);
		// Passive abilities
		abilityManager.addAbilityToPlayer(player,EAbilityType.ERunAbility,6,4);
		abilityManager.addAbilityToPlayer(player,EAbilityType.EViewAbility,7,5);

		bossDefeated = false;
		chooseNextEnvironmentalChange ();

		enemyManager = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		EAbilityType[] necessaryAbilities = {EAbilityType.EThornShieldAbility, EAbilityType.EWaterShieldAbility};
		enemyManager.setNecessaryAbilities(necessaryAbilities);

		abilityModificationPanelScript = (abilityModificationPanel)GameObject.Find ("AbilityModificationPanel").GetComponent (typeof(abilityModificationPanel));
	//	abilityModificationPanelScript.gameObject.SetActive (false);
		Time.timeScale = 0.0f;
	//	startGame ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (player && playerScript.size <= 0) 
		{
			print ("Game Over");
			Time.timeScale = 0.1f;
			gameStarted = false;
			abilityModificationPanelScript.gameObject.SetActive (false);
		}

		// Pause / resume game upon player input
		if (Input.GetButtonDown ("Pause"))
		{ 
			// Start game
			if(!gameStarted){
				startGame ();
				gameStarted = true;
				abilityModificationPanelScript.gameObject.SetActive (true);
				return;
			}

			if (paused) {
				// Continue
				playerScript.setStunned(0.0f);
				Time.timeScale = 1.0f;
				paused = false;
			}
			else
			{
				// Make sure ram ability is stopped
				playerScript.setStunned(9999999999999.0f);
				Time.timeScale = 0.0f;
				paused = true;
			}
		}

		// Make sure time stays at 0.0f (ram ability could interfer with the time)
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

	private void startGame() 
	{
		environmentManager.environmentOccuranceProbability = 0.7f;
		playerScript.dead = false;
		highscoreManager.resetHighscore ();
		highscoreManager.showHighscore (true);
		enemyManager.nofEnemies = 40;
		enemyManager.difficulty = 5;
		enemyManager.resetEnemies ();
		Time.timeScale = 1.0f;
		// Hide the main menu
	//	mainMenu.SetActive (false);
		abilityModificationPanelScript.gameObject.SetActive (false);
	}

	private void startTutorial(ETutorialType tutorialType) 
	{
		tutorialManager.activateTutorial (tutorialType);
	}

}
