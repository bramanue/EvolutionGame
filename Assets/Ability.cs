using UnityEngine;
using System.Collections;

public class ability : MonoBehaviour {

	// Defines how strong the ability is
	public int level;

	public int maxLevel;

	// Defines whether this is an active or passive ability
	public bool isActiveAbility;

	// The name of the ability
	public string name;

	// Cooldowntimer for active abilities
	public float cooldownTimer;

	// The cooldown time after each use
	public float cooldownTime;
	
	public float damage;

	public GameObject parentBlob;

	public enemy parentEnemyScript;

	public player parentPlayerScript;

	public bool isPlayer;

	void Start () {

	}

	void Update () {
		cooldownTimer -= Time.deltaTime;
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
		level = Mathf.Min(level + x, maxLevel);
		return level - previousLevel;

		// TODO Change appearance of ability sprite
	}

}
