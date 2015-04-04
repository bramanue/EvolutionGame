using UnityEngine;
using System.Collections;

public class ability : MonoBehaviour {

	// Defines how strong the ability is
	public int level;

	// Sets the maximally achievable level
	public int maxLevel;

	// The name of the ability
	public string abilityName;

	// Cooldowntimer for active abilities
	public float cooldownTimer;

	// The cooldown time after each use
	public float cooldownTime;

	public GameObject parentBlob;

	public enemy parentEnemyScript;

	public player parentPlayerScript;

	public bool isPlayer;


	void Start () {

	}

	void Update () {

	}

	// Activate effect of ability
	public virtual bool useAbility()
	{
		Debug.Log ("Ability Base class : useAbility()");
		if (cooldownTimer <= 0)
			return true;
		else
			return false;
	}

	// Increases the lebel of the ability by x and returns how many levels it really increased
	public virtual int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		return level - previousLevel;

		// TODO Change appearance of ability sprite
	}

	// Returns whether this ability is ready to execute (i.e. cooldown timer below 0)
	public bool isReady()
	{
		return cooldownTimer <= 0;
	}

	// Returns the enum of this ability
	public virtual EAbilityType getAbilityEnum()
	{
		return EAbilityType.EEmptyAbility;
	}
}
