using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalklingMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed;
	[SerializeField] Transform target;

    Rigidbody2D rb;
	Grid2D pathfinder;
	List<Node2D> path;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		pathfinder = GetComponent<Grid2D>();
	}

	private void FixedUpdate()
	{
		path = pathfinder.FindPath(transform.position, target.position);

		//if no path found OR already at target, stop
		if (path == null || path.Count == 0)
		{
			return;
		}

		Vector2 moveDir = (path[0].worldPosition - transform.position).normalized; //the direction to move towards target
		Vector2 newPos = (moveDir * moveSpeed * Time.deltaTime) + (Vector2)transform.position;

		rb.MovePosition(newPos);
	}

	private void OnValidate()
	{
		moveSpeed = Mathf.Max(moveSpeed, 0);
	}

	void OnDrawGizmos()
	{
		if (path == null) { return; }

		foreach (Node2D n in path)
		{
			Gizmos.color = Color.black;

			Gizmos.DrawSphere(n.worldPosition, pathfinder.nodeRadius);
		}
	}
}
