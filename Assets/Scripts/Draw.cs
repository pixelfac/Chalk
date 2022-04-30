using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Draw : MonoBehaviour
{
    Controls controls;
	[SerializeField] bool isDrawing = false;

	[Range(0.005f,0.1f)]
	[SerializeField] float maxNodeDistance;

	[SerializeField] GameObject chalkLinePrefab;
	LineBuilder lineBuilder;
	GameObject lineObject;

	private void Awake()
	{
		controls = new Controls();
	}

	private void Update()
	{
		if (isDrawing)
		{
			lineBuilder.UpdateLine();
			if (lineBuilder.isEnclosed())
			{
				StopDrawing();
			}
		}
	}

	public void OnEnable()
	{
		//add Start/StopDrawing coroutines to control scheme
		controls.Draw.Draw.performed += StartDrawing;
		controls.Draw.Draw.canceled += StopDrawing;
		controls.Draw.Draw.Enable();
	}

	public void OnDisable()
	{
		//remove Start/StopDrawing coroutines to control scheme
		controls.Draw.Draw.performed -= StartDrawing;
		controls.Draw.Draw.canceled -= StopDrawing;
		controls.Draw.Draw.Disable();
	}

	private void StartDrawing(InputAction.CallbackContext ctx)
	{
		Debug.Log("Started Drawing");

		lineObject = Instantiate(chalkLinePrefab, Vector3.zero, Quaternion.identity);

		lineBuilder = new LineBuilder(maxNodeDistance, lineObject.GetComponent<LineRenderer>());
		lineBuilder.CreateLine();

		isDrawing = true;
	}

	//Manual Stop Drawing
	//Tied to event listener for input
	private void StopDrawing(InputAction.CallbackContext ctx)
	{
		//if already stopped drawing, do nothing
		if (!isDrawing) { return; }

		Debug.Log("Stopped Drawing");
		isDrawing = false;

		//if line is too short, discard line
		if (lineBuilder.TooShort())
		{
			Destroy(lineObject);
			return;
		}

		ChalkLine chalkline = lineObject.GetComponent<ChalkLine>();
		lineBuilder.BuildChalkLine(chalkline, false);
		Debug.Log("Line Initiallized NOT ENCLOSED");
	}

	//Automatically Stop Drawing
	//Not tied to input
	private void StopDrawing()
	{
		Debug.Log("Stopped Drawing");
		isDrawing = false;
		lineBuilder.CloseLine();

		if (lineBuilder.TooShort())
		{
			Destroy(lineObject);
			return;
		}

		ChalkLine chalkline = lineObject.GetComponent<ChalkLine>();
		lineBuilder.BuildChalkLine(chalkline, true);
		Debug.Log("Line Initiallized ENCLOSED");
	}
}

