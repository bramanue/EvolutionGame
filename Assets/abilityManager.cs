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

public class abilityManager : MonoBehaviour {

	public GameObject runAbility;

	public GameObject ramAbility;

	public GameObject biteAbility;

	public GameObject viewAbility;

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

		abilityObject.transform.parent = parent.transform;
		ability abilityScript = (ability)abilityObject.GetComponent(typeof(ability));
		abilityScript.level = level;

		player playerScript = (player)parent.GetComponent(typeof(player));
		playerScript.addAbility(abilityObject, slot);
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
		enemyScript.addAbility(abilityObject, slot);
	}
}
