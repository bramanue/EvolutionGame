using UnityEngine;
using System.Collections;

public class runAbility : ability {

	// The current speed boos tinduced by this ability
	private float currentSpeed;

	// The maximum speed boost achievable with this ability
	private float maxSpeed;

	// How long it takes to reach max speed
	public float timeToTopSpeed = 3.0f;

	// Indicates whether player/enemy is currently running or not
	private bool inUse;


	void Start () {
		// Get the game object which has this run ability
		parentBlob = transform.parent.gameObject;
		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent (typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent (typeof(player));
		// Decide whether this ability is attached to the player or to an enemy
		isPlayer = (parentPlayerScript != null);
		// The current speed
		currentSpeed = 0.0f;
		// The maximally auireable speed at the current ability level
		maxSpeed = level;
		// This ability is currently not used
		inUse = false;

		
		if (isPlayer) 
			parentPlayerScript.maxVelocity = parentPlayerScript.baseVelocity + maxSpeed;
		else
			parentEnemyScript.maxVelocity = parentEnemyScript.baseVelocity + maxSpeed;
		
		cooldownTime = 0;
		maxLevel = 40;
		abilityName = "Running ability";
		abilitySuperClassEnum = EAbilityClass.EPassiveAbility;
	}
	
	void Update () 
	{
		transform.Rotate (0, Time.deltaTime*360, 0);
	//	transform.localRotation = new Quaternion ();
		transform.localScale = new Vector3(1,1,1);
		transform.localPosition = new Vector3(0,0,0);

		maxSpeed = 0.5f*level;

		// Set current speed back to default value if it is not used for some time
		if (!inUse) {
			currentSpeed = Mathf.Max(0.0f, currentSpeed - maxSpeed / timeToTopSpeed * Time.deltaTime);
		}
		if (isPlayer) 
			parentPlayerScript.runVelocityBoost = currentSpeed;
		else
			parentEnemyScript.runVelocityBoost = currentSpeed;
		inUse = false;

	}

	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		maxSpeed = 0.5f*level;
		Debug.Log (x + " to run ability");

		return level - previousLevel;
	}

	public override bool useAbility() 
	{
		inUse = true;
		// Current speed is either maxSpeed or the accelerated min speed
		currentSpeed = Mathf.Min(maxSpeed, currentSpeed + maxSpeed / timeToTopSpeed * Time.deltaTime);
		if (isPlayer) 
			parentPlayerScript.runVelocityBoost = currentSpeed;
		else
			if(parentEnemyScript)
				parentEnemyScript.runVelocityBoost = currentSpeed;
		return true;
	}

	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.ERunAbility;
	}

}
