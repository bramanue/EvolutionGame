using UnityEngine;
using System.Collections;

public enum EDistortionType {
	// Shield abilities
	EIceShieldDistortion,
	ELavaShieldDistortion,
	EDustShieldDistortion,
	EThornShieldDistortion,
	EWaterShieldDistortion,
	EGlowingShieldDistortion,
	EElectricityShieldDistortion
}

public class distortionDatabase : MonoBehaviour  {

	private float[] shieldWobbleFrequencies = new float[7];
	
	private float[] shieldWobbleIntensities = new float[7];
	
	private float[] shieldTimeMultipliers = new float[7];

	// Use this for initialization
	void Start () 
	{
		// ICE SHIELD
		shieldWobbleFrequencies [0] = 0.9f;
		shieldWobbleIntensities [0] = 0.1f;
		shieldTimeMultipliers [0] = 1;

		// LAVA SHIELD
		shieldWobbleFrequencies [1] = 0.9f;
		shieldWobbleIntensities [1] = 0.1f;
		shieldTimeMultipliers[1] = 1;

		// DUST SHIELD
		shieldWobbleFrequencies [2] = 0.8f;
		shieldWobbleIntensities [2] = 0.1f;
		shieldTimeMultipliers[2] = 1;

		// THORN SHIELD
		shieldWobbleFrequencies [2] = 0.8f;
		shieldWobbleIntensities [2] = 0.1f;
		shieldTimeMultipliers[2] = 1;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void getShieldDistortionParameters(EDistortionType distortionType, ref float frequency, ref float intensity, ref float timeMultiplier) 
	{
		int index = -1;
		switch (distortionType) {
		case (EDistortionType.EIceShieldDistortion) :
			index = 0;
			break;
		case (EDistortionType.ELavaShieldDistortion) :
			index = 1;
			break;
		case (EDistortionType.EDustShieldDistortion) :
			index = 2;
			break;
		case (EDistortionType.EThornShieldDistortion) :
			index = 3;
			break;
		case (EDistortionType.EWaterShieldDistortion) :
			index = 4;
			break;
		case (EDistortionType.EGlowingShieldDistortion) :
			index = 5;
			break;
		case (EDistortionType.EElectricityShieldDistortion) :
			index = 6;
			break;
		default:
			break;
		};

		if (index != -1) {
			frequency = shieldWobbleFrequencies[index];
			intensity = shieldWobbleIntensities[index];
			timeMultiplier = shieldTimeMultipliers[index];
		}
	}
}
