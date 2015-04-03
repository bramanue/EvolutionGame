using UnityEngine;
using System.Collections;


public class player : MonoBehaviour
{
	// Max velocity of the player (depends on ability "Speed")
	public float maxVelocity = 7.0f;

	// Max rotation speed of the player (depends on ability "Speed")
	public float rotationSpeed = 10.0f;

	// Viewing Range of the player (depends on ability "Eyes")
	public float viewingRange = 0.0f;

	// Viewing direction of the player (unit vector)
	public Vector3 viewingDirection;

	// Size of the player's blob
	public float size;

	// Defines whether player is allowed to move
	public bool cannotMove;

	// Defines whether player is stunned (not allowed to do anything)
	public bool stunned;

	// Enemy manager for performing eating, etc operations
	private enemyManager enemyMngr;

	// Stores all ability scripts
	private ability[] abilities = new ability[5];

	// Stores all ability game objects
	private GameObject[] abilityObjects = new GameObject[5];

	Vector4 abilityTimers;
	
	// Use this for initialization
	void Start()
	{
		// Get an instance of the enemy manager
		enemyMngr = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		// Get size
		size = transform.localScale.x;
		// Initialize the ability cooldwon timers
		abilityTimers = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f);
	}
	
	// Update is called once per frame
	void Update()
	{
		// Get the theta angle of the current rotation, corresponding to position on the unit circle
		float theta = (transform.localEulerAngles.z + 90.0f) * Mathf.Deg2Rad;
		// Get the viewing direction based on the current rotation (resp. on the previously calculated theta)
		viewingDirection = new Vector3 (Mathf.Cos (theta), Mathf.Sin (theta), 0.0f);

		// Reduce the ability cooldown timers
		abilityTimers -= new Vector4 (Time.deltaTime,Time.deltaTime,Time.deltaTime,Time.deltaTime);

		if (Input.GetButton("Fire1")) {
		//	Debug.Log ("Fire1");
			abilities[0].useAbility();
		}
		if (Input.GetButton("Fire2")) {
		//	Debug.Log ("Fire2");
			abilities[1].useAbility();
		}
		if (Input.GetButton("Fire3")) {
		//	Debug.Log ("Fire3");
			abilities[2].useAbility();
		}
		if (Input.GetButton("Jump") && abilityTimers.w <= 0) {
		//	Debug.Log ("Jump");
			abilities[3].useAbility();
		}



		if (!cannotMove) 
		{
			// Use runAbility if the player is moving
			if (Input.GetAxis ("Vertical") != 0 || Input.GetAxis ("Horizontal") != 0) {
				abilities [4].useAbility ();
			}

			// Move blob according to joystick input
			transform.Rotate (0.0f, 0.0f, -Input.GetAxis ("Horizontal") * rotationSpeed);
			transform.position += Input.GetAxis ("Vertical") * maxVelocity * transform.up * Time.deltaTime; // vorwärts bewegen
		}

	}

	void  OnTriggerEnter2D(Collider2D other)
	{
		// Check whether the player collided with an enemy or with something else
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		if (enemyScript != null)
		{
			// If we are bigger than the enemy, then eat it
			if (other.transform.localScale.x <= transform.localScale.x) 
			{
			/*	if(enemyScript.hasAbilities)
				{

				} */
				// Define by how much the player's blob grows
				float growFactor = other.gameObject.transform.localScale.x / transform.localScale.x;
				// Set scaling of the blob
				transform.localScale += new Vector3 (0.1f, 0.1f, 0.1f) * growFactor * growFactor;
				// Reposition enemy
				enemyMngr.respawnEnemy (other.gameObject);
			} else { 	// If the player's creature is smaller than the enemy, then reduce player's size
				transform.localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
				print ("Game Over");
			}
		}
	}

	private void aquireAbility(ability newAbility)
	{

	}

	public void addAbility(GameObject ability, int slot)
	{
		abilityObjects [slot] = ability;
		abilities[slot] = (ability)abilityObjects [slot].GetComponent (typeof(ability));
	}
	
}