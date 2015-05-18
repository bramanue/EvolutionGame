using UnityEngine;
using System.Collections;

public enum ETutorialType {
	eatTutorial,
	environmentTutorial,
	abilityTutorial,
	noTutorial
};

public class tutorialManager : MonoBehaviour {

	private player playerScript;

	private enemyManager enemyManager;

	private highscoreManager highscoreManager;

	private environmentManager environmentManager;

	private lootManager lootManager;

	private ETutorialType activeTutorial;

	private bool currentTutorialComplete;

	public Vector3 playerStartPos;


	// EAT TUTORIAL
	int count = 0;



	// Use this for initialization
	void Start () 
	{
		lootManager = (lootManager)GameObject.Find ("LootManager").GetComponent (typeof(lootManager));
		enemyManager = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		highscoreManager = (highscoreManager)GameObject.Find ("HighscoreManager").GetComponent (typeof(highscoreManager));
		environmentManager = (environmentManager)GameObject.Find ("HighscoreManager").GetComponent (typeof(environmentManager));

		playerScript = (player)GameObject.Find ("Blob").GetComponent (typeof(player));

		if(activeTutorial == null)
			activeTutorial = ETutorialType.noTutorial;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Perform update only if there is an active tutorial playing
		if (activeTutorial != ETutorialType.noTutorial) 
		{
			if(currentTutorialComplete)
			{

			}
			else
			{
				if(activeTutorial == ETutorialType.eatTutorial || activeTutorial == ETutorialType.abilityTutorial) {
					if(count <= 0)
					{
						currentTutorialComplete = true;
						enemyManager.setEnemiesHostile (false);
					}
				}
			}
		}
	}

	public void activateTutorial(ETutorialType tutorialType)
	{
		activeTutorial = tutorialType;
		currentTutorialComplete = false;

		switch (tutorialType) {
		case (ETutorialType.eatTutorial) :
			environmentManager.environmentOccuranceProbability = 0.0f;
			highscoreManager.showHighscore(false);
			playerScript.size = 1.0f;
			playerScript.gameObject.transform.localScale = new Vector3(1,1,1);
			enemyManager.nofEnemies = 10;
			enemyManager.maxNofAbilities = 0;
			enemyManager.respawnEnemies = false;
			enemyManager.resetEnemies();
			enemyManager.setEnemiesHostile (true);
			count = 10;
			break;
		case (ETutorialType.abilityTutorial) :
			environmentManager.environmentOccuranceProbability = 0.0f;
			highscoreManager.showHighscore(false);
			playerScript.size = 1.0f;
			playerScript.gameObject.transform.localScale = new Vector3(1,1,1);
			enemyManager.nofEnemies = 20;
			enemyManager.maxNofAbilities = 2;
			enemyManager.respawnEnemies = false;
			enemyManager.resetEnemies();
			enemyManager.setEnemiesHostile (true);
			count = 20;
			break;
		case (ETutorialType.environmentTutorial):
			environmentManager.environmentOccuranceProbability = 0.0f;
			playerScript.gameObject.transform.position = playerStartPos;
			highscoreManager.showHighscore(false);
			playerScript.size = 1.0f;
			playerScript.gameObject.transform.localScale = new Vector3(1,1,1);
			enemyManager.nofEnemies = 10;
			enemyManager.maxNofAbilities = 0;
			enemyManager.respawnEnemies = false;
			enemyManager.resetEnemies();
			enemyManager.setEnemiesHostile (true);
			count = 10;
			break;
		default:
			activeTutorial = ETutorialType.noTutorial;
			currentTutorialComplete = true;
			break;
		};

	}

	public void deactivateTutorial() 
	{
		activeTutorial = ETutorialType.noTutorial;
	}

	public void enemyKilled() {
		if (activeTutorial == ETutorialType.eatTutorial || activeTutorial == ETutorialType.abilityTutorial)
			count--;
	}
}
