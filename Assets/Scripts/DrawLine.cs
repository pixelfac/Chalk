using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DrawLine : MonoBehaviour
{
    Controls controls;
	[SerializeField] bool isDrawing = false;

	[Range(0.005f,0.1f)]
	[SerializeField] float maxNodeDistance;

	[SerializeField] GameObject chalkLinePrefab;
	[SerializeField] GameObject startLineTargetPrefab; //prefab for target on start of line while drawing

	GameObject startLineTarget;
	GameObject lineObject;
	List<Vector2> nodePositions;
	LineRenderer lr;


	private void Awake()
	{
		controls = new Controls();
	}

	private void Update()
	{
		if (isDrawing)
		{
			ChalkMeter.DisableChalkRegen();
			UpdateLine();

			if (isEnclosed())
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

		CreateLine();

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
		if (TooShort())
		{
			Destroy(lineObject);
			AbortLine();
			return;
		}

		ChalkLine chalkline = lineObject.GetComponent<ChalkLine>();
		BuildChalkLine(chalkline, false);
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
		CloseLine();

		if (TooShort())
		{
			Destroy(lineObject);
			return;
		}

		ChalkLine chalkline = lineObject.GetComponent<ChalkLine>();
		BuildChalkLine(chalkline, true);
		Debug.Log("Line Initiallized ENCLOSED");
	}

	//Create and set up new line object
	public void CreateLine()
	{
		Debug.Log("Creating Line");

		//Make the actual Line object
		lineObject = Instantiate(chalkLinePrefab, Vector3.zero, Quaternion.identity);

		nodePositions = new List<Vector2>();
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		nodePositions.Add(mousePos);
		nodePositions.Add(mousePos);

		lr = lineObject.GetComponent<LineRenderer>();
		lr.SetPosition(0, nodePositions[0]);
		lr.SetPosition(1, nodePositions[1]);

		//enable and set startLineTarget
		startLineTarget = Instantiate(startLineTargetPrefab, Vector3.zero, Quaternion.identity);
		startLineTarget.SetActive(true);
		startLineTarget.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
	}

	//update the length of the line
	public void UpdateLine()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

		//if mouse has not moved far enough from last node, don't add node to line
		if (Vector2.Distance(nodePositions[nodePositions.Count - 1], mousePos) < maxNodeDistance)
		{
			return;
		}

		//add new node + LineRenderer segment
		nodePositions.Add(mousePos);
		lr.positionCount++;
		lr.SetPosition(lr.positionCount - 1, mousePos);
		ChalkMeter.UseChalk();
	}

	//checks to see if the ends of line drawn meet to enclose a shape
	public bool isEnclosed()
	{
		//prevent line from being enclosed immediately when start drawing
		if (nodePositions.Count < 3) { return false; }

		//distance between first and last node
		float distanceBetween = (nodePositions[0] - nodePositions[nodePositions.Count - 1]).magnitude;

		if (distanceBetween < maxNodeDistance)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	//true if # of nodes is too short to save, false otherwise
	public bool TooShort()
	{
		if (nodePositions.Count < 10)
		{
			Debug.Log("TOO short");
			return true;
		}
		else
		{
			return false;
		}
	}

	//closes start and end of line renderer
	public void CloseLine()
	{
		lr.loop = true;
	}

	//called when drawing is finished, but line is too short so is deleted
	//deleting the actual line object happens in Draw.cs
	public void AbortLine()
	{
		Debug.Log("Abort Line");
		ResetLineVars();
	}

	//initialize lineObject
	public void BuildChalkLine(ChalkLine line, bool isEnclosed)
	{
		line.Init(ref nodePositions, isEnclosed);
		ResetLineVars();
	}

	//resets local line variables to prevent bugs
	private void ResetLineVars()
	{
		Destroy(startLineTarget);
		lr = null;
		nodePositions = null;
		lineObject = null;
	}
}

