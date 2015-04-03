using UnityEngine;
using System.Collections;

public class biteAbility : ability {
	

	
	public override bool useAbility() 
	{
		// TODO Cast effects
		if (cooldownTimer <= 0.0f) {
			// TODO use ability
			
			cooldownTimer = 1.0f;
			return true;
		} 
		else 
		{
			return false;
		}
	}

	public override int increaseLevel(int x)
	{
		int previousLevel = level;
		level = Mathf.Max(level + x, maxLevel);
		return level - previousLevel;
	}
}