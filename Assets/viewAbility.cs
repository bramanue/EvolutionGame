using UnityEngine;
using System.Collections;

public class viewAbility : ability {

	// Sets the minimal viewing range
	public float minViewingRange;

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
		// Set the minimal viewing range
		minViewingRange = 5.0f;

		cooldownTime = 0;
		maxLevel = 45;
		abilityName = "ViewAbility";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		// TODO update viusals
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		viewingRange = minViewingRange + level;

		if (isPlayer) {
			parentPlayerScript.viewingRange = viewingRange;
		} else {
			parentEnemyScript.viewingRange = viewingRange;
		}
		Debug.Log (x + " to view ability");
		return level - previousLevel;
	}

	// Not really necessary (does not need to be called in every frame but only at level increase)
	public override bool useAbility()
	{
		if (isPlayer) {
			parentPlayerScript.viewingRange = viewingRange;
		} else {
			parentEnemyScript.viewingRange = viewingRange;
		}
		return true;
	}

	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EViewAbility;
	}
}
