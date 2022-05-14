using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding2D : MonoBehaviour
{
    public List<Node2D> path { get; private set; }
    Grid2D grid;
    Node2D seekerNode, targetNode;

	private void Awake()
	{
        grid = FindObjectOfType<Grid2D>();
	}

    public List<Node2D> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        //reset path
        path = new List<Node2D>();

        //get player and target position in grid coords
        seekerNode = grid.NodeFromWorldPoint(startPos);
        targetNode = grid.NodeFromWorldPoint(targetPos);


        //if already at target, skip calculations
        if (seekerNode == targetNode)
        {
            return RetracePath(seekerNode, targetNode);
        }

        List<Node2D> openSet = new List<Node2D>();
        HashSet<Node2D> closedSet = new HashSet<Node2D>();
        openSet.Add(seekerNode);
        
        //calculates path for pathfinding
        while (openSet.Count > 0)
        {

            //iterates through openSet and finds lowest FCost
            Node2D node = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost <= node.FCost)
                {
                    if (openSet[i].hCost < node.hCost)
                        node = openSet[i];
                }
            }

            openSet.Remove(node);
            closedSet.Add(node);

            //If target found, retrace path
            if (node == targetNode)
            {
                return RetracePath(seekerNode, targetNode);
            }
            
            //adds neighbor nodes to openSet
            foreach (Node2D neighbour in grid.GetNeighbors(node))
            {
                if (neighbour.obstacle || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = node;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        //No path possible
        return null;
    }

    //reverses calculated path so first node is closest to seeker
    List<Node2D> RetracePath(Node2D startNode, Node2D endNode)
    {
        Node2D currentNode = endNode;

        //if path is 0
        if (startNode == endNode)
		{
            return path;
		}

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    //gets distance between 2 nodes for calculating cost
    int GetDistance(Node2D nodeA, Node2D nodeB)
    {
        int dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public float GetNodeRadius()
	{
        return grid.nodeRadius;
	}
}
