using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class audioManager : MonoBehaviour {

	Dictionary<string,AudioSource> soundLibrary = new Dictionary<string,AudioSource>();

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public bool playSound(string soundName)
	{
		AudioSource sound;
		soundLibrary.TryGetValue(soundName, out sound);
		if (sound == null)
			return false;
		else {
		//	sound.Play(sound.clip);
			return true;
		}

	}
}
