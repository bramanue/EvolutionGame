using UnityEngine;
using System.Collections;

public class mouth : MonoBehaviour {

	public GameObject parentBlob;
	
	public enemy parentEnemyScript;
	
	public player parentPlayerScript;
	
	public bool isPlayer;

	private GameObject player;

	private player playerScript;


	// Use this for initialization
	void Start () {
		parentBlob = transform.parent.gameObject;

		// Get the script
		parentEnemyScript = (enemy)parentBlob.GetComponent(typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent(typeof(player));
		// Check whether it is a player or AI blob
		isPlayer = (bool)parentPlayerScript;

		player = GameObject.Find ("Blob");
		playerScript = (player)player.GetComponent (typeof(player));
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = new Vector3 (0, 0.8f, 0);
		transform.localScale = new Vector3 (1, 1, 1);
	}

	void OnTriggerEnter(Collider other)
	{
		if (isPlayer) {
			// Check whether the player collided with an enemy or with something else
			enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
			if (enemyScript != null) {
				// If player is bigger than enemy, then eat it
				if (other.gameObject.transform.localScale.x < parentBlob.transform.localScale.x) {
					// If enemy has not been eaten yet, eat him
					if (enemyScript.size > 0) {
						// Define by how much the player's blob grows
						float growFactor = other.gameObject.transform.localScale.x / parentBlob.transform.localScale.x;
						// Set scaling of the blob (transform will be changed during next Update())
						parentPlayerScript.size += 0.1f*growFactor*growFactor;
						// Kill enemy, will be respawned by the emeny manager
						enemyScript.eat();
					}
				} 
				return;
			}
			loot loot = (loot)other.gameObject.GetComponent(typeof(loot));
			if(loot != null && loot.readyToEat) {
				ELootType lootType = loot.lootType;
				if(lootType == ELootType.ESizeLoot)
					loot.acquire(playerScript);
				else
				{
					abilityLoot abilityLoot = (abilityLoot)loot;
					EAbilityClass abilityClass = abilityLoot.abilityClass;
					if(abilityClass == EAbilityClass.EPassiveAbility)
						// Passive abilities are no problem to acquire
						abilityLoot.acquire(parentPlayerScript);
					else
					{
						// Otherwise check if player already has this ability -> if so, then it is no problem to acquire
						EAbilityType abilityType = abilityLoot.abilityType;
						if(parentPlayerScript.hasAbility(abilityType) != -1)
							abilityLoot.acquire (parentPlayerScript);
						else
						{
							// Show the player, that there is a new ability he could acquire
							parentPlayerScript.nearbyAbilityLoot = abilityLoot;
							Debug.Log ("Nearby ability loot");
						}
					}
				}
			}

		} else {

			if (other.gameObject == player) { 
				if(parentEnemyScript.isHuntingPlayer) {
					playerScript.inflictDamage(0.05f*parentBlob.transform.localScale.x/other.gameObject.transform.localScale.x);
				}
				return;
			}

			// If the enemy is hunting the player and collides with a different enemy, then the smaller enemy
			// gets eaten
			if (parentEnemyScript.isHuntingPlayer) 
			{
				enemy enemyScript = (enemy)other.gameObject.GetComponent(typeof(enemy));
				if(enemyScript != null && parentBlob.transform.localScale.x > other.gameObject.transform.localScale.x) {
					// Define by how much the enemy blob grows
					float growFactor = enemyScript.size / parentEnemyScript.size;
					// Set scaling of the blob
					parentEnemyScript.size += 0.1f*growFactor*growFactor;
					// Kill enemy
					enemyScript.size = 0;
				}
			}
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (isPlayer) {
			// Check whether the player collided with an enemy or with something else
			enemy enemyScript = (enemy)other.gameObject.GetComponent (typeof(enemy));
			if (enemyScript != null) {
				// If player is bigger than enemy, then eat it
				if (other.gameObject.transform.localScale.x < parentBlob.transform.localScale.x) {
					// If enemy has not been eaten yet, eat him
					if (enemyScript.size > 0) {
						// Define by how much the player's blob grows
						float growFactor = other.gameObject.transform.localScale.x / parentBlob.transform.localScale.x;
						// Set scaling of the blob (transform will be changed during next Update())
						parentPlayerScript.size += 0.1f*growFactor*growFactor;
						// Kill enemy, will be respawned by the emeny manager
						enemyScript.eat();
					}
				} 
			}

			loot loot = (loot)other.gameObject.GetComponent(typeof(loot));
			if(loot != null && loot.readyToEat) 
			{
				ELootType lootType = loot.lootType;
				if(lootType == ELootType.ESizeLoot)
					loot.acquire(playerScript);
				else
				{
					abilityLoot abilityLoot = (abilityLoot)loot;
					EAbilityClass abilityClass = abilityLoot.abilityClass;
					if(abilityClass == EAbilityClass.EPassiveAbility)
						// Passive abilities are no problem to acquire
						abilityLoot.acquire(parentPlayerScript);
					else
					{
						// Otherwise check if player already has this ability -> if so, then it is no problem to acquire
						EAbilityType abilityType = abilityLoot.abilityType;
						if(parentPlayerScript.hasAbility(abilityType) != -1)
							abilityLoot.acquire (parentPlayerScript);
						else
						{
							// Show the player, that there is a new ability he could acquire
							parentPlayerScript.nearbyAbilityLoot = abilityLoot;
						}
					}
				}
			}
		}
		if (other.gameObject == player) { 
			if (parentEnemyScript.isHuntingPlayer) {
				playerScript.inflictDamage(0.02f*parentBlob.transform.localScale.x/other.gameObject.transform.localScale.x);
			}
			return;
		} 
	}
}
