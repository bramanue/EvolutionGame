using UnityEngine;
using System.Collections;

public class glowingShieldAbility : ability {
	
	private float timer;
	
	private float maxTimeToGlow = 30.0f;
	
	private float blindnessDuration = 1.0f;
	
	private bool inUse;

	private bool deactivateInNextFrame;

	public Material glowingShieldMaterial;
	
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

		maxTimeToGlow = 30.0f + 30.0f * level;
		timer = maxTimeToGlow;
		blindnessDuration = 1.0f + level * 0.3f;

		abilitySuperClassEnum = EAbilityClass.EShieldAbility;
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = new Vector3 (0, 0, 0);
		transform.localScale = new Vector3 (1, 1, 1);
	}

	void LateUpdate() {
		// Deactivate shield if requested in previous frame
			if (deactivateInNextFrame)
				inUse = false;

		if (inUse) {
			// TODO change visuals
			((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = glowingShieldMaterial;
		//	((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = new Color(0.0f,1.0f,0.0f,0.5f);
			
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
			timer = Mathf.Min (maxTimeToGlow, timer + Time.deltaTime);
			
			// Reset to default sprite if no other shield is active
			if(isPlayer) {
				if(parentPlayerScript.shieldInUse == null)
					((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = parentPlayerScript.defaultMaterial;
					//((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = parentPlayerScript.defaultColor;
			} else {
				if(parentEnemyScript.shieldInUse == null)
					((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = parentEnemyScript.defaultMaterial;
					//((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = parentEnemyScript.defaultColor;
			}
		}
		deactivateInNextFrame = true;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (!inUse)
			return;

		// If collision with own blob, do nothing
		if (other.gameObject == parentBlob)
			return;

		// Check whether the teeth of the blob collided with another blob
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent (typeof(player));
		
		if (isPlayer && enemyScript) {
			// Enemy is blinded by player's bioluminescence shield if enemy does not have a glowing shield itself
		/*	if(enemyScript.hasAbility(EAbilityType.EGlowingShieldAbility) == -1 )
			{
				// TODO Only if it is dark and if enemy is looking into this direction
				// TODO Cast effect
				enemyScript.setBlinded(blindnessDuration);
				enemyScript.setAlertState();
			}*/

			if(enemyScript.shieldInUse == null || enemyScript.shieldInUse.getAbilityEnum() != EAbilityType.EGlowingShieldAbility )
			{
				// TODO Only if it is dark and if enemy is looking into this direction
				// TODO Cast effect
				Debug.Log ("Enemy blinded by glowing shield");
				enemyScript.setBlinded(blindnessDuration);
				enemyScript.setAlertedState();
			}
		} else if (!isPlayer && playerScript) {
			// Player is hurt by enemy's thorn shield if player does not have a thorn shield or dust shield
		/*	if(playerScript.hasAbility(EAbilityType.EDustShieldAbility) == -1 )
			{
				// TODO Only if it is dark and if player is looking into this direction
				// TODO Cast effect
				playerScript.setBlinded(blindnessDuration);
			}*/

			if(playerScript.shieldInUse == null || playerScript.shieldInUse.getAbilityEnum() != EAbilityType.EGlowingShieldAbility )
			{
				// TODO Only if it is dark and if player is looking into this direction
				// TODO Cast effect
				playerScript.setBlinded(blindnessDuration);
			}
		}

	}
	
	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		blindnessDuration = 1.0f + level * 0.3f;
		maxTimeToGlow = 30.0f + 30.0f * level;
		return level - previousLevel;
	}
	
	public override bool useAbility() 
	{
		if (timer > 0 && cooldownTimer < 0) {
			// TODO change visuals (activate point light)
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
		
		// If we are in the water, return a high probability
		if (parentEnemyScript.currentEnvironment!= null && parentEnemyScript.currentEnvironment.requiredAbility == EAbilityType.EGlowingShieldAbility) {
			return 0.9f;
		}
		
		// If we are close to the water, also return a high probability
		if (parentEnemyScript.environmentProximityData != null && parentEnemyScript.environmentProximityData.requiredAbility == EAbilityType.EGlowingShieldAbility) {
			return 0.7f;
		}
		
		// If running away from player and player is close enough, activate shield for defense
		if (attack == false && canSeePlayer && toPlayer.magnitude - parentBlob.transform.localScale.x - playerScript.size < parentBlob.transform.localScale.x) {
			return 0.8f;
		}
		
		// If attacking player and remaining use time is high enough, then activate the shield
		if (attack && maxTimeToGlow > 15) {
			return 0.9f;
		}
		
		return 0.0f;
	}
	
	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EGlowingShieldAbility;
	}
	
}
