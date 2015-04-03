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

	private bool once = true;


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

		radius = 100.0f;
		nofEnemies = 100;

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
			// Set abilities
			abilityManagerScript.addAbilityToEnemy(enemyGameObjects[i],EAbilityType.ERamAbility,0,4);
		}
	}
	
	// Update is called once per frame
	void Update () {

		// Make sure the radius increases with the player's size
		radius = 8.0f*playerScript.viewingRange + Mathf.Sqrt(8.0f*nofEnemies*player.transform.localScale.x) + 4.0f * playerScript.maxVelocity;

		// Get the current position of the player
		Vector3 playerPosition = player.transform.position;
		// Loop over all enemies and check whether they need to be repositioned (out of the radius)
		for (int it = 0; it < nofEnemies; it++) {
			float distance = (enemyGameObjects [it].transform.position - playerPosition).magnitude;
			if (distance > radius)
			{
				setRandomInitialValues (enemyScripts [it], enemyGameObjects [it]);
				distance = (enemyGameObjects [it].transform.position - playerPosition).magnitude;
			}	
			// Deactivate enemies if they are too far from the player
			if(distance > 0.25*radius)
				enemyGameObjects[it].SetActive(false);
			else
				enemyGameObjects[it].SetActive(true);
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
		float size = Random.Range(playerSize - 0.5f*playerSize,playerSize + 0.5f*playerSize);
		// Set size of GameObject
		enemyObject.transform.localScale = new Vector3(size,size,size);
		// Get spawn point...
		Vector3 spawnPoint = calculateSpawnPosition();
		// ...and put enemy there
		enemyObject.transform.position = spawnPoint;

		// Set values for this script
		enemyScript.size = size;
		enemyScript.viewingRange = (size + Random.Range(7.0f,12.0f));
		enemyScript.idleOperationRadius = size + 20.0f;
		enemyScript.activeOperationRadius = size + enemyScript.viewingRange + 40.0f;
		enemyScript.cosViewingAngle = Random.Range(0.0f,0.7f);
		enemyScript.maxVelocity = Random.Range(4.0f,6.0f);

		enemyScript.resetStates();
	}
	
	private Vector3 calculateSpawnPosition()
	{
		// Find a new place for this enemy object to spawn
		float theta = Random.Range (0.0f, 2.0f * Mathf.PI);
		float r = Random.Range ((2.0f*(playerScript.viewingRange + playerScript.size)), radius);
		Vector3 offset = new Vector3(r*Mathf.Cos(theta), r*Mathf.Sin(theta), 0);
		return player.transform.position + offset;
	}

}
