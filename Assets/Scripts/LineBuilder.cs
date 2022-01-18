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

	//checks to see if the ends of line drawn meet to enclose a shape
	public bool isEnclosed()
	{
		if (nodePositions.Count < 3) { return false; }

		float distanceBetween = (nodePositions[0] - nodePositions[nodePositions.Count-1]).magnitude;
		Debug.Log("distanceBetween: " + distanceBetween);

		if (distanceBetween < maxNodeDistance)
		{
			Debug.Log("is closed");
			return true;
		}
		else
		{
			return false;
		}
	}

	public void UpdateLine()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		if (Vector2.Distance(nodePositions[nodePositions.Count - 1], mousePos) < maxNodeDistance)
		{
			return;
		}
		nodePositions.Add(mousePos);
		lr.positionCount++;
		lr.SetPosition(lr.positionCount - 1, mousePos);
	}

	//closes start and end of a line
	public void CloseLine()
	{

		lr.positionCount++;
		lr.SetPosition(lr.positionCount - 1, nodePositions[0]);
	}

	public void BuildChalkLine(ChalkLine line, bool isEnclosed)
	{
		line.Init(ref nodePositions, isEnclosed);
	}
}
