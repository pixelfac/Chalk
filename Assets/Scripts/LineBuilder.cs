using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class LineBuilder : MonoBehaviour
{
	float maxNodeDistance;

	List<Vector2> nodePositions;
	LineRenderer lr;

	GameObject startLineTarget; //prefab for target on start of line while drawing


	public LineBuilder(float _maxNodeDistance, LineRenderer _lr, GameObject startLineTargetPrefab)
	{
		maxNodeDistance = _maxNodeDistance;
		nodePositions = new List<Vector2>();
		lr = _lr;

		//init startLineTarget and disable it
		startLineTarget = Instantiate(startLineTargetPrefab, Vector3.zero, Quaternion.identity);
		startLineTarget.SetActive(false);
	}


	public void CreateLine()
	{
		Debug.Log("Creating Line");

		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		nodePositions.Add(mousePos);
		nodePositions.Add(mousePos);
	
		lr.SetPosition(0, nodePositions[0]);
		lr.SetPosition(1, nodePositions[1]);

		//enable and set startLineTarget
		startLineTarget.SetActive(true);
		startLineTarget.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
	}

	//checks to see if the ends of line drawn meet to enclose a shape
	public bool isEnclosed()
	{
		//prevent line from being enclosed immediately when start drawing
		if (nodePositions.Count < 3) { return false; }

		//distance between first and last node
		float distanceBetween = (nodePositions[0] - nodePositions[nodePositions.Count-1]).magnitude;

		if (distanceBetween < maxNodeDistance)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	//update the length of the line
	public void UpdateLine()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

		//if mouse has not moved far enough from last node, don't add node to line
		if (Vector2.Distance(nodePositions[nodePositions.Count-1], mousePos) < maxNodeDistance)
		{
			return;
		}

		//add new node + LineRenderer segment
		nodePositions.Add(mousePos);
		lr.positionCount++;
		lr.SetPosition(lr.positionCount - 1, mousePos);
	}

	//closes start and end of a line
	public void CloseLine()
	{
		lr.loop = true;
	}

	//true if # of nodes is too short to save, false otherwise
	public bool TooShort()
	{
		if (nodePositions.Count < 10)
		{
			Debug.Log("TOO short");
			return true;
		}
		else
		{
			return false;
		}
	}


	//initialize lineObject
	public void BuildChalkLine(ChalkLine line, bool isEnclosed)
	{
		Destroy(startLineTarget);
		line.Init(ref nodePositions, isEnclosed);
	}
}
