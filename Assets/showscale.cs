using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class showscale : MonoBehaviour {

	public Text Bro;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		 Bro.text = transform.localScale.x.ToString();
	
	}
}
