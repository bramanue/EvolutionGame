using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class fpsCount : MonoBehaviour {
	
	public Text fpsDisplay;

	private float timer;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// Update FPS count every second
		if (timer <= 0) {
			fpsDisplay.text = (((int)(1.0f / Time.deltaTime)).ToString () + " FPS");
			timer = 1.0f;
		} else
			timer -= Time.deltaTime;
		
	}
}
