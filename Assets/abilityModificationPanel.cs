using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum EButtonType {
	// Default ability
	EAButton,
	EBButton,
	EXButton,
	EYButton,
	ERightTrigger,
	ELeftTrigger
}

public class abilityModificationPanel : MonoBehaviour {

	private Quaternion originalRotation;

	private int highlightButton;

	private GameObject[] buttons = new GameObject[4];

	private CanvasRenderer[] buttonRenderers = new CanvasRenderer[4];

	private Text[] textBoxes = new Text[4];

	private Text messageBox;

	private Text titleBox;

	private bool active;

	private float timer;

	private float originalTimer;

	public bool isInChosingState;

	public enemy enemyScript;

	public ability newAbility;

	private GameObject player;

	private float id;

	private float fadeAnimationId;


	// Use this for initialization
	void Start () 
	{
		originalRotation = transform.rotation;

		buttons = new GameObject[4];
		buttons [0] = GameObject.Find ("AButton");
		buttons [1] = GameObject.Find ("BButton");
		buttons [2] = GameObject.Find ("XButton");
		buttons [3] = GameObject.Find ("YButton");

		textBoxes [0] = (Text)( GameObject.Find ("AAbility").GetComponent(typeof(Text)));
		textBoxes [1] = (Text)( GameObject.Find ("BAbility").GetComponent(typeof(Text)));
		textBoxes [2] = (Text)( GameObject.Find ("XAbility").GetComponent(typeof(Text)));
		textBoxes [3] = (Text)( GameObject.Find ("YAbility").GetComponent(typeof(Text)));

		buttonRenderers[0] = (CanvasRenderer)buttons[0].GetComponent(typeof(CanvasRenderer));
		buttonRenderers[1] = (CanvasRenderer)buttons[1].GetComponent(typeof(CanvasRenderer));
		buttonRenderers[2] = (CanvasRenderer)buttons[2].GetComponent(typeof(CanvasRenderer));
		buttonRenderers[3] = (CanvasRenderer)buttons[3].GetComponent(typeof(CanvasRenderer));

		highlightButton = -1;

		messageBox = (Text)( GameObject.Find ("MessageBox").GetComponent(typeof(Text)));
		messageBox.text = "Something";

		titleBox = (Text)( GameObject.Find ("TitleBox").GetComponent(typeof(Text)));
		messageBox.text = "New Ability Name";

		player = GameObject.Find ("Blob");

		fadeAnimationId = -1;

	}
	
	// Update is called once per frame
	void Update () 
	{
		if (active) 
		{
			Time.timeScale = 0.01f;
			// Make sure that if the fade animation is active, that the panel has not been opened again
			if(fadeAnimationId == id)
			{
				Time.timeScale = 1.0f;
				if(timer > 0) 
				{
					timer -= Time.deltaTime;
					if(highlightButton != -1) {
						for (int i = 0; i < 4; i++) {
							if (i == highlightButton) {
								if(timer >= 0.5*originalTimer) {
									// Increase alpha value until one
									float alpha = Mathf.Min (1.0f,buttonRenderers [i].GetAlpha() + Time.deltaTime*0.6f);
									buttonRenderers [i].SetAlpha (alpha);
									textBoxes [i].color = new Color (1.0f, 1.0f, 1.0f, alpha);
								}
								else
								{
									// Decrease alpha value until 0
									float alpha = Mathf.Max (0.0f,buttonRenderers [i].GetAlpha() - Time.deltaTime);
									buttonRenderers [i].SetAlpha (alpha);
									textBoxes [i].color = new Color (1.0f, 1.0f, 1.0f, alpha);
								}
							} else {
								// Not the changed button - Decrease alpha values until 0
								float alpha = Mathf.Max (0.0f,buttonRenderers [i].GetAlpha() - Time.deltaTime*0.6f);
								buttonRenderers [i].SetAlpha (alpha);
								textBoxes [i].color = new Color (1.0f, 1.0f, 1.0f, alpha);
							}
						}
					} else {
						for (int i = 0; i < 4; i++) {
							// No buttons were changed - simply let the panel fade away
							float alpha = Mathf.Max (0.0f,buttonRenderers [i].GetAlpha() - Time.deltaTime*0.6f);
							buttonRenderers [i].SetAlpha (alpha);
							textBoxes [i].color = new Color (1.0f, 1.0f, 1.0f, alpha);
						}
					}
				}
				else
				{
					if(!isInChosingState)
					{
						active = false;
						highlightButton = -1;
						gameObject.SetActive(false);
					}
				}
			}
		}
	}

	void LateUpdate()
	{
		// Keep UI panel from rotating with the player's blob
		transform.rotation = originalRotation;
		float size = 0.1f/player.transform.localScale.x;
		transform.localScale = new Vector3 (size, size, size);
		if(isInChosingState)
			gameObject.SetActive(true);
	}

	public void showPanel(string[] abilityNames, ability newAbility, enemy enemyScript)
	{
		active = true;
		isInChosingState = true;
		id = Random.value;	// Random value as id

		Time.timeScale = 0.01f;
		this.enemyScript = enemyScript;
		this.newAbility = newAbility;

		gameObject.SetActive(true);

		// Display names and mark already mapped buttons
		for (int i = 0; i < 4; i++) {
			if (!abilityNames[i].Equals (string.Empty)) {
				((Image)(buttons[i].GetComponent(typeof(Image)))).CrossFadeAlpha(0.8f,1.0f,true);
				textBoxes [i].text = abilityNames[i];
				textBoxes [i].color = new Color (1.0f, 1.0f, 1.0f, 0.8f);
			} else {
				buttonRenderers [i].SetAlpha (0.0f);
				((Image)(buttons[i].GetComponent(typeof(Image)))).CrossFadeAlpha(0.5f,1.0f,true);
				textBoxes [i].text = "(empty)";
				textBoxes [i].color = new Color (1.0f, 1.0f, 1.0f, 0.5f);
			}
		}
	}

	public void hidePanel()
	{
		fadeAnimationId = id;
		isInChosingState = false;
		Time.timeScale = 1.0f;
		originalTimer = 1.0f;
		timer = 1.0f;

		newAbility = null;
		enemyScript = null;
	}

	public void highlightButtonAndHide(string newAbilityName, EButtonType button)
	{
		fadeAnimationId = id;
		isInChosingState = false;
		Time.timeScale = 1.0f;
		timer = 1.5f;
		originalTimer = 1.0f;
		highlightButton = (int)button;
		textBoxes [highlightButton].text = newAbilityName;
		newAbility = null;
		enemyScript = null;
	}

	public void displayMessage(string message) {
		messageBox.text = message;
	}

	public void displayTitle(string title) {
		titleBox.text = title;
	}
}
