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
			lineType = LineType.WARD;

			//first node is duplicated in creation process
			//this line removes the duplicated node
			nodePositions.RemoveAt(1);

			//redraw LineRenderer to omit duplicated point
			List<Vector3> lrNodes = new List<Vector3>();
			foreach (Vector2 node in nodePositions)
			{
				lrNodes.Add(node);
			}
			_lr.SetPositions(lrNodes.ToArray());

			//Set EdgeCollider
			_hitbox.SetPoints(nodePositions);
			_hitbox.edgeRadius = _lr.startWidth * colliderRadiusFactor;
			if (isEnclosed)
			{
				//loop EdgeCollider
				_hitbox.adjacentEndPoint = nodePositions[0];
				_hitbox.useAdjacentEndPoint = true;
			}

			_grid = grid;
			if (_grid) //not dependent on grid
			{
				_grid.UpdateGrid();
			}
			
			//populate _lineNodes
			_lineNodes = new List<LineNode>();
			foreach (Vector2 nodePos in nodePositions)
			{
				_lineNodes.Add(new LineNode(nodePos));
			}

			_isEnclosed = isEnclosed;

			UpdateHP();
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
			throw new System.NotImplementedException();
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
	}
}