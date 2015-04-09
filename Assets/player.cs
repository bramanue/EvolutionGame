using UnityEngine;
using System.Collections;


public class player : MonoBehaviour
{
	// Max velocity of the player (depends on ability "Speed")
	public float runVelocityBoost;

	// The current pace of movement
	public float currentSpeed;

	// The base velocity of this blob (without any running ability)
	public float baseVelocity = 5.0f;

	// The maximally achievable velocity
	public float maxVelocity;

	// Viewing Range of the player (depends on ability "Eyes")
	public float viewingRange = 0.0f;

	// Viewing direction of the player (unit vector)
	public Vector3 viewingDirection;

	// Size of the player's blob
	public float size;

	// Defines how much the blob can grow per second (for grow animation)
	public float growSpeed = 0.1f;

	public float shrinkSpeed = 1.0f;

	private float environmentalDamage;

	private float abilityDamage;

	private Vector3 environmentalPushBack;

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

	public bool dead;


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
		if (!dead) {
			// Get the viewing range
			abilities [5].useAbility ();
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
				if (Input.GetButton ("Ability0")) {
					//	Debug.Log ("Fire1");
					if (abilities [0] != null && abilities [0].isReady ())
						abilities [0].useAbility ();
				}
				if (Input.GetButton ("Ability1")) {
					//	Debug.Log ("Fire2");
					if (abilities [1] != null && abilities [1].isReady ())
						abilities [1].useAbility ();
				}
				if (Input.GetButton ("Ability2")) {
					//	Debug.Log ("Fire3");
					if (abilities [2] != null && abilities [2].isReady ())
						abilities [2].useAbility ();
				}
				if (Input.GetButton ("Ability3")) {
					//	Debug.Log ("Jump");
					if (abilities [3] != null && abilities [3].isReady ())
						abilities [3].useAbility ();
				}


				if (canMove) {
					// Use runAbility if the player is moving
					if (Input.GetAxis ("Vertical") != 0 || Input.GetAxis ("Horizontal") != 0) {
						abilities [4].useAbility ();
					}
					currentSpeed = baseVelocity + runVelocityBoost;

					Vector3 targetDirection = new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"), 0.0f);
					if(targetDirection.magnitude > 0)
					{
						float angleBetween = Mathf.Sign (Vector3.Cross (viewingDirection, targetDirection).z) * Vector3.Angle (viewingDirection, targetDirection);
						// Calculate the necessary rotation speed to perform rotation within 0.5s
						// Get the rotation target
						Quaternion rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, transform.localEulerAngles.z + angleBetween));
						// Reset time scale to normal
						transform.rotation = Quaternion.Slerp (transform.rotation, rotationTargetQuaternion, Time.deltaTime * currentSpeed);
					}
					// Move blob according to joystick input
					//transform.Rotate (0.0f, 0.0f, -Input.GetAxis ("Horizontal") * currentSpeed);
					transform.position += viewingDirection*Time.deltaTime*targetDirection.magnitude*currentSpeed;
					//transform.position += Input.GetAxis ("Vertical") * currentSpeed * transform.up * Time.deltaTime; // vorwärts bewegen
				}
			} else {
				stunnedTimer -= Time.deltaTime;
				if (stunnedTimer <= 0)
					stunned = false;
			}
			// Apply environmental push back force
			transform.position += environmentalPushBack;
			environmentalPushBack = new Vector3 (0, 0, 0);

			// Inflict environmental damage
			size -= environmentalDamage;
			// Reset it to 0 for the next frame
			environmentalDamage = 0.0f;

			size -= abilityDamage;
			abilityDamage = 0.0f;

			if (size <= 0.0f) {
				size = 0.0f;
				dead = true;
			}
		}

		// Change appearance according to current size
		grow();
	}


	void LateUpdate() {
		if (size <= 0)
			size = 0;
	}

	void OnTriggerEnter(Collider other)
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

	public void inflictEnvironmentalDamage(float damage)
	{
		// There will be only environmental damage by one collider in each frame (the maximum)
		environmentalDamage = Mathf.Max (environmentalDamage,damage);
	}

	public void inflictDamage(float damage)
	{
		abilityDamage += damage;
	}

	public void addEnvironmentPushBackForce(Vector3 force)
	{
		if (force.magnitude > environmentalPushBack.magnitude)
			environmentalPushBack = force;
	}
	
}