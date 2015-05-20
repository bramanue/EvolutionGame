using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class audioManager : MonoBehaviour {

	Dictionary<string,AudioSource> soundLibrary = new Dictionary<string,AudioSource>();

	public AudioClip hurtSound; //assign clips from inspector
	public AudioClip lootSound;
	private AudioSource audio;

	// Use this for initialization
	void Start () 
	{
		audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void PlayhurtSound()
	{
		audio.PlayOneShot(hurtSound);
	}

	public void PlaylootSound()
	{
		audio.PlayOneShot(lootSound);
	}

//	public bool playSound(string soundName)
//	{
//
//		audio.PlayOneShot(soundName.toA);
//
////		AudioSource sound;
////		soundLibrary.TryGetValue(soundName, out sound);
////		if (sound == null)
////			return false;
////		else {
////		//	sound.Play(sound.clip);
////			return true;
////		}
//
//	}
}
