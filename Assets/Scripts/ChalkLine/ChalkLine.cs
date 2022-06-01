using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Pathfinding;

namespace ChalkLine
{
	public enum LineType { WARD, MISSILE };

	public class ChalkLine : MonoBehaviour
	{
		[SerializeField] private int _baseNodeHP;    //base HP for each node in a line
		[Range(0.1f, 2f)]
		[SerializeField] private float colliderRadiusFactor; //how big the collider is relative to rendered line. 0.5f matches visual
		[SerializeField] private int nodeReduceFactor;
		[Range(0f, 1f)]
		[SerializeField] private float straightnessThreshold;

		private bool _isEnclosed;
		private EdgeCollider2D _hitbox;
		private List<LineNode> _lineNodes;
		private LineRenderer _lr;
		private Grid2D _grid;

		public LineType lineType { get; private set; }
		public GameObject startCircle;
		public GameObject endCircle;

		private void Awake()
		{
			_hitbox = GetComponent<EdgeCollider2D>();
			_lr = GetComponent<LineRenderer>();
			startCircle.GetComponent<SpriteRenderer>().color = _lr.startColor;
			endCircle.GetComponent<SpriteRenderer>().color = _lr.endColor;
		}

		//basically a constructor, but since can't call constructor
		//on gameobject prefab component, this is the best alternative
		public void Init(List<Vector2> nodePositions, Grid2D grid, bool isEnclosed)
		{
			_grid = grid;
			_isEnclosed = isEnclosed;

			//reduce # of nodes in nodePositions
			List<Vector2> reducedNodePos = new List<Vector2>();
			for (int i = 1; i < nodePositions.Count; i += nodeReduceFactor)
			{
				reducedNodePos.Add(nodePositions[i]);
			}
			//if last node not added, add it
			if (reducedNodePos[reducedNodePos.Count-1] != nodePositions[nodePositions.Count-1])
			{
				reducedNodePos.Add(nodePositions[nodePositions.Count - 1]);
			}

			lineType = IdentifyLineType();

			switch (lineType)
			{
				case LineType.WARD:
					InitWard();
					break;
				case LineType.MISSILE:
					InitMissile();
					break;
			}

			LineType IdentifyLineType()
			{
				if (_isEnclosed)
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

					float straightness = 1 / (1 + sumOfDelta / reducedNodePos.Count);
					Debug.Log("Straightness: " + straightness);
					return straightness;
				}
			}

			//initializes this object as a Warding Line
			void InitWard()
			{
				//redraw LineRenderer to omit duplicated point
				List<Vector3> lrNodes = new List<Vector3>();
				for (int i = 0; i < nodePositions.Count; i++)
				{
					lrNodes.Add(nodePositions[i]);
				}
				_lr.SetPositions(lrNodes.ToArray());

				//Set EdgeCollider
				_hitbox.SetPoints(reducedNodePos);
				_hitbox.edgeRadius = _lr.startWidth * colliderRadiusFactor;
				if (isEnclosed)
				{
					//loop EdgeCollider
					_hitbox.adjacentEndPoint = reducedNodePos[0];
					_hitbox.useAdjacentEndPoint = true;
				}

				if (_grid) //not dependent on grid
				{
					_grid.UpdateGrid();
				}

				//populate _lineNodes
				_lineNodes = new List<LineNode>();
				for (int i = 0; i < reducedNodePos.Count; i++)
				{
					_lineNodes.Add(new LineNode(reducedNodePos[i]));
				}

				UpdateHP();
			}

			//initializes this object as a Line Missile
			void InitMissile()
			{
				Debug.LogWarning("InitMissile not implemented");
			}
		}

		//TODO: only placeholder value presently
		public void UpdateHP()
		{
			for (int i = 1; i < _lineNodes.Count - 1; i++)
			{
				_lineNodes[i].health = 42;
			}
		}

		//TODO
		//redraws the line renderer based on current lineNodes info
		private void redrawLineRenderer()
		{
			Debug.LogWarning("InitMissile not implemented");
		}

		//TODO: Not Finished
		//When chalkline is broken, dissipates
		public void Dissipate()
		{
			//clear hitboxes
			_hitbox.points = Array.Empty<Vector2>();

			//call UpdateGrid
			if (_grid) //not dependent on grid
			{
				_grid.UpdateGrid();
			}

			//play fade-out animation

			Destroy(gameObject);
		}

		private void OnValidate()
		{
			nodeReduceFactor = Mathf.Max(nodeReduceFactor, 1);
		}
	}
}