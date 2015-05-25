using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum ETutorialType {
	eatTutorial,
	environmentTutorial,
	abilityTutorial,
	noTutorial
};

public class tutorialManager : MonoBehaviour {

	private player playerScript;

	private gameManager gameManager;

	private enemyManager enemyManager;

	private abilityManager abilityManager;

	private highscoreManager highscoreManager;

	private environmentManager environmentManager;

	private lootManager lootManager;

	private ETutorialType activeTutorial;

	private bool currentTutorialComplete;

	public Vector3 playerStartPos;

	private bool waitForInput;

	private Text counter;

	private Text explanations;

	private bool UIShowing;

	private GameObject tutorialUI;

	private bool first;


	// EAT TUTORIAL
	int count = 0;



	// Use this for initialization
	void Start () 
	{
		gameManager = (gameManager)GameObject.Find ("GameManager").GetComponent (typeof(gameManager));
		lootManager = (lootManager)GameObject.Find ("LootManager").GetComponent (typeof(lootManager));
		enemyManager = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		abilityManager = (abilityManager)GameObject.Find ("AbilityManager").GetComponent (typeof(abilityManager));
		highscoreManager = (highscoreManager)GameObject.Find ("HighscoreManager").GetComponent (typeof(highscoreManager));
		environmentManager = (environmentManager)GameObject.Find ("EnvironmentManager").GetComponent (typeof(environmentManager));

		playerScript = (player)GameObject.Find ("Blob").GetComponent (typeof(player));

		tutorialUI = GameObject.Find ("TutorialUI");
		counter = (Text)GameObject.Find ("Counter").GetComponent<Text> ();
		explanations = (Text)GameObject.Find ("Explanation").GetComponent<Text> ();

		counter.text = "";
		explanations.text = "";

		activeTutorial = ETutorialType.noTutorial;
		currentTutorialComplete = false;
		UIShowing = false;

		first = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (first) {
			tutorialUI.SetActive (false);
			first = false;
		}

		if (gameManager.isPaused ()) {
			explanations.gameObject.SetActive (false);
			counter.gameObject.SetActive (false);
		} else if (activeTutorial != ETutorialType.noTutorial) {
			explanations.gameObject.SetActive (true);
			counter.gameObject.SetActive (true);
		}

		if (UIShowing) {

			playerScript.setStunned (999);
			Time.timeScale = 0.0f;
			if (Input.GetButton ("Ability1")) {
				playerScript.setStunned (0);
				UIShowing = false;
				explanations.text = "";
				counter.text = "0 / " + count;
				Time.timeScale = 1.0f;
				enemyManager.setEnemiesHostile (true);
			}
		} 


		// Perform update only if there is an active tutorial playing
		if (activeTutorial != ETutorialType.noTutorial) 
		{
			if(currentTutorialComplete)
			{
				if(!waitForInput) {
					waitForInput = true;
					explanations.text = "Congratulations! \n you successfully completed this tutorial.\n\nPress \"B\" to return to the main menu.";
				}
				else
				{
					if (Input.GetButton ("Ability1")) {
						currentTutorialComplete = false;
						activeTutorial = ETutorialType.noTutorial;
						gameManager.finishGame();
						counter.text = "";
						explanations.text = "";
						tutorialUI.SetActive (false);
					}
				}
			}
			else
			{
				counter.text = enemyManager.nofEnemiesKilled + " / " + count;
				if(enemyManager.nofEnemiesKilled >= count)
				{
					currentTutorialComplete = true;
					enemyManager.setEnemiesHostile (false);
				}
			}
		}
	}

	public void activateTutorial(ETutorialType tutorialType)
	{
		activeTutorial = tutorialType;
		currentTutorialComplete = false;
		environmentManager.reset ();
		abilityManager.reset ();
		enemyManager.reset ();


		switch (tutorialType) {
		case (ETutorialType.eatTutorial) :
			environmentManager.environmentOccuranceProbability = 0.0f;
			highscoreManager.showHighscore(false);
			playerScript.size = 1.0f;
			playerScript.baseVelocity = 9.0f;
			playerScript.gameObject.transform.localScale = new Vector3(1,1,1);
			enemyManager.maxEnemySize = 1.1f;
			enemyManager.nofEnemies = 10;
			enemyManager.maxNofAbilities = 0;
			enemyManager.respawnEnemies = false;
			enemyManager.resetEnemies();
			enemyManager.setEnemiesHostile (false);
			count = enemyManager.nofEnemies;
			explanations.text = "Goal of this tutorial is to make you familiar with eating other creatures. \nYou can eat any creature smaller than you by simply running over them. Creatures bigger than you will hunt you however - so be careful! If they hit you, you will shrink in size, making you a potential target for smaller creatures. The light emmited by the enemies indicates, whether that enemy is bigger or smaller than you. We hope the colors are self-explanatory ;) \n \n Eat all 10 enemies to complete this tutorial. \n\n Press \"B\" to start...";
			UIShowing = true;
			tutorialUI.SetActive (true);
			break;
		case (ETutorialType.abilityTutorial) :
			environmentManager.environmentOccuranceProbability = 0.0f;
			highscoreManager.showHighscore(false);
			playerScript.size = 1.0f;
			playerScript.baseVelocity = 9.0f;
			playerScript.gameObject.transform.localScale = new Vector3(1,1,1);
			abilityManager.addAbilityToPlayer (playerScript.gameObject, EAbilityType.EViewAbility, 7, 0);

			abilityManager.addAllAbilitiesToTheGame();

			enemyManager.nofEnemies = 20;
			enemyManager.maxNofAbilities = 3;
			enemyManager.respawnEnemies = false;
			enemyManager.resetEnemies();
			enemyManager.setEnemiesHostile (false);
			count = enemyManager.nofEnemies;
			explanations.text = "Goal of this tutorial is to make you more familiar with the abilities. \nMost enemies around you will have some abilities. When you defeat them, they might drop their ability in form of a glowing cube. You can gather that ability by pressing \"RB\" or \"LB\" when you get close to such a cube. On the top right of the screen you will see the name of the ability and a short explanation to how it works in the center of the screen. \nAttack abilities allow you to attack enemies larger than you. If your attack hits, they will shrink in size, eventually allowing you to eat them. Attack abilities can be mapped to any of the four colored buttons on the gamepad. \nShield abilities, which you should already know from the previous tutorial can only be mapped to the triggers on the top of the gamepad. \n \nDefeat and eat all 20 enemies to complete this tutorial. \n\n Press \"B\" to start...";
			UIShowing = true;
			tutorialUI.SetActive (true);

			GameObject ramAbilityObject = abilityManager.getPrefab(EAbilityType.ERamAbility);
			ability ramAbility = (ability)(ramAbilityObject.GetComponent(typeof(ability)));
			lootManager.throwAbilityLoot(ramAbility,5,playerScript.transform.position, playerScript.transform.position + new Vector3(0,1.5f,0));
			GameObject.Destroy(ramAbilityObject);

			break;
		case (ETutorialType.environmentTutorial):

			EAbilityType abilityType = EAbilityType.EEmptyAbility;
			float thetaStepSize = 2.0f*Mathf.PI/16.0f;
			EEnvironmentClass environmentClass = environmentManager.getRandomEnvrionmentClass();
			for(float theta = 0; theta < 2.0*Mathf.PI; theta += thetaStepSize)
			{
				Vector3 position = playerScript.transform.position + new Vector3(10.0f*Mathf.Cos(theta), 10.0f*Mathf.Sin(theta),0);
				hazardousEnvironment hazard = environmentManager.addEnvironmentObstacle(environmentClass,position,false,2.0f,false,new Vector2(0,0));
				hazard.damagePerSecond = 1.0f;
				hazard.slowDownFactor = 0.5f;
				abilityType = hazard.requiredAbility;
			}
			GameObject shieldAbilityObject = abilityManager.getPrefab(abilityType);
			ability shieldAbility = (ability)(shieldAbilityObject.GetComponent(typeof(ability)));
			lootManager.throwAbilityLoot(shieldAbility,10,playerScript.transform.position,playerScript.transform.position + new Vector3(0,1.5f,0));
			GameObject.Destroy(shieldAbilityObject);

			environmentManager.environmentOccuranceProbability = 0.0f;
		//	playerScript.gameObject.transform.position = playerStartPos;
			highscoreManager.showHighscore(false);
			playerScript.size = 1.0f;
			playerScript.baseVelocity = 9.0f;
			playerScript.gameObject.transform.localScale = new Vector3(1,1,1);

			enemyManager.maxEnemySize = 1.2f;
			enemyManager.nofEnemies = 10;
			enemyManager.maxNofAbilities = 0;
			enemyManager.resetEnemies();
			enemyManager.respawnEnemies = false;
			enemyManager.setEnemiesHostile (false);

			count = enemyManager.nofEnemies;
			explanations.text = "Goal of this tutorial is to make you more familiar with the environments and the shield abilities. \nYour prismus is surrounded by a dangerous environment. Touching it will hurt and eventually kill your prismus. For the sake of this tutorial, the environments deal more damage than they would in the game ;) But there is a shield ability lying just in front of you in form of a glowing cube. You can collect it by pressing \"RB\" or \"LB\" when you get near it. Now you only have to press one of the two triggers on the gamepad. The shield ability will then be mapped to that trigger and you can activate it anytime by pressing that trigger again. With the shield activated you can leave the circle of dangerous environments and eat all enemies outside the ring. As you will see, some shields do also damage enemies, when you touch them - no matter their size. \n\nDefeat and eat all 10 enemies to complete this tutorial. \n\n Press \"B\" to start...";
			UIShowing = true;
			tutorialUI.SetActive (true);
			break;
		default:
			activeTutorial = ETutorialType.noTutorial;
			currentTutorialComplete = true;
			UIShowing = false;
			tutorialUI.SetActive (false);
			break;
		};
	}

	public void deactivateTutorial() 
	{
		activeTutorial = ETutorialType.noTutorial;
	}

}
