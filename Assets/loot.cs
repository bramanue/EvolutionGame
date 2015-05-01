using UnityEngine;
using System.Collections;

public class loot : MonoBehaviour {

	public ELootType lootType;

	// Defines whether this loot has been taken or not
	public bool eaten = false;

	public bool readyToEat = false;

	// Use this for initialization
	void Start () {
		eaten = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public virtual void acquire(player playerScript, int slot = 0) {

	}
}
