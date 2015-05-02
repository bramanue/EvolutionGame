﻿using UnityEngine;
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

	private IEnumerator cleanUpIEnumerator;


	// Use this for initialization
	void Start () {
		player = GameObject.Find ("Blob");
		playerScript = (player)player.GetComponent (typeof(player));
		abilityManager = (abilityManager)GameObject.Find ("AbilityManager").GetComponent(typeof(abilityManager));
		index = 0;
		cleanUpIEnumerator = cleanUpRoutine ();
		StartCoroutine (cleanUpIEnumerator);
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

	public void destroyLootManager()
	{
		for (int i = 0; i < lootObjects.Length; i++) {
			GameObject.Destroy (lootObjects [i]);
		}
		StopCoroutine(cleanUpIEnumerator);
	}

	IEnumerator cleanUpRoutine()
	{
		while (true) 
		{
			Vector3 playerPos = player.transform.position;
			float radius = 3.0f * (player.transform.localScale.x + playerScript.currentViewingRange);
			for (int i = 0; i < lootObjects.Length; i++) {
				if (lootObjects [i] == null)
					continue;
			
				Vector3 position = lootObjects [i].transform.position;
				if ((playerPos - position).magnitude < radius) {
					if (((loot)lootObjects [i].GetComponent (typeof(loot))).eaten) {
						GameObject.Destroy (lootObjects [i]);
						continue;
					}
					lootObjects [i].SetActive (true);
				} else
					lootObjects [i].SetActive (false);
			}
			// Perform clean up only 10 times per second
			yield return new WaitForSeconds(0.1f);
		}
	}


	IEnumerator throwLootCoroutine(int index, Vector3 from, Vector3 to, float flightTime) 
	{
		// Throw loot in a parabel slope
		Vector3 direction = to - from;
		float xPerSecond = direction.x/flightTime;
		float yPerSecond = direction.y/flightTime;
		float halfDistance = direction.magnitude * 0.5f;
		float halfDistanceSquared = halfDistance * halfDistance;

		// Rotation axis times number of rotations
		Vector3 randomRotationAxis = Random.insideUnitSphere;
		// Total degrees of rotation around this axis
		float totalDegreesOfRotationPerSecond = 360.0f*Random.Range (1.0f,6.0f)/flightTime;

		// TODO Change time according to distance
		for (float time = 0.0f; time < flightTime; time += Time.deltaTime) 
		{
			float x = time*xPerSecond;
			float y = time*yPerSecond;
			float x_parabelSpace = Mathf.Sqrt (x*x + y*y);
			float z = Mathf.Max (-Mathf.Pow(x_parabelSpace - halfDistance,2.0f) + halfDistanceSquared,0)*5.0f;

			// Change position and rotation
			lootObjects[index].transform.position = from + new Vector3(x,y,z);
			lootObjects[index].transform.Rotate(randomRotationAxis,totalDegreesOfRotationPerSecond*Time.deltaTime*Mathf.Sqrt(flightTime-time));
			Debug.Log ("time = " + time + " Distance = " + 2.0f*halfDistance + " x2_parabelSpace = " + x_parabelSpace + "vector = " + new Vector3(x,y,z) + " Rotation = " + lootObjects[index].transform.rotation);
			yield return null;
		}
		Vector3 position = lootObjects [index].transform.position;
		Debug.Log ("z at rest = " + position.z);
		position.z = 0;
		lootObjects [index].transform.position = position;
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
		// Set size loot parameters
		loot.eaten = false;
		loot.size = size;

		// Set position of object
		newLootObject.transform.position = from;
		newLootObject.transform.localScale = 20.0f*new Vector3 (size, size, size);
		// Add object to the list
		lootObjects [index] = newLootObject;
		lootObjects [index].SetActive (true);
		// throw it
		StartCoroutine (throwLootCoroutine (index, from, to, 1.0f));
		
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

		// Set the loot parameters
		loot.eaten = false;
		loot.level = level;
		loot.abilityName = ability.name;
		loot.abilityType = ability.abilityEnum;
		loot.abilityClass = ability.abilitySuperClassEnum;

		// Set the appearance of the loot
		newLootObject.transform.position = from;
		float size = Mathf.Min(0.5f,0.5f*player.transform.localScale.x);
		newLootObject.transform.localScale = 2.0f*new Vector3 (size, size, size);

		lootObjects [index] = newLootObject;
		lootObjects [index].SetActive (true);

		StartCoroutine (throwLootCoroutine (index, from, to, 1.0f));
		
		index++;
		index %= lootObjects.Length;
	}

	private GameObject createLootGameObject(ELootType lootType) {

		switch (lootType) {
		case (ELootType.ESizeLoot) :
			return (GameObject)GameObject.Instantiate(sizeLootPrefab);
			break;
		case (ELootType.EAbilityLoot) :
			return (GameObject)GameObject.Instantiate(abilityLootPrefab);
			break;
		default :
			Debug.Log ("Bad loot type");
			return null;
		};

	}
}