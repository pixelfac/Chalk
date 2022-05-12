using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalklingMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed;
	[SerializeField] Transform target;

    Rigidbody2D rb;
	Pathfinding2D pathfind;
	List<Node2D> path;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		pathfind = GetComponent<Pathfinding2D>();
	}

	private void FixedUpdate()
	{
		Debug.Log("currPos: " + transform.position);
		Debug.Log("targetPos: " + target.position);

		pathfind.FindPath(transform.position, target.position);
		path = pathfind.GetPath();

		//if no path found OR already at target, stop
		if (path == null || path.Count == 0)
		{
			return;
		}

		Debug.Log("path-0: " + path[0].worldPosition);

		Vector2 moveDir = (path[0].worldPosition - transform.position).normalized; //the direction to move towards target
		Debug.Log("moveDir: " + moveDir);

		Vector2 newPos = (moveDir * moveSpeed * Time.deltaTime) + (Vector2)transform.position;
		Debug.Log("newPos: " + newPos);

		rb.MovePosition(newPos);
	}
}
