using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum LineType { WARD, RESTRICT, MISSILE };

public class ChalkLine : MonoBehaviour
{
	[SerializeField] int baseNodeHP;	//base HP for each node in a line
	bool isEnclosed;

	EdgeCollider2D hitbox;
	List<LineNode> lineNodes;
    LineRenderer lr;

	public GameObject startCircle;
	public GameObject endCircle;


	private void Awake()
	{
		hitbox = GetComponent<EdgeCollider2D>();
		lr = GetComponent<LineRenderer>();
		startCircle.GetComponent<SpriteRenderer>().color = lr.startColor;
		endCircle.GetComponent<SpriteRenderer>().color = lr.endColor;
	}
	//basically a constructor, but since can't call constructor
	//on gameobject prefab component, this is the best alternative
	public void Init(ref List<Vector2> nodePositions, bool _isEnclosed)
	{
		//Set EdgeCollider
		hitbox.SetPoints(nodePositions);
		//loop EdgeCollider
		hitbox.adjacentEndPoint = nodePositions[0]; 
		hitbox.useAdjacentEndPoint = true;

		lineNodes = new List<LineNode>();
		foreach (Vector2 nodePos in nodePositions)
		{
			lineNodes.Add(new LineNode(nodePos));
		}
		Debug.Log("lineNodes populated");

		isEnclosed = _isEnclosed;

		UpdateHP();
	}

	public void UpdateHP()
	{
		for (int i = 1; i < lineNodes.Count - 1; i++)
		{
			lineNodes[i].health = 42;
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
}
