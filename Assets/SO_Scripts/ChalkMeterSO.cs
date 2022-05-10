using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Scriptable Object is meant to be a Singleton. Do not create another SO asset!
/// </summary>

[CreateAssetMenu(fileName = "ChalkMeterSO", menuName = "")]
public class ChalkMeterSO : ScriptableObject
{
    [SerializeField] public float maxChalk;
    [SerializeField] int chalkRegenSpeed;
	bool regenEnabled = true;
    public float currChalk;
	
	//when drawing, depletes chalk
	public void UseChalk()
	{
		currChalk -= 1;
		if (currChalk < 0)
		{
			currChalk = 0;
		}
	}

	//regens chalk if regen is enabled
	public void RegenChalk()
	{
		if (!regenEnabled) { return; }

		currChalk += chalkRegenSpeed;
		if (currChalk > maxChalk)
		{
			currChalk = maxChalk;
		}
	}

	public void EnableChalkRegen()
	{
		regenEnabled = true;
	}

	public void DisableChalkRegen()
	{
		regenEnabled = false;
	}
}
