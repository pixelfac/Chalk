using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Draw : MonoBehaviour
{
    Controls controls;
	[SerializeField] bool isDrawing = false;
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

	private void StopDrawing(InputAction.CallbackContext ctx)
	{
		Debug.Log("Stopped Drawing");
		isDrawing = false;
		ChalkLine chalkline = lineObject.GetComponent<ChalkLine>();
		lineBuilder.BuildChalkLine(chalkline);
	}
}

