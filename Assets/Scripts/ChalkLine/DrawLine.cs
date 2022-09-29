using Pathfinding;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChalkLine
{
	public enum LineType { WARD, MISSILE };

	public class DrawLine : MonoBehaviour
	{
		[SerializeField] private bool _isDrawing = false;

		[Range(0.1f, 1f)] //arbitrary values, may need to adjust minNodesInLine if you change this one
		[SerializeField] private float _maxNodeDistance;
		[Range(0.1f, 1f)] //arbitrary values, may need to adjust minNodesInLine if you change this one
		[SerializeField] private float _maxEncloseDistance; //the farthest apart ends of a line can be to be considered enclosed
		[SerializeField] private float _minNodesInLine;
		[Range(0f, 1f)]
		[SerializeField] private float straightnessThreshold; //how straight for line to be considered LineMissile
		[SerializeField] private int nodeReduceFactor; //factor by which to reduce node density in line


		[SerializeField] private GameObject _lineWardPrefab;
		[SerializeField] private GameObject _lineMissilePrefab;
		[SerializeField] private GameObject _startLineTargetPrefab; //prefab for target on start of line while drawing

		[SerializeField] private ChalkMeterSO _chalkMeterSO;

		private Controls _controls;

		private GameObject _startLineTarget;

		private GameObject _lineDrawing; //dummy line that will be replaced with real chalkline of some kind after finished drawing
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

				//if line goes over goal node (preventing pathfinding entirely) abort line
				if (_grid.PosOnGoalNode(_nodePositions[_nodePositions.Count - 1]))
				{
					AbortLine();
					return;
				}

				if (isEnclosed())
				{
					FinishLine();
				}

				if (IfStopDrawing())
				{
					Debug.LogWarning("Forcefully Stop Drawing line");
					StopDrawing();
				}
			}
			else
			{
				_chalkMeterSO.RegenChalk();
			}

			//return true if conditions are met to forcefully stop drawing line
			bool IfStopDrawing()
			{
				if (_chalkMeterSO.currChalk <= 0) { return true; }

				if (LineOutOfBounds()) { return true; }

				return false;

				//return true if line goes too close to the boundary
				bool LineOutOfBounds()
				{
					if (_nodePositions == null) { return false; }
					
					Vector2 currentLineNodePos = _nodePositions[_nodePositions.Count - 1];
					Vector2 gridSize = _grid.GetGridWorldSize();

					float leftBound =	-(gridSize.x / 2) + _grid.nodeRadius * 8;
					float rightBound =	 (gridSize.x / 2) - _grid.nodeRadius * 8;
					float topBound =	 (gridSize.y / 2) - _grid.nodeRadius * 8;
					float bottomBound = -(gridSize.y / 2) + _grid.nodeRadius * 8;

					if (currentLineNodePos.x < leftBound || currentLineNodePos.x > rightBound)
					{
						return true;
					}

					if (currentLineNodePos.y > topBound || currentLineNodePos.y < bottomBound)
					{
						return true;
					}

					return false;
				}

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
			Debug.Log("Stop Drawing line");
			//if already stopped drawing, do nothing
			if (!_isDrawing) { return; }

			_isDrawing = false;

			//if line is too short, discard line
			if (TooShort())
			{
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
			Debug.Log("finish line");
			//if already stopped drawing, do nothing
			if (!_isDrawing) { return; }

			_isDrawing = false;

			if (TooShort())
			{
				AbortLine();
				return;
			}

			BuildChalkLine(true);
		}

		//initialize lineObject
		public void BuildChalkLine(bool isEnclosed)
		{
			List<Vector2> reducedNodePos = new List<Vector2>();
			ReduceNodes();

			LineType lineType = IdentifyLineType();

			switch (lineType)
			{
				case LineType.WARD:
					InitWard();
					break;
				case LineType.MISSILE:
					InitMissile();
					break;
			}

			ResetLineVars();

			LineType IdentifyLineType()
			{
				if (isEnclosed)
				{
					return LineType.WARD;
				}

				if (Straightness() > straightnessThreshold)
				{
					return LineType.MISSILE;
				}

				return LineType.WARD;

				//measure straightness of chalkline
				//1f = straight, 0f = not straight
				float Straightness()
				{
					float sumOfDelta = 0f;

					for (int i = 0; i < reducedNodePos.Count - 3; i++)
					{
						Vector2 segment1 = (reducedNodePos[i + 1] - reducedNodePos[i]).normalized;
						Vector2 segment2 = (reducedNodePos[i + 2] - reducedNodePos[i + 1]).normalized;
						Vector2 segment3 = (reducedNodePos[i + 3] - reducedNodePos[i + 2]).normalized;

						Vector2 delta1 = segment2 - segment1;
						Vector2 delta2 = segment3 - segment2;

						sumOfDelta += (delta2 - delta1).sqrMagnitude;
					}

					float straightness = 1 / (1 + sumOfDelta);
					Debug.Log("Straightness: " + straightness);
					return straightness;
				}
			}

			//reduce # of nodes in nodePositions
			void ReduceNodes()
			{
				for (int i = 1; i < _nodePositions.Count; i += nodeReduceFactor)
				{
					reducedNodePos.Add(_nodePositions[i]);
				}
				//if last node not added, add it
				if (reducedNodePos[reducedNodePos.Count - 1] != _nodePositions[_nodePositions.Count - 1])
				{
					reducedNodePos.Add(_nodePositions[_nodePositions.Count - 1]);
				}
			}

			//initializes this object as a Warding Line
			void InitWard()
			{
				Debug.Log("Initting LineWard");
				GameObject cl = Instantiate(_lineWardPrefab);
				cl.GetComponent<LineWard>().Init(_nodePositions, reducedNodePos, _grid, isEnclosed);
			}

			//initializes this object as a Line Missile
			void InitMissile()
			{
				Debug.Log("Initting LineMissile");
				GameObject lm = Instantiate(_lineMissilePrefab);
				lm.GetComponent<LineMissile>().Init(_nodePositions, _lr);
			}

		}

		//Create and set up new line object
		private void CreateLine()
		{
			//Make the actual Line object
			_lineDrawing = Instantiate(_lineWardPrefab, Vector3.zero, Quaternion.identity);

			//set internal lineNode List
			_nodePositions = new List<Vector2>();
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			_nodePositions.Add(mousePos);
			_nodePositions.Add(mousePos);


			//init and set startLineTarget
			_startLineTarget = Instantiate(_startLineTargetPrefab, Vector3.zero, Quaternion.identity);
			_startLineTarget.SetActive(true);
			_startLineTarget.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

			//assign and set lineRenderer
			_lr = _lineDrawing.GetComponent<LineRenderer>();
			_lr.useWorldSpace = true;
			_lr.SetPosition(0, _nodePositions[0]);
			_lr.SetPosition(1, _nodePositions[1]);

			//assign and set start/end circles
			//start stays at the start
			_startCircle = _lineDrawing.GetComponent<LineWard>().startCircle;
			_startCircle.transform.position = _nodePositions[0];
			//end starts at start
			_endCircle = _lineDrawing.GetComponent<LineWard>().endCircle;
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

				//add new node
				_nodePositions.Add(nextSegment);
				//update line visual representation
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

			if (distanceBetween < _maxEncloseDistance)
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
				return true;
			}
			else
			{
				return false;
			}
		}

		//called when drawing is finished, but line is deleted
		public void AbortLine()
		{
			_isDrawing = false;
			ResetLineVars();
		}

		//resets local line variables to prevent bugs
		private void ResetLineVars()
		{
			Destroy(_lineDrawing);
			Destroy(_startLineTarget);
			_lr = null;
			_nodePositions = null;
			_lineDrawing = null;
		}

		private void OnValidate()
		{
			nodeReduceFactor = Mathf.Max(nodeReduceFactor, 1);
			_minNodesInLine = Mathf.Max(5, _minNodesInLine);
		}
	}
}