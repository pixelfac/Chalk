using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Pathfinding;

namespace ChalkLine
{
	public enum LineType { WARD, RESTRICT, MISSILE };

	public class ChalkLine : MonoBehaviour
	{
		[SerializeField] private int _baseNodeHP;    //base HP for each node in a line
		[Range(0.1f, 2f)]
		[SerializeField] private float colliderRadiusFactor; //how big the collider is relative to rendered line. 0.5f matches visual


		private bool _isEnclosed;
		private EdgeCollider2D _hitbox;
		private List<LineNode> _lineNodes;
		private LineRenderer _lr;

		public GameObject startCircle;
		public GameObject endCircle;

		private Grid2D _grid;


		private void Awake()
		{
			_hitbox = GetComponent<EdgeCollider2D>();
			_lr = GetComponent<LineRenderer>();
			startCircle.GetComponent<SpriteRenderer>().color = _lr.startColor;
			endCircle.GetComponent<SpriteRenderer>().color = _lr.endColor;
		}
		//basically a constructor, but since can't call constructor
		//on gameobject prefab component, this is the best alternative
		public void Init(List<Vector2> nodePositions, Grid2D grid, bool _isEnclosed)
		{
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
			if (_isEnclosed)
			{
				//loop EdgeCollider
				_hitbox.adjacentEndPoint = nodePositions[0];
				_hitbox.useAdjacentEndPoint = true;
			}

			_grid = grid;
			_grid.UpdateGrid();

			_lineNodes = new List<LineNode>();
			foreach (Vector2 nodePos in nodePositions)
			{
				_lineNodes.Add(new LineNode(nodePos));
			}
			Debug.Log("lineNodes populated");

			this._isEnclosed = _isEnclosed;

			UpdateHP();
		}

		public void UpdateHP()
		{
			for (int i = 1; i < _lineNodes.Count - 1; i++)
			{
				_lineNodes[i].health = 42;
			}
		}

		//TODO
		//checks to see if lineNodes encloses a region i.e. is a shape w/ no holes
		//Only gets the first enclosed region, ignores rest
		//RETURN the starting and ending indices that make the enclosed line segment
		private void checkEnclosed(out int startPos, out int endPos)
		{
			throw new System.NotImplementedException();
		}

		//TODO
		//redraws the line renderer based on current lineNodes info
		private void redrawLineRenderer()
		{
			throw new System.NotImplementedException();
		}


		public void Dissipate()
		{
			//clear hitboxes
			_hitbox.points = Array.Empty<Vector2>();

			//call UpdateGrid
			_grid.UpdateGrid();

			Destroy(gameObject);
		}
	}
}