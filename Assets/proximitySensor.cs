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

	void OnCollisionEnter2D(Collision2D col) 
	{
		hazardousEnvironment hazardousObject = (hazardousEnvironment)col.gameObject.GetComponent (typeof(hazardousEnvironment));
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
				// Shoot 3 rays: one along the viewing direction, one 45° to the left and one 45° to the right
				for(int i = -1; i <= 1; i++)
				{
					Vector3 rayDirection = (parentEnemyScript.viewingDirection + i*right);
					// TODO replace 3.0 with current speed
					if (Physics.Raycast(parentBlob.transform.position, rayDirection, out hit, parentEnemyScript.size + 3.0f))
					{ 
						// Get the point on the bounding box of the hazardous environment)
						if(hit.collider.gameObject == hazardousObject)
						{
							Vector3 pos = hit.point;
							Vector3 posNormal = hit.normal;
							float cos = Vector3.Dot(posNormal, -parentEnemyScript.viewingDirection);
							float distance = hit.distance;
							if(distance - parentEnemyScript.size < 1.0f) {
								// TODO stop hunting after player
							}
							else
							{
								// TODO adapt running direction of blob in normal direction of the collider
							}
						}
					}
				}

			}
		}
	}


}
