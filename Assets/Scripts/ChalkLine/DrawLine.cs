using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace ChalkLine
{
	public class DrawLine : MonoBehaviour
	{
		private Controls controls;
		[SerializeField] private bool isDrawing = false;

		[Range(0.1f, 1f)] //arbitrary values, may need to adjust minNodesInLine if you change this one
		[SerializeField] private float maxNodeDistance;
		[SerializeField] private float minNodesInLine;

		[SerializeField] private GameObject chalkLinePrefab;
		[SerializeField] private GameObject startLineTargetPrefab; //prefab for target on start of line while drawing

		[SerializeField] private ChalkMeterSO chalkMeterSO;

		private GameObject startLineTarget;

		private GameObject lineObject;
		private List<Vector2> nodePositions;
		private LineRenderer lr;
		private GameObject startCircle, endCircle;


		private void Awake()
		{
			controls = new Controls();
			chalkMeterSO.ResetChalk();
		}

		private void FixedUpdate()
		{
			if (isDrawing)
			{
				UpdateLine();

				if (isEnclosed())
				{
					FinishLine();
				}

				if (chalkMeterSO.currChalk <= 0)
				{
					StopDrawing();
				}
			}
			else
			{
				chalkMeterSO.RegenChalk();
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
			if (chalkMeterSO.currChalk == 0)
			{
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
				AbortLine();
				return;
			}

			ChalkLine chalkline = lineObject.GetComponent<ChalkLine>();
			BuildChalkLine(chalkline, true);
			Debug.Log("Line Initiallized ENCLOSED");
		}

		//Create and set up new line object
		private void CreateLine()
		{
			Debug.Log("Creating Line");

			//Make the actual Line object
			lineObject = Instantiate(chalkLinePrefab, Vector3.zero, Quaternion.identity);

			//set internal lineNode List
			nodePositions = new List<Vector2>();
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			nodePositions.Add(mousePos);
			nodePositions.Add(mousePos);

			//assign and set lineRenderer
			lr = lineObject.GetComponent<LineRenderer>();
			lr.useWorldSpace = true;
			lr.SetPosition(0, nodePositions[0]);
			lr.SetPosition(1, nodePositions[1]);

			//init and set startLineTarget
			startLineTarget = Instantiate(startLineTargetPrefab, Vector3.zero, Quaternion.identity);
			startLineTarget.SetActive(true);
			startLineTarget.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

			//assign and set start/end circles
			//start stays at the start
			startCircle = lineObject.GetComponent<ChalkLine>().startCircle;
			startCircle.transform.position = nodePositions[0];
			//end starts at start
			endCircle = lineObject.GetComponent<ChalkLine>().endCircle;
			endCircle.transform.position = nodePositions[0];
		}

		//update the length of the line
		private void UpdateLine()
		{
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

			//while mouse is far enough from line, add node to line
			while (Vector2.Distance(nodePositions[nodePositions.Count - 1], mousePos) >= maxNodeDistance)
			{
				//unit-vector pointing from last line segment to mousePos
				Vector2 direction = (mousePos - nodePositions[nodePositions.Count - 1]).normalized;
				//Vector of maxNodeDistance in length pointing towards mousePos
				Vector2 nextSegment = nodePositions[nodePositions.Count - 1] + direction * maxNodeDistance;

				//add new node + LineRenderer segment
				nodePositions.Add(nextSegment);
				lr.positionCount++;
				lr.SetPosition(lr.positionCount - 1, nextSegment);
				endCircle.transform.position = nextSegment;
				chalkMeterSO.UseChalk();
			}
		}

		//checks to see if the ends of line drawn meet to enclose a shape
		private bool isEnclosed()
		{
			//prevent line from being enclosed immediately when start drawing
			if (TooShort()) { return false; }

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
		private bool TooShort()
		{
			if (nodePositions.Count < minNodesInLine)
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
		private void CloseLine()
		{
			lr.loop = true;
			endCircle.transform.position = nodePositions[0];
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

		private void OnValidate()
		{
			minNodesInLine = Mathf.Max(5, minNodesInLine);
		}
	}
}