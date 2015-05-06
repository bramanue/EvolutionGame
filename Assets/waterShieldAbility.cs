using UnityEngine;
using System.Collections;

public class waterShieldAbility : ability {
	
	private float timer;
	
	private float maxTimeInWater = 30.0f;
	
	private float baseDamage = 0.3f;

	private float damagePerSecond;
	
	private bool inUse;

	private bool deactivateInNextFrame;

	public Material waterShieldMaterial;
	
	// Use this for initialization
	void Start () {
		// Get the game object which has this ram ability
		parentBlob = transform.parent.gameObject;
		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent(typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent(typeof(player));
		isPlayer = (bool)parentPlayerScript;
		
		cooldownTimer = 0.0f;

		increaseLevel (0);

		maxTimeInWater = 30.0f + 30.0f * level;
		timer = maxTimeInWater;
		damagePerSecond = baseDamage + level * 0.1f;

		abilitySuperClassEnum = EAbilityClass.EShieldAbility;
		distortionType = EDistortionType.EWaterShieldDistortion;
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
			((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = waterShieldMaterial;
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
			timer = Mathf.Min (maxTimeInWater, timer + Time.deltaTime);
			
			// Reset to default sprite if no other shield is active
			if(isPlayer) {
				if(parentPlayerScript.shieldInUse == null)
					((MeshRenderer)parentBlob.GetComponent<MeshRenderer>()).material = parentPlayerScript.defaultMaterial;
			} else {
				if(parentEnemyScript.shieldInUse == null)
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

			// Damage enemy if he has an active lava shield
			if(enemyScript.shieldInUse != null && enemyScript.shieldInUse.getAbilityEnum() == EAbilityType.ELavaShieldAbility )
			{
				// TODO Play "Zisch" sound
				Debug.Log ("Enemy hurt by water shield: Damage = " + damagePerSecond*Time.deltaTime);
				enemyScript.inflictAbilityDamage(damagePerSecond*Time.deltaTime);
				enemyScript.setAlertedState();
			}
		} else if (!isPlayer && playerScript) {

			// Player is damaged if lava shield active
			if(playerScript.shieldInUse != null && playerScript.shieldInUse.getAbilityEnum() == EAbilityType.ELavaShieldAbility )
			{
				// TODO Play "Zisch" sound
				playerScript.size -= damagePerSecond*Time.deltaTime;
			}
		}
	}

	void OnTriggerStay(Collider other)
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
			
			// Damage enemy if he has an active lava shield
			if(enemyScript.shieldInUse != null && enemyScript.shieldInUse.getAbilityEnum() == EAbilityType.ELavaShieldAbility )
			{
				Debug.Log ("Enemy hurt by water shield: Damage = " + damagePerSecond*Time.deltaTime);
				enemyScript.inflictAbilityDamage(damagePerSecond*Time.deltaTime);
				enemyScript.setAlertedState();
			}
		} else if (!isPlayer && playerScript) {
			
			// Player is damaged if lava shield active
			if(playerScript.shieldInUse != null && playerScript.shieldInUse.getAbilityEnum() == EAbilityType.ELavaShieldAbility )
			{
				playerScript.size -= damagePerSecond*Time.deltaTime;
			}
		}
		
	}
	
	// Increases the level of this ability by x and returns the effective change in levels
	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max (0, Mathf.Min(level + x, maxLevel));
		damagePerSecond = damagePerSecond + level * 0.1f;
		maxTimeInWater = 30.0f + 30.0f * level;
		return level - previousLevel;
	}
	
	public override bool useAbility() 
	{
		if (timer > 0 && cooldownTimer < 0) {
			inUse = true;
			deactivateInNextFrame = false;
			if(isPlayer)
				parentPlayerScript.shieldInUse = this;
			else
				parentEnemyScript.shieldInUse = this;
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
		if (parentEnemyScript.currentEnvironment != null && parentEnemyScript.currentEnvironment.requiredAbility == EAbilityType.EWaterShieldAbility) {
			return 0.9f;
		}

		// If we are close to the water, also return a high probability
		if (parentEnemyScript.environmentProximityData != null && parentEnemyScript.environmentProximityData.requiredAbility == EAbilityType.EWaterShieldAbility) {
			return 0.7f;
		}

		// If player has an active lava shield and is close to the enemy, then activate the water shield
		if (canSeePlayer && playerScript.shieldInUse != null && playerScript.shieldInUse.abilityEnum == EAbilityType.ELavaShieldAbility && toPlayer.magnitude - parentBlob.transform.localScale.x - playerScript.size < parentBlob.transform.localScale.x) {
			return 0.8f;
		}

		return 0.0f;
	}
	
	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EWaterShieldAbility;
	}


	
}
