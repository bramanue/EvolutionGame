using UnityEngine;
using System.Collections;

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



	// Use this for initialization
	void Start () {

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

	public void addAbilityToEnemy(GameObject parent, EAbilityType abilityType, int slot, int level)
	{
		// Get the corresponding prefab
		GameObject abilityObject = getPrefab(abilityType);

		if (!abilityObject) {
			Debug.Log ("Could not resolve ability enum : " + abilityType);
			return;
		}

		abilityObject.transform.parent = parent.transform;
		ability abilityScript = (ability)ramAbility.GetComponent(typeof(ability));
		abilityScript.level = level;

		enemy enemyScript = (enemy)parent.GetComponent(typeof(enemy));
		// Passive abilities have a specific predefined index
		if (EAbilityType.ERunAbility == abilityType)
			slot = 6;
		else if (EAbilityType.EViewAbility == abilityType)
			slot = 7;

		enemyScript.addAbility(abilityObject, slot);
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
