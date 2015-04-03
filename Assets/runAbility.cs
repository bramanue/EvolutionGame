using UnityEngine;
using System.Collections;

public class runAbility : ability {

	private float currentSpeed;

	private float maxSpeed;

	public float timeToTopSpeed;

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
		currentSpeed = 4.0f;
		// The maximally auireable speed at the current ability level
		maxSpeed = 4.0f + level;
		// Blob needs 3 seconds to get to max speed
		timeToTopSpeed = 3.0f;
		// This ability is currently not used
		inUse = false;
		
		cooldownTime = 0;
		maxLevel = 20;
		name = "RunAbility";
	}
	
	void Update () {
		// Set current speed back to default value if it is not used for some time
		if (!inUse) {
			currentSpeed = Mathf.Max(4.0f, currentSpeed - (maxSpeed - 4.0f) / timeToTopSpeed * 2.0f * Time.deltaTime);
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
		level = Mathf.Min(level + x, maxLevel);
		maxSpeed = 4.0f + level;
		return level - previousLevel;
	}

	public override bool useAbility() 
	{
		inUse = true;
		// Current speed is either maxSpeed or the accelerated min speed
		currentSpeed = Mathf.Min(maxSpeed, currentSpeed + (maxSpeed - 4.0f) / timeToTopSpeed * Time.deltaTime);
		if (isPlayer) 
			parentPlayerScript.maxVelocity = currentSpeed;
		else
			parentEnemyScript.maxVelocity = currentSpeed;
		return true;
	}

}
