using UnityEngine;
using System.Collections;

public class mouth : MonoBehaviour {

	public GameObject parentBlob;
	
	public enemy parentEnemyScript;
	
	public player parentPlayerScript;
	
	public bool isPlayer;

	private GameObject player;

	private player playerScript;


	// Use this for initialization
	void Start () {
		parentBlob = transform.parent.gameObject;

		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent(typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent(typeof(player));
		// Check whether it is a player or AI blob
		isPlayer = (bool)parentPlayerScript;

		player = GameObject.Find ("Blob");
		playerScript = (player)player.GetComponent (typeof(player));
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = new Vector3 (0, 0.8f, 0);
		transform.localScale = new Vector3 (1, 1, 1);
	}

	void OnTriggerEnter(Collider other)
	{
		if (isPlayer) {
			// Check whether the player collided with an enemy or with something else
			enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
			if (enemyScript != null) {
				// If player is bigger than enemy, then eat it
				if (enemyScript.transform.localScale.x < parentBlob.transform.localScale.x) {
					// If enemy has not been eaten yet, eat him
					if (enemyScript.size > 0)
						parentPlayerScript.eatBlob (enemyScript, other.gameObject);
				} else { 	// If the player's creature is smaller than the enemy, then reduce player's size
					// size -= 0.1f;
				}
			}
		} else {
			// This is handled in the player script
			if (other.gameObject == player) { 
				if(parentEnemyScript.isHuntingPlayer) {
					playerScript.size -= 0.05f*transform.localScale.x/other.transform.localScale.x;
				}
				return;
			}

			// If the enemy is hunting the player and collides with a different enemy, then the smaller enemy
			// gets eaten
			if (parentEnemyScript.isHuntingPlayer) 
			{
				enemy enemyScript = (enemy)other.gameObject.GetComponent(typeof(enemy));
				if(enemyScript != null && parentBlob.transform.localScale.x > other.gameObject.transform.localScale.x) {
					// Define by how much the player's blob grows
					float growFactor = enemyScript.size / parentEnemyScript.size;
					// Set scaling of the blob
					parentEnemyScript.size += 0.1f*growFactor*growFactor;
					// Kill enemy
					enemyScript.size = 0;
				}
			}
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (isPlayer) {
			// Check whether the player collided with an enemy or with something else
			enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
			if (enemyScript != null) {
				// If player is bigger than enemy, then eat it
				if (enemyScript.transform.localScale.x < transform.localScale.x) {
					// If enemy has not been eaten yet, eat him
					if (enemyScript.size > 0)
						parentPlayerScript.eatBlob (enemyScript, other.gameObject);
				} 
			}
		}
	}
}
