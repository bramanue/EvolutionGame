using UnityEngine;
using System.Collections;



public class player : MonoBehaviour
{
	public float maxVelocity = 5.5f;

	public float rotationSpeed = 10.0f;

	public float viewingRange = 10.0f;


	private enemyManager enemyMngr;
	
	
	// Use this for initialization
	void Start()
	{
		// Get an instance of the enemy manager
		enemyMngr = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
	}
	
	// Update is called once per frame
	void Update()
	{
		transform.Rotate(  0.0f, 0.0f, -Input.GetAxis ("Horizontal") * rotationSpeed);
		transform.position +=  Input.GetAxis("Vertical") * maxVelocity * transform.up * Time.deltaTime; // vorwärts bewegen
	}

	void  OnTriggerEnter2D(Collider2D other)
	{
		// If we are bigger than the enemy, then eat it
		if (other.transform.localScale.x <= transform.localScale.x) {

			// Reposition enemy
			enemyMngr.repositionEnemy(other.gameObject);
			// Define by how much the player's blob grows
			float growFactor = other.gameObject.transform.localScale.x / transform.localScale.x;
			// Set scaling of the blob
			transform.localScale += (new Vector3(0.1f,0.1f,0.1f)*growFactor);
		} 
		else 	// If the player's creature is smaller than the enemy, then reduce player's size
		{
			transform.localScale -= new Vector3(0.1f,0.1f,0.1f);
			print ("Game Over");
		}
	}
	
}