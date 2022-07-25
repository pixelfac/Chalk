using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Pathfinding;
using MEC;

namespace ChalkLine
{
	public enum LineType { WARD, MISSILE };

	public class ChalkLine : MonoBehaviour
	{
		[SerializeField] private int _baseNodeHP;    //base HP for each node in a line
		[SerializeField] private float _enclosedHPScale;
		[Range(0.1f, 2f)]
		[SerializeField] private float colliderRadiusFactor; //how big the collider is relative to rendered line. 0.5f matches visual
		[SerializeField] private int nodeReduceFactor;
		[Range(0f, 1f)]
		[SerializeField] private float straightnessThreshold;
		[SerializeField] private GameObject lineMissilePrefab;

		private bool _isEnclosed;
		private EdgeCollider2D _hitbox;
		private List<LineNode> _lineNodes;
		private LineRenderer _lr;
		private Grid2D _grid;
		private float dmgTaken; //running total of damage taken across line

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
			if (!_isEnclosed)
			{
				_enclosedHPScale = 1;
			}

			List<Vector2> reducedNodePos = new List<Vector2>();
			ReduceNodes();

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

					float straightness = 1 / (1 + sumOfDelta);
					Debug.Log("Straightness: " + straightness);
					return straightness;
				}
			}

			Debug.Log("Chalkline Initted");

			//reduce # of nodes in nodePositions
			void ReduceNodes()
			{
				for (int i = 1; i < nodePositions.Count; i += nodeReduceFactor)
				{
					reducedNodePos.Add(nodePositions[i]);
				}
				//if last node not added, add it
				if (reducedNodePos[reducedNodePos.Count - 1] != nodePositions[nodePositions.Count - 1])
				{
					reducedNodePos.Add(nodePositions[nodePositions.Count - 1]);
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

				//populate _lineNodes
				_lineNodes = new List<LineNode>();
				for (int i = 0; i < reducedNodePos.Count; i++)
				{
					_lineNodes.Add(new LineNode(reducedNodePos[i]));
				}

				UpdateHP();
				UpdateGrid();
			}

			//initializes this object as a Line Missile
			void InitMissile()
			{
				GameObject lm = Instantiate(lineMissilePrefab);
				lm.GetComponent<LineMissile>().Init(startCircle, endCircle, reducedNodePos.Count, _lr);
				Destroy(gameObject);
			}
		}

		//TODO: only placeholder value presently
		public void UpdateHP()
		{
			if (_lineNodes.Count < 3)
			{
				foreach (LineNode n in _lineNodes)
				{
					n.strongSideNormal = Vector2.zero;
					n.weakSideNormal = Vector2.zero;
					n.strongHealth = _baseNodeHP;
					n.weakHealth = _baseNodeHP;
				}
				return;
			}

			if (_isEnclosed)
			{
				for (int i = 0; i < _lineNodes.Count; i++)
				{
					//store data
					LineNode currNode = _lineNodes[i];
					Vector2 leftNodePos = _lineNodes[Mod(i - 1, _lineNodes.Count)].worldPos;
					Vector2 rightNodePos = _lineNodes[Mod(i + 1, _lineNodes.Count)].worldPos;

					Vector2 leftVector = leftNodePos - currNode.worldPos;
					Vector2 rightVector = rightNodePos - currNode.worldPos;

					float angle = Vector2.SignedAngle(leftVector, rightVector);

					if (angle > 0)
					{
						currNode.strongSideNormal = (Quaternion.Euler(0, 0, -angle / 2) * leftVector).normalized;
						currNode.weakSideNormal = -currNode.strongSideNormal;
						currNode.strongHealth = CalcNodeHealth(angle / 4);
						currNode.weakHealth = CalcNodeHealth(90f - angle / 4);
					}
					else
					{
						currNode.strongSideNormal = (Quaternion.Euler(0, 0, -angle / 2) * leftVector).normalized;
						currNode.weakSideNormal = -currNode.strongSideNormal;
						currNode.strongHealth = CalcNodeHealth(angle / 4);
						currNode.weakHealth = CalcNodeHealth(90f + angle / 4);
					}
				}
			}
			else
			{
				LineNode startNode = _lineNodes[0];
				startNode.strongSideNormal = Vector2.zero;
				startNode.weakSideNormal = Vector2.zero;
				startNode.strongHealth = _baseNodeHP;
				startNode.weakHealth = _baseNodeHP;

				for (int i = 1; i < _lineNodes.Count - 1; i++)
				{
					//cache data
					LineNode currNode = _lineNodes[i];
					Vector2 leftNodePos = _lineNodes[i - 1].worldPos;
					Vector2 rightNodePos = _lineNodes[i + 1].worldPos;

					Vector2 leftVector = leftNodePos - currNode.worldPos;
					Vector2 rightVector = rightNodePos - currNode.worldPos;

					float angle = Vector2.SignedAngle(leftVector, rightVector);

					if (angle > 0)
					{
						currNode.strongSideNormal = (Quaternion.Euler(0, 0, -angle / 2) * leftVector).normalized;
						currNode.weakSideNormal = -currNode.strongSideNormal;
						currNode.strongHealth = CalcNodeHealth(angle / 4);
						currNode.weakHealth = CalcNodeHealth(90f - angle / 4);
					}
					else
					{
						currNode.strongSideNormal = (Quaternion.Euler(0, 0, -angle / 2) * leftVector).normalized;
						currNode.weakSideNormal = -currNode.strongSideNormal;
						currNode.strongHealth = CalcNodeHealth(angle / 4);
						currNode.weakHealth = CalcNodeHealth(90f + angle / 4);
					}
				}

				LineNode endNode = _lineNodes[_lineNodes.Count - 1];
				endNode.strongSideNormal = Vector2.zero;
				endNode.weakSideNormal = Vector2.zero;
				endNode.strongHealth = _baseNodeHP;
				endNode.weakHealth = _baseNodeHP;

			}

			//always returns positive val, even for negative mod
			//taken from https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
			int Mod(int num, int mod)
			{
				int rem = num % mod;
				return rem < 0 ? rem + mod : rem;
			}

			//converts angle in degrees to radians
			float ToRad(float angle)
			{
				return angle * Mathf.Deg2Rad;
			}

			//calc health value for node, given angle in degrees
			int CalcNodeHealth(float angle)
			{
				float bonusScale = Mathf.Cos(ToRad(angle)) * _enclosedHPScale;
				int bonusHP = (int)(_baseNodeHP * bonusScale);
				int baseHP = 2 * _baseNodeHP;
				return baseHP + bonusHP;
			}
		}

		//TODO: Damage overall line based on damage applied to specific node
		public void Damage(float damage, int damagedLineNodeIndex, Vector3 attackOrigin)
		{
			Debug.Log("Line has been Damaged");
			dmgTaken += damage;
			LineNode lineNode = _lineNodes[damagedLineNodeIndex];
			float nodeHP = lineNode.GetDirectionalHealth(attackOrigin);

			Debug.Log(dmgTaken + "/" + nodeHP + " HP");

			if (dmgTaken >= nodeHP)
			{
				Dissipate();
			}
		}
		//TODO
		//redraws the line renderer based on current lineNodes info
		private void RedrawLineRenderer()
		{
			Debug.LogWarning("RedrawLineRenderer not implemented");
		}

		//TODO: Not Finished
		//When chalkline is broken, dissipates
		public void Dissipate()
		{
			//change layer to avoid collision
			gameObject.layer = 2; //"ignore raycast" layer

			UpdateGrid();

			//play fade-out animation (coroutines :shrug:)
			Timing.RunCoroutine(Suicide(1f));

			//Destroys itself
			IEnumerator<float> Suicide(float duration)
			{
				yield return Timing.WaitUntilDone(Timing.RunCoroutine(FadeOut(duration)));
				Destroy(gameObject);
			}

			//fade lines alpha to 0 over duration
			IEnumerator<float> FadeOut(float duration)
			{
				Color lineColor = _lr.material.color; //assume starts at 1f
				float startAlpha = lineColor.a;
				for (float timer = 0f; timer < duration; timer += Time.deltaTime)
				{
					float newAlpha = (1 - timer / duration);
					Debug.Log("New Alpha: " + newAlpha);
					lineColor.a = newAlpha;
					_lr.material.color = lineColor;
					yield return Timing.WaitForOneFrame;
				}
			}
		}

		//Use this instead of _grid.UpdateGrid()
		private void UpdateGrid()
		{
			if (_grid) //not dependent on grid
			{
				_grid.UpdateGrid();
			}
		}

		public LineNode GetLineNode(int index)
		{
			return _lineNodes[index];
		}

		//returns the lowest health of all line segments within nodeRadius of gridNodePos
		public int ClosestLineHealthFromGridNode(Vector3 gridNodePos)
		{
			int minHealth = int.MaxValue;
			float checkRadius = (_grid.nodeRadius * _grid.GetNodeOverlapDistance() + _hitbox.edgeRadius) * 1.1f; //*1.1f buffer for rounding
			//iterate through nodepositions
			for (int i = 0; i < _lineNodes.Count; i++)
			{
				//if |nodepos.worldpos - gridNodePos| < nodeRadius
				if ((_lineNodes[i].worldPos - (Vector2)gridNodePos).magnitude <= checkRadius)
				{
					//find LineNode.GetDirectionalHealth
					minHealth = Mathf.Min(minHealth, _lineNodes[i].GetDirectionalHealth(gridNodePos));
				}
			}

			if (minHealth == int.MaxValue)
			{
				Debug.Log("minHealth was unchanged from MaxValue");
				minHealth = 0;
			}
			return minHealth;
		}

		public int ClosestLineHealthFromGridNode(Vector3 gridNodePos, out int lineNodeIndex)
		{
			int minHealth = int.MaxValue;
			lineNodeIndex = 0;
			float checkRadius = (_grid.nodeRadius * _grid.GetNodeOverlapDistance() + _hitbox.edgeRadius) * 1.1f; //*1.1f buffer for rounding
																												 //iterate through nodepositions
			for (int i = 0; i < _lineNodes.Count; i++)
			{
				//if |nodepos.worldpos - gridNodePos| < nodeRadius
				if ((_lineNodes[i].worldPos - (Vector2)gridNodePos).magnitude <= checkRadius)
				{
					//find LineNode.GetDirectionalHealth
					minHealth = Mathf.Min(minHealth, _lineNodes[i].GetDirectionalHealth(gridNodePos));
					lineNodeIndex = i;
				}
			}

			if (minHealth == int.MaxValue)
			{
				Debug.Log("minHealth was unchanged from MaxValue");
				minHealth = 0;
				lineNodeIndex = 0;
			}
			return minHealth;
		}

		private void OnValidate()
		{
			nodeReduceFactor = Mathf.Max(nodeReduceFactor, 1);
			_enclosedHPScale = Mathf.Max(_enclosedHPScale, 1);
		}
	}
}