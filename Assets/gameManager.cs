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

	private bool tutorialStarted;

	private GUIStyle textStyle = new GUIStyle();

	private Light directionalLight;

	public float dayLength = 180f;

	private int stage;

	public float stageTimer;

	public bool hideUI;

	private GameObject tutorialUI;

	public bool restartTutorialPending;

	private ETutorialType activeTutorialType;
	
	// Use this for initialization
	void Start () 
	{
		player = GameObject.Find("Blob");
		playerScript = (player)player.GetComponent(typeof(player));
		lootManager = (lootManager)GameObject.Find("LootManager").GetComponent (typeof(lootManager));
		abilityManager = (abilityManager)GameObject.Find("AbilityManager").GetComponent (typeof(abilityManager));
		tutorialManager = (tutorialManager)GameObject.Find ("TutorialManager").GetComponent (typeof(tutorialManager));
		environmentManager = (environmentManager)GameObject.Find ("EnvironmentManager").GetComponent (typeof(environmentManager));
		highscoreManager = (highscoreManager)GameObject.Find ("HighscoreManager").GetComponent (typeof(highscoreManager));

		tutorialUI = GameObject.Find ("TutorialUI");
		mainMenu = (mainMenu)GameObject.Find ("MainMenu").GetComponent(typeof(mainMenu));
		pauseMenu = (pauseMenu)GameObject.Find ("PauseMenu").GetComponent(typeof(pauseMenu));
		directionalLight = (Light)GameObject.Find ("DirectionalLight").GetComponent<Light>();

		enemyManager = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		EAbilityType[] necessaryAbilities = {EAbilityType.EThornShieldAbility, EAbilityType.EWaterShieldAbility};
		enemyManager.setNecessaryAbilities(necessaryAbilities);
		enemyManager.nofEnemies = 10;

		abilityModificationPanelScript = (abilityModificationPanel)GameObject.Find ("AbilityModificationPanel").GetComponent (typeof(abilityModificationPanel));

		Time.timeScale = 0.4f;

		playerScript.dead = true;

	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!gameStarted && !tutorialStarted)
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
		if (gameStarted) 
		{
			gameTimer += Time.deltaTime;
			stageTimer += Time.deltaTime;

			if(stageTimer > 45)
			{
				if(stage == 0 && playerScript.size > 1.5f) {
					// Add ram ability to the game
					abilityManager.addAbilityToTheGame(EAbilityType.ERamAbility, EAbilityClass.EActiveAbility);
					abilityManager.addAbilityToTheGame(EAbilityType.EViewAbility, EAbilityClass.EPassiveAbility);
					// Increase enemy size
					enemyManager.maxNofAbilities = 1;
					stage = 1;
					stageTimer = 0;
				}
				else if(stage == 1) {
					// Add passive abilities to the game
					abilityManager.addAbilityToTheGame(EAbilityType.ERunAbility, EAbilityClass.EPassiveAbility);
					stageTimer = 0;
					stage = 2;
				}
				else if(stage == 2) {
					// Add first environment && corresponding shield
					environmentManager.maxNofEnvironmentTypes = 1;
					EEnvironmentClass envClass = environmentManager.addRandomPossibleEnvironment();
					switch (envClass) {
					case EEnvironmentClass.EDesertEnvironment :
						abilityManager.addAbilityToTheGame(EAbilityType.EDustShieldAbility,EAbilityClass.EShieldAbility);
						break;
					case EEnvironmentClass.EIceEnvironment :
						abilityManager.addAbilityToTheGame(EAbilityType.EIceShieldAbility,EAbilityClass.EShieldAbility);
						break;
					case EEnvironmentClass.EElectricityEnvironment :
						abilityManager.addAbilityToTheGame(EAbilityType.EElectricityShieldAbility,EAbilityClass.EShieldAbility);
						break;
					case EEnvironmentClass.ELavaEnvironment :
						abilityManager.addAbilityToTheGame(EAbilityType.ELavaShieldAbility,EAbilityClass.EShieldAbility);
						break;
					case EEnvironmentClass.EThornEnvironment :
						abilityManager.addAbilityToTheGame(EAbilityType.EThornShieldAbility,EAbilityClass.EShieldAbility);
						break;
					case EEnvironmentClass.EWaterEinviornment :
						abilityManager.addAbilityToTheGame(EAbilityType.EWaterShieldAbility,EAbilityClass.EShieldAbility);
						break;
					default :
						break;
					};
					// Add a random shield
					abilityManager.addRandomShieldAbilityToTheGame();

					enemyManager.maxNofAbilities++;
					stageTimer = 0;
					stage = 3;
				}
				else if (stage == 3) {
					// Add sting ability to the game and a random shield
					abilityManager.addAbilityToTheGame(EAbilityType.EBiteAbility, EAbilityClass.EActiveAbility);
					abilityManager.addRandomShieldAbilityToTheGame();
					stageTimer = 0;
					stage = 4;
				}
				else if (stage == 4) {
					// Increase difficulty, add / replace environments
					float rnd = Random.value;
					if(rnd > 0.96f) 
					{
						enemyManager.difficulty = Mathf.Max (enemyManager.difficulty, 1);
					}
					else if (rnd > 0.7f)
					{
						abilityManager.addRandomAbilityToTheGame();
					}
					else if (rnd > 0.5f)
					{
						environmentManager.maxNofEnvironmentTypes = Mathf.Min (4,environmentManager.maxNofEnvironmentTypes+1);
					}
					else if (rnd > 0.3f)
					{
						environmentManager.addRandomPossibleEnvironment();
					}
					else if (rnd > 0.2f)
					{
						//environmentManager.removeRandomPossibleEnvironment();
					}
					else if (rnd > 0.05)
					{
						enemyManager.maxNofAbilities++;
					}
					else
					{
						environmentManager.maxNofEnvironmentTypes = Mathf.Max (1,environmentManager.maxNofEnvironmentTypes-1);
					}

					stageTimer = 0;
				}
			}
		}

		if (paused) 
		{
			playerScript.setStunned(9999999999999.0f);
			Time.timeScale = 0.0f;
		}


		if (gameStarted || tutorialStarted) {
			// Cheats to add specific abilities to the player
			if(Input.GetKeyDown("v")) {
				abilityManager.addAbilityToPlayer(player,EAbilityType.ERunAbility,6,2);
			}
			if(Input.GetKeyDown("r")) {
				abilityManager.addAbilityToPlayer(player,EAbilityType.ERamAbility,0,6);
			}
			if(Input.GetKeyDown("b")) {
				abilityManager.addAbilityToPlayer(player,EAbilityType.EBiteAbility,1,6);
			}
			if(Input.GetKeyDown("t")) {
				abilityManager.addAbilityToPlayer(player,EAbilityType.EThornShieldAbility,4,10);
			}
			if(Input.GetKeyDown("l")) {
				abilityManager.addAbilityToPlayer(player,EAbilityType.ELavaShieldAbility,5,10);
			}
			if(Input.GetKeyDown("w")) {
				abilityManager.addAbilityToPlayer(player,EAbilityType.EWaterShieldAbility,4,10);
			}
			if(Input.GetKeyDown("e")) {
				abilityManager.addAbilityToPlayer(player,EAbilityType.EElectricityShieldAbility,5,10);
			}
			if(Input.GetKeyDown("s")) {
				abilityManager.addAbilityToPlayer(player,EAbilityType.EDustShieldAbility,4,10);
			}
			if(Input.GetKeyDown("i")) {
				abilityManager.addAbilityToPlayer(player,EAbilityType.EIceShieldAbility,5,10);
			}

			if(playerScript.size <= 0) {

				Time.timeScale = 0.4f;
				playerScript.removeAllAbilities ();

				if(tutorialStarted) {
					mainMenu.showGameOverScreen(false,0);
					restartTutorialPending = true;
					tutorialStarted = false;
				}
				else {
					mainMenu.showGameOverScreen(true, highscoreManager.getHighscore());
					gameStarted = false;
				}
				tutorialUI.SetActive (false);
				abilityModificationPanelScript.gameObject.SetActive (false);
			}

			// Pause / resume game upon player input
			if (Input.GetButtonDown ("Pause"))
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
		}


		// Make sure time stays at 0.0f (ram ability could interfer with the time)
		if (paused) {
			Time.timeScale = 0.0f;
			playerScript.setStunned (9999999999999.0f);
		}
	}


	public void startGame() 
	{
		if (restartTutorialPending) {
			startTutorial (activeTutorialType);
		} 
		else 
		{
			environmentManager.environmentOccuranceProbability = 0.7f;

			playerScript.removeAllAbilities ();
			playerScript.dead = false;
			playerScript.size = 1.0f;
			playerScript.baseVelocity = 8.0f;
			playerScript.baseViewingRange = 5.0f;
			playerScript.viewingRangeBoost = 0.0f;
			playerScript.runVelocityBoost = 0.0f;
			playerScript.currentViewingRange = playerScript.baseViewingRange;
			playerScript.gameObject.transform.localScale = new Vector3 (1, 1, 0.5f);
			((MeshRenderer)playerScript.gameObject.GetComponent<MeshRenderer> ()).material = playerScript.defaultMaterial;

			stage = 0;
			stageTimer = 0.0f;

			lootManager.reset ();
			abilityManager.reset ();
			environmentManager.reset ();

			abilityManager.addAbilityToPlayer (player, EAbilityType.EViewAbility, 7, 0);

			highscoreManager.resetHighscore ();
			if (!hideUI)
				highscoreManager.showHighscore (true);

			enemyManager.reset ();
			enemyManager.maxNofAbilities = 0;
			enemyManager.nofEnemies = 30;
			enemyManager.difficulty = 5;
			enemyManager.resetEnemies ();
			enemyManager.setEnemiesHostile (true);

			gameTimer = 0.0f;

			((BloomPro)GameObject.Find ("MainCamera").GetComponent (typeof(BloomPro))).ChromaticAberrationOffset = 1.0f;

			// Hide the main menu
			mainMenu.hide ();
			abilityModificationPanelScript.resetPanel ();
			if (!hideUI)
				abilityModificationPanelScript.gameObject.SetActive (true);
			gameStarted = true;
			tutorialStarted = false;

			continueGame ();
		}
	}

	public void finishGame()
	{
		playerScript.removeAllAbilities ();
		abilityModificationPanelScript.gameObject.SetActive (false);
		tutorialUI.SetActive (false);

		playerScript.dead = true;
		tutorialStarted = false;
		gameStarted = false;
		paused = false;

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

	public void startEnvironmentTutorial()
	{
		startTutorial (ETutorialType.environmentTutorial);
	}

	public void startEatTutorial()
	{
		startTutorial (ETutorialType.eatTutorial);
	}

	public void startAbilityTutorial()
	{
		startTutorial (ETutorialType.abilityTutorial);
	}

	private void startTutorial(ETutorialType tutorialType) 
	{
		activeTutorialType = tutorialType;

		lootManager.reset ();
		enemyManager.reset ();
		abilityManager.reset ();
		environmentManager.reset ();

		playerScript.removeAllAbilities ();
		playerScript.dead = false;
		playerScript.size = 1.0f;
		playerScript.viewingRangeBoost = 0.0f;
		playerScript.runVelocityBoost = 0.0f;
		playerScript.currentViewingRange = playerScript.baseViewingRange;
		playerScript.gameObject.transform.localScale = new Vector3 (1, 1, 0.5f);
		((MeshRenderer)playerScript.gameObject.GetComponent<MeshRenderer>()).material = playerScript.defaultMaterial;

		mainMenu.hide ();
		if(!hideUI)
			abilityModificationPanelScript.gameObject.SetActive (true);
		abilityModificationPanelScript.resetPanel ();
		tutorialManager.activateTutorial (tutorialType);

		tutorialStarted = true;

		continueGame ();
	}

	public bool isGameRunning() {
		return gameStarted;
	}

	public bool isPaused() {
		return paused;
	}

}
