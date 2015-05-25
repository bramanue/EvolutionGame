using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class mainMenu : MonoBehaviour {

	private GameObject startButton;

	private GameObject tutorialButton;

	private GameObject highscoreButton;

	private GameObject tutorial1Button;

	private GameObject tutorial2Button;

	private GameObject tutorial3Button;

	private GameObject tutorialToMainMenuButton;

	private GameObject gameOverText;

	private GameObject retryButton;

	private GameObject gameOverToMainMenuButton;

	private GameObject gameOverHighscoreText;

	private Text highscoreText;

	private pauseMenu pauseMenu;

	private GameObject tutorialUI;


	// Use this for initialization
	void Start () 
	{
		startButton = GameObject.Find ("StartButton");
		tutorialButton = GameObject.Find ("TutorialButton");
		highscoreButton = GameObject.Find("HighscoreButton");
		tutorial1Button = GameObject.Find ("Tutorial1Button");
		tutorial2Button = GameObject.Find ("Tutorial2Button");
		tutorial3Button = GameObject.Find ("Tutorial3Button");

		tutorialToMainMenuButton = GameObject.Find ("TutorialToMainMenuButton");
		gameOverHighscoreText = GameObject.Find ("GameOverHighscoreText");
		gameOverText = GameObject.Find ("GameOverText");
		retryButton = GameObject.Find ("RetryButton");
		gameOverToMainMenuButton = GameObject.Find ("GameOverToMainMenuButton");

		pauseMenu = (pauseMenu)GameObject.Find ("PauseMenu").GetComponent(typeof(pauseMenu));
		tutorialUI = GameObject.Find ("TutorialUI");

		highscoreText = (Text)gameOverHighscoreText.GetComponent<Text> ();

		startButton.SetActive (true);
		tutorialButton.SetActive (true);
		highscoreButton.SetActive (true);
		
		tutorial1Button.SetActive (false);
		tutorial2Button.SetActive (false);
		tutorial3Button.SetActive (false);
		tutorialToMainMenuButton.SetActive (false);

		gameOverText.SetActive (false);
		retryButton.SetActive (false);
		gameOverHighscoreText.SetActive (false);
		gameOverToMainMenuButton.SetActive (false);
		
		EventSystem.current.SetSelectedGameObject (startButton);
		
	}

	public void showListOfTutorials()
	{
		startButton.SetActive (false);
		tutorialButton.SetActive (false);
		highscoreButton.SetActive (false);

		tutorial1Button.SetActive (true);
		tutorial2Button.SetActive (true);
		tutorial3Button.SetActive (true);
		tutorialToMainMenuButton.SetActive (true);

		EventSystem.current.SetSelectedGameObject (tutorial1Button);
	}

	public void showMainMenu()
	{
		pauseMenu.hide ();
		tutorialUI.SetActive (false);
		((gameManager)GameObject.Find ("GameManager").GetComponent (typeof(gameManager))).restartTutorialPending = false;

		startButton.SetActive (true);
		tutorialButton.SetActive (true);
		highscoreButton.SetActive (true);

		tutorial1Button.SetActive (false);
		tutorial2Button.SetActive (false);
		tutorial3Button.SetActive (false);
		tutorialToMainMenuButton.SetActive (false);

		retryButton.SetActive (false);
		gameOverText.SetActive (false);
		gameOverHighscoreText.SetActive (false);
		gameOverToMainMenuButton.SetActive (false);

		EventSystem.current.SetSelectedGameObject (startButton);
	}

	public void showGameOverScreen(bool showHighscore, float highscore)
	{
		pauseMenu.hide ();
		tutorialUI.SetActive (false);

		startButton.SetActive (false);
		tutorialButton.SetActive (false);
		highscoreButton.SetActive (false);

		tutorial1Button.SetActive (false);
		tutorial2Button.SetActive (false);
		tutorial3Button.SetActive (false);
		tutorialToMainMenuButton.SetActive (false);
		
		tutorialToMainMenuButton.SetActive (false);
		gameOverText.SetActive (true);
		retryButton.SetActive (true);

		gameOverToMainMenuButton.SetActive (true);
		if(showHighscore) {
			gameOverHighscoreText.SetActive (true);
			highscoreText.text = "Your Highscore : " + highscore;
		}

		EventSystem.current.SetSelectedGameObject (retryButton);
	}

	public void hide() 
	{
		startButton.SetActive (false);
		tutorialButton.SetActive (false);
		highscoreButton.SetActive (false);
		
		tutorial1Button.SetActive (false);
		tutorial2Button.SetActive (false);
		tutorial3Button.SetActive (false);
		tutorialToMainMenuButton.SetActive (false);
		
		tutorialToMainMenuButton.SetActive (false);
		gameOverText.SetActive (false);
		retryButton.SetActive (false);
		gameOverToMainMenuButton.SetActive (false);

		EventSystem.current.firstSelectedGameObject = null;
		EventSystem.current.SetSelectedGameObject (null);
	}
}
