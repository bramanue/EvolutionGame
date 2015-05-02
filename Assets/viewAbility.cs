using UnityEngine;
using System.Collections;

public class viewAbility : ability {

	// Sets the minimal viewing range
	public float baseViewingRange = 1.0f;

	// Defines the 
	private float targetViewingRange;

	// Defines the current viewing range
	private float currentViewingRange;

	// Defines how much sight the blob gains per second on leveling up this ability
	public float gainPerSecond = 1.0f;


	// Use this for initialization
	void Start () {
		// Get the game object which has this run ability
		parentBlob = transform.parent.gameObject;
		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent (typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent (typeof(player));
		// Decide whether this ability is attached to the player or to an enemy
		isPlayer = (parentPlayerScript != null);
		// Calculate the initial viewing range
		currentViewingRange = baseViewingRange + level;

		cooldownTime = 0;
		abilityName = "View Ability";
		abilitySuperClassEnum = EAbilityClass.EPassiveAbility;

		if (isPlayer) {
			parentPlayerScript.viewingRangeBoost = currentViewingRange;
		} else {
			parentEnemyScript.viewingRangeBoost = currentViewingRange;
		}
		targetViewingRange = currentViewingRange;
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.localPosition = new Vector3 (0, 0, 0);
		transform.localRotation = new Quaternion ();
		transform.localScale = new Vector3 (0, 0, 0);

		targetViewingRange = baseViewingRange + level;

		// Smoothly change the currentViewingRange after level-up/-down
		float difference = targetViewingRange - currentViewingRange;
		if (Mathf.Abs (difference) < Time.deltaTime * gainPerSecond) {
			currentViewingRange = targetViewingRange;
		} else {
			float sign = Mathf.Sign (difference);
			currentViewingRange += sign * gainPerSecond * Time.deltaTime;
		}

		if (isPlayer) {
			// TODO Change light intensity of mesh according to currentViewingRange
			parentPlayerScript.viewingRangeBoost = currentViewingRange;
		} else {
			parentEnemyScript.viewingRangeBoost = currentViewingRange;
		}
	}

	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		// TODO update viusals
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		targetViewingRange = baseViewingRange + level;

		return level - previousLevel;
	}

	// Not really necessary (does not need to be called in every frame but only at level increase)
	public override bool useAbility()
	{
		if (isPlayer) {
			parentPlayerScript.viewingRangeBoost = currentViewingRange;
		} else {
			// After respawn the script reference can be null
			if(parentEnemyScript != null)
				parentEnemyScript.viewingRangeBoost = currentViewingRange;
		}
		return true;
	}

	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EViewAbility;
	}
}
