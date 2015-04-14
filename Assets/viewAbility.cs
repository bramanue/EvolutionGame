using UnityEngine;
using System.Collections;

public class viewAbility : ability {

	// Sets the minimal viewing range
	public float baseViewingRange = 1.0f;

	// Defines the current viewing range
	private float viewingRange;


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
		viewingRange = baseViewingRange + level;

		cooldownTime = 0;
		abilityName = "View Ability";
		abilitySuperClassEnum = EAbilityClass.EPassiveAbility;

		if (isPlayer) {
			parentPlayerScript.viewingRangeBoost = viewingRange;
		} else {
			parentEnemyScript.viewingRangeBoost = viewingRange;
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = new Vector3 (0, 0, 0);
		if (isPlayer) {
			parentPlayerScript.viewingRangeBoost = viewingRange;
		} else {
			parentEnemyScript.viewingRangeBoost = viewingRange;
		}
	}

	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		// TODO update viusals
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		viewingRange = baseViewingRange + level;

		if (isPlayer) {
			parentPlayerScript.viewingRangeBoost = viewingRange;
		} else {
			parentEnemyScript.viewingRangeBoost = viewingRange;
		}
		return level - previousLevel;
	}

	// Not really necessary (does not need to be called in every frame but only at level increase)
	public override bool useAbility()
	{
		if (isPlayer) {
			parentPlayerScript.viewingRangeBoost = viewingRange;
		} else {
			parentEnemyScript.viewingRangeBoost = viewingRange;
		}
		return true;
	}

	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EViewAbility;
	}
}
