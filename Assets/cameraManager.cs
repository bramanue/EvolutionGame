using UnityEngine;
using System.Collections;

public class cameraManager : MonoBehaviour {

	private Quaternion originalRotation;

	private Transform playerTransform;

	private player playerScript;

	private float heightVsRange;


	// Use this for initialization
	void Start () {
		originalRotation = transform.rotation;
		playerTransform = GameObject.Find("Blob").transform;
		playerScript = (player)GameObject.Find ("Blob").GetComponent (typeof(player));
		Camera.main.fieldOfView = 90;
		// Camera.main.orthographic = false;
		heightVsRange = 1.0f / Mathf.Tan(Camera.main.fieldOfView*Mathf.Deg2Rad);
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = originalRotation;
		// Get half the FOV in radians (for persective)
	//	float theta = 0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView;
	//	float cameraHeight = (playerScript.viewingRange + playerTransform.localScale.x) / Mathf.Tan (theta);
	//	transform.position = playerTransform.position + new Vector3 (0, 0, -cameraHeight);
		transform.position = playerTransform.position + new Vector3 (0, 0, -33);
		Camera.main.orthographicSize = playerScript.viewingRange + playerTransform.localScale.x;
	}
}
