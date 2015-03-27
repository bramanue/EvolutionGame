using UnityEngine;
using System.Collections;

public class dontrotate : MonoBehaviour {

	private Quaternion startrot;

	// Use this for initialization
	void Start () {

		startrot = transform.rotation;
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = startrot;
	
	}
}
