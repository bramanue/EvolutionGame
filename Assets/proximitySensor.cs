using UnityEngine;
using System.Collections;

public class proximitySensor : MonoBehaviour {

	public GameObject parentBlob;

	public enemy parentEnemyScript;

	public player parentPlayerScript;

	// Make only one check per frame and enemy blob
	private bool alreadyCheckedStay;

	private bool alreadyCheckedEnter;

	private bool isPlayer;

	private BoxCollider collider;

	private dangerProximity proximityData = new dangerProximity();



	// Use this for initialization
	void Start () {
		parentBlob = transform.parent.gameObject;
		parentEnemyScript = (enemy)parentBlob.GetComponent (typeof(enemy));
		parentPlayerScript = (player)parentBlob.GetComponent (typeof(player));
		if (parentPlayerScript)
			isPlayer = true;

		collider = this.gameObject.GetComponent<BoxCollider> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (!isPlayer) {
			// Make the collider bigger, the faster the blob moves
			float speed = parentEnemyScript.currentSpeed;
			collider.transform.localPosition = new Vector3 (0, 1.0f + speed * 0.75f, 0);
			collider.transform.localScale = new Vector3 (3, speed*1.5f, 3);

			RaycastHit hit;
			Vector3 right = parentBlob.transform.right;
			bool report = false;
			
			// Shoot 3 rays: one along the viewing direction, one 45° to the left and one 45° to the right
			// Check for proximity of environmental hazards
			for (int i = -1; i <= 1; i++) {
				float maxRayLength = parentEnemyScript.size + parentEnemyScript.currentSpeed;
				Vector3 rayDirection = ((1 - Mathf.Abs (i)) * parentEnemyScript.viewingDirection + i * right);
				proximityData.directions [i + 1] = rayDirection.normalized;
				// Check whether the ray hits an object
				if (Physics.Raycast (parentBlob.transform.position, rayDirection, out hit, maxRayLength)) { 
					// If the object hit is an environmental hazard...
					if (hit.collider.gameObject.GetComponent (typeof(hazardousEnvironment))) {
						report = true;
						proximityData.registerIntersection (parentBlob.transform.position + hit.distance * rayDirection);
					}
				}
			}

			if(report)
				parentEnemyScript.environmentProximityData = proximityData;
		}
	}

	void LateUpdate()
	{
		// Check only one intersection in the next frame
		alreadyCheckedEnter = false;
		alreadyCheckedStay = false;
		proximityData.clearData ();
	}


	void OnTriggerEnter(Collider other) 
	{
		// We do not care about this intersection if we already got one during this frame
	//	if (alreadyCheckedEnter)
	//		return;

		// We only care about collisions with hazardous environment
		hazardousEnvironment hazardousObject = (hazardousEnvironment)other.gameObject.GetComponent (typeof(hazardousEnvironment));
		if (hazardousObject == null)
			return;

		// Parent enemy script can be null, at beginning of the game in case that this blob was placed inside an environmental hazard 
		if (parentEnemyScript == null)
			return;

		// Give the parent the necessary information about the proximity of the environmental hazard
		parentEnemyScript.environmentProximityData = getProximityDataOfHazardousEnvironment(other);
		alreadyCheckedEnter = true;
	}


	void OnTriggerStay(Collider other) 
	{
	//	if (alreadyCheckedStay)
	//		return;

		hazardousEnvironment hazardousObject = (hazardousEnvironment)other.gameObject.GetComponent (typeof(hazardousEnvironment));
		if (!hazardousObject)
			return;

		// Parent enemy script can be null, at beginning of the game in case that this blob is placed inside an environmental hazard 
		if (parentEnemyScript == null)
			return;

		// Give the parent the necessary information about the proximity of the environmental hazard
		parentEnemyScript.environmentProximityData = getProximityDataOfHazardousEnvironment(other);
		alreadyCheckedStay = true;
	}


	private dangerProximity getProximityDataOfHazardousEnvironment(Collider hazard)
	{
		Vector3 intersection = hazard.bounds.ClosestPoint (parentBlob.transform.position);

		proximityData.registerIntersection (intersection-parentBlob.transform.position);

		// Shoot a ray into that direction to make sure the distance is not shorter
		RaycastHit hit;
		if (Physics.Raycast(parentBlob.transform.position, parentEnemyScript.viewingDirection, out hit, parentEnemyScript.currentSpeed))
		{ 
			// If the object hit is an environmental hazard...
			hazardousEnvironment environment = (hazardousEnvironment)hit.collider.gameObject.GetComponent(typeof(hazardousEnvironment));
			if(environment != null)
			{
				proximityData.registerIntersection (hit.distance*parentEnemyScript.viewingDirection);
				proximityData.requiredAbility = environment.requiredAbility;
			}
		}
		return proximityData;



		// Check how close the hazardous environment is and in which direction it resides
	//	RaycastHit hit;
		Vector3 right = parentBlob.transform.right;

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
