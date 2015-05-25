using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class highscoreManager : MonoBehaviour {

	private float currentHighscore = 0.0f;

	private float visibleHighscore = 0.0f;

	private float multiplier = 1.0f;

	private float timer;

	private GameObject highscoreDisplayObject;

	public Text highscoreDisplay;

	private float scoreUpdateSpeed;

	public Button postHighscoreButton;

	private player playerScript;


	// Use this for initialization
	void Start () 
	{
		highscoreDisplayObject = GameObject.Find ("HighscoreCanvas");
		postHighscoreButton.enabled = true;
		currentHighscore = 0.0f;
		visibleHighscore = 0.0f;
		playerScript = (player)GameObject.Find ("Blob").GetComponent (typeof(player));
	}
	
	// Update is called once per frame
	void Update () {

		if (timer > 0) {
			timer -= Time.deltaTime;

		} else {
			// TODO Show that multiplier is over
			multiplier = 1.0f;
			timer = 0.0f;
		}
		if (visibleHighscore < currentHighscore) {
			visibleHighscore += scoreUpdateSpeed*Time.deltaTime;
			visibleHighscore = Mathf.Min (Mathf.Ceil(visibleHighscore),currentHighscore);
		}
		// Update the highscore display
		highscoreDisplay.text = "Highscore : " + visibleHighscore;
	}

	public void applyMultiplier(float factor, float time)
	{
		// TODO Show that multiplier is active
		multiplier = multiplier*factor;
		timer = Mathf.Max (timer, 0);
		timer += time;
	}

	public void enemyKilled(float score)
	{
		if (!playerScript.dead) {
			if (float.IsInfinity (score))
				Debug.Log (score * multiplier + " points for killing enemy");

			// TODO Display for what the points are
			currentHighscore += Mathf.Floor (score * multiplier);
			scoreUpdateSpeed = 2.0f * (currentHighscore - visibleHighscore);
		}
	}

	public void lootDropped(float score)
	{
		if (!playerScript.dead) {
			if (float.IsInfinity (score))
				Debug.Log (score * multiplier + " points for killing enemy");

			// TODO Display for what the points are
			currentHighscore += Mathf.Floor (score * multiplier);
			visibleHighscore += Mathf.Floor (score * multiplier);
		}
	}

	public void playerIsGrowing(float score)
	{
		if (!playerScript.dead) {
			// TODO Display for what the points are
			currentHighscore += Mathf.Floor (score * multiplier);
			visibleHighscore += Mathf.Floor (score * multiplier);
		}
	}

	public void resetHighscore()
	{
		currentHighscore = 0.0f;
		visibleHighscore = 0.0f;
		multiplier = 1.0f;
		postHighscoreButton.interactable = true;
		((Text)postHighscoreButton.transform.FindChild("Text").GetComponent<Text>()).text = "Share your Highscore";
	}

	public void showHighscore(bool active) 
	{
		highscoreDisplayObject.SetActive (active);
	}

	public float getHighscore() {
		return currentHighscore;
	}

	public void postScoreOnline()
	{
		// Post a new score
		leaderboardInterface leaderboardInterface = (leaderboardInterface)GameObject.Find ("LeaderboardInterface").GetComponent (typeof(leaderboardInterface));
		postHighscoreButton.interactable = false;
		((Text)postHighscoreButton.transform.FindChild("Text").GetComponent<Text>()).text = "Highscore uploaded!";
		leaderboardInterface.PostScore((long)currentHighscore);
		((mainMenu)GameObject.Find ("MainMenu").GetComponent (typeof(mainMenu))).showGameOverScreen(true, currentHighscore);
	}
}
