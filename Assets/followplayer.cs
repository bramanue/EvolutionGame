using UnityEngine;
using System.Collections;

public class followplayer : MonoBehaviour {

	public Transform player;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		transform.position = player.transform.position + new Vector3 (0, 0, -33);
		Camera.main.orthographicSize = 10 + player.transform.localScale.x;
	
	}
}
