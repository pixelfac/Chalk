using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace Chalkling
{
	public class ChalklingMovement : MonoBehaviour
	{
		public float _moveSpeed { get; set; }
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

			//if moveDir not infinity, add scaled moveDir
			if (!float.IsNaN(moveDir.magnitude))
			{
				//set new position
				Vector2 newPos = (Vector2)transform.position + (moveDir * _moveSpeed * Time.deltaTime);
				rb.MovePosition(newPos);

				//set facing new direction
				Quaternion newDirection = Quaternion.LookRotation(Vector3.forward, moveDir);
				transform.rotation = newDirection;
			}
			//otherwise, don't move

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