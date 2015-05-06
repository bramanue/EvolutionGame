using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EAbilityType {
	// Default ability
	EEmptyAbility,
	// Passive abilities
	ERunAbility,
	EViewAbility,
	// Active abilities
	ERamAbility,
	EBiteAbility,
	EJumpAbility,
	// Shield abilities
	EIceShieldAbility,
	ELavaShieldAbility,
	EDustShieldAbility,
	EThornShieldAbility,
	EWaterShieldAbility,
	EGlowingShieldAbility,
	EElectricityShieldAbility
}

public enum EAbilityClass {
	EEmptyAbility,
	EPassiveAbility,
	EActiveAbility,
	EShieldAbility
}

public class abilityManager : MonoBehaviour {

	// PASSIVE ABILITY PREFABS
	public GameObject runAbility;

	public GameObject ramAbility;

	// ACTIVE ABILITY PREFABS
	public GameObject biteAbility;

	public GameObject viewAbility;

	// SHIELD ABILITY PREFABS
	public GameObject iceShieldAbility;

	public GameObject lavaShieldAbility;

	public GameObject dustShieldAbility;

	public GameObject thornShieldAbility;

	public GameObject waterShieldAbility;

	public GameObject glowingShieldAbility;

	public GameObject electricityShieldAbility;

	public Dictionary<EAbilityType, float> abilityMaxScores = new Dictionary<EAbilityType, float>();


	// Use this for initialization
	void Start () {
		abilityMaxScores.Add (EAbilityType.ERunAbility, 1000);
		abilityMaxScores.Add (EAbilityType.EViewAbility, 1000);
		abilityMaxScores.Add (EAbilityType.ERamAbility, 2000);
		abilityMaxScores.Add (EAbilityType.EBiteAbility, 2000);
		abilityMaxScores.Add (EAbilityType.EJumpAbility, 100);
		abilityMaxScores.Add (EAbilityType.EIceShieldAbility, 2000);
		abilityMaxScores.Add (EAbilityType.ELavaShieldAbility, 2000);
		abilityMaxScores.Add (EAbilityType.EDustShieldAbility, 3000);
		abilityMaxScores.Add (EAbilityType.EThornShieldAbility, 2000);
		abilityMaxScores.Add (EAbilityType.EWaterShieldAbility, 1000);
		abilityMaxScores.Add (EAbilityType.EGlowingShieldAbility, 1000);
		abilityMaxScores.Add (EAbilityType.EElectricityShieldAbility, 300);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private GameObject getPrefab(EAbilityType abilityType)
	{
		// Get the correct prefab
		switch (abilityType) {
		// Passive abilities
		case EAbilityType.ERunAbility :
			return (GameObject)GameObject.Instantiate(runAbility);
		case EAbilityType.EViewAbility :
			return (GameObject)GameObject.Instantiate(viewAbility);
		// Active abilities
		case EAbilityType.ERamAbility :
			return (GameObject)GameObject.Instantiate(ramAbility);
		case EAbilityType.EBiteAbility :
			return (GameObject)GameObject.Instantiate(biteAbility);
		// Shield abilities
		case EAbilityType.EIceShieldAbility :
			return (GameObject)GameObject.Instantiate(iceShieldAbility);
		case EAbilityType.ELavaShieldAbility :
			return (GameObject)GameObject.Instantiate(lavaShieldAbility);
		case EAbilityType.EDustShieldAbility :
			return (GameObject)GameObject.Instantiate(dustShieldAbility);
		case EAbilityType.EThornShieldAbility :
			return (GameObject)GameObject.Instantiate(thornShieldAbility);
		case EAbilityType.EWaterShieldAbility :
			return (GameObject)GameObject.Instantiate(waterShieldAbility);
		case EAbilityType.EGlowingShieldAbility :
			return (GameObject)GameObject.Instantiate(glowingShieldAbility);
		case EAbilityType.EElectricityShieldAbility :
			return (GameObject)GameObject.Instantiate(electricityShieldAbility);
		default :
			return null;
		}
	}

	public void addAbilityToPlayer(GameObject parent, EAbilityType abilityType, int slot, int level)
	{
		// Get the corresponding prefab
		GameObject abilityObject = getPrefab(abilityType);

		if (!abilityObject) {
			Debug.Log ("Could not resolve ability enum : " + abilityType);
			return;
		}

		player playerScript = (player)parent.GetComponent(typeof(player));
		int existingSlot = playerScript.hasAbility (abilityType);
		if (existingSlot != -1) {
			playerScript.addAbility (abilityObject, existingSlot);
		} else {
			playerScript.addAbility (abilityObject, slot);
		}

		abilityObject.transform.parent = parent.transform;
		ability abilityScript = (ability)abilityObject.GetComponent(typeof(ability));
		abilityScript.level = level;
	}

	public float addAbilityToEnemy(GameObject parent, EAbilityType abilityType, int slot, int level)
	{
		enemy enemyScript = (enemy)parent.GetComponent(typeof(enemy));
		// Check whether enemy already has this ability
		int existingSlot = enemyScript.hasAbility (abilityType);
		if (existingSlot != -1) {
			return 0;
		}

		// Get the corresponding prefab
		GameObject abilityObject = getPrefab(abilityType);
		
		if (!abilityObject) {
			Debug.Log ("Could not resolve ability enum : " + abilityType);
			return 0;
		}

		abilityObject.transform.parent = parent.transform;
		ability abilityScript = (ability)abilityObject.GetComponent(typeof(ability));
		abilityScript.level = level;

		// Passive abilities have a specific predefined index
		if (EAbilityType.ERunAbility == abilityType)
			slot = 6;
		else if (EAbilityType.EViewAbility == abilityType)
			slot = 7;

		enemyScript.addAbility(abilityObject, slot);
		float score;
		abilityMaxScores.TryGetValue (abilityType, out score);
		return score*(float)(level/abilityScript.maxLevel);
	}

	public EAbilityType getRandomPassiveAbility()
	{
		int index = Random.Range (0, 2);
		Debug.Log (index);
		switch (index) {
		// Passive abilities
		case 0:
			return EAbilityType.ERunAbility;
		case 1:
			return EAbilityType.EViewAbility;
		default :
			return EAbilityType.EEmptyAbility;
		}
	}

	public EAbilityType getRandomActiveAbility()
	{
		int index = Random.Range (0, 2);
		
		switch (index) {
		case 0:
			return EAbilityType.ERamAbility;
		case 1:
			return EAbilityType.EBiteAbility;
		default :
			return EAbilityType.EEmptyAbility;
		}
	}

	public EAbilityType getRandomShieldAbility()
	{
		int index = Random.Range (0, 7);
		
		switch (index) {
		case 0 :
			return EAbilityType.EIceShieldAbility;
		case 1 :
			return EAbilityType.ELavaShieldAbility;
		case 2 :
			return EAbilityType.EDustShieldAbility;
		case 3 :
			return EAbilityType.EThornShieldAbility;
		case 4 :
			return EAbilityType.EWaterShieldAbility;
		case 5 :
			return EAbilityType.EGlowingShieldAbility;
		case 6 :
			return EAbilityType.EElectricityShieldAbility;
		default :
			return EAbilityType.EEmptyAbility;
		}
	}

	public EAbilityType getRandomAbilityType() 
	{
		int index = Random.Range (0, 11);
		
		switch (index) {
			// Passive abilities
		case 0 :
			return EAbilityType.ERunAbility;
		case 1 :
			return EAbilityType.EViewAbility;
			// Active abilities
		case 2 :
			return EAbilityType.ERamAbility;
		case 3 :
			return EAbilityType.EBiteAbility;
			// Shield abilities
		case 4 :
			return EAbilityType.EIceShieldAbility;
		case 5 :
			return EAbilityType.ELavaShieldAbility;
		case 6 :
			return EAbilityType.EDustShieldAbility;
		case 7 :
			return EAbilityType.EThornShieldAbility;
		case 8 :
			return EAbilityType.EWaterShieldAbility;
		case 9 :
			return EAbilityType.EGlowingShieldAbility;
		case 10 :
			return EAbilityType.EElectricityShieldAbility;
		default :
			return EAbilityType.EEmptyAbility;
		}
	}
}
