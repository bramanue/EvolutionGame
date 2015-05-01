using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class enemyManager : MonoBehaviour {

	// The numbe rof enemies on the field.
	public int nofEnemies;

	// The radius in which enemies can spawn
	public float radius;

	// The prototype for each enemy
	public GameObject basicEnemy;

	// Pointer to the player
	private GameObject player;

	// Pointer to the script of the player
	private player playerScript;

	// Local timer for update
	private float timer;

	// List<GameObject> enemies = new List<GameObject>();
	private GameObject[] enemyGameObjects = new GameObject[100];

	private enemy[] enemyScripts = new enemy[100];

	private GameObject abilityMngr;
	
	private abilityManager abilityManagerScript;

	private EAbilityType[] necessaryAbilities;

	private int[] playerAbilityLevels;

	public Mesh[] enemyMeshes = new Mesh[4];

	// 1 is the highest difficulty and 5 the lowest
	// Defines for each enemy, how many abilities it gets
	public float difficulty;


	// Use this for initialization
	void Start () 
	{
		// Get an instance of the player
		player = GameObject.Find("Blob");
		// Get the player script
		playerScript = (player)player.GetComponent (typeof(player));
		// Get the ability manager
		abilityMngr = GameObject.Find("AbilityManager");
		// Get the ability manager script
		abilityManagerScript = (abilityManager)abilityMngr.GetComponent(typeof(abilityManager));

		// Create the basic amount of enemies
		for(int i = 0; i < nofEnemies; i++) 
		{
			// Calculate a random spawn position
			Vector3 spawnPoint = calculateSpawnPosition();
			// Calculate random initial rotation
			Quaternion q = Quaternion.Euler(new Vector3(0.0f,0.0f,Random.Range(-180,180)));
			// Create new enemy GameObject
			enemyGameObjects[i] = ((GameObject)GameObject.Instantiate(basicEnemy,spawnPoint,q));
			// Add enemy script to the array
			enemyScripts[i] = (enemy)enemyGameObjects[i].GetComponent(typeof(enemy));
			// Set values for this script
			setRandomInitialValues(enemyScripts[i], enemyGameObjects[i]);
		}
	}
	
	// Update is called once per frame
	void Update () {

		// Make sure the radius increases with the player's size
		radius = 4.0f*playerScript.currentViewingRange + nofEnemies*player.transform.localScale.x + 2.0f*playerScript.maxVelocity;

		// Get the current position of the player
		Vector3 playerPosition = player.transform.position;
		// Loop over all enemies and check whether they need to be repositioned (out of the radius)
		for (int i = 0; i < nofEnemies; i++) 
		{
			float distance = (enemyGameObjects [i].transform.position - playerPosition).magnitude;
			if (distance > radius)
			{
				setRandomInitialValues (enemyScripts [i], enemyGameObjects [i]);
				distance = (enemyGameObjects [i].transform.position - playerPosition).magnitude;
			}	

			// Respawn defeated enemies
			if(enemyGameObjects[i].transform.localScale.x <= 0) 
			{
				setRandomInitialValues (enemyScripts [i], enemyGameObjects [i]);
				distance = (enemyGameObjects [i].transform.position - playerPosition).magnitude;
			}

			// Deactivate enemies if they are too far from the player
			if(distance > 2.2f*(playerScript.currentViewingRange + player.transform.localScale.x + enemyGameObjects[i].transform.localScale.x))
				enemyGameObjects[i].SetActive(false);
			else
				enemyGameObjects[i].SetActive(true);
		}

	}

	public void respawnEnemy(GameObject enemy) 
	{
		// Get the script of this enemy
		enemy enemyScript = (enemy)enemy.GetComponent(typeof(enemy));
		// Set random initial values for this enemy
		setRandomInitialValues (enemyScript, enemy);
	}

	private void setRandomInitialValues(enemy enemyScript, GameObject enemyObject)
	{
		float playerSize = player.transform.localScale.x;
		// Define a random size
		float size = Random.Range(playerSize - 0.5f*playerSize, playerSize + 0.5f*playerSize);
		// Set size of GameObject
		enemyObject.transform.localScale = new Vector3(size,size,size);
		// Get spawn point...
		Vector3 spawnPoint = calculateSpawnPosition();
		// ...and put enemy there
		enemyObject.transform.position = spawnPoint;

		// Set values for this script
		enemyScript.size = size;
		enemyScript.transform.localScale = new Vector3 (size, size, size);
		enemyScript.viewingRange = (size + Random.Range(7.0f,12.0f));
		enemyScript.idleOperationRadius = size + 2.0f*enemyScript.viewingRange;
		enemyScript.activeOperationRadius = size + enemyScript.maxVelocity*15.0f;
		enemyScript.baseViewingRange = Random.Range(5.0f,8.0f);
		enemyScript.cosViewingAngle = Random.Range(0.0f,0.7f);
		enemyScript.baseVelocity = Random.Range(4.0f,6.0f);

		enemyScript.resetAllStates();

		// Calculate how many abilities this enemy should get
		int nofAbilities = (int)Mathf.Floor(1.0f/Mathf.Exp(difficulty*Random.value) * 8 + 0.8f);
		float rndValue = Random.value;
		// Decide which abilities it should get
		switch (nofAbilities) {
		case 0: 
			break;
		case 1: 
			if (rndValue > 0.5)
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomPassiveAbility (), 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
			else
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 4, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			break;
		case 2:
			if (rndValue > 0.5) {
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomPassiveAbility (), 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 4, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			} else {
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomPassiveAbility (), 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 0, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			}
			break;
		case 3:
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 0, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomPassiveAbility (), 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 4, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			break;
		case 4:
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 0, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomPassiveAbility (), 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 4, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			if (rndValue > 0.7) {
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 1, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			} else if (rndValue > 0.3) {
				if(enemyScript.hasAbility(EAbilityType.ERunAbility) != -1)
					abilityManagerScript.addAbilityToEnemy (enemyObject, EAbilityType.EViewAbility, 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
				else
					abilityManagerScript.addAbilityToEnemy (enemyObject, EAbilityType.ERunAbility, 0, Random.Range (0, 45 - (int)Mathf.Floor (2*difficulty)));
			} else {
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 5, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			}
			break;
		case 5:
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 0, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomPassiveAbility (), 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 4, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			if (rndValue > 0.7) {
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 1, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 2, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			} else if (rndValue > 0.3) {
				if(enemyScript.hasAbility(EAbilityType.ERunAbility) != -1)
					abilityManagerScript.addAbilityToEnemy (enemyObject, EAbilityType.EViewAbility, 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
				else
					abilityManagerScript.addAbilityToEnemy (enemyObject, EAbilityType.ERunAbility, 0, Random.Range (0, 45 - (int)Mathf.Floor (2*difficulty)));
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 5, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			} else {
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 1, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 5, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			}
			break;
		case 6:
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 0, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 0, Random.Range (1, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomPassiveAbility (), 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 4, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			if (rndValue > 0.7) {
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 2, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 5, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			} else if (rndValue > 0.3) {
				if(enemyScript.hasAbility(EAbilityType.ERunAbility) != -1)
					abilityManagerScript.addAbilityToEnemy (enemyObject, EAbilityType.EViewAbility, 0, Random.Range (0, 45 - (int)Mathf.Floor (2*difficulty)));
				else
					abilityManagerScript.addAbilityToEnemy (enemyObject, EAbilityType.ERunAbility, 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 5, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			} else {
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 1, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
				abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 5, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			}
			break;
		case 7:
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 0, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 1, Random.Range (1, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 2, Random.Range (1, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, EAbilityType.ERunAbility, 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, EAbilityType.EViewAbility, 0, Random.Range (0, 45 - (int)Mathf.Floor (2*difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 4, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 4, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			break;
		case 8:
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 0, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 1, Random.Range (1, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 2, Random.Range (1, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomActiveAbility (), 3, Random.Range (1, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, EAbilityType.ERunAbility, 0, Random.Range (0, 40 - (int)Mathf.Floor (2*difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, EAbilityType.EViewAbility, 0, Random.Range (0, 45 - (int)Mathf.Floor (2*difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 4, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			abilityManagerScript.addAbilityToEnemy (enemyObject, abilityManagerScript.getRandomShieldAbility (), 4, Random.Range (0, 10 - (int)Mathf.Floor (difficulty)));
			break;
		default:
			break;
		}
	}
	
	private Vector3 calculateSpawnPosition()
	{
		// Find a new place for this enemy object to spawn
		float theta = Random.Range (0.0f, 2.0f * Mathf.PI);
		float r = Random.Range ((2.0f*(playerScript.currentViewingRange + playerScript.size)), radius);
		Vector3 offset = new Vector3(r*Mathf.Cos(theta), r*Mathf.Sin(theta), 0);
		return player.transform.position + offset;
	}

	public void setNecessaryAbilities(EAbilityType[] abilities)
	{
		necessaryAbilities = abilities;
	}

}
