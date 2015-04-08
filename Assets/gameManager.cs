using UnityEngine;
using System.Collections;

public class gameManager : MonoBehaviour {
	
	private GameObject player;
	
	private player playerScript;
	
	private GameObject abilityMngr;

	private abilityManager abilityManagerScript;
	
	// Use this for initialization
	void Start () 
	{
		player = GameObject.Find("Blob");
		playerScript = (player)player.GetComponent(typeof(player));
		abilityMngr = GameObject.Find("AbilityManager");
		abilityManagerScript = (abilityManager)abilityMngr.GetComponent (typeof(abilityManager));
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.ERunAbility,4,4);
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.ERamAbility,0,4);
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.EBiteAbility,1,1);
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.EThornShieldAbility,2,1);
		abilityManagerScript.addAbilityToPlayer(player,EAbilityType.EViewAbility,5,5);
	}
	
	// Update is called once per frame
	void Update () {
		if (playerScript.size <= 0)
			Debug.Log ("Game Over");

		if (Input.GetButtonDown ("Cancel")) {
			Debug.Log ("Start button pressed");
		}
	}
}
