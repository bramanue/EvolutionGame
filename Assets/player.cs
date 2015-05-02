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

	// The base viewing range without any eye ability
	public float baseViewingRange = 5.0f;

	// The viewing range boost given by the eye ability
	public float viewingRangeBoost;

	// Viewing Range of the player (depends on ability "Eyes")
	public float currentViewingRange;

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
	// 4 to 5 for shield abilities
	// 6 is the running ability
	// 7 is the viewing ability
	private ability[] abilities = new ability[8];

	// Stores all ability game objects
	private GameObject[] abilityObjects = new GameObject[8];

	// The total number of abilities
	private int nofAbilities;

	// Definies whether player blob is dead
	public bool dead;

	// Stores the currently active shield
	public ability shieldInUse;

	// Stores in which environment the player blob currently resides in
	public hazardousEnvironment currentEnvironment;

	private abilityManager abilityManager;

	private abilityModificationPanel abilityModificationScript;

	public bool isAbilityModificationPanelOpen;

	private bool[] selectionConfirmed = new bool[4];

	public Color defaultColor = new Color(0.7f,0.7f,1.0f);

	public float environmentalSlowDown = 1.0f; 

	public Material defaultMaterial;

	public abilityLoot nearbyAbilityLoot;


	// Use this for initialization
	void Start()
	{
		// Get an instance of the enemy manager
		enemyMngr = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		// Get size
		size = transform.localScale.x;
		// Player is allowed to move
		canMove = true;
		// Get the ability modification panel
		abilityModificationScript = (abilityModificationPanel)(this.GetComponentInChildren (typeof(abilityModificationPanel)));
		abilityModificationScript.gameObject.SetActive (true);

		abilityManager = (abilityManager)GameObject.Find ("AbilityManager").GetComponent(typeof(abilityManager));
	}
	
	// Update is called once per frame
	void Update()
	{
		if (!dead) {

			Debug.Log ("Nearby ability loot = " + nearbyAbilityLoot);
			Debug.Log (Input.GetButtonDown ("PauseForAbilityGatheringLB") + " " + Input.GetButtonDown ("PauseForAbilityGatheringRB"));

			if(nearbyAbilityLoot != null) 
			{
				// Show display to press RB or LB if player wants to acquire the ability
				string abilityName = nearbyAbilityLoot.abilityName;
				EAbilityClass abilityClass = nearbyAbilityLoot.abilityClass;

				if(Input.GetButtonDown ("PauseForAbilityGatheringLB") || Input.GetButtonDown ("PauseForAbilityGatheringRB"))
				{
					Debug.Log ("Button press recognized");

					// Open the modification panel if it is not enabled
					if(!abilityModificationScript.isActive()) 
					{
						Debug.Log ("Opening ability modification panel");
						if(abilityClass == EAbilityClass.EActiveAbility)
						{
							string[] abilityNames = new string[4];
							for(int i = 0; i < 4; i++) {
								abilityNames[i] = (abilities[i] != null) ? (abilities[i].abilityName + " (lvl. " + abilities[i].level +")") : string.Empty;
							}
							abilityModificationScript.showPanel(abilityNames, abilityClass);
							abilityModificationScript.displayTitle(nearbyAbilityLoot.abilityName);
							abilityModificationScript.displayMessage("You found a new attack ability! \n Press the button you want to map it to. \n Press \"RB\" or \"LB\" to continue.");
							return;
						}
						else if(abilityClass == EAbilityClass.EShieldAbility)
						{
							string[] abilityNames = new string[2];
							for(int i = 0; i < 2; i++) {
								abilityNames[i] = (abilities[4+i] != null) ? (abilities[4+i].abilityName + " (lvl. " + abilities[4+i].level +")") : string.Empty;
							}
							abilityModificationScript.showPanel(abilityNames, abilityClass);
							abilityModificationScript.displayTitle(nearbyAbilityLoot.abilityName);
							abilityModificationScript.displayMessage("You found a new attack ability! \n Press the button you want to map it to. \n Press \"RB\" or \"LB\" to continue.");
							return;
						}
					}
					else
					{
						// Close modification panel if it was already enabled
						abilityModificationScript.hidePanel();
					}
					
				}

				if(abilityModificationScript.isActiveAndEnabled && abilityModificationScript.isInChosingState) 
				{
					// Capture player input
					if (Input.GetButtonDown ("Ability0")) {
						if (abilities [0] != null && !selectionConfirmed[0]) {
							selectionConfirmed[0] = true;
							abilityModificationScript.displayMessage("Please press \"A\" again to confirm \n Press \"LB\" or \"RB\" to continue \n Press one of the other buttons to map the new ability to them");
						} else {
							abilityManager.addAbilityToPlayer(this.gameObject,nearbyAbilityLoot.abilityType,0,1);
							Debug.Log("You gained a new ability : " + abilities[0].abilityName + " at level " + abilities[0].level);
							abilityModificationScript.highlightButtonAndHide(abilities[0].abilityName, EButtonType.EAButton);
							selectionConfirmed[0] = false;
						}
						// Reset the other selections
						selectionConfirmed[1] = selectionConfirmed[2] = selectionConfirmed[3] = false;
					}
					else if (Input.GetButtonDown ("Ability1")) {
						if (abilities [1] != null && !selectionConfirmed[1]) {
							selectionConfirmed[1] = true;
							abilityModificationScript.displayMessage("Please press \"B\" again to confirm \n Press \"LB\" or \"RB\" to continue \n Press one of the other buttons to map the new ability to them.");
						} else {
							abilityManager.addAbilityToPlayer(this.gameObject,nearbyAbilityLoot.abilityType,1,1);
							Debug.Log("You gained a new ability : " + abilities[1].abilityName + " at level " + abilities[1].level);
							abilityModificationScript.highlightButtonAndHide(abilities[1].abilityName, EButtonType.EBButton);
							selectionConfirmed[1] = false;
						}
						// Reset the other selections
						selectionConfirmed[0] = selectionConfirmed[2] = selectionConfirmed[3] = false;
					}
					else if (Input.GetButtonDown ("Ability2")) {
						if (abilities [2] != null && !selectionConfirmed[2]) {
							selectionConfirmed[2] = true;
							abilityModificationScript.displayMessage("Please press \"X\" again to confirm \n Press \"LB\" or \"RB\" to continue \n Press one of the other buttons to map the new ability to them.");
						} else {
							abilityManager.addAbilityToPlayer(this.gameObject,nearbyAbilityLoot.abilityType,2,1);
							Debug.Log("You gained a new ability : " + abilities[2].abilityName + " at level " + abilities[2].level);
							abilityModificationScript.highlightButtonAndHide(abilities[2].abilityName, EButtonType.EXButton);
							selectionConfirmed[2] = false;
						}
						// Reset the other selections
						selectionConfirmed[0] = selectionConfirmed[1] = selectionConfirmed[3] = false;
					}
					else if (Input.GetButtonDown ("Ability3")) {
						if (abilities [3] != null && !selectionConfirmed[3]) {
							selectionConfirmed[3] = true;
							abilityModificationScript.displayMessage("Please press \"Y\" again to confirm \n Press \"LB\" or \"RB\" to continue \n Press one of the other buttons to map the new ability to them");
						} else {
							abilityManager.addAbilityToPlayer(this.gameObject,nearbyAbilityLoot.abilityType,3,1);
							Debug.Log("You gained a new ability : " + abilities[3].abilityName + " at level " + abilities[3].level);
							abilityModificationScript.highlightButtonAndHide(abilities[3].abilityName, EButtonType.EYButton);
							selectionConfirmed[3] = false;
						}
						// Reset the other selections
						selectionConfirmed[0] = selectionConfirmed[1] = selectionConfirmed[2] = false;
					}
				}
				else
				{
					nearbyAbilityLoot = null;
				}
			}

			// If the user is about to chose whether to keep a new ability or not
			if(abilityModificationScript.isActiveAndEnabled && abilityModificationScript.isInChosingState) {
				// Capture player input
				if (Input.GetButtonDown ("Ability0")) {
					if (abilities [0] != null && !selectionConfirmed[0]) {
						selectionConfirmed[0] = true;
						abilityModificationScript.displayMessage("Please press \"A\" again to confirm \n Press \"Start\" to throw the new ability away \n Press one of the other buttons to map the new ability to them");
					} else {
						ability newAbility = abilityModificationScript.newAbility;
						removeAndDestroyAbility(0);
						// Transfer ability from enemy to player
						newAbility.gameObject.transform.parent = this.gameObject.transform;
						newAbility.updateParent();
						// Reduce the level (maybe set it 1 ?)
						newAbility.level /= 2;
						addAbility(newAbility.gameObject, 0);
						// Remove the ability from the enemy
						abilityModificationScript.enemyScript.removeAbility(abilityModificationScript.enemyScript.hasAbility(newAbility.getAbilityEnum()));
						Debug.Log("You gained a new ability : " + abilities[0].abilityName + " at level " + abilities[0].level);
						abilityModificationScript.highlightButtonAndHide(abilities[0].abilityName, EButtonType.EAButton);
						selectionConfirmed[0] = false;
					}
					// Reset the other selections
					selectionConfirmed[1] = selectionConfirmed[2] = selectionConfirmed[3] = false;
				}
				else if (Input.GetButtonDown ("Ability1")) {
					if (abilities [1] != null && !selectionConfirmed[1]) {
						selectionConfirmed[1] = true;
						abilityModificationScript.displayMessage("Please press \"B\" again to confirm \n Press \"Start\" to throw the new ability away. \n Press one of the other buttons to map the new ability to them.");
					} else {
						ability newAbility = abilityModificationScript.newAbility;
						removeAndDestroyAbility(1);
						// Transfer ability from enemy to player
						newAbility.gameObject.transform.parent = this.gameObject.transform;
						newAbility.updateParent();
						// Reduce the level (maybe set it 1 ?)
						newAbility.level /= 2;
						addAbility(newAbility.gameObject, 1);
						// Remove the ability from the enemy
						abilityModificationScript.enemyScript.removeAbility(abilityModificationScript.enemyScript.hasAbility(newAbility.getAbilityEnum()));
						Debug.Log("You gained a new ability : " + abilities[1].abilityName + " at level " + abilities[1].level);
						abilityModificationScript.highlightButtonAndHide(abilities[1].abilityName, EButtonType.EBButton);
						selectionConfirmed[1] = false;
					}
					// Reset the other selections
					selectionConfirmed[0] = selectionConfirmed[2] = selectionConfirmed[3] = false;
				}
				else if (Input.GetButtonDown ("Ability2")) {
					if (abilities [2] != null && !selectionConfirmed[2]) {
						selectionConfirmed[2] = true;
						abilityModificationScript.displayMessage("Please press \"X\" again to confirm \n Press \"Start\" to throw the new ability away. \n Press one of the other buttons to map the new ability to them.");
					} else {
						ability newAbility = abilityModificationScript.newAbility;
						removeAndDestroyAbility(2);
						// Transfer ability from enemy to player
						newAbility.gameObject.transform.parent = this.gameObject.transform;
						newAbility.updateParent();
						// Reduce the level (maybe set it 1 ?)
						newAbility.level /= 2;
						addAbility(newAbility.gameObject, 2);
						// Remove the ability from the enemy
						abilityModificationScript.enemyScript.removeAbility(abilityModificationScript.enemyScript.hasAbility(newAbility.getAbilityEnum()));
						Debug.Log("You gained a new ability : " + abilities[2].abilityName + " at level " + abilities[2].level);
						abilityModificationScript.highlightButtonAndHide(abilities[2].abilityName, EButtonType.EXButton);
						selectionConfirmed[2] = false;
					}
					// Reset the other selections
					selectionConfirmed[0] = selectionConfirmed[1] = selectionConfirmed[3] = false;
				}
				else if (Input.GetButtonDown ("Ability3")) {
					if (abilities [3] != null && !selectionConfirmed[3]) {
						selectionConfirmed[3] = true;
						abilityModificationScript.displayMessage("Please press \"Y\" again to confirm \n Press \"Start\" to throw the new ability away \n Press one of the other buttons to map the new ability to them");
					} else {
						ability newAbility = abilityModificationScript.newAbility;
						removeAndDestroyAbility(3);
						// Transfer ability from enemy to player
						newAbility.gameObject.transform.parent = this.gameObject.transform;
						newAbility.updateParent();
						// Reduce the level (maybe set it 1 ?)
						newAbility.level /= 2;
						addAbility(newAbility.gameObject, 3);
						// Remove the ability from the enemy
						abilityModificationScript.enemyScript.removeAbility(abilityModificationScript.enemyScript.hasAbility(newAbility.getAbilityEnum()));
						Debug.Log("You gained a new ability : " + abilities[3].abilityName + " at level " + abilities[3].level);
						abilityModificationScript.highlightButtonAndHide(abilities[3].abilityName, EButtonType.EYButton);
						selectionConfirmed[3] = false;
					}
					// Reset the other selections
					selectionConfirmed[0] = selectionConfirmed[1] = selectionConfirmed[2] = false;
				}
				else if(Input.GetButtonDown ("Cancel")) {
					abilityModificationScript.hidePanel();
				}
				return;
			}

			ability oldShield = shieldInUse;
			shieldInUse = null;

			// Get the viewing range
			abilities [7].useAbility ();
			currentViewingRange = baseViewingRange + viewingRangeBoost;
			// Get the viewing direction
			viewingDirection = transform.up;

			if (blinded && blindedTimer > 0) {
				// TODO make screen less bright (exponentially)
				blindedTimer -= Time.deltaTime;
			} else {
				blinded = false;
			}

			if (!stunned) {

				// Capture player input
				if (Input.GetButton ("Ability0")) {
					if (abilities [0] != null) {
						shieldInUse = abilities[0];
						abilities [0].useAbility ();
					}
				}
				if (Input.GetButton ("Ability1")) {
					if (abilities [1] != null) {
						shieldInUse = abilities[1];
						abilities [1].useAbility ();
					}
				}
				if (Input.GetButton ("Ability2")) {
					if (abilities [2] != null){
						shieldInUse = abilities[2];
						abilities [2].useAbility ();
					}
				}
				if (Input.GetButton ("Ability3")) {
					if (abilities [3] != null){
						shieldInUse = abilities[3];
						abilities [3].useAbility ();
					}
				}

				// Capture player input concerning shields (XBOX360 only)
				float shields = Input.GetAxis("Shields");
				if(shields > 0.05f)
				{
					Debug.Log ("Activate shield0");
					if (abilities [4] != null && abilities [4].useAbility ()) {
						shieldInUse = abilities[4];
					}
				}
				else if (shields < -0.05f)
				{
					Debug.Log ("Activate shield1");
					if (abilities [5] != null && abilities [5].useAbility ()) {
						shieldInUse = abilities[5];
					}
				}


				if (canMove) 
				{
					// Get the target direction from user input
					Vector3 targetDirection = new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"), 0.0f).normalized;
					// Calculate the current maximally achievable speed
					currentSpeed = (baseVelocity + runVelocityBoost)*environmentalSlowDown;
					environmentalSlowDown = 1.0f;
					// Get the desired speed fraction
					float speedFraction =  targetDirection.magnitude;

					// If the player is moving
					if (speedFraction > 0) {
						// Use the run ability to become faster
						abilities [6].useAbility ();
						// Calculate the angle to rotate about
						float angleBetween = Mathf.Sign (Vector3.Cross (viewingDirection, targetDirection).z) * Vector3.Angle (viewingDirection, targetDirection);
						// Get the rotation target
						Quaternion rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, transform.localEulerAngles.z + angleBetween));
						// Reset time scale to normal
						transform.rotation = Quaternion.Slerp (transform.rotation, rotationTargetQuaternion, Time.deltaTime * Mathf.Max (10.0f,currentSpeed));
						// Move blob according to joystick input
						transform.position += viewingDirection*Time.deltaTime*speedFraction*currentSpeed;
					}
				}
			} 
			else 
			{
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
		if (size <= 0) {
			size = 0;
			dead = true;
		}
		// Reset the current environment
		currentEnvironment = null;
	}


	void OnTriggerEnter(Collider other)
	{
		// Check whether the player collided with an enemy or with something else

	}



	// Makes the blob grow/shrink smoothly
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

	// Function called, when eating another blob, to grow in size and to acquire its abilities
	public void eatBlob(enemy enemyScript, GameObject enemyObject)
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
				Debug.Log("Your ability " + abilities[abilityIndex].abilityName + " increased its level by " + increase);
			}
			else
			{
				string[] abilityNames = new string[4];
				for(int i = 0; i < 4; i++) {
					abilityNames[i] = (abilities[i] != null) ? (abilities[i].abilityName + " (lvl. " + abilities[i].level +")") : string.Empty;
				}
				abilityModificationScript.showPanel(abilityNames, newAbility, enemyScript);
				abilityModificationScript.displayTitle(newAbility.abilityName);
				abilityModificationScript.displayMessage("You acuired the other blob's ability! \n Press the button you want to map it to. \n Press \"Start\" to throw it away.");
			}
			
		}
		
		// Define by how much the player's blob grows
		float growFactor = enemyObject.transform.localScale.x / transform.localScale.x;
		// Set scaling of the blob (transform will be changed during next Update())
		size += 0.1f*growFactor*growFactor;
		// Kill enemy, will be respawned by the emeny manager
		enemyScript.size = 0.0f;
	}


	// Returns -1 when the player does not have this ability and otherwise the index to where this ability resides in the ability array
	public int hasAbility(EAbilityType abilityType)
	{
		if (nofAbilities == 0)
			return -1;

		for (int i = 0; i < abilities.Length; i++) {
			if(abilities[i] != null && abilities[i].getAbilityEnum() == abilityType)
				return i;
		}
		return -1;
	}

	public void addAbility(GameObject ability, int slot)
	{
		nofAbilities++;
		if (abilities [slot] != null) {
			removeAndDestroyAbility(slot);
		}
		abilityObjects [slot] = ability;
		abilities[slot] = (ability)abilityObjects [slot].GetComponent (typeof(ability));
	}

	public void improveAbility(int slot, int levelUp) 
	{
		int currentLevel = abilities [slot].level;
		int levelIncrease = (int)Mathf.Max (1, (levelUp - currentLevel) * 0.5f);
		int actualIncrease = abilities [slot].increaseLevel (levelIncrease);
	}

	public void improveAbility(EAbilityType abilityType, int levelUp) 
	{
		int slot = hasAbility (abilityType);
		if(slot == -1)
			return;
		int currentLevel = abilities [slot].level;
		int levelIncrease = (int)Mathf.Max (1, (levelUp - currentLevel) * 0.5f);
		int actualIncrease = abilities [slot].increaseLevel (levelIncrease);
	}

	public void removeAndDestroyAbility(int slot) {
		if (abilities [slot] != null) {
			nofAbilities--;
			abilities [slot] = null;
			GameObject.Destroy (abilityObjects [slot].gameObject);
		}
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

	public void applyEnvironmentalSlowDown(float slowDown)
	{
		environmentalSlowDown = slowDown;
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