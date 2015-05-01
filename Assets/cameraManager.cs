using UnityEngine;
using System.Collections;

public class cameraManager : MonoBehaviour {

	private Quaternion originalRotation;

	private Transform playerTransform;

	private player playerScript;

	public bool isOrthographic = true;


	// Use this for initialization
	void Start () {
		originalRotation = transform.rotation;
		playerTransform = GameObject.Find("Blob").transform;
		playerScript = (player)GameObject.Find ("Blob").GetComponent (typeof(player));
		Camera.main.fieldOfView = 60;
		Camera.main.orthographic = isOrthographic;

	}
	
	// Update is called once per frame
	void Update () {

		if (isOrthographic) 
		{
			transform.position = playerTransform.position + new Vector3 (0, 0, -200);
			Camera.main.orthographicSize = playerScript.currentViewingRange + 1.28f*playerTransform.localScale.x;
		} 
		else 
		{
			// Get half the FOV in radians (for persective)
			float theta = 0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView;
			float cameraHeight = (playerScript.currentViewingRange + playerTransform.localScale.x) / Mathf.Tan (theta);
			transform.position = playerTransform.position + new Vector3 (0, 0, -cameraHeight);
		}
				
	}

}
