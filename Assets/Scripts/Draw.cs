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
	[SerializeField] GameObject startLineTarget; //prefab for target on start of line while drawing
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
			ChalkMeter.DisableChalkRegen();
			lineBuilder.UpdateLine();

			if (lineBuilder.isEnclosed())
			{
				FinishLine();
			}

			if (ChalkMeter.currChalk <= 0)
			{
				StopDrawing();
			}
		}
		else
		{
			ChalkMeter.EnableChalkRegen();
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
		//Check if can start drawing
		if (ChalkMeter.currChalk == 0) {
			Debug.Log("Can't Start Draw: No Chalk");
			return;
		}

		Debug.Log("Started Drawing");

		//Make the actual Line object
		lineObject = Instantiate(chalkLinePrefab, Vector3.zero, Quaternion.identity);

		//Make LineBuilder obj and start linebuilding process
		lineBuilder = ScriptableObject.CreateInstance<LineBuilder>();
		lineBuilder.Init(maxNodeDistance, lineObject.GetComponent<LineRenderer>(), startLineTarget);
		lineBuilder.CreateLine();

		isDrawing = true;
	}

	//Manual Stop Drawing
	//Tied to event listener for input
	private void StopDrawing(InputAction.CallbackContext ctx)
	{
		StopDrawing();
	}


	//Automatically Stop Drawing
	//Not tied to input
	//ONLY Triggers when chalk runs out
	private void StopDrawing()
	{
		//if already stopped drawing, do nothing
		if (!isDrawing) { return; }

		Debug.Log("Stopped Drawing");
		isDrawing = false;

		//if line is too short, discard line
		if (lineBuilder.TooShort())
		{
			Destroy(lineObject);
			lineBuilder.AbortLine();
			return;
		}

		ChalkLine chalkline = lineObject.GetComponent<ChalkLine>();
		lineBuilder.BuildChalkLine(chalkline, false);
		Debug.Log("Line Initiallized NOT ENCLOSED");
	}

	//Automatically Stop Drawing
	//Not tied to input
	//ONLY Triggers when line is enclosed
	private void FinishLine()
	{
		//if already stopped drawing, do nothing
		if (!isDrawing) { return; }

		Debug.Log("Finished Line");
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

