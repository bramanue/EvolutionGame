using UnityEngine;
using System.Collections;

public class abilityLoot : loot {

	public EAbilityType abilityType;

	public EAbilityClass abilityClass;

	public string abilityName;

	public int level;

	// Use this for initialization
	void Start () {
		lootType = ELootType.EAbilityLoot;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void acquire(player playerScript, int slot = 0) 
	{
		int existingSlot = playerScript.hasAbility (abilityType);
		if (existingSlot == -1) 
		{
			// Player has not this ability yet
			abilityManager abilityManager = (abilityManager)(GameObject.Find ("AbilityManager").GetComponent(typeof(abilityManager)));
			abilityManager.addAbilityToPlayer(playerScript.gameObject,abilityType, slot, level);
		} 
		else 
		{
			playerScript.improveAbility(abilityType,level);
		}
	}
}
