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
		float radius = 3.0f*(player.transform.localScale.x + playerScript.currentViewingRange);
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


	IEnumerator throwLootCoroutine(int index, Vector3 from, Vector3 to) 
	{
		// Thow loot in a parabel slope
		Vector3 direction = to - from;
		float xPerSecond = direction.x;
		float yPerSecond = direction.y;
		float halfDistance = direction.magnitude * 0.5f;
		float halfDistanceSquared = halfDistance * halfDistance;
		float startPosSquared = from.x * from.x + from.y * from.y;
		for (float time = 0.0f; time < 1.0f; time += Time.deltaTime) 
		{
			float x = time*xPerSecond;
			float y = time*yPerSecond;
			float x_parabelSpace = Mathf.Sqrt (x*x + y*y);

			float z = Mathf.Max (-Mathf.Pow(x_parabelSpace - halfDistance,2.0f) + halfDistanceSquared,0);
			if(lootObjects[index] == null)
				break;
			lootObjects[index].transform.position = from + new Vector3(x,y,z);
			Debug.Log ("time = " + time + " Distance = " + 2.0f*halfDistance + " x2_parabelSpace = " + x_parabelSpace + "vector = " + new Vector3(x,y,z));
			yield return null;
		}
		((loot)lootObjects [index].GetComponent (typeof(loot))).readyToEat = true;
	}


	// Instantiates and places a loot that, will let the player's blob grow on acquiration
	public void throwSizeLoot(float size, Vector3 from, Vector3 to) 
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
		newLootObject.transform.position = from;
		newLootObject.transform.localScale = 2.0f*new Vector3 (size, size, size);
		lootObjects [index] = newLootObject;

		StartCoroutine (throwLootCoroutine (index, from, to));
		
		index++;
		index %= lootObjects.Length;
	}

	// Instantiates and places a loot that, will let the player's ability improve or let it learn a new ability if acquired
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
		newLootObject.transform.position = from;
		float size = Mathf.Min(0.5f,0.25f*player.transform.localScale.x);
		newLootObject.transform.localScale = 2.0f*new Vector3 (size, size, size);
		loot.level = 1;
		lootObjects [index] = newLootObject;

		StartCoroutine (throwLootCoroutine (index, from, to));
		
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
