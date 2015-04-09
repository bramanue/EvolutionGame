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
		transform.localScale = new Vector3(parentEnemyScript.transform.localScale.x, 1.0f, parentEnemyScript.transform.localScale.z);	
	}

	void OnTriggerEnter(Collider other) 
	{
		hazardousEnvironment hazardousObject = (hazardousEnvironment)other.gameObject.GetComponent (typeof(hazardousEnvironment));
		if (!hazardousObject)
			return;

		Debug.Log ("Hazardous environment detected!");

		EAbilityType requiredAbility = hazardousObject.requiredAbility;
		int abilityIndex = parentEnemyScript.hasAbility (requiredAbility);
		if (abilityIndex != -1) {
			// Parent blob has required ability to enter the hazardous environment
			parentEnemyScript.activateAbility(abilityIndex);
		} else {
			if(parentEnemyScript.isHuntingPlayer)
			{
				RaycastHit hit;
				Vector3 right = parentBlob.transform.right;
				float minDistanceToHazardousObject = 8.0f*parentEnemyScript.size;
				Vector3 minDistanceObjectNormal = new Vector3(0,0,0);
				// Shoot 3 rays: one along the viewing direction, one 45° to the left and one 45° to the right
				// Check for proximity of environmental hazards
				for(int i = -1; i <= 1; i++)
				{
					Vector3 rayDirection = ((1-Mathf.Abs (i))*parentEnemyScript.viewingDirection + i*right);
					if (Physics.Raycast(parentBlob.transform.position, rayDirection, out hit, parentEnemyScript.size + 2.0f*parentEnemyScript.currentSpeed))
					{ 
						// Get the point on the bounding box of the hazardous environment)
						if(hit.collider.gameObject.GetComponent(typeof(hazardousEnvironment)))
						{
							Vector3 posNormal = hit.normal;
							float distance = hit.distance;
							if(distance - parentEnemyScript.size < 0.25f*parentEnemyScript.size) {
								parentEnemyScript.isHuntingPlayer = false;
								// TODO stop hunting after player
								return;
							}
							else
							{
								if(distance < minDistanceToHazardousObject) {
									minDistanceObjectNormal = posNormal;
									minDistanceToHazardousObject = distance;
								}

							}
						}
					}
				}

				float correctionFactor = Vector3.Dot (-parentEnemyScript.viewingDirection,minDistanceObjectNormal);
				// Apply running direction correction depending on the angle between the hazardous object and the current running direction
				// Correction at most by half speed in opposite direction of running
				Debug.Log ("Correcting course by " + Time.deltaTime*minDistanceObjectNormal*correctionFactor*parentEnemyScript.currentSpeed*0.8f);
				parentEnemyScript.addCourseCorrection(Time.deltaTime*minDistanceObjectNormal*correctionFactor*parentEnemyScript.currentSpeed*0.8f);

			}
			else
			{
				RaycastHit hit;
				Vector3 right = parentBlob.transform.right;
				float minDistanceToHazardousObject = 8.0f*parentEnemyScript.size;
				Vector3 minDistanceObjectNormal = new Vector3(0,0,0);
				// Shoot 3 rays: one along the viewing direction, one 45° to the left and one 45° to the right
				// Check for proximity of environmental hazards
				for(int i = -1; i <= 1; i++)
				{
					Vector3 rayDirection = ((1-Mathf.Abs (i))*parentEnemyScript.viewingDirection + i*right);
					if (Physics.Raycast(parentBlob.transform.position, rayDirection, out hit, parentEnemyScript.size + 2.0f*parentEnemyScript.currentSpeed))
					{ 
						// Get the point on the bounding box of the hazardous environment)
						if(hit.collider.gameObject.GetComponent(typeof(hazardousEnvironment)))
						{
							Vector3 posNormal = hit.normal;
							float distance = hit.distance;
							if(distance - parentEnemyScript.size < 0.25f*parentEnemyScript.size) {
								parentEnemyScript.isHuntingPlayer = false;
								// TODO stop hunting after player
								return;
							}
							else
							{
								if(distance < minDistanceToHazardousObject) {
									minDistanceObjectNormal = posNormal;
									minDistanceToHazardousObject = distance;
								}
								
							}
						}
					}
				}
				
				float correctionFactor = Vector3.Dot (-parentEnemyScript.viewingDirection,minDistanceObjectNormal);
				// Apply running direction correction depending on the angle between the hazardous object and the current running direction
				// Correction at most by half speed in opposite direction of running
				Debug.Log ("Correcting course by " + Time.deltaTime*minDistanceObjectNormal*correctionFactor*parentEnemyScript.currentSpeed*0.8f);
				parentEnemyScript.addCourseCorrection(Time.deltaTime*minDistanceObjectNormal*correctionFactor*parentEnemyScript.currentSpeed*0.8f);

			}
		}
	}


	void OnTriggerStay(Collider other) 
	{
		hazardousEnvironment hazardousObject = (hazardousEnvironment)other.gameObject.GetComponent (typeof(hazardousEnvironment));
		if (!hazardousObject)
			return;
		
		Debug.Log ("Hazardous environment detected!");
		
		EAbilityType requiredAbility = hazardousObject.requiredAbility;
		int abilityIndex = parentEnemyScript.hasAbility (requiredAbility);
		if (abilityIndex != -1) {
			// Parent blob has required ability to enter the hazardous environment
			parentEnemyScript.activateAbility(abilityIndex);
		} else {
			if(parentEnemyScript.isHuntingPlayer)
			{
				RaycastHit hit;
				Vector3 right = parentBlob.transform.right;
				float minDistanceToHazardousObject = 8.0f*parentEnemyScript.size;
				Vector3 minDistanceObjectNormal = new Vector3(0,0,0);
				// Shoot 3 rays: one along the viewing direction, one 45° to the left and one 45° to the right
				// Check for proximity of environmental hazards
				for(int i = -1; i <= 1; i++)
				{
					Vector3 rayDirection = (parentEnemyScript.viewingDirection + i*right);
					if (Physics.Raycast((1-Mathf.Abs (i))*parentBlob.transform.position, rayDirection, out hit, parentEnemyScript.size + 2.0f*parentEnemyScript.currentSpeed))
					{ 
						// Get the point on the bounding box of the hazardous environment)
						if(hit.collider.gameObject.GetComponent(typeof(hazardousEnvironment)))
						{
							Vector3 posNormal = hit.normal;
							float distance = hit.distance;
							if(distance - parentEnemyScript.size < 0.25f*parentEnemyScript.size) {
								parentEnemyScript.isHuntingPlayer = false;
								// TODO stop hunting after player
								return;
							}
							else
							{
								if(distance < minDistanceToHazardousObject) {
									minDistanceObjectNormal = posNormal;
									minDistanceToHazardousObject = distance;
								}
								
							}
						}
					}
				}
				
				float correctionFactor = Vector3.Dot (-parentEnemyScript.viewingDirection,minDistanceObjectNormal);
				// Apply running direction correction depending on the angle between the hazardous object and the current running direction
				// Correction at most by half speed in opposite direction of running
				Debug.Log ("Correcting course by " + Time.deltaTime*minDistanceObjectNormal*correctionFactor*parentEnemyScript.currentSpeed*0.5f);
				parentEnemyScript.addCourseCorrection(Time.deltaTime*minDistanceObjectNormal*correctionFactor*parentEnemyScript.currentSpeed*0.5f);
				
			}
			else
			{
				RaycastHit hit;
				Vector3 right = parentBlob.transform.right;
				float minDistanceToHazardousObject = 8.0f*parentEnemyScript.size;
				Vector3 minDistanceObjectNormal = new Vector3(0,0,0);
				// Shoot 3 rays: one along the viewing direction, one 45° to the left and one 45° to the right
				// Check for proximity of environmental hazards
				for(int i = -1; i <= 1; i++)
				{
					Vector3 rayDirection = ((1-Mathf.Abs (i))*parentEnemyScript.viewingDirection + i*right);
					if (Physics.Raycast(parentBlob.transform.position, rayDirection, out hit, parentEnemyScript.size + 2.0f*parentEnemyScript.currentSpeed))
					{ 
						// Get the point on the bounding box of the hazardous environment)
						if(hit.collider.gameObject.GetComponent(typeof(hazardousEnvironment)))
						{
							Vector3 posNormal = hit.normal;
							float distance = hit.distance;
							if(distance - parentEnemyScript.size < 0.25f*parentEnemyScript.size) {
								parentEnemyScript.isHuntingPlayer = false;
								// TODO stop hunting after player
								return;
							}
							else
							{
								if(distance < minDistanceToHazardousObject) {
									minDistanceObjectNormal = posNormal;
									minDistanceToHazardousObject = distance;
								}
								
							}
						}
					}
				}
				
				float correctionFactor = Vector3.Dot (-parentEnemyScript.viewingDirection,minDistanceObjectNormal);
				// Apply running direction correction depending on the angle between the hazardous object and the current running direction
				// Correction at most by half speed in opposite direction of running
				Debug.Log ("Correcting course by " + Time.deltaTime*minDistanceObjectNormal*correctionFactor*parentEnemyScript.currentSpeed*0.5f);
				parentEnemyScript.addCourseCorrection(Time.deltaTime*minDistanceObjectNormal*correctionFactor*parentEnemyScript.currentSpeed*0.5f);
				
			}
		}
	}


}
