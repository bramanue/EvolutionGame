using UnityEngine;
using System.Collections;

public class dustShieldAbility : ability {
	
	private float timer;
	
	private float maxTimeInSandstorm = 30.0f;
	
	private float damageMultiplier = 1.1f;
	
	private bool inUse;

	private bool deactivateInNextFrame;

	public Material dustShieldMaterial;
	
	// Use this for initialization
	void Start () {
		// Get the game object which has this ram ability
		parentBlob = transform.parent.gameObject;
		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent(typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent(typeof(player));
		isPlayer = (bool)parentPlayerScript;

		increaseLevel (0);
		cooldownTimer = 0.0f;

		maxTimeInSandstorm = 30.0f + 30.0f * level;
		timer = maxTimeInSandstorm;
		damageMultiplier = 0.1f + level * 0.1f;

		abilitySuperClassEnum = EAbilityClass.EShieldAbility;
		distortionType = EDistortionType.EDustShieldDistortion;
	}
	
	// Update is called once per frame
	void Update () {

	}

	public override void resetTransform()
	{
		transform.localScale = new Vector3 (1, 1, 1);
		transform.localPosition = new Vector3 (0, 0, 0);
		transform.localRotation = new Quaternion ();
	}

	void LateUpdate() 
	{
		if (deactivateInNextFrame)
			inUse = false;

		if (inUse) {
			// TODO change visuals
			((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = dustShieldMaterial;
			((meshDistorter)parentBlob.GetComponent(typeof(meshDistorter))).activateShield(distortionType);
			
			// Make sure ability is not able to be used forever unless the ability is at its max level
			if(level < maxLevel)
				timer -= Time.deltaTime;
		} 
		else 
		{
			cooldownTimer -= Time.deltaTime;
			
			if (timer <= 0 && cooldownTimer <= 0) {
				// Once the ability has been used to its end, give it a 5 second cooldown, befor it can be used again
				cooldownTimer = 5.0f;
			} 
			
			// Restore timer, when ability is not in use
			timer = Mathf.Min (maxTimeInSandstorm, timer + Time.deltaTime);
			
			// Reset to default sprite if no other shield is active
			if(isPlayer){
				if(parentPlayerScript.shieldInUse == null)
					((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = parentPlayerScript.defaultMaterial;
					//((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = parentPlayerScript.defaultColor;
			}else{
				if(parentEnemyScript.shieldInUse == null)
					((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = parentEnemyScript.defaultMaterial;
					//((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = parentEnemyScript.defaultColor;
			}
		}
		deactivateInNextFrame = true;
	}

	
	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		damageMultiplier = 0.1f + level * 0.1f;
		maxTimeInSandstorm = 30.0f + 30.0f * level;
		return level - previousLevel;
	}
	
	public override bool useAbility() 
	{
		if (timer > 0 && cooldownTimer < 0) {
			inUse = true;
			deactivateInNextFrame = false;
			return true;
		} else {
			return false;
		}
	}

	public override float calculateUseProbability(player playerScript, Vector3 toPlayer, bool attack, bool canSeePlayer) 
	{
		if (cooldownTimer > 0)
			return 0.0f;
		
		// If we are in the desert, return a high probability
		if (parentEnemyScript.currentEnvironment != null && parentEnemyScript.currentEnvironment.requiredAbility == EAbilityType.EDustShieldAbility) {
			return 0.9f;
		}
		
		// If we are close to the desert, also return a high probability
		if (parentEnemyScript.environmentProximityData != null && parentEnemyScript.environmentProximityData.requiredAbility == EAbilityType.EDustShieldAbility) {
			return 0.7f;
		}
		
		// If running away from player and player is close enough, activate shield for defense
		if (attack == false && canSeePlayer && toPlayer.magnitude - parentBlob.transform.localScale.x - playerScript.size < parentBlob.transform.localScale.x) {
			if(playerScript.shieldInUse != null && playerScript.shieldInUse.abilityEnum == EAbilityType.EThornShieldAbility)
				return 0.9f;
			else
				return 0.8f;
		}
		
		// If attacking player and remaining use time is high enough, then activate the shield
		if (attack && maxTimeInSandstorm > 15) {
			return 0.9f;
		}
		
		return 0.0f;
	}
	
	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EDustShieldAbility;
	}
	
}
