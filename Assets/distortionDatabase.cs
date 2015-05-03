using UnityEngine;
using System.Collections;

public enum EDistortionType {
	ENoDistortion,
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

	private float[] shieldDistortionIntensity = new float[7];

	private float[] shieldWobbleFrequencies = new float[7];
	
	private float[] shieldWobbleIntensities = new float[7];
	
	private float[] shieldTimeMultipliers = new float[7];

	// Use this for initialization
	void Start () 
	{
		// ICE SHIELD
		shieldDistortionIntensity [0] = 0.3f;
		shieldWobbleFrequencies [0] = 2.0f;
		shieldWobbleIntensities [0] = 0.3f;
		shieldTimeMultipliers [0] = 0.3f;

		// LAVA SHIELD
		shieldDistortionIntensity [1] = 0.1f;
		shieldWobbleFrequencies [1] = 0.9f;
		shieldWobbleIntensities [1] = 0.6f;
		shieldTimeMultipliers[1] = 0.8f;

		// DUST SHIELD
		shieldDistortionIntensity [2] = 0.7f;
		shieldWobbleFrequencies [2] = 0.7f;
		shieldWobbleIntensities [2] = 0.4f;
		shieldTimeMultipliers[2] = 0.1f;

		// THORN SHIELD
		shieldDistortionIntensity [3] = 0.0f;
		shieldWobbleFrequencies [3] = 6.0f;
		shieldWobbleIntensities [3] = 2.4f;
		shieldTimeMultipliers[3] = 1.0f;

		// WATER SHIELD
		shieldDistortionIntensity [4] = 0.0f;
		shieldWobbleFrequencies [4] = 0.9f;
		shieldWobbleIntensities [4] = 0.2f;
		shieldTimeMultipliers[4] = 1.1f;

		// GLOWING SHIELD
		shieldDistortionIntensity [5] = 0.0f;
		shieldWobbleFrequencies [5] = 0.95f;
		shieldWobbleIntensities [5] = 0.05f;
		shieldTimeMultipliers[5] = 2.0f;

		// ELECTRICITY SHIELD
		shieldDistortionIntensity [6] = 0.5f;
		shieldWobbleFrequencies [6] = 10.0f;
		shieldWobbleIntensities [6] = 2.0f;
		shieldTimeMultipliers[6] = 8.0f;
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
