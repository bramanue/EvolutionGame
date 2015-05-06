using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class highscoreManager : MonoBehaviour {

	private float currentHighscore;

	private float visibleHighscore;

	private float multiplier = 1.0f;

	private float timer;

	public Text highscoreDisplay;

	private float scoreUpdateSpeed;


	// Use this for initialization
	void Start () {
		currentHighscore = 0.0f;

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
		// TODO Display for what the points are
		currentHighscore += Mathf.Floor(score*multiplier);
		scoreUpdateSpeed = 0.5f*(currentHighscore - visibleHighscore);
		Debug.Log (score * multiplier + " points for killing enemy");
	}

	public void lootDropped(float score)
	{
		// TODO Display for what the points are
		currentHighscore += Mathf.Floor(score*multiplier);
		scoreUpdateSpeed = 0.5f*(currentHighscore - visibleHighscore);
	}

	public void playerIsGrowing(float score)
	{
		// TODO Display for what the points are
		currentHighscore += Mathf.Floor(score*multiplier);
		visibleHighscore += Mathf.Floor(score*multiplier);
	}
}
