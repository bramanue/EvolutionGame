using UnityEngine;
using System.Collections;

public class cameraManager : MonoBehaviour {

	// Transform of the player
	private Transform playerTransform;

	// Pointer to the script of the player (necessary to fetch viewing range)
	private player playerScript;

	// Defines whether this camera is perspective or orthographic
	public bool isOrthographic = false;

	// The current height of the camera
	private float cameraHeight;

	// The height the camera should have after it has adapted to the new viewing range
	private float targetHeight;

	// Defines how fast the camera zooms out, when viewing range is increased
	public float zoomPerSecond = 1.0f;

	public float fieldOfView = 60.0f;


	// Use this for initialization
	void Start () 
	{
		playerTransform = GameObject.Find("Blob").transform;
		playerScript = (player)GameObject.Find ("Blob").GetComponent (typeof(player));
		Camera.main.fieldOfView = fieldOfView;
		Camera.main.orthographic = isOrthographic;

		// Initialize camera height
		if (isOrthographic) 
		{
			transform.position = playerTransform.position + new Vector3 (0, 0, -200);
			Camera.main.orthographicSize = playerScript.currentViewingRange + 1.28f*playerTransform.localScale.x;
		} 
		else 
		{
			// Get half the FOV in radians (for persective)
			float theta = 0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView;
			cameraHeight = -(playerScript.currentViewingRange + playerTransform.localScale.x) / Mathf.Tan (theta);
			transform.position = playerTransform.position + new Vector3 (0, 0, cameraHeight);
			targetHeight = cameraHeight;
		}

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
			// Get half the FOV in radians
			float theta = 0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView;
			// Calculate the next height of the camera (changes only if player's viewing range or size has changed)
			cameraHeight = -(playerScript.currentViewingRange + playerTransform.localScale.x) / Mathf.Tan (theta);
		/*	if(targetHeight != nextHeight)
			{
				targetHeight = nextHeight;
				float difference = targetHeight - cameraHeight;

				StartCoroutine(zoomOut (difference));
			}*/
		}	
	}

	void LateUpdate()
	{
		transform.position = playerTransform.position + new Vector3 (0, 0, cameraHeight);
	}

	IEnumerator zoomOut(float totalDifference)
	{
		float zoomTime = Mathf.Abs (totalDifference) / zoomPerSecond;
		float direction = Mathf.Sign (totalDifference);
		Debug.Log ("Start coroutine at CameraHeight = " + cameraHeight + " targetHeight = " + targetHeight + " totalDifference = " + totalDifference + " zoomTime = " + zoomTime);
		float time = 0.0f;
		for(; time < zoomTime; time += Time.deltaTime)
		{
			cameraHeight += direction*zoomPerSecond*Time.deltaTime;
			Debug.Log ("CameraHeight at time = " + time + " : " + cameraHeight);
			transform.position = playerTransform.position + new Vector3 (0, 0, cameraHeight);
			yield return null;
		}
		// Transform the camera also for the rest of the path
		float remainingTime = time - zoomTime;
		Debug.Log ("remainingTime = " + remainingTime);
		cameraHeight -= totalDifference * remainingTime;
	}

}
