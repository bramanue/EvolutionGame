using UnityEngine;
using System.Collections;

public class thornShieldAbility : ability {

	private float timer;

	private float maxTimeInThorns = 30.0f;

	private float damage = 0.1f;

	private bool inUse;

	private bool deactivateInNextFrame;

	public Material thornShieldMaterial;

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
		maxTimeInThorns = 30.0f + 30.0f * level;
		timer = maxTimeInThorns;
		damage = 0.1f + level * 0.1f;

		abilitySuperClassEnum = EAbilityClass.EShieldAbility;
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = new Vector3 (0, 0, 0);
		transform.localScale = new Vector3 (1, 1, 1);
	}

	void LateUpdate() {

		if (deactivateInNextFrame)
			inUse = false;

		if (inUse) {
			// TODO change visuals
			((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = thornShieldMaterial;
		//	((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = new Color(0.9f,0,0.9f,1.0f);
			
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
			timer = Mathf.Min (maxTimeInThorns, timer + Time.deltaTime);
			
			// Reset to default sprite if no other shield is active
			if(isPlayer) {
				if(parentPlayerScript.shieldInUse == null) 
				//	((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = parentPlayerScript.defaultColor;
					((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = parentPlayerScript.defaultMaterial;
			} else {
				if(parentEnemyScript.shieldInUse == null)
				//	((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = parentEnemyScript.defaultColor;
					((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = parentEnemyScript.defaultMaterial;
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
			// Enemy is hurt by player's thorn shield if enemy does not have a thorn shield, dust shield or lava shield
		/*	if(enemyScript.hasAbility(EAbilityType.EThornShieldAbility) == -1 && 
			   enemyScript.hasAbility(EAbilityType.EDustShieldAbility) == -1 && 
			   enemyScript.hasAbility(EAbilityType.ELavaShieldAbility) == -1)
			{
				enemyScript.size -= damage;
				enemyScript.setAlertState();
			}*/

			// Enemy is hurt by player's thorn shield if enemy does not have an active thorn, dust or lava shield
			if(enemyScript.shieldInUse == null || (
			   enemyScript.shieldInUse.getAbilityEnum() != EAbilityType.EThornShieldAbility && 
			   enemyScript.shieldInUse.getAbilityEnum() != EAbilityType.EDustShieldAbility && 
			   enemyScript.shieldInUse.getAbilityEnum() != EAbilityType.ELavaShieldAbility ) )
			{
				Debug.Log ("Enemy hurt by thorn shield: Damage = " + damage);
				enemyScript.size -= damage;
				enemyScript.setAlertedState();
			}
		} else if (!isPlayer && playerScript) {
			// Player is hurt by enemy's thorn shield if player does not have a thorn shield or dust shield
		/*	if(playerScript.hasAbility(EAbilityType.EThornShieldAbility) == -1 && playerScript.hasAbility(EAbilityType.EDustShieldAbility) == -1 )
				playerScript.size -= damage;
		*/
			// Player is hurt by enemy's thorn shield if player does not have an active thorn, dust or lava shield
			if(playerScript.shieldInUse == null || (
			   playerScript.shieldInUse.getAbilityEnum() != EAbilityType.EThornShieldAbility && 
			   playerScript.shieldInUse.getAbilityEnum() != EAbilityType.EDustShieldAbility && 
			   playerScript.shieldInUse.getAbilityEnum() != EAbilityType.ELavaShieldAbility ) )
			{
				playerScript.size -= damage;
			}
		}
	}

	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		damage = 0.1f + level * 0.1f;
		maxTimeInThorns = 30.0f + 30.0f * level;
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
		
		// If we are in the water, return a high probability
		if (parentEnemyScript.currentEnvironment != null && parentEnemyScript.currentEnvironment.requiredAbility == EAbilityType.EThornShieldAbility) {
			return 0.9f;
		}
		
		// If we are close to the water, also return a high probability
		if (parentEnemyScript.environmentProximityData != null && parentEnemyScript.environmentProximityData.requiredAbility == EAbilityType.EThornShieldAbility) {
			return 0.7f;
		}

		// If running away from player and player is close enough, activate shield for defense
		if (attack == false && canSeePlayer && toPlayer.magnitude - parentBlob.transform.localScale.x - playerScript.size < parentBlob.transform.localScale.x) {
			return 0.8f;
		}

		// If attacking player and remaining use time is high enough, then activate the shield
		if (attack && maxTimeInThorns > 15) {
			if(playerScript.shieldInUse != null && playerScript.shieldInUse.abilityEnum == EAbilityType.ELavaShieldAbility)
				return Random.Range(0.0f,0.3f);
			else
				return 0.8f;
		}
		
		return 0.0f;
	}

	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EThornShieldAbility;
	}

}
