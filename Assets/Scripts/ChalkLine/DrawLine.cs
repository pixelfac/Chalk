using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Pathfinding;

namespace ChalkLine
{
	public class DrawLine : MonoBehaviour
	{
		[SerializeField] private bool _isDrawing = false;

		[Range(0.1f, 1f)] //arbitrary values, may need to adjust minNodesInLine if you change this one
		[SerializeField] private float _maxNodeDistance;
		[SerializeField] private float _minNodesInLine;

		[SerializeField] private GameObject _chalkLinePrefab;
		[SerializeField] private GameObject _startLineTargetPrefab; //prefab for target on start of line while drawing

		[SerializeField] private ChalkMeterSO _chalkMeterSO;

		private Controls _controls;

		private GameObject _startLineTarget;

		private GameObject _lineObject;
		private List<Vector2> _nodePositions;
		private LineRenderer _lr;
		private GameObject _startCircle, _endCircle;
		private Grid2D _grid;

		private void Awake()
		{
			_controls = new Controls();
			_chalkMeterSO.ResetChalk();
			_grid = FindObjectOfType<Grid2D>();
		}

		private void FixedUpdate()
		{
			if (_isDrawing)
			{
				UpdateLine();

				if (isEnclosed())
				{
					FinishLine();
				}

				if (_chalkMeterSO.currChalk <= 0)
				{
					StopDrawing();
				}
			}
			else
			{
				_chalkMeterSO.RegenChalk();
			}
		}

		public void OnEnable()
		{
			//add Start/StopDrawing coroutines to control scheme
			_controls.Draw.Draw.performed += StartDrawing;
			_controls.Draw.Draw.canceled += StopDrawing;
			_controls.Draw.Draw.Enable();
		}

		public void OnDisable()
		{
			//remove Start/StopDrawing coroutines to control scheme
			_controls.Draw.Draw.performed -= StartDrawing;
			_controls.Draw.Draw.canceled -= StopDrawing;
			_controls.Draw.Draw.Disable();
		}

		private void StartDrawing(InputAction.CallbackContext ctx)
		{
			//Check if can start drawing
			if (_chalkMeterSO.currChalk == 0)
			{
				Debug.Log("Can't Start Draw: No Chalk");
				return;
			}

			Debug.Log("Started Drawing");

			CreateLine();

			_isDrawing = true;
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
			if (!_isDrawing) { return; }

			Debug.Log("Stopped Drawing");
			_isDrawing = false;

			//if line is too short, discard line
			if (TooShort())
			{
				Destroy(_lineObject);
				AbortLine();
				return;
			}

			BuildChalkLine(false);
		}

		//Automatically Stop Drawing
		//Not tied to input
		//ONLY Triggers when line is enclosed
		private void FinishLine()
		{
			//if already stopped drawing, do nothing
			if (!_isDrawing) { return; }

			Debug.Log("Finished Line");
			_isDrawing = false;
			CloseLine();

			if (TooShort())
			{
				Destroy(_lineObject);
				AbortLine();
				return;
			}

			BuildChalkLine(true);
		}

		//initialize lineObject
		public void BuildChalkLine(bool isEnclosed)
		{
			ChalkLine chalkline = _lineObject.GetComponent<ChalkLine>();
			chalkline.Init(_nodePositions, _grid, isEnclosed);
			ResetLineVars();
		}

		//Create and set up new line object
		private void CreateLine()
		{
			Debug.Log("Creating Line");

			//Make the actual Line object
			_lineObject = Instantiate(_chalkLinePrefab, Vector3.zero, Quaternion.identity);

			//set internal lineNode List
			_nodePositions = new List<Vector2>();
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			_nodePositions.Add(mousePos);
			_nodePositions.Add(mousePos);

			//assign and set lineRenderer
			_lr = _lineObject.GetComponent<LineRenderer>();
			_lr.useWorldSpace = true;
			_lr.SetPosition(0, _nodePositions[0]);
			_lr.SetPosition(1, _nodePositions[1]);

			//init and set startLineTarget
			_startLineTarget = Instantiate(_startLineTargetPrefab, Vector3.zero, Quaternion.identity);
			_startLineTarget.SetActive(true);
			_startLineTarget.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

			//assign and set start/end circles
			//start stays at the start
			_startCircle = _lineObject.GetComponent<ChalkLine>().startCircle;
			_startCircle.transform.position = _nodePositions[0];
			//end starts at start
			_endCircle = _lineObject.GetComponent<ChalkLine>().endCircle;
			_endCircle.transform.position = _nodePositions[0];
		}

		//update the length of the line
		private void UpdateLine()
		{
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

			//while mouse is far enough from line, add node to line
			while (Vector2.Distance(_nodePositions[_nodePositions.Count - 1], mousePos) >= _maxNodeDistance)
			{
				//unit-vector pointing from last line segment to mousePos
				Vector2 direction = (mousePos - _nodePositions[_nodePositions.Count - 1]).normalized;
				//Vector of maxNodeDistance in length pointing towards mousePos
				Vector2 nextSegment = _nodePositions[_nodePositions.Count - 1] + direction * _maxNodeDistance;

				//add new node + LineRenderer segment
				_nodePositions.Add(nextSegment);
				_lr.positionCount++;
				_lr.SetPosition(_lr.positionCount - 1, nextSegment);
				_endCircle.transform.position = nextSegment;
				_chalkMeterSO.UseChalk();
			}
		}

		//checks to see if the ends of line drawn meet to enclose a shape
		private bool isEnclosed()
		{
			//prevent line from being enclosed immediately when start drawing
			if (TooShort()) { return false; }

			//distance between first and last node
			float distanceBetween = (_nodePositions[0] - _nodePositions[_nodePositions.Count - 1]).magnitude;

			if (distanceBetween < _maxNodeDistance)
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
			if (_nodePositions.Count < _minNodesInLine)
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
			_lr.loop = true;
			_endCircle.transform.position = _nodePositions[0];
		}

		//called when drawing is finished, but line is too short so is deleted
		//deleting the actual line object happens in Draw.cs
		public void AbortLine()
		{
			Debug.Log("Abort Line");
			ResetLineVars();
		}

		//resets local line variables to prevent bugs
		private void ResetLineVars()
		{
			Destroy(_startLineTarget);
			_lr = null;
			_nodePositions = null;
			_lineObject = null;
		}

		private void OnValidate()
		{
			_minNodesInLine = Mathf.Max(5, _minNodesInLine);
		}
	}
}