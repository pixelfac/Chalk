using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace Chalkling
{
	public class ChalklingMovement : MonoBehaviour
	{
		[SerializeField] private float _moveSpeed;
		[SerializeField] private Transform _targetTransform;

		private Rigidbody2D rb;
		private Grid2D pathfinder;
		private List<Node2D> path;

		private void Awake()
		{
			rb = GetComponent<Rigidbody2D>();
			pathfinder = FindObjectOfType<Grid2D>();
		}

		private void FixedUpdate()
		{
			Vector2 moveDir = pathfinder.NodeFromWorldPoint(transform.position).goalVector; //the direction to move towards target

			Vector2 newPos = transform.position;

			//if moveDir not infinity, add scaled moveDir
			if (!float.IsNaN(moveDir.magnitude))
			{
				newPos += (moveDir * _moveSpeed * Time.deltaTime);
			}

			rb.MovePosition(newPos);
		}

		private void OnValidate()
		{
			_moveSpeed = Mathf.Max(_moveSpeed, 0);
		}

		private void OnDrawGizmos()
		{
			if (path == null) { return; }

			foreach (Node2D n in path)
			{
				Gizmos.color = Color.black;

				Gizmos.DrawSphere(n.worldPosition, pathfinder.nodeRadius);
			}
		}
	}
}