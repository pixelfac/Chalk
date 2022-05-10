using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalkMeter : MonoBehaviour
{
    [SerializeField] RectTransform meterRectTransform;
	[SerializeField] ChalkMeterSO chalkMeterSO;

	private void Update()
	{
		//update height if chalkMeter
		meterRectTransform.sizeDelta = new Vector2(meterRectTransform.rect.width, HeightFromCurrChalk());
		chalkMeterSO.RegenChalk();
	}

	//calculate Height of chalkMeter UI element from currChalk remaining
	public float HeightFromCurrChalk()
	{
		//the chalkMeter UI element is 400 px tall
		//get % max chalk: currChalk / maxChalk
		//scale by 400 px
		//Debug.Log("currChalk is: " + currChalk);
		return (chalkMeterSO.currChalk / chalkMeterSO.maxChalk) * 400;
	}
}
