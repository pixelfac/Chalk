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

	private void Update()
	{
		Debug.Log("currPos: " + transform.position);

		pathfind.FindPath(transform.position, target.position);
		path = pathfind.GetPath();
		Debug.Log("path-0: " + path[0].worldPosition);
		Debug.Log("path-1: " + path[1].worldPosition);

		Vector2 moveDir = (path[1].worldPosition - transform.position).normalized; //the direction to move towards target
		Debug.Log("moveDir: " + moveDir);

		Vector2 newPos = moveDir * moveSpeed;
		Debug.Log("newPos: " + newPos);

		rb.MovePosition(newPos);
	}
}
