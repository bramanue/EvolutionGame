using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class fpsCount : MonoBehaviour {
	
	public Text fpsDisplay;

	private float timer;

	private int count;
	
	// Use this for initialization
	void Start () {
		count = 0;
	}
	
	// Update is called once per frame
	void Update () {
		// Update FPS count every second
		if (timer <= 0) {
			fpsDisplay.text = (count + "FPS");
			count = 0;
		//	fpsDisplay.text = (((int)(1.0f / Time.deltaTime)).ToString () + " FPS");
			timer = 1.0f;
		} else {
			timer -= Time.deltaTime;
			count++;
		}
		
	}
}
