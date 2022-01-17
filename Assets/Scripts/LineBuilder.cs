using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class LineBuilder
{
	float maxNodeDistance;

	List<Vector2> nodePositions;
	LineRenderer lr;

	public LineBuilder(float _maxNodeDistance, LineRenderer _lr)
	{
		maxNodeDistance = _maxNodeDistance;
		nodePositions = new List<Vector2>();
		lr = _lr;
	}

	public void CreateLine()
	{
		Debug.Log("Creating Line");

		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		nodePositions.Add(mousePos);
		nodePositions.Add(mousePos);
	
		lr.SetPosition(0, nodePositions[0]);
		lr.SetPosition(1, nodePositions[1]);
	}

	public void UpdateLine()
	{
		Debug.Log("Drawing");
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		if (Vector2.Distance(nodePositions[nodePositions.Count - 1], mousePos) < maxNodeDistance)
		{
			return;
		}
		nodePositions.Add(mousePos);
		lr.positionCount++;
		lr.SetPosition(lr.positionCount - 1, mousePos);
	}

	public void BuildChalkLine(ChalkLine line)
	{
		line.Init(ref nodePositions);
	}
}
