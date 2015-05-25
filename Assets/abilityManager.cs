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
//	EJumpAbility,
//	EBombAbility,
//	EShootAbility,
	// Shield abilities
	EIceShieldAbility,
	ELavaShieldAbility,
	EDustShieldAbility,
	EThornShieldAbility,
	EWaterShieldAbility,
//	EGlowingShieldAbility,
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

	private List<EAbilityType> availableAbilities = new List<EAbilityType>();

	private List<EAbilityType> availableActiveAbilities = new List<EAbilityType>();

	private List<EAbilityType> availablePassiveAbilities = new List<EAbilityType>();

	public List<EAbilityType> availableShieldAbilities = new List<EAbilityType>();

	private int nofAvailableActiveAbilities;

	private int nofAvailablePassiveAbilities;

	private int nofAvailableShieldAbilities;

	private int nofAvailableAbilities;


	// Use this for initialization
	void Start () {
		abilityMaxScores.Add (EAbilityType.ERunAbility, 1000);
		abilityMaxScores.Add (EAbilityType.EViewAbility, 1000);
		abilityMaxScores.Add (EAbilityType.ERamAbility, 2000);
		abilityMaxScores.Add (EAbilityType.EBiteAbility, 2000);
	//	abilityMaxScores.Add (EAbilityType.EJumpAbility, 100);
		abilityMaxScores.Add (EAbilityType.EIceShieldAbility, 2000);
		abilityMaxScores.Add (EAbilityType.ELavaShieldAbility, 2000);
		abilityMaxScores.Add (EAbilityType.EDustShieldAbility, 3000);
		abilityMaxScores.Add (EAbilityType.EThornShieldAbility, 2000);
		abilityMaxScores.Add (EAbilityType.EWaterShieldAbility, 1000);
	//	abilityMaxScores.Add (EAbilityType.EGlowingShieldAbility, 1000);
		abilityMaxScores.Add (EAbilityType.EElectricityShieldAbility, 300);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public ability getAbility(EAbilityType abilityType)
	{
		// Get the correct prefab
		switch (abilityType) {
		// Passive abilities
		case EAbilityType.ERunAbility:
			return new runAbility ();
		case EAbilityType.EViewAbility:
			return new viewAbility ();
		// Active abilities
		case EAbilityType.ERamAbility:
			return new ramAbility ();
		case EAbilityType.EBiteAbility:
			return new biteAbility ();
		// Shield abilities
		case EAbilityType.EIceShieldAbility:
			return new iceShieldAbility ();
		case EAbilityType.ELavaShieldAbility:
			return new lavaShieldAbility ();
		case EAbilityType.EDustShieldAbility:
			return new dustShieldAbility ();
		case EAbilityType.EThornShieldAbility:
			return new thornShieldAbility ();
		case EAbilityType.EWaterShieldAbility:
			return new waterShieldAbility ();
		//	case EAbilityType.EGlowingShieldAbility :
		//		return (new glowingShieldAbility();
		case EAbilityType.EElectricityShieldAbility:
			return new electricityShieldAbility ();
		default :
			return null;
		}
	}

	public GameObject getPrefab(EAbilityType abilityType)
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
	//	case EAbilityType.EGlowingShieldAbility :
	//		return (GameObject)GameObject.Instantiate(glowingShieldAbility);
		case EAbilityType.EElectricityShieldAbility :
			return (GameObject)GameObject.Instantiate(electricityShieldAbility);
		default :
			return null;
		}
	}

	public void addAbilityToPlayer(GameObject parent, EAbilityType abilityType, int slot, int level)
	{
		if (abilityType == EAbilityType.EEmptyAbility)
			return;

		// Get the corresponding prefab
		GameObject abilityObject = getPrefab(abilityType);

		if (!abilityObject) {
			Debug.Log ("Could not resolve ability enum : " + abilityType);
			return;
		}

		// Passive abilities have a specific predefined index
		if (EAbilityType.ERunAbility == abilityType)
			slot = 6;
		else if (EAbilityType.EViewAbility == abilityType)
			slot = 7;

		ability abilityScript = (ability)abilityObject.GetComponent(typeof(ability));
		player playerScript = (player)parent.GetComponent(typeof(player));
		int existingSlot = playerScript.hasAbility (abilityType);

		if (existingSlot != -1) {
			playerScript.improveAbility(existingSlot, 1);
		} else {
			abilityObject.transform.parent = parent.transform;
			playerScript.addAbility (abilityObject, slot);
			abilityScript.level = level;
		}
	}

	public float addAbilityToEnemy(GameObject parent, EAbilityType abilityType, int slot, int level)
	{
		if (abilityType == EAbilityType.EEmptyAbility)
			return 0;

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

		// Define score for defeating enemy with this ability
		float score;
		abilityMaxScores.TryGetValue (abilityType, out score);
		return score*(float)(level/abilityScript.maxLevel);
	}

	public EAbilityType getRandomPassiveAbility()
	{
		if (nofAvailablePassiveAbilities == 0)
			return EAbilityType.EEmptyAbility;
		
		int index = Random.Range (0, nofAvailablePassiveAbilities);
		return availablePassiveAbilities [index];
	}

	public EAbilityType getRandomActiveAbility()
	{
		if (nofAvailableActiveAbilities == 0)
			return EAbilityType.EEmptyAbility;
		
		int index = Random.Range (0, nofAvailableActiveAbilities);
		return availableActiveAbilities [index];
	}

	public EAbilityType getRandomShieldAbility()
	{
		if (nofAvailableShieldAbilities == 0)
			return EAbilityType.EEmptyAbility;
		
		int index = Random.Range (0, nofAvailableShieldAbilities);
		return availableShieldAbilities [index];
	}

	public EAbilityType getRandomAbilityType() 
	{
		if (nofAvailableAbilities == 0)
			return EAbilityType.EEmptyAbility;

		int index = Random.Range (0, nofAvailableAbilities);
		return availableAbilities [index];
	}

	public void addAbilityToTheGame(EAbilityType abilityType, EAbilityClass abilityClass) 
	{
		if(!availableAbilities.Contains(abilityType)) 
		{
			nofAvailableAbilities++;
			availableAbilities.Add (abilityType);

			switch (abilityClass) {
			case EAbilityClass.EActiveAbility :
				availableActiveAbilities.Add (abilityType);
				nofAvailableActiveAbilities++;
				break;
			case EAbilityClass.EPassiveAbility :
				availablePassiveAbilities.Add (abilityType);
				nofAvailablePassiveAbilities++;
				break;
			case EAbilityClass.EShieldAbility :
				availableShieldAbilities.Add (abilityType);
				nofAvailableShieldAbilities++;
				break;
			};
		}
	}

	public void addAllAbilitiesToTheGame() 
	{
		addAbilityToTheGame(EAbilityType.ERunAbility, EAbilityClass.EPassiveAbility);
		addAbilityToTheGame(EAbilityType.EViewAbility, EAbilityClass.EPassiveAbility);
		addAbilityToTheGame(EAbilityType.ERamAbility, EAbilityClass.EActiveAbility);
		addAbilityToTheGame(EAbilityType.EBiteAbility, EAbilityClass.EActiveAbility);
		addAbilityToTheGame(EAbilityType.EIceShieldAbility, EAbilityClass.EShieldAbility);
		addAbilityToTheGame(EAbilityType.ELavaShieldAbility, EAbilityClass.EShieldAbility);
		addAbilityToTheGame(EAbilityType.EDustShieldAbility, EAbilityClass.EShieldAbility);
		addAbilityToTheGame(EAbilityType.EThornShieldAbility, EAbilityClass.EShieldAbility);
		addAbilityToTheGame(EAbilityType.EWaterShieldAbility, EAbilityClass.EShieldAbility);
		addAbilityToTheGame(EAbilityType.EElectricityShieldAbility, EAbilityClass.EShieldAbility);
		//	addAbilityToTheGame(EAbilityType.EGlowingShieldAbility, EAbilityClass.EShieldAbility);
	}

	public void addRandomAbilityToTheGame() {
		int index = Random.Range (0, 10);

		switch (index) {
			// Passive abilities
		case 0 :
			addAbilityToTheGame(EAbilityType.ERunAbility, EAbilityClass.EPassiveAbility);
			break;
		case 1 :
			addAbilityToTheGame(EAbilityType.EViewAbility, EAbilityClass.EPassiveAbility);
			break;
			// Active abilities
		case 2 :
			addAbilityToTheGame(EAbilityType.ERamAbility, EAbilityClass.EActiveAbility);
			break;
		case 3 :
			addAbilityToTheGame(EAbilityType.EBiteAbility, EAbilityClass.EActiveAbility);
			break;
			// Shield abilities
		case 4 :
			addAbilityToTheGame(EAbilityType.EIceShieldAbility, EAbilityClass.EShieldAbility);
			break;
		case 5 :
			addAbilityToTheGame(EAbilityType.ELavaShieldAbility, EAbilityClass.EShieldAbility);
			break;
		case 6 :
			addAbilityToTheGame(EAbilityType.EDustShieldAbility, EAbilityClass.EShieldAbility);
			break;
		case 7 :
			addAbilityToTheGame(EAbilityType.EThornShieldAbility, EAbilityClass.EShieldAbility);
			break;
		case 8 :
			addAbilityToTheGame(EAbilityType.EWaterShieldAbility, EAbilityClass.EShieldAbility);
			break;
		case 9 :
			addAbilityToTheGame(EAbilityType.EElectricityShieldAbility, EAbilityClass.EShieldAbility);
			break;
	//	case 10 :
		//	addAbilityToTheGame(EAbilityType.EGlowingShieldAbility, EAbilityClass.EShieldAbility);
		//	break;
		default :
			break;
		}
	}

	public void addRandomShieldAbilityToTheGame() {
		int index = Random.Range (4, 10);
		
		switch (index) {
			// Passive abilities
		case 4 :
			addAbilityToTheGame(EAbilityType.EIceShieldAbility, EAbilityClass.EShieldAbility);
			break;
		case 5 :
			addAbilityToTheGame(EAbilityType.ELavaShieldAbility, EAbilityClass.EShieldAbility);
			break;
		case 6 :
			addAbilityToTheGame(EAbilityType.EDustShieldAbility, EAbilityClass.EShieldAbility);
			break;
		case 7 :
			addAbilityToTheGame(EAbilityType.EThornShieldAbility, EAbilityClass.EShieldAbility);
			break;
		case 8 :
			addAbilityToTheGame(EAbilityType.EWaterShieldAbility, EAbilityClass.EShieldAbility);
			break;
		case 9 :
			addAbilityToTheGame(EAbilityType.EElectricityShieldAbility, EAbilityClass.EShieldAbility);
			break;
		//	case 10 :
			//	addAbilityToTheGame(EAbilityType.EGlowingShieldAbility, EAbilityClass.EShieldAbility);
			//	break;
		default :
			break;
		}
	}

	public void addRandomActiveAbilityToTheGame() {
		int index = Random.Range (2, 4);
		
		switch (index) {
			// Passive abilities
		case 2 :
			addAbilityToTheGame(EAbilityType.ERamAbility, EAbilityClass.EActiveAbility);
			break;
		case 3 :
			addAbilityToTheGame(EAbilityType.EBiteAbility, EAbilityClass.EActiveAbility);
			break;
		default :
			break;
		}
	}

	public void addRandomPassiveAbilityToTheGame() {
		int index = Random.Range (0, 2);
		
		switch (index) {
			// Passive abilities
		case 0 :
			addAbilityToTheGame(EAbilityType.ERunAbility, EAbilityClass.EPassiveAbility);
			break;
		case 1 :
			addAbilityToTheGame(EAbilityType.EViewAbility, EAbilityClass.EPassiveAbility);
			break;
		default :
			break;
		}
	}

	public void reset() 
	{
		availableAbilities.Clear ();
		availableShieldAbilities.Clear ();
		availableActiveAbilities.Clear ();
		availablePassiveAbilities.Clear ();

		nofAvailableAbilities = 0;
		nofAvailableShieldAbilities = 0;
		nofAvailableActiveAbilities = 0;
		nofAvailablePassiveAbilities = 0;
	}
}
