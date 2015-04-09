using UnityEngine;
using System.Collections;

public class proximitySensor : MonoBehaviour {

	GameObject parentBlob;

	enemy parentEnemyScript;

	player parentPlayerScript;

	// Use this for initialization
	void Start () {
		parentBlob = transform.parent.gameObject;
		parentEnemyScript = (enemy)parentBlob.GetComponent (typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent (typeof(player));
	}
	
	// Update is called once per frame
	void Update () {
		// Do not change length of the collider
	//	transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);	
	}

	void OnTriggerEnter(Collider other) 
	{
		hazardousEnvironment hazardousObject = (hazardousEnvironment)other.gameObject.GetComponent (typeof(hazardousEnvironment));
		if (!hazardousObject)
			return;

		EAbilityType requiredAbility = hazardousObject.requiredAbility;
		int abilityIndex = parentEnemyScript.hasAbility (requiredAbility);
		if (abilityIndex != -1) {
			// Parent blob has required ability to enter the hazardous environment
			parentEnemyScript.activateAbility(abilityIndex);
		} else {
			// Give the parent the necessary information about the proximity of the environmental hazard
			parentEnemyScript.environmentProximityData = getProximityDataOfHazardousEnvironment();
		}
	}


	void OnTriggerStay(Collider other) 
	{
		hazardousEnvironment hazardousObject = (hazardousEnvironment)other.gameObject.GetComponent (typeof(hazardousEnvironment));
		if (!hazardousObject)
			return;

		EAbilityType requiredAbility = hazardousObject.requiredAbility;
		int abilityIndex = parentEnemyScript.hasAbility (requiredAbility);
		if (abilityIndex != -1) {
			// Parent blob has required ability to enter the hazardous environment
			parentEnemyScript.activateAbility(abilityIndex);
		} else {
			// Give the parent the necessary information about the proximity of the environmental hazard
			parentEnemyScript.environmentProximityData = getProximityDataOfHazardousEnvironment();
		}
	}


	private dangerProximity getProximityDataOfHazardousEnvironment()
	{
		// Check how close the hazardous environment is and in which direction it resides
		RaycastHit hit;
		Vector3 right = parentBlob.transform.right;
		dangerProximity proximityData = new dangerProximity();
		// Shoot 3 rays: one along the viewing direction, one 45° to the left and one 45° to the right
		// Check for proximity of environmental hazards
		for(int i = -1; i <= 1; i++)
		{
			float maxRayLength = parentEnemyScript.size + 2.0f*parentEnemyScript.currentSpeed;
			Vector3 rayDirection = ((1-Mathf.Abs (i))*parentEnemyScript.viewingDirection + i*right);
			proximityData.directions[i+1] = rayDirection.normalized;
			// Check whether the ray hits an object
			if (Physics.Raycast(parentBlob.transform.position, rayDirection, out hit, maxRayLength))
			{ 
				// If the object hit is an environmental hazard...
				if(hit.collider.gameObject.GetComponent(typeof(hazardousEnvironment)))
				{
					// ...then store the distance to this object and the normal on its surface
					proximityData.normals[i+1] = hit.normal;
					proximityData.distances[i+1] = hit.distance;
				}
			}
			else
			{
				// If the ray does not hit an object, then set the distance to the max ray length and the zero-vector as surface normal
				proximityData.normals[i+1] = new Vector3(0,0,0);
				proximityData.distances[i+1] = parentEnemyScript.size + 2.0f*parentEnemyScript.currentSpeed;
			}
		}
		return proximityData;
	}

}
