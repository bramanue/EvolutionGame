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

	private lootManager lootManager;

	private bool paused;

	private float timer;

	private float gameTimer;

	private bool bossDefeated;

	private abilityModificationPanel abilityModificationPanelScript;

	private mainMenu mainMenu;

	private pauseMenu pauseMenu;

	private bool gameStarted;

	private GUIStyle textStyle = new GUIStyle();

	private Light directionalLight;

	public float dayLength = 180f;
	
	// Use this for initialization
	void Start () 
	{
		player = GameObject.Find("Blob");
		playerScript = (player)player.GetComponent(typeof(player));
		lootManager = (lootManager)GameObject.Find("LootManager").GetComponent (typeof(lootManager));
		abilityManager = (abilityManager)GameObject.Find("AbilityManager").GetComponent (typeof(abilityManager));
		environmentManager = (environmentManager)GameObject.Find ("EnvironmentManager").GetComponent (typeof(environmentManager));
		highscoreManager = (highscoreManager)GameObject.Find ("HighscoreManager").GetComponent (typeof(highscoreManager));
		mainMenu = (mainMenu)GameObject.Find ("MainMenu").GetComponent(typeof(mainMenu));
		pauseMenu = (pauseMenu)GameObject.Find ("PauseMenu").GetComponent(typeof(pauseMenu));
		directionalLight = (Light)GameObject.Find ("DirectionalLight").GetComponent<Light>();

		bossDefeated = false;
		chooseNextEnvironmentalChange ();

		enemyManager = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		EAbilityType[] necessaryAbilities = {EAbilityType.EThornShieldAbility, EAbilityType.EWaterShieldAbility};
		enemyManager.setNecessaryAbilities(necessaryAbilities);
		enemyManager.nofEnemies = 0;

		abilityModificationPanelScript = (abilityModificationPanel)GameObject.Find ("AbilityModificationPanel").GetComponent (typeof(abilityModificationPanel));

		Time.timeScale = 0.4f;

		playerScript.dead = true;

	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!gameStarted)
		{
			playerScript.setStunned(9999999999999.0f);
			highscoreManager.showHighscore (false);
			abilityModificationPanelScript.gameObject.SetActive (false);
			Time.timeScale = 0.4f;
		}

	/*	float anglesPerSecond = 180.0f / dayLength;
		directionalLight.transform.Rotate(new Vector3(1,0,0),Time.deltaTime*anglesPerSecond);
		if (directionalLight.transform.rotation.eulerAngles.x > 90) {
			float angle = (360f-directionalLight.transform.rotation.eulerAngles.x);
			directionalLight.intensity = angle/60f + 0.3f;
		}else{
			directionalLight.intensity = (directionalLight.transform.rotation.eulerAngles.x)/60f + 0.3f;
		}
*/
		if (gameStarted) {
			gameTimer += Time.deltaTime;
		}

		if (paused) 
		{
			playerScript.setStunned(9999999999999.0f);
			Time.timeScale = 0.0f;
		}

		if (gameStarted && playerScript.size <= 0) 
		{
			mainMenu.showGameOverScreen();
			Time.timeScale = 0.4f;
			playerScript.removeAllAbilities ();
			gameStarted = false;
			abilityModificationPanelScript.gameObject.SetActive (false);
		}

		// Pause / resume game upon player input
		if (Input.GetButtonDown ("Pause") && gameStarted)
		{ 
			if (paused) {
				// Continue
				continueGame();
			}
			else
			{
				// Make sure ram ability is stopped
				pauseMenu.show();
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

	public void startGame() 
	{
		environmentManager.environmentOccuranceProbability = 0.7f;
		playerScript.removeAllAbilities ();
		playerScript.dead = false;
		playerScript.size = 1.0f;
		playerScript.gameObject.transform.localScale = new Vector3 (1, 1, 0.5f);

		// Active Abilities
	//	abilityManager.addAbilityToPlayer(player,EAbilityType.ERamAbility,0,4);
		abilityManager.addAbilityToPlayer(player,EAbilityType.EBiteAbility,1,1);
		// Shield abilities
	//	abilityManager.addAbilityToPlayer(player,EAbilityType.EThornShieldAbility,4,1);
	//	abilityManager.addAbilityToPlayer(player,EAbilityType.EElectricityShieldAbility,5,1);
		// Passive abilities
		abilityManager.addAbilityToPlayer(player,EAbilityType.ERunAbility,6,0);
		abilityManager.addAbilityToPlayer(player,EAbilityType.EViewAbility,7,0);

		highscoreManager.resetHighscore ();
		highscoreManager.showHighscore (true);
		enemyManager.nofEnemies = 30;
		enemyManager.difficulty = 5;
		enemyManager.resetEnemies ();
		enemyManager.setEnemiesHostile (true);

				gameTimer = 0.0f;

		lootManager.removeAndDestroyAllLoot ();

		((BloomPro)GameObject.Find ("MainCamera").GetComponent (typeof(BloomPro))).ChromaticAberrationOffset = 1.0f;

		// Hide the main menu
		mainMenu.hide();
		abilityModificationPanelScript.resetPanel ();
		abilityModificationPanelScript.gameObject.SetActive (true);
		gameStarted = true;

		continueGame ();
	}

	public void finishGame()
	{
		playerScript.removeAllAbilities ();
		abilityModificationPanelScript.gameObject.SetActive (false);

		playerScript.dead = true;
		gameStarted = false;

		mainMenu.showMainMenu ();
		Time.timeScale = 0.4f;
		enemyManager.setEnemiesHostile (false);
	}

	public void continueGame() 
	{
		paused = false;
		playerScript.setStunned(0.0f);
		Time.timeScale = 1.0f;
		pauseMenu.hide ();
	}

	public void startTutorial(ETutorialType tutorialType) 
	{
		abilityModificationPanelScript.resetPanel ();
		tutorialManager.activateTutorial (tutorialType);
	}

}
