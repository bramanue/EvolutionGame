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
	{ }

	public Vector3 getClosestDirection() 
	{
		int closestIndex = 0;
		float closestDistance = distances [0];
		for (int i = 1; i < nofSamples; i++) {
			if(distances[i] < closestDistance) {
				closestDistance = distances[i];
				closestIndex = i;
			}
		}		
		return directions[closestIndex];
	}

	// Get the vector in which direction there most probably is no dangerous structure
	public Vector3 getSafestDirection()
	{
		// The index where the safest direction is stored
		int furthestIndex = 0;
		// The distance to the dangerous object in the safest direction
		float furthestDistance = distances [0];
		// The shortest distance to the dangerous along all directions
		float closestDistance = distances [0];
		// The normal of the dangerous structure at the closest intersection
		Vector3 normal = normals [0];
		// Maybe there are 2 directions, that have infinite distance to the next dangerous environment - store it as well
		int alternativeIndex = -1;

		int nofAlternatives = 0;

		for (int i = 1; i < nofSamples; i++) {
			// Check whether this direction is safer
			if(distances[i] > furthestDistance) {
				furthestDistance = distances[i];
				furthestIndex = i;
			}
			// Check whether it is equally save
			else if (distances[i] == furthestDistance)
			{
				nofAlternatives++;
				alternativeIndex = i;
			}
			// In case there are several safest direction, also store the normal of the object at the closest intersection
			else
			{
				// TODO Comparison not necessary in case of only 3 rays
				if(distances[i] < closestDistance) {
					closestDistance = distances[i];
					normal = normals[i];
				}
			}
		}

		// If there were 2 directions pointing to a safe place, take the one that rather points in direction of the obstacle normal
		if (alternativeIndex != -1 && normal != new Vector3 (0, 0, 0)) {
			if ((directions [alternativeIndex] - normal).magnitude < (directions [furthestIndex] - normal).magnitude)
				return directions [alternativeIndex];
			else
				return directions [furthestIndex];
		} 
		// If no direction showed a danger, then move straight on
		else if (nofAlternatives == nofSamples - 1) 
		{ 
			return directions [((int)nofSamples/2)];
		}
		else
			return directions[furthestIndex];
	}

}
