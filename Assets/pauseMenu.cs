using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class pauseMenu : MonoBehaviour {


	private GameObject pauseText;

	private GameObject continueButton;

	private GameObject pauseMenuToMainMenuButton;

	private abilityModificationPanel abilityPanel;


	// Use this for initialization
	void Start () {
		pauseText = GameObject.Find("PauseText");
		continueButton = GameObject.Find ("ContinueButton");
		pauseMenuToMainMenuButton = GameObject.Find ("PauseMenuToMainMenuButton");

		abilityPanel = (abilityModificationPanel)GameObject.Find ("AbilityModificationPanel").GetComponent (typeof(abilityModificationPanel));

		hide ();
	}

	public void show()
	{
		pauseText.SetActive(true);
		continueButton.SetActive (true);
		pauseMenuToMainMenuButton.SetActive (true);

		EventSystem.current.SetSelectedGameObject (continueButton);
	}

	public void hide()
	{
		pauseText.SetActive(false);
		continueButton.SetActive (false);
		pauseMenuToMainMenuButton.SetActive (false);

		EventSystem.current.firstSelectedGameObject = null;
		EventSystem.current.SetSelectedGameObject (null);
	}
}
