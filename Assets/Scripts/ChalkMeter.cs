using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChalkMeter : MonoBehaviour
{
    [SerializeField] RectTransform chalkMeter;
    [SerializeField] float maxChalk;
    [SerializeField] int chalkRegenSpeed;
    float currChalk;
	Controls controls;

	private void Awake()
	{
		currChalk = maxChalk;
		controls = new Controls();
	}

	public void OnEnable()
	{
		//add Start/StopDrawing coroutines to control scheme
		controls.Draw.Draw.performed += UseChalk;
		controls.Draw.Draw.Enable();
	}

	public void OnDisable()
	{
		//remove Start/StopDrawing coroutines to control scheme
		controls.Draw.Draw.performed -= UseChalk;
		controls.Draw.Draw.Disable();
	}

	private void Update()
	{
		//update height if chalkMeter
		chalkMeter.sizeDelta = new Vector2(chalkMeter.rect.width, HeightFromCurrChalk());
		RegenChalk();
	}

	//calculate Height of chalkMeter UI element from currChalk remaining
	private float HeightFromCurrChalk()
	{
		//the chalkMeter UI element is 400 px tall
		//get % max chalk: currChalk / maxChalk
		//scale by 400 px
		Debug.Log("currChalk is: " + currChalk);
		return (currChalk / maxChalk) * 400;
	}

	//when drawing, depletes chalk
	public void UseChalk(InputAction.CallbackContext ctx)
	{
		Debug.Log("using chalk");
		
		currChalk -= 100;
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

}
