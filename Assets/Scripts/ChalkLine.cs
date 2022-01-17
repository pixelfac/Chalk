using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum LineType { WARD, RESTRICT, MISSILE };

public class ChalkLine : MonoBehaviour
{
	[SerializeField] int baseNodeHP;

	List<LineNode> lineNodes;
    LineRenderer lr;

	//basically a constructor, but since can't call constructor
	//on gameobject prefab component, this is the best alternative
	public void Init(ref List<Vector2> nodePositions)
	{
		lineNodes = new List<LineNode>();
		foreach (Vector2 nodePos in nodePositions) {
			lineNodes.Add(new LineNode(nodePos));
		}
		Debug.Log("lineNodes populated");

		lr = GetComponent<LineRenderer>();

		UpdateHP();
	}

	public void UpdateHP()
	{
		foreach (LineNode node in lineNodes)
		{
			node.health = 42;
		}
		Debug.Log("Health Set");
		Debug.Log("Health is" + lineNodes[0].health);
	}
}
