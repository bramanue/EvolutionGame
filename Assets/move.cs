using UnityEngine;
using System.Collections;



public class move : MonoBehaviour
{
	public float speed = 5.5f;
	public float rotationSpeed = 10.0f;

	
	
	// Use this for initialization
	void Start()
	{

	}
	
	// Update is called once per frame
	void Update()
	{
		transform.Rotate(  0.0f, 0.0f, -Input.GetAxis ("Horizontal") * rotationSpeed);
		transform.position +=  Input.GetAxis("Vertical") * speed * transform.up * Time.deltaTime; // vorwärts bewegen
	}

	void  OnTriggerEnter2D(Collider2D other)
	{

		if (other.transform.localScale.x <= transform.localScale.x) {

			Destroy (other.gameObject);
			transform.localScale += new Vector3(0.1f,0.1f,0.1f);
		} 
		else 
		{
			transform.localScale -= new Vector3(0.1f,0.1f,0.1f);
			print ("Game Over");
		}
	}
	
}