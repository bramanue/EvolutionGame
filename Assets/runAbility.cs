using UnityEngine;
using System.Collections;

public class runAbility : ability {

	private float currentSpeed;

	private float maxSpeed;

	public float timeToTopSpeed;

	public float baseSpeed;

	private bool inUse;


	void Start () {
		// Get the game object which has this run ability
		parentBlob = transform.parent.gameObject;
		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent (typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent (typeof(player));
		// Decide whether this ability is attached to the player or to an enemy
		isPlayer = (parentPlayerScript != null);
		// The lowest speed
		baseSpeed = 4.0f;
		// The current speed
		currentSpeed = baseSpeed;
		// The maximally auireable speed at the current ability level
		maxSpeed = baseSpeed + level;
		// Blob needs 3 seconds to get to max speed
		timeToTopSpeed = 3.0f;
		// This ability is currently not used
		inUse = false;
		
		cooldownTime = 0;
		maxLevel = 20;
		abilityName = "RunAbility";
	}
	
	void Update () {
		// Set current speed back to default value if it is not used for some time
		if (!inUse) {
			currentSpeed = Mathf.Max(baseSpeed, currentSpeed - (maxSpeed - baseSpeed) / timeToTopSpeed * Time.deltaTime);
			if (isPlayer) 
				parentPlayerScript.maxVelocity = currentSpeed;
			else
				parentEnemyScript.maxVelocity = currentSpeed;
		}
		inUse = false;
	}

	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		maxSpeed = baseSpeed + level;
		Debug.Log (x + " to run ability");
		return level - previousLevel;
	}

	public override bool useAbility() 
	{
		inUse = true;
		// Current speed is either maxSpeed or the accelerated min speed
		currentSpeed = Mathf.Min(maxSpeed, currentSpeed + (maxSpeed - baseSpeed) / timeToTopSpeed * Time.deltaTime);
		if (isPlayer) 
			parentPlayerScript.maxVelocity = currentSpeed;
		else
			parentEnemyScript.maxVelocity = currentSpeed;
		return true;
	}

	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.ERunAbility;
	}

}
