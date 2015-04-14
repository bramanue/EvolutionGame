using UnityEngine;
using System.Collections;

public class electricityShieldAbility : ability {
	
	private float timer;
	
	private float maxTimeInThunderstorm = 30.0f;
	
	private float damage = 0.1f;
	
	private bool inUse;

	private bool deactivate;
	
	// Use this for initialization
	void Start () {
		// Get the game object which has this ram ability
		parentBlob = transform.parent.gameObject;
		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent(typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent(typeof(player));
		isPlayer = (bool)parentPlayerScript;
		
		cooldownTimer = 0.0f;

		maxTimeInThunderstorm = 30.0f + 30.0f * level;
		timer = maxTimeInThunderstorm;
		damage = 0.1f + level * 0.1f;

		abilitySuperClassEnum = EAbilityClass.EShieldAbility;
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = new Vector3 (0, 0, 0);
		transform.localScale = new Vector3 (1, 1, 1);
	}

	void LateUpdate() 
	{
		if (deactivate)
			inUse = false;

		if (inUse) {
			// TODO change visuals
			((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = new Color(0.5f,0.5f,1,0.5f);
			
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
			timer = Mathf.Min (maxTimeInThunderstorm, timer + Time.deltaTime);
			
			// Reset to default sprite if no other shield is active
			if(isPlayer) {
				if(parentPlayerScript.shieldInUse == null)
					((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = parentPlayerScript.defaultColor;
			} else {
				if(parentEnemyScript.shieldInUse == null)
					((SpriteRenderer)parentBlob.GetComponent(typeof(SpriteRenderer))).color = parentEnemyScript.defaultColor;
			}
		}
		// Delay the deactivation (i.e. inUse = false) by one frame due to update order. Collision triggers are called before the update and therefore inUse would always be false
		deactivate = true;
	}

	void OnTriggerEnter(Collider other)
	{
		if (!inUse) {
			Debug.Log ("Electricity shield collided but not activated");
			return;
		}

		// If collision with own blob, do nothing
		if (other.gameObject == parentBlob)
			return;
		
		// Check whether the teeth of the blob collided with another blob
		enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
		player playerScript = (player)other.gameObject.GetComponent (typeof(player));
		
		if (isPlayer && enemyScript) {
			// Enemy is hurt by player's electricity shield if enemy does not have a dust shield
		/*	if(enemyScript.hasAbility(EAbilityType.EDustShieldAbility) == -1 )
			{
				if(enemyScript.hasAbility(EAbilityType.EWaterShieldAbility) != -1)
					enemyScript.size -= 2.0f*damage; 	// Very effective against water shield
				else
					enemyScript.size -= damage;
				enemyScript.setAlertState();
			}*/

			// Enemy is hurt by player's electricity shield if enemy does not have an active dust or electricity shield
			if(enemyScript.shieldInUse == null || ( 
			   enemyScript.shieldInUse.getAbilityEnum() != EAbilityType.EDustShieldAbility &&
			   enemyScript.shieldInUse.getAbilityEnum() != EAbilityType.EElectricityShieldAbility ) )
			{  
				// Very effective against water shield
				if(enemyScript.shieldInUse != null && enemyScript.shieldInUse.getAbilityEnum() == EAbilityType.EWaterShieldAbility) {
					enemyScript.size -= 2.0f*damage; 	
				}else{
					enemyScript.size -= damage;
				}
				Debug.Log ("Enemy hurt by electricity shield: Damage = " + damage);
				enemyScript.setAlertState();
			}
		} else if (!isPlayer && playerScript) {
			// Player is hurt by enemy's electricity shield if enemy does not have a dust or electricity shield
		/*	if(playerScript.hasAbility(EAbilityType.EDustShieldAbility) == -1 )
			{
				if(playerScript.hasAbility(EAbilityType.EWaterShieldAbility) != -1)
					playerScript.size -= 2.0f*damage; 	// Very effective against water shield
				else
					playerScript.size -= damage;
			}*/

			// Player is hurt by enemy's electricity shield if enemy does not have a dust or electricity shield
			if(playerScript.shieldInUse == null || ( 
			   playerScript.shieldInUse.getAbilityEnum() != EAbilityType.EDustShieldAbility &&
			   playerScript.shieldInUse.getAbilityEnum() != EAbilityType.EElectricityShieldAbility ) )
			{
				if(playerScript.shieldInUse != null && playerScript.shieldInUse.getAbilityEnum() == EAbilityType.EWaterShieldAbility)
					playerScript.size -= 2.0f*damage; 	// Very effective against water shield
				else
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
		maxTimeInThunderstorm = 30.0f + 30.0f * level;
		return level - previousLevel;
	}
	
	public override bool useAbility() 
	{
		if (timer > 0) {
			deactivate = false;
			inUse = true;
			return true;
		} else {
			return false;
		}
	}
	
	public override EAbilityType getAbilityEnum()
	{
		return EAbilityType.EElectricityShieldAbility;
	}
	
}
