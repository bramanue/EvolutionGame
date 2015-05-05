using UnityEngine;
using System.Collections;

public class hazardousEnvironment : MonoBehaviour {

	public EEnvironmentClass environmentClass;

	public EAbilityType requiredAbility;

	// By how much will creatures, not having the ability to resist this environment, be slowed down
	// 1 = cannot enter
	// 0 = no slow down
	public float slowDownFactor;

	// The damage per second taken, if blob has not required ability
	public float damagePerSecond;

	// Defines the minimum scaling relative to the texture quad it is initialized on 
	public float minScaleFactor;
	// Defines the maximum scaling relative to the texture quad it is initialized on 
	public float maxScaleFactor;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


}
