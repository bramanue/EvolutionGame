using UnityEngine;
using System.Collections;

public class sizeLoot : loot {

	public float size;

	// Use this for initialization
	void Start () {
		lootType = ELootType.ESizeLoot;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void acquire(player playerScript, int slot = 0) {
		if(!eaten)
			playerScript.size += size;
		eaten = true;
	}
}
