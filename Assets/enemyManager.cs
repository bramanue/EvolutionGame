using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class enemyManager : MonoBehaviour {

	// The numbe rof enemies on the field.
	public int nofEnemies;

	// The radius in which enemies can spawn
	public float radius;

	// The base radius
	private float baseRadius;

	// The prototype for each enemy
	private GameObject basicEnemy;

	// Pointer to the player
	private GameObject player;

	// List<GameObject> enemies = new List<GameObject>();
	GameObject[] enemyGameObjects = new GameObject[150];

	enemy[] enemyScripts = new enemy[150];

	// Use this for initialization
	void Start () 
	{
		baseRadius = 50.0f;
		radius = 50.0f;
		nofEnemies = 50;

		// Get an instance of the player
		player = GameObject.Find ("Blob");
		// Get the enemy prefab
		basicEnemy = GameObject.Find ("BasicEnemy");
		// Get the size of the player's blob
		float playerSize = player.transform.localScale.x;

		// Create the basic amount of enemies
		for(int i = 0; i < nofEnemies; i++) {
			// Calculate a random spawn position
			Vector3 spawnPoint = calculateSpawnPosition();
			// Calculate random initial rotation
			Quaternion q = Quaternion.Euler(new Vector3(0.0f,0.0f,Random.Range(-180,180)));
			// Create new enemy GameObject
			enemyGameObjects[i] = ((GameObject)GameObject.Instantiate(basicEnemy,spawnPoint,q));
			// Define a random size
			float size = Random.Range(playerSize - 0.8f*playerSize,playerSize + 0.8f*playerSize);
			// Add GameObject to the array
			enemyGameObjects[i].transform.localScale = new Vector3(size,size,size);
			// Add enemy script to the array
			enemyScripts[i] = (enemy)enemyGameObjects[i].GetComponent(typeof(enemy));
			// Set values for this script
			enemyScripts[i].viewingRange = enemyGameObjects[i].transform.localScale.x + Random.Range(2.0f,10.0f);
			enemyScripts[i].activeOperationRadius = enemyScripts[i].viewingRange + 20.0f;
			enemyScripts[i].cosViewingAngle = Random.Range(0.2f,0.9f);
			enemyScripts[i].maxVelocity = Random.Range(4.0f,5.0f);
			enemyScripts[i].idleOperationRadius = 10.0f;
		}
	}
	
	// Update is called once per frame
	void Update () {
		// Make sure the radius increases with the player's size
		radius = baseRadius * player.transform.localScale.x;
	}

	public void repositionEnemy(GameObject enemy) 
	{
		float playerSize = player.transform.localScale.x;
		float newEnemySize = Random.Range(playerSize - 0.8f*playerSize,playerSize + 0.8f*playerSize);
		enemy enemyScript = (enemy)enemy.GetComponent(typeof(enemy));
		enemyScript.size = newEnemySize;

		enemy.transform.localScale = new Vector3 (newEnemySize, newEnemySize, newEnemySize);

		Vector3 spawnPoint = calculateSpawnPosition();
		enemy.transform.position = spawnPoint;
	}

	private Vector3 calculateSpawnPosition()
	{
		// Find a new place for this enemy object to spawn
		Vector2 rnd2D = Random.insideUnitCircle;
		Vector3 offset = new Vector3(rnd2D.x, rnd2D.y, 0.0f)*radius;
		Vector3 spawnPoint = player.transform.position + offset;
		while (true) {
			// TODO: replace < player.transform.localScale.x * 2.0f with viewing range of player
			if((spawnPoint - player.transform.position).magnitude < player.transform.localScale.x  * 5.0f) {
				// Find a new spawn point, if the current one is too close to the player
				rnd2D = Random.insideUnitCircle;
				offset = new Vector3(rnd2D.x, rnd2D.y, 0.0f)*radius;
				spawnPoint = player.transform.position + offset;
			}
			else
			{
				break;
			}
		}
		return spawnPoint;
	}
}
