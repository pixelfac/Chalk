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
    [SerializeField] private int _chalkRegenSpeed;
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

	public void RegenChalk()
	{
		currChalk += _chalkRegenSpeed;
		if (currChalk > maxChalk)
		{
			currChalk = maxChalk;
		}
	}

	//sets chalk to max
	//used at start of round
	public void ResetChalk()
	{
		currChalk = maxChalk;
	}
}
