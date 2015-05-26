using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class leaderboardInterface : MonoBehaviour
{
	public InputField nameInput;

	Leaderboard leaderboard;

	void Awake()
	{
		// Create leaderboard
		leaderboard = new Leaderboard();
	}
	
	public void PostScore(long score)
	{
		// Post a new score
		leaderboard.PostScore(new Score(nameInput.text, score));
	}
	
	public void LogScores()
	{
		Debug.Log ("===HIGHSCORES===");
		
		// Show scores
		foreach (var scores in leaderboard.GetScores())
		{
			Debug.Log(scores.ToString());
		}
	}
}