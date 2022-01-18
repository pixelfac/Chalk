using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Draw : MonoBehaviour
{
    Controls controls;
	[SerializeField] bool isDrawing = false;
	[Range(0.01f,1f)]
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
		controls.Draw.Draw.performed += StartDrawing;
		controls.Draw.Draw.canceled += StopDrawing;
		controls.Draw.Draw.Enable();
	}

	public void OnDisable()
	{
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

	//Tied to event listener for input
	private void StopDrawing(InputAction.CallbackContext ctx)
	{
		//if already stopped drawing, do nothing
		if (!isDrawing) { return; }

		Debug.Log("Stopped Drawing");
		isDrawing = false;
		ChalkLine chalkline = lineObject.GetComponent<ChalkLine>();
		lineBuilder.BuildChalkLine(chalkline, false);
		Debug.Log("NOT enclosed");

	}

	//Not tied to input
	private void StopDrawing()
	{
		Debug.Log("Stopped Drawing");
		isDrawing = false;
		lineBuilder.CloseLine();
		ChalkLine chalkline = lineObject.GetComponent<ChalkLine>();
		lineBuilder.BuildChalkLine(chalkline, true);
		Debug.Log("Enclosed");
	}
}

