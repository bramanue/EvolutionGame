using UnityEngine;
using System.Collections;

public enum ELootType {
	ESizeLoot,
	EAbilityLoot,
}

public class lootManager : MonoBehaviour {

	public GameObject sizeLootPrefab;

	public GameObject abilityLootPrefab;

	public GameObject[] lootObjects = new GameObject[100];

	public loot[] lootScripts = new loot[100];

	private int index;

	private GameObject player;

	private player playerScript;

	private abilityManager abilityManager;


	// Use this for initialization
	void Start () {
		player = GameObject.Find ("Blob");
		playerScript = (player)player.GetComponent (typeof(player));
		abilityManager = (abilityManager)GameObject.Find ("AbilityManager").GetComponent(typeof(abilityManager));
		index = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 playerPos = player.transform.position;
		float radius = 3.0f*(player.transform.localScale.x + playerScript.viewingRange);
		for (int i = 0; i < lootObjects.Length; i++) {
			if(lootObjects[i] == null)
				continue;

			Vector3 position = lootObjects[i].transform.position;
			if((playerPos - position).magnitude < radius) 
			{
				if(((loot)lootObjects[i].GetComponent(typeof(loot))).eaten) {
					GameObject.Destroy(lootObjects[i]);
					continue;
				}
				lootObjects[i].SetActive(true);
			} 
			else
				lootObjects[i].SetActive(false);
		}
	}

	private void throwLoot(int index, Vector3 from, Vector3 to)
	{


	}

	public void throwSizeLoot(float size) 
	{
		GameObject newLootObject = createLootGameObject (ELootType.ESizeLoot);
		if (!newLootObject)
			return;
		
		// Destroy the old loot at that position
		if (lootObjects [index] != null)
			GameObject.Destroy (lootObjects [index]);

		sizeLoot loot = (sizeLoot)newLootObject.GetComponent (typeof(sizeLoot));
		loot.eaten = false;
		loot.size = size;
		lootObjects [index] = newLootObject;
		
		index++;
		index %= lootObjects.Length;
	}

	public void throwAbilityLoot(ability ability, int level, Vector3 from, Vector3 to) 
	{
		GameObject newLootObject = createLootGameObject (ELootType.EAbilityLoot);
		if (!newLootObject)
			return;
		
		// Destroy the old loot at that position
		if (lootObjects [index] != null)
			GameObject.Destroy (lootObjects [index]);
		
		abilityLoot loot = (abilityLoot)newLootObject.GetComponent (typeof(abilityLoot));
		loot.eaten = false;
		loot.abilityType = ability.abilityEnum;
		loot.abilityName = ability.name;
		loot.abilityClass = ability.abilitySuperClassEnum;
		loot.level = 1;
		lootObjects [index] = newLootObject;
		
		index++;
		index %= lootObjects.Length;
	}

	private GameObject createLootGameObject(ELootType lootType) {

		switch (lootType) {
		case (ELootType.ESizeLoot) :
			return (GameObject)GameObject.Instantiate(sizeLootPrefab);
			break;
		case (ELootType.EAbilityLoot) :
			return (GameObject)GameObject.Instantiate(sizeLootPrefab);
			break;
		default :
			Debug.Log ("Bad loot type");
			return null;
		};

	}
}
