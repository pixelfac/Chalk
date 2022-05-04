using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalkMeter : MonoBehaviour
{
    [SerializeField] RectTransform chalkMeter;
    [SerializeField] float maxChalk;
    [SerializeField] int chalkRegenSpeed;
	static bool regenEnabled;

	public static float currChalk { get; private set; }

	private void Awake()
	{
		currChalk = maxChalk;
		regenEnabled = true;
	}

	private void Update()
	{
		//update height if chalkMeter
		chalkMeter.sizeDelta = new Vector2(chalkMeter.rect.width, HeightFromCurrChalk());
		if (regenEnabled)
		{
			RegenChalk();
		}
	}

	//calculate Height of chalkMeter UI element from currChalk remaining
	private float HeightFromCurrChalk()
	{
		//the chalkMeter UI element is 400 px tall
		//get % max chalk: currChalk / maxChalk
		//scale by 400 px
			//Debug.Log("currChalk is: " + currChalk);
		return (currChalk / maxChalk) * 400;
	}

	//when drawing, depletes chalk
	public static void UseChalk()
	{
		currChalk -= 1;
		if (currChalk < 0)
		{
			currChalk = 0;
		}
	}

	public void RegenChalk()
	{
		currChalk += chalkRegenSpeed;
		if (currChalk > maxChalk)
		{
			currChalk = maxChalk;
		}
	}

	public static void EnableChalkRegen()
	{
		regenEnabled = true;
	}

	public static void DisableChalkRegen()
	{
		regenEnabled = false;
	}

}
