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
	public bool canMove;

	// Defines whether player is stunned (not allowed to do anything)
	public bool stunned;

	// Enemy manager for performing eating, etc operations
	private enemyManager enemyMngr;

	// Stores all ability scripts
	// 0 to 3 are user chosen abilities
	// 4 is the running ability
	// 5 is the viewing ability
	private ability[] abilities = new ability[6];

	// Stores all ability game objects
	private GameObject[] abilityObjects = new GameObject[6];


	// Use this for initialization
	void Start()
	{
		// Get an instance of the enemy manager
		enemyMngr = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		// Get size
		size = transform.localScale.x;
		// Player is allowed to move
		canMove = true;
	}
	
	// Update is called once per frame
	void Update()
	{
		// Get the theta angle of the current rotation, corresponding to position on the unit circle
		float theta = (transform.localEulerAngles.z + 90.0f) * Mathf.Deg2Rad;
		// Get the viewing direction based on the current rotation (resp. on the previously calculated theta)
		viewingDirection = new Vector3 (Mathf.Cos (theta), Mathf.Sin (theta), 0.0f);

		if (!stunned) {

			// Capture player input
			if (Input.GetButton ("Fire1")) {
				//	Debug.Log ("Fire1");
				if (abilities [0] != null && abilities [0].isReady ())
					abilities [0].useAbility ();
			}
			if (Input.GetButton ("Fire2")) {
				//	Debug.Log ("Fire2");
				if (abilities [1] != null && abilities [1].isReady ())
					abilities [1].useAbility ();
			}
			if (Input.GetButton ("Fire3")) {
				//	Debug.Log ("Fire3");
				if (abilities [2] != null && abilities [2].isReady ())
					abilities [2].useAbility ();
			}
			if (Input.GetButton ("Jump")) {
				//	Debug.Log ("Jump");
				if (abilities [3] != null && abilities [3].isReady ())
					abilities [3].useAbility ();
			}



			if (canMove) {
				// Use runAbility if the player is moving
				if (Input.GetAxis ("Vertical") != 0 || Input.GetAxis ("Horizontal") != 0) {
					abilities [4].useAbility ();
				}

				// Move blob according to joystick input
				transform.Rotate (0.0f, 0.0f, -Input.GetAxis ("Horizontal") * rotationSpeed);
				transform.position += Input.GetAxis ("Vertical") * maxVelocity * transform.up * Time.deltaTime; // vorwärts bewegen
			}
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
				if(enemyScript.hasAbilities())
				{
					// Get a random ability from the defeated enemy
					ability newAbility = enemyScript.getRandomAbility();
					// Check whether player already has this ability
					int abilityIndex = hasPlayerAbility(newAbility.getAbilityEnum());
					// If player already has this ability, then increase its level by a certain amount
					if(abilityIndex >= 0) {
						// The increase in ability is half the difference between enemie's level and player's level but at least 1
						int increase = abilities[abilityIndex].increaseLevel((int)Mathf.Max (1, Mathf.Floor((newAbility.level - abilities[abilityIndex].level) * 0.5f)) );
						// TODO Make a nice GUI print on screen
						Debug.Log("Your ability " + abilities[abilityIndex].name + " increased its level by " + increase);
					}

				}
				// Define by how much the player's blob grows
				float growFactor = other.gameObject.transform.localScale.x / transform.localScale.x;
				// Set scaling of the blob
				transform.localScale += new Vector3 (0.1f, 0.1f, 0.1f) * growFactor * growFactor;
				size = transform.localScale.x;
				// Reposition enemy
				enemyMngr.respawnEnemy (other.gameObject);
			} 
			else 
			{ 	// If the player's creature is smaller than the enemy, then reduce player's size
				transform.localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
				size = transform.localScale.x;
				print ("Game Over");
			}
		}
	}

	// Returns -1 when the player does not have this ability and otherwise the index to where this ability resides in the ability array
	private int hasPlayerAbility(EAbilityType abilityType)
	{
		for (int i = 0; i < 6; i++) {
			if(abilities[i] != null && abilities[i].getAbilityEnum() == abilityType)
				return i;
		}
		return -1;
	}

	public void addAbility(GameObject ability, int slot)
	{
		abilityObjects [slot] = ability;
		abilities[slot] = (ability)abilityObjects [slot].GetComponent (typeof(ability));
	}
	
}