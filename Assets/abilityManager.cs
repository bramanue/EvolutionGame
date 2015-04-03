using UnityEngine;
using System.Collections;

public enum EAbilityType {
	ERunAbility,
	ERamAbility,
	EBiteAbility
}

public class abilityManager : MonoBehaviour {

	public GameObject runAbility;

	public GameObject ramAbility;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void addAbilityToPlayer(GameObject parent, EAbilityType abilityType, int slot, int level)
	{
		GameObject abilityObject = null;
		// Get the correct prefab
		switch (abilityType) {
		case EAbilityType.ERunAbility :
			abilityObject = (GameObject)GameObject.Instantiate(runAbility);
			break;
		case EAbilityType.ERamAbility :
			abilityObject = (GameObject)GameObject.Instantiate(ramAbility);
			break;
		case EAbilityType.EBiteAbility :
			abilityObject = (GameObject)GameObject.Instantiate(runAbility);
			break;
		default :
			abilityObject = null;
			break;
		}

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
		GameObject abilityObject = null;
		switch (abilityType) {
		case EAbilityType.ERunAbility:
			abilityObject = (GameObject)GameObject.Instantiate(runAbility);
			break;
		case EAbilityType.ERamAbility:
			abilityObject = (GameObject)GameObject.Instantiate(ramAbility);
			break;
		case EAbilityType.EBiteAbility:
			abilityObject = (GameObject)GameObject.Instantiate(runAbility);
			break;
		default :
			abilityObject = null;
			break;
		}

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
