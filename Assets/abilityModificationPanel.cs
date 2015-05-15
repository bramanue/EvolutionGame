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


	private EAbilityClass currentAbilityClass;

	private GameObject[] buttons = new GameObject[4];

	private GameObject[] triggers = new GameObject[2];

	private CanvasRenderer[] buttonRenderers = new CanvasRenderer[4];

	private CanvasRenderer[] triggerRenderers = new CanvasRenderer[2];

	private Text[] buttonTextBoxes = new Text[4];

	private Text[] triggerTextBoxes = new Text[2];

	private Text messageBox;

	private Text titleBox;

	private string[] currentAbilities = new string[6];


	private bool active;

	public bool isInChosingState;


	private float timer;

	private float originalTimer;


	private GameObject player;


	private float id;

	private float fadeAnimationId;

	private int highlightButton;

	private int highlightTrigger;


	private GUIStyle guiStyle = new GUIStyle();



	// Use this for initialization
	void Start () 
	{
		buttons [0] = GameObject.Find ("AButton");
		buttons [1] = GameObject.Find ("BButton");
		buttons [2] = GameObject.Find ("XButton");
		buttons [3] = GameObject.Find ("YButton");

		triggers[0] = GameObject.Find ("LTrigger");
		triggers[1] = GameObject.Find ("RTrigger");

		buttonTextBoxes [0] = (Text)( GameObject.Find ("AAbility").GetComponent(typeof(Text)));
		buttonTextBoxes [1] = (Text)( GameObject.Find ("BAbility").GetComponent(typeof(Text)));
		buttonTextBoxes [2] = (Text)( GameObject.Find ("XAbility").GetComponent(typeof(Text)));
		buttonTextBoxes [3] = (Text)( GameObject.Find ("YAbility").GetComponent(typeof(Text)));

		triggerTextBoxes [0] = (Text)( GameObject.Find ("LShield").GetComponent(typeof(Text)));
		triggerTextBoxes [1] = (Text)( GameObject.Find ("RShield").GetComponent(typeof(Text)));

		buttonRenderers[0] = (CanvasRenderer)buttons[0].GetComponent(typeof(CanvasRenderer));
		buttonRenderers[1] = (CanvasRenderer)buttons[1].GetComponent(typeof(CanvasRenderer));
		buttonRenderers[2] = (CanvasRenderer)buttons[2].GetComponent(typeof(CanvasRenderer));
		buttonRenderers[3] = (CanvasRenderer)buttons[3].GetComponent(typeof(CanvasRenderer));

		triggerRenderers[0] = (CanvasRenderer)triggers[0].GetComponent(typeof(CanvasRenderer));
		triggerRenderers[1] = (CanvasRenderer)triggers[1].GetComponent(typeof(CanvasRenderer));

		highlightButton = -1;
		highlightTrigger = -1;

		messageBox = (Text)( GameObject.Find ("MessageBox").GetComponent(typeof(Text)));
		messageBox.text = "";

		titleBox = (Text)( GameObject.Find ("TitleBox").GetComponent(typeof(Text)));
		titleBox.text = "";

		player = GameObject.Find ("Blob");

		fadeAnimationId = -1;

	}
	
	// Update is called once per frame
	void Update () 
	{
		if (active) {
			Time.timeScale = 0.0f;
		} 
	}


	public void showPanel (string[] abilityNames, EAbilityClass abilityClass)
	{
		currentAbilityClass = abilityClass;
		currentAbilities = abilityNames;
		if(currentAbilityClass == EAbilityClass.EActiveAbility) 
		{
			active = true;
			isInChosingState = true;
			id = Random.value;	// Random value as id
			Time.timeScale = 0.0f;

			// Display names and mark already mapped buttons
			for (int i = 0; i < 4; i++) {
				if (!abilityNames[i].Equals (string.Empty)) {
					((Image)(buttons[i].GetComponent(typeof(Image)))).CrossFadeAlpha(1.0f,0.5f,true);
					buttonTextBoxes [i].text = abilityNames[i];
					buttonTextBoxes [i].color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
				} else {
					((Image)(buttons[i].GetComponent(typeof(Image)))).CrossFadeAlpha(0.6f,0.5f,true);
					buttonTextBoxes [i].text = "(empty)";
					buttonTextBoxes [i].color = new Color (1.0f, 1.0f, 1.0f, 0.6f);
				}
			}
		}
		else if(currentAbilityClass == EAbilityClass.EShieldAbility)
		{
			active = true;
			isInChosingState = true;
			id = Random.value;	// Random value as id
			Time.timeScale = 0.0f;

			// Display names and mark already mapped buttons
			for (int i = 4; i < 6; i++) {
				if (!abilityNames[i].Equals (string.Empty)) {
					((Image)(triggers[i-4].GetComponent(typeof(Image)))).CrossFadeAlpha(1.0f,0.5f,true);
					triggerTextBoxes [i-4].text = abilityNames[i];
					triggerTextBoxes [i-4].color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
				} else {
					((Image)(triggers[i-4].GetComponent(typeof(Image)))).CrossFadeAlpha(0.6f,0.5f,true);
					triggerTextBoxes [i-4].text = "(empty)";
					triggerTextBoxes [i-4].color = new Color (1.0f, 1.0f, 1.0f, 0.6f);
				}
			}
		}
	}

	public bool isActive()
	{
		return isInChosingState;
	}

	public void hidePanel()
	{
		for (int i = 0; i < 4; i++) {
			if (!currentAbilities[i].Equals (string.Empty)) {
				((Image)(buttons[i].GetComponent(typeof(Image)))).CrossFadeAlpha(0.8f,0.5f,true);
				buttonTextBoxes [i].text = "";
			} else {
				((Image)(buttons[i].GetComponent(typeof(Image)))).CrossFadeAlpha(0.3f,0.5f,true);
				buttonTextBoxes [i].text = "";
			}
		}
		for (int i = 0; i < 2; i++) {
			if (!currentAbilities[i+4].Equals (string.Empty)) {
				((Image)(triggers[i].GetComponent(typeof(Image)))).CrossFadeAlpha(0.8f,0.5f,true);
				triggerTextBoxes [i].text = "";
			} else {
				((Image)(buttons[i].GetComponent(typeof(Image)))).CrossFadeAlpha(0.3f,0.5f,true);
				triggerTextBoxes [i].text = "";
			}
		}

		messageBox.CrossFadeAlpha (0.0f, 0.5f, true);
		fadeAnimationId = id;
		active = false;
		isInChosingState = false;
		Time.timeScale = 1.0f;
	}

	public void displayMessage(string message) {
		messageBox.text = message;
		messageBox.CrossFadeAlpha (1, 0.5f, true);
	}

	public void hideMessage() {
		messageBox.CrossFadeAlpha (0, 0.5f, true);
	}

	public void displayTitle(string title) {
		titleBox.text = title;
		titleBox.CrossFadeAlpha (1, 0.3f, true);
	}

	public void hideTitle() {
		titleBox.CrossFadeAlpha (0, 0.2f, true);
	}

}
