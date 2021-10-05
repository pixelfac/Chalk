using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChalkLine : MonoBehaviour
{
	[SerializeField] float minNodeDistance;

	List<Vector2> lineNodes;
    LineRenderer lr;

	private void Awake()
	{
		lineNodes = new List<Vector2>();
		lr = GetComponent<LineRenderer>();
		CreateLine();
	}

	public void CreateLine()
	{
		Debug.Log("Creating Line");
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		lineNodes.Add(mousePos);
		lineNodes.Add(mousePos);
		lr.SetPosition(0, lineNodes[0]);
		lr.SetPosition(1, lineNodes[1]);
	}

	public void UpdateLine()
	{
		Debug.Log("Drawing");
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		if (Vector2.Distance(lineNodes[lineNodes.Count - 1], mousePos) < minNodeDistance)
		{
			return;
		}
		lineNodes.Add(mousePos);
		lr.positionCount++;
		lr.SetPosition(lr.positionCount - 1, mousePos);
	}

}
