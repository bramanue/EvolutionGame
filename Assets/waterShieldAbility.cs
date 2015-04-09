using UnityEngine;
using System.Collections;

public class waterShieldAbility : ability {
	
	private float timer;
	
	private float maxTimeInWater = 30.0f;
	
	private float damage = 0.1f;
	
	private bool inUse;
	
	// Use this for initialization
	void Start () {
		maxLevel = 10;
		timer = maxTimeInWater;
		damage = 0.1f + level * 0.1f;
		
		abilityName = "WaterShieldAbility";
	}
	
	// Update is called once per frame
	void Update () {
		if (inUse) {
			// Make sure ability is not able to be used forever unless the ability is at its max level
			if(level < maxLevel)
				timer -= Time.deltaTime;
			// If the timer runs out, the shield cannot be used until it's restored
			if(timer <= 0)
				inUse = false;
		} else {
			// Restore timer, when ability is not in use
			timer = Mathf.Max (maxTimeInWater, timer += Time.deltaTime);
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		// If collision with own blob, do nothing
		if (other.gameObject == parentBlob)
			return;
		
		// Check whether the teeth of the blob collided with another blob
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent (typeof(player));
		
		if (isPlayer && enemyScript) {
			// Enemy is hurt by player's water shield if enemy does  have a lava shield
			if(enemyScript.hasAbility(EAbilityType.ELavaShieldAbility) >= 0 )
			{
				enemyScript.size -= damage;
				enemyScript.setAlertState();
			}
		} else if (!isPlayer && playerScript) {
			// Player is hurt by enemy's water shield if player has a lava shield
			if(enemyScript.hasAbility(EAbilityType.ELavaShieldAbility) >= 0 )
				playerScript.size -= damage;
		}
		
		// TODO If collision with thorn bushes (maybe put into collider function of blob)
		
	}
	
	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		damage = 0.1f + level * 0.1f;
		maxTimeInWater = 30.0f + 30.0f * level;
		return level - previousLevel;
	}
	
	public override bool useAbility() 
	{
		if (inUse) {
			inUse = false;
			return true;
		}
		if (timer > 0) {
			// TODO change visuals
			inUse = true;
			return true;
		} else {
			return false;
		}
	}
	
	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EWaterShieldAbility;
	}
	
}
