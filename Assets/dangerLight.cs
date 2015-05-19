using UnityEngine;
using System.Collections;

public class dangerLight : MonoBehaviour {

	public enemy parentEnemyScript;

	public player parentPlayerScript;

	private GameObject parent;

	private bool isPlayer;

	public Light pointLight;

	public float frequency = 1.0f;

	public float intensity = 2.0f;

	private bool attackMode;

	private bool fleeMode;


	// Use this for initialization
	void Start () 
	{
		parent = transform.parent.gameObject;

		parentEnemyScript = (enemy)parent.GetComponent (typeof(enemy));
		parentPlayerScript = (player)parent.GetComponent (typeof(player));

		isPlayer = (bool)parentPlayerScript;

		pointLight = this.gameObject.GetComponent<Light> ();
	}
	
	// Update is called once per frame
	void Update () {
		pointLight.range = 10f * parent.transform.localScale.x;
		pointLight.intensity = 3.0f + 2.0f*Mathf.Sin (frequency*Time.time);
	}

	public void reset(){
		transform.localPosition = new Vector3 (0, 0, 0);
	}

	public void setToFlee()
	{
		pointLight.color = new Color (0, 0, 1);
	}

	public void setToAttack()
	{
		pointLight.color = new Color (1, 0, 0);
	}

	public void setToWhite()
	{
		pointLight.color = new Color (1, 1, 1);
	}

}
