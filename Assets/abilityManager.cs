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
		case EAbilityType.ERunAbility :
			return (GameObject)GameObject.Instantiate(runAbility);
		case EAbilityType.ERamAbility :
			return (GameObject)GameObject.Instantiate(ramAbility);
		case EAbilityType.EBiteAbility :
			return (GameObject)GameObject.Instantiate(biteAbility);
		case EAbilityType.EViewAbility :
			return (GameObject)GameObject.Instantiate(viewAbility);
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
