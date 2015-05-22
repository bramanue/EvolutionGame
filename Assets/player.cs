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

	// Sound manager
	private audioManager audioMngr;

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

	private bool[] triggerReleased = new bool[2];

	public Color defaultColor = new Color(0.7f,0.7f,1.0f);

	public float environmentalSlowDown = 1.0f; 

	public Material defaultMaterial;

	public abilityLoot nearbyAbilityLoot;

	private float previousSize;

	private highscoreManager highscoreManager;

	private bool waitedOneFrame = false;

	private BloomPro videoFilter;

	private float damageTimer;

	private float currentDamageOffset = 0.0f;


	// Use this for initialization
	void Start()
	{
		// Get an instance of the enemy manager
		enemyMngr = (enemyManager)GameObject.Find ("EnemyManager").GetComponent (typeof(enemyManager));
		audioMngr = (audioManager)GameObject.Find ("GameManager").GetComponent (typeof(audioManager));
		// Get size
		size = transform.localScale.x;
		// Player is allowed to move
		canMove = true;
		// Get the ability modification panel
		abilityModificationScript = (abilityModificationPanel)(GameObject.Find("AbilityModificationPanel").GetComponent(typeof(abilityModificationPanel)));
		abilityManager = (abilityManager)GameObject.Find ("AbilityManager").GetComponent(typeof(abilityManager));
		highscoreManager = (highscoreManager)GameObject.Find ("HighscoreManager").GetComponent(typeof(highscoreManager));
		GameObject cam = GameObject.Find ("MainCamera");
		videoFilter = (BloomPro)GameObject.Find ("MainCamera").GetComponent (typeof(BloomPro));
		videoFilter.ChromaticAberrationOffset = 1.7f;
		previousSize = size;
	}
	
	// Update is called once per frame
	void Update()
	{
		if (!dead) {
			if (nearbyAbilityLoot != null) {
				// Show display to press RB or LB if player wants to acquire the ability
				string abilityName = nearbyAbilityLoot.abilityName;
				EAbilityClass abilityClass = nearbyAbilityLoot.abilityClass;
				abilityModificationScript.displayTitle (abilityName);
				waitedOneFrame = false;

				if (Input.GetButtonDown ("PauseForAbilityGatheringLB") || Input.GetButtonDown ("PauseForAbilityGatheringRB")) {
					// Open the modification panel if it is not enabled
					if (!abilityModificationScript.isActive ()) {
						string[] abilityNames = new string[6];
						for (int i = 0; i < 6; i++) {
							abilityNames [i] = (abilities [i] != null) ? (abilities [i].abilityName + " (lvl. " + abilities [i].level + ")") : string.Empty;
						}
						abilityModificationScript.showPanel (abilityNames, abilityClass);

						if (abilityClass == EAbilityClass.EActiveAbility) {
							abilityModificationScript.displayMessage (abilityName + "\n" + nearbyAbilityLoot.abilityDescription + "\nPress the button you want to map it to. \n Press \"RB\" or \"LB\" to continue.");
							return;
						} else if (abilityClass == EAbilityClass.EShieldAbility) {
							abilityModificationScript.displayMessage (abilityName + "\n" + nearbyAbilityLoot.abilityDescription + "\nPress the trigger you want to map it to. \n Press \"RB\" or \"LB\" to continue.");
							return;
						}
					} else {
						// Close modification panel if it was already enabled
						abilityModificationScript.hidePanel ();
						triggerReleased [0] = triggerReleased [1] = false;
						selectionConfirmed [0] = selectionConfirmed [1] = selectionConfirmed [2] = selectionConfirmed [3] = false;
					}
					
				}

				if (abilityModificationScript.isActiveAndEnabled && abilityModificationScript.isInChosingState) {
					if (abilityClass == EAbilityClass.EActiveAbility) {
						// Capture player input
						if (Input.GetButtonDown ("Ability0")) {
							if (abilities [0] != null && !selectionConfirmed [0]) {
								selectionConfirmed [0] = true;
								abilityModificationScript.displayMessage ("Please press \"A\" again to confirm \n" + abilityName + "\n" + nearbyAbilityLoot.abilityDescription +  "\nPress \"LB\" or \"RB\" to continue \n Press one of the other buttons to map the new ability to them");
							} else {
								abilityManager.addAbilityToPlayer (this.gameObject, nearbyAbilityLoot.abilityType, 0, 1);
								Debug.Log ("You gained a new ability : " + abilities [0].abilityName + " at level " + abilities [0].level);
								abilityModificationScript.hidePanel ();
								selectionConfirmed [0] = false;
							}
							// Reset the other selections
							selectionConfirmed [1] = selectionConfirmed [2] = selectionConfirmed [3] = false;
						} else if (Input.GetButtonDown ("Ability1")) {
							if (abilities [1] != null && !selectionConfirmed [1]) {
								selectionConfirmed [1] = true;
								abilityModificationScript.displayMessage ("Please press \"B\" again to confirm \n" + abilityName + "\n" + nearbyAbilityLoot.abilityDescription +  "\n Press \"LB\" or \"RB\" to continue \n Press one of the other buttons to map the new ability to them.");
							} else {
								abilityManager.addAbilityToPlayer (this.gameObject, nearbyAbilityLoot.abilityType, 1, 1);
								Debug.Log ("You gained a new ability : " + abilities [1].abilityName + " at level " + abilities [1].level);
								abilityModificationScript.hidePanel ();
								selectionConfirmed [1] = false;
							}
							// Reset the other selections
							selectionConfirmed [0] = selectionConfirmed [2] = selectionConfirmed [3] = false;
						} else if (Input.GetButtonDown ("Ability2")) {
							if (abilities [2] != null && !selectionConfirmed [2]) {
								selectionConfirmed [2] = true;
								abilityModificationScript.displayMessage ("Please press \"X\" again to confirm \n" + abilityName + "\n" + nearbyAbilityLoot.abilityDescription +  "\n Press \"LB\" or \"RB\" to continue \n Press one of the other buttons to map the new ability to them.");
							} else {
								abilityManager.addAbilityToPlayer (this.gameObject, nearbyAbilityLoot.abilityType, 2, 1);
								Debug.Log ("You gained a new ability : " + abilities [2].abilityName + " at level " + abilities [2].level);
								abilityModificationScript.hidePanel ();
								selectionConfirmed [2] = false;
							}
							// Reset the other selections
							selectionConfirmed [0] = selectionConfirmed [1] = selectionConfirmed [3] = false;
						} else if (Input.GetButtonDown ("Ability3")) {
							if (abilities [3] != null && !selectionConfirmed [3]) {
								selectionConfirmed [3] = true;
								abilityModificationScript.displayMessage ("Please press \"Y\" again to confirm \n" + abilityName + "\n" + nearbyAbilityLoot.abilityDescription +  "\n Press \"LB\" or \"RB\" to continue \n Press one of the other buttons to map the new ability to them");
							} else {
								abilityManager.addAbilityToPlayer (this.gameObject, nearbyAbilityLoot.abilityType, 3, 1);
								Debug.Log ("You gained a new ability : " + abilities [3].abilityName + " at level " + abilities [3].level);
								abilityModificationScript.hidePanel ();
								selectionConfirmed [3] = false;
							}
							// Reset the other selections
							selectionConfirmed [0] = selectionConfirmed [1] = selectionConfirmed [2] = false;
						}
					} else if (abilityClass == EAbilityClass.EShieldAbility) {
						float shields = Input.GetAxis ("Shields");
						if (shields > 0.5f) {
							if (abilities [4] != null && !selectionConfirmed [0]) {
								selectionConfirmed [0] = true;
								triggerReleased [0] = false;
								abilityModificationScript.displayMessage ("Please press \"RT\" again to confirm \n" + abilityName + "\n" + nearbyAbilityLoot.abilityDescription +  "\n Press \"LB\" or \"RB\" to continue \n Press the LT to map the new ability to the other trigger");
							} else if (abilities [4] == null || triggerReleased [0]) {
								abilityManager.addAbilityToPlayer (this.gameObject, nearbyAbilityLoot.abilityType, 4, 1);
								Debug.Log ("You gained a new ability : " + abilities [4].abilityName + " at level " + abilities [4].level);
								abilityModificationScript.hidePanel ();
								selectionConfirmed [0] = false;
							}
							// Reset the other selections
							selectionConfirmed [1] = false;
							triggerReleased [1] = false;
						} else if (shields < -0.5f) {
							if (abilities [5] != null && !selectionConfirmed [1]) {
								selectionConfirmed [1] = true;
								triggerReleased [1] = false;
								abilityModificationScript.displayMessage ("Please press \"LT\" again to confirm \n" + abilityName + "\n" + nearbyAbilityLoot.abilityDescription +  "\n Press \"LB\" or \"RB\" to continue \n Press RT to map the new ability to the other trigger.");
							} else if (abilities [5] == null || triggerReleased [1]) {
								abilityManager.addAbilityToPlayer (this.gameObject, nearbyAbilityLoot.abilityType, 5, 1);
								Debug.Log ("You gained a new ability : " + abilities [5].abilityName + " at level " + abilities [5].level);
								abilityModificationScript.hidePanel ();
								selectionConfirmed [1] = false;
							}
							selectionConfirmed [0] = false;
							triggerReleased [0] = false;
						} else if (Mathf.Abs (shields) < 0.05) {
							// Check for release of trigger after first selection
							if (selectionConfirmed [0]) {
								triggerReleased [0] = true;
							}
							if (selectionConfirmed [1]) {
								triggerReleased [1] = true;
							}
						}
					}
					return;
				} 
				else 
				{
					// If not paused yet, then remove reference
					abilityModificationScript.displayMessage("Press \"RB\" or \"LB\" to gather new ability");
					nearbyAbilityLoot = null;
				}
			} 
			else 
			{
				// If no ability loot in reach
				if(waitedOneFrame) {
					abilityModificationScript.hideTitle();
					abilityModificationScript.displayMessage("");
					waitedOneFrame = false;
				}
				else
				{
					waitedOneFrame = true;
				}
			}

			// Get the viewing range
			if(abilities[7])
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

			shieldInUse = null;

			if (!stunned) {

				// Capture player input
				if (Input.GetButton ("Ability0")) {
					if (abilities [0] != null) {
						shieldInUse = abilities [0];
						abilities [0].useAbility ();
					}
				}
				if (Input.GetButton ("Ability1")) {
					if (abilities [1] != null) {
						shieldInUse = abilities [1];
						abilities [1].useAbility ();
					}
				}
				if (Input.GetButton ("Ability2")) {
					if (abilities [2] != null) {
						shieldInUse = abilities [2];
						abilities [2].useAbility ();
					}
				}
				if (Input.GetButton ("Ability3")) {
					if (abilities [3] != null) {
						shieldInUse = abilities [3];
						abilities [3].useAbility ();
					}
				}

				// Capture player input concerning shields (XBOX360 only)
				float shields = Input.GetAxis ("Shields");
				if (shields > 0.05f) {
					Debug.Log ("Activate shield0");
					if (abilities [4] != null && abilities [4].useAbility ()) {
						shieldInUse = abilities [4];
					}
				} else if (shields < -0.05f) {
					Debug.Log ("Activate shield1");
					if (abilities [5] != null && abilities [5].useAbility ()) {
						shieldInUse = abilities [5];
					}
				}


				if (canMove) {
					// Get the target direction from user input
					Vector3 targetDirection = new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"), 0.0f).normalized;
					// Get the desired speed fraction
					float speedFraction = targetDirection.magnitude;
					// Calculate the current maximally achievable speed
					currentSpeed = (baseVelocity + runVelocityBoost) * environmentalSlowDown;
					environmentalSlowDown = 1.0f;

					// If the player is moving
					if (speedFraction > 0) {
						// Use the run ability to become faster
						if(abilities[6])
							abilities [6].useAbility ();
						// Calculate the angle to rotate about
						float angleBetween = Mathf.Sign (Vector3.Cross (viewingDirection, targetDirection).z) * Vector3.Angle (viewingDirection, targetDirection);
						// Get the rotation target
						Quaternion rotationTargetQuaternion = Quaternion.Euler (new Vector3 (0.0f, 0.0f, transform.localEulerAngles.z + angleBetween));
						// Reset time scale to normal
						transform.rotation = Quaternion.Slerp (transform.rotation, rotationTargetQuaternion, Time.deltaTime * Mathf.Min (10.0f, currentSpeed));
						// Move blob according to joystick input
						transform.position += viewingDirection * Time.deltaTime * speedFraction * currentSpeed;
					}
				}
			} else {
				stunnedTimer -= Time.deltaTime;
				if (stunnedTimer <= 0)
					stunned = false;
			}

			// Apply environmental push back force
			transform.position += environmentalPushBack;
			environmentalPushBack = new Vector3 (0, 0, 0);

			float totalDamage = environmentalDamage + abilityDamage;
			if(totalDamage > 0)
			{
				audioMngr.PlayhurtSound();
				float factor = Mathf.Min (2.0f,totalDamage/size);
				currentDamageOffset = 1.5f*factor*10f;
				StartCoroutine(visualizeDamage(Mathf.Max (factor,0.3f),currentDamageOffset));
				damageTimer = Mathf.Max (factor,0.3f);
			}
			else
			{
				if(damageTimer > 0)
					damageTimer -= Time.deltaTime;
				else
				{
					videoFilter.ChromaticAberrationOffset = Mathf.Max (1.7f, videoFilter.ChromaticAberrationOffset - 3.0f*Time.deltaTime);
					videoFilter.BloomParams.BloomIntensity = Mathf.Max (1.5f, videoFilter.BloomParams.BloomIntensity - 3.0f*Time.deltaTime);
					videoFilter.BloomParams.BloomThreshold = Mathf.Min (0.8f, videoFilter.BloomParams.BloomThreshold + 1.2f*Time.deltaTime);
				}
			}

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
		} else {
			// If dead
			abilityModificationScript.displayTitle("");
			// Reset viewing range towards 0
			if(abilities[7]) {
				abilities [7].level = 0;
				abilities [7].useAbility ();
			}
			currentViewingRange = baseViewingRange + viewingRangeBoost;

			videoFilter.ChromaticAberrationOffset = Mathf.Max (1.7f, videoFilter.ChromaticAberrationOffset - 3.0f*Time.deltaTime);
			videoFilter.BloomParams.BloomIntensity = Mathf.Max (1.5f, videoFilter.BloomParams.BloomIntensity - 3.0f*Time.deltaTime);
			videoFilter.BloomParams.BloomThreshold = Mathf.Min (0.8f, videoFilter.BloomParams.BloomThreshold + 1.2f*Time.deltaTime);
		}

		// Change appearance according to current size
		shrinkSpeed = Mathf.Max (1.0f,size);
		growSpeed = Mathf.Max (1.0f, size);

		grow();
	}

	IEnumerator visualizeDamage(float duration, float offset)
	{
		float offsetPerSecond = offset / duration;
		for (float time = 0; time < duration; time += Time.deltaTime) 
		{
			videoFilter.ChromaticAberrationOffset = Mathf.Min (2.5f, videoFilter.ChromaticAberrationOffset + Time.deltaTime*offsetPerSecond);
			videoFilter.BloomParams.BloomIntensity = Mathf.Min (3.0f, videoFilter.BloomParams.BloomIntensity + Time.deltaTime*offsetPerSecond);
			videoFilter.BloomParams.BloomThreshold = Mathf.Max (0.0f, videoFilter.BloomParams.BloomThreshold - Time.deltaTime*offsetPerSecond);
			yield return null;
		}
	}

	void LateUpdate() {
		if (size <= 0) {
			size = 0;
			dead = true;
		}
		// Reset the current environment
		currentEnvironment = null;
		// Make sure new bloom parameters are applied
		videoFilter.Init( false );
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
			float nextSize = Mathf.Max(size,currentSize - Time.deltaTime*shrinkSpeed);
			transform.localScale = new Vector3(nextSize, nextSize, nextSize);
		}
		else
		{
			// Growing
			float nextSize = Mathf.Min(size,currentSize + Time.deltaTime*growSpeed);
			transform.localScale = new Vector3(nextSize, nextSize, nextSize);
			highscoreManager.playerIsGrowing((nextSize-currentSize)*100);

		}
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
		abilities [slot].resetTransform ();
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

	public int getNofAbilities()
	{
		return nofAbilities;
	}

	public void removeAllAbilities() 
	{
		for (int slot = 0; slot < 8; slot++) {
			removeAndDestroyAbility(slot);
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