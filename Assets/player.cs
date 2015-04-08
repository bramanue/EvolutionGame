using UnityEngine;
using System.Collections;


public class player : MonoBehaviour
{
	// Max velocity of the player (depends on ability "Speed")
	public float maxVelocity = 7.0f;

	// Max rotation speed of the player (depends on ability "Speed")
	public float rotationSpeed = 10.0f;

	public float currentSpeed;

	// Viewing Range of the player (depends on ability "Eyes")
	public float viewingRange = 0.0f;

	// Viewing direction of the player (unit vector)
	public Vector3 viewingDirection;

	// Size of the player's blob
	public float size;

	// Defines how much the blob can grow per second (for grow animation)
	public float growSpeed = 0.1f;

	public float shrinkSpeed = 1.0f;

	// Defines whether player is allowed to move
	public bool canMove;

	// Defines whether player is stunned (not allowed to do anything)
	public bool stunned;

	public float stunnedTimer;

	// Defines whether player is currently blinded
	public bool blinded;
	private float blindedTimer;

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
		// Get the viewing range
		abilities[5].useAbility();
		// Change appearance according to current size
		grow();
		// transform.localScale = new Vector3 (size, size, size);
		// Get the theta angle of the current rotation, corresponding to position on the unit circle
		float theta = (transform.localEulerAngles.z + 90.0f) * Mathf.Deg2Rad;
		// Get the viewing direction based on the current rotation (resp. on the previously calculated theta)
		viewingDirection = new Vector3 (Mathf.Cos (theta), Mathf.Sin (theta), 0.0f);

		if (blinded && blindedTimer > 0) {
			// TODO make screen less bright (exponentially)
			blindedTimer -= Time.deltaTime;
		} else {
			blinded = false;
		}

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
				currentSpeed = maxVelocity;
				// Move blob according to joystick input
				transform.Rotate (0.0f, 0.0f, -Input.GetAxis ("Horizontal") * rotationSpeed);
				transform.position += Input.GetAxis ("Vertical") * maxVelocity * transform.up * Time.deltaTime; // vorwärts bewegen
			}
		} else {
			stunnedTimer -= Time.deltaTime;
			if(stunnedTimer <= 0)
				stunned = false;
		}

	}

	void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log ("Collision detected");
		// Check whether the player collided with an enemy or with something else
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		if (enemyScript != null)
		{
			// If player is bigger than enemy, then eat it
			if (enemyScript.size < size) 
			{
				eatBlob (enemyScript, other.gameObject);
			} 
			else 
			{ 	// If the player's creature is smaller than the enemy, then reduce player's size
				size -= 0.1f;
			}
		}
	}

	// Makes the blob grow smoothly
	private void grow()
	{
		float currentSize = transform.localScale.x;
		float diff = size - currentSize;
		if (diff < 0) 
		{
			// Shrinking
			float nextSize = Mathf.Max(size,currentSize - Time.deltaTime*shrinkSpeed*Mathf.Max (1.0f,-diff));
			transform.localScale = new Vector3(nextSize, nextSize, nextSize);
		}
		else
		{
			// Growing
			float nextSize = Mathf.Min(size,currentSize + Time.deltaTime*growSpeed);
			transform.localScale = new Vector3(nextSize, nextSize, nextSize);
		}
	}

	private void eatBlob(enemy enemyScript, GameObject enemyObject)
	{
		if(enemyScript.hasAbilities())
		{
			// Get a random ability from the defeated enemy
			ability newAbility = enemyScript.getRandomAbility();
			// Check whether player already has this ability
			int abilityIndex = hasAbility(newAbility.getAbilityEnum());
			// If player already has this ability, then increase its level by a certain amount
			if(abilityIndex >= 0) {
				// The increase in ability is half the difference between enemie's level and player's level but at least 1
				int increase = abilities[abilityIndex].increaseLevel((int)Mathf.Max (1, Mathf.Floor((newAbility.level - abilities[abilityIndex].level) * 0.5f)) );
				// TODO Make a nice GUI print on screen
				Debug.Log("Your ability " + abilities[abilityIndex].name + " increased its level by " + increase);
			}
			
		}
		
		// Define by how much the player's blob grows
		float growFactor = enemyObject.transform.localScale.x / transform.localScale.x;
		// Set scaling of the blob (transform will be changed during next Update())
		size += 0.1f*growFactor*growFactor;
		// Reposition enemy
		enemyMngr.respawnEnemy(enemyObject);
	}

	// Returns -1 when the player does not have this ability and otherwise the index to where this ability resides in the ability array
	public int hasAbility(EAbilityType abilityType)
	{
		for (int i = 0; i < abilities.Length; i++) {
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

	public void setBlinded(float time) {
		if (!blinded) 
		{
			// TODO make screen insanely bright
			blindedTimer = time;
			blinded = true;
		}
	}

	public void setStunned(float time) {
		// TODO make stun visible
		stunned = true;
		stunnedTimer = time;
	}
	
}