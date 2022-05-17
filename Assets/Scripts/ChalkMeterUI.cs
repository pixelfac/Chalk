using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalkMeterUI : MonoBehaviour
{
    [SerializeField] private RectTransform _meterRectTransform;
	[SerializeField] private ChalkMeterSO _chalkMeterSO;

	private void Update()
	{
		//update height if chalkMeter
		_meterRectTransform.sizeDelta = new Vector2(_meterRectTransform.rect.width, HeightFromCurrChalk());
	}

	//calculate Height of chalkMeter UI element from currChalk remaining
	public float HeightFromCurrChalk()
	{
		//the chalkMeter UI element is 400 px tall
		//get % max chalk: currChalk / maxChalk
		//scale by 400 px
		//Debug.Log("currChalk is: " + currChalk);
		return (_chalkMeterSO.currChalk / _chalkMeterSO.maxChalk) * 400;
	}
}
