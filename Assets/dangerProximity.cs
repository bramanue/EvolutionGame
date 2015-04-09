using UnityEngine;
using System.Collections;

public class dangerProximity {

	public GameObject structure;

	public EAbilityType requiredAbility;

	public int nofSamples = 3;

	public float[] distances = new float[3];

	public Vector3[] normals = new Vector3[3];

	public Vector3[] directions = new Vector3[3];

	public dangerProximity()
	{

	}

	public Vector3 getClosestDirection() 
	{
		int closestIndex = 0;
		float closestDistance = distances [0];
		for (int i = 1; i < nofSamples; i++) {
			if(distances[i] < closestDistance) {
				closestDistance = distances[i];
				closestIndex = i;
			}
		}		return directions[closestIndex];
	}

	public Vector3 getSafestDirection()
	{
		int furthestIndex = 0;
		float furthestDistance = distances [0];
		Vector3 normal = normals [0];
		int alternativeIndex = -1;
		for (int i = 1; i < nofSamples; i++) {
			if(distances[i] > furthestDistance) {
				furthestDistance = distances[i];
				furthestIndex = i;
			}
			else if (distances[i] == furthestDistance)
			{
				alternativeIndex = i;
			}
			else
			{
				normal = normals[i];
			}
		}

		// If there were 2 directions pointing to a safe place, take the one that rather points in direction of the obstacle normal
		if (alternativeIndex != -1 && normal != new Vector3 (0, 0, 0)) {
			if ((directions [alternativeIndex] - normal).magnitude < (directions [furthestIndex] - normal).magnitude)
				return directions [alternativeIndex];
			else
				return directions [furthestIndex];
		}
		else
			return directions[furthestIndex];
	}

}
