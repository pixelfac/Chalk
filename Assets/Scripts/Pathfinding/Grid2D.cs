using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Grid2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _gridWorldSize;
        [SerializeField] public Vector2Int gridSize { get; private set; }
        [SerializeField] public float nodeRadius;
        [SerializeField] public Vector2 goalPos;
        [SerializeField] private LayerMask _obstacleMask;

        private Node2D[,] _Grid;
        private Vector3 _worldBottomLeft;
        private float _nodeDiameter;


        private void Awake()
        {
            _nodeDiameter = nodeRadius * 2;
            gridSize = new Vector2Int(Mathf.RoundToInt(_gridWorldSize.x / _nodeDiameter),
                                      Mathf.RoundToInt(_gridWorldSize.y / _nodeDiameter));
        }

	    private void Start()
	    {
            CreateGrid();
        }

		private void CreateGrid()
        {
            _Grid = new Node2D[gridSize.x, gridSize.y];
            _worldBottomLeft = transform.position - Vector3.right * _gridWorldSize.x / 2 - Vector3.up * _gridWorldSize.y / 2;

            UpdateObstacles();
        }

        public void UpdateGrid()
		{
            UpdateObstacles();
		}

        private void UpdateObstacles()
	    {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 worldPoint = _worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius) + Vector3.up * (y * _nodeDiameter + nodeRadius);
                    _Grid[x, y] = new Node2D(false, worldPoint, x, y);

                    if (Physics2D.OverlapCircle(worldPoint, nodeRadius, _obstacleMask) != null) //null == no collision
                    {
                        _Grid[x, y].SetObstacle(true);
                    }
                    else
                    {
                        _Grid[x, y].SetObstacle(false);
                    }
                }
            }
        }

        //resets the goalDist for all nodes
        private void ResetGoalDists()
		{
            foreach (Node2D n in _Grid)
			{
                n.goalDist = int.MaxValue;
			}
		}

        //Vector-goal pathfinding
        //computes vectors for each node to direct towards goal
        private void ComputeVectorField()
		{
            List<Node2D> open = new List<Node2D>();
            HashSet<Node2D> closed = new HashSet<Node2D>();
            ResetGoalDists();

            Node2D goalNode = NodeFromWorldPoint(goalPos);
            goalNode.goalDist = 0;

            if (goalNode.obstacle)
            {
                //path not possible, grid defaults to max distance for all nodes
                Debug.LogError("Grid goal is on an obstacle node");
                return;
            }

            Node2D currNode;
            open.Add(goalNode);

            //start computation
            while (open.Count > 0)
			{
                currNode = open[0];
                open.RemoveAt(0);
                closed.Add(currNode);

                foreach (Node2D n in GetNeighbors(currNode))
                {
                    if (n.obstacle)
                    {
                        continue;
                    }

                    if (closed.Contains(n))
                    {
                        if (n.goalDist > currNode.goalDist + GetDistance(n,currNode))
						{
                            n.goalDist = currNode.goalDist + GetDistance(n, currNode);
                        }
                    }

                    n.goalDist = currNode.goalDist + GetDistance(n, currNode);
                    open.Add(n);
                }
			}

        }



        //gets the neighboring nodes in the 4 cardinal directions. If you would like to enable diagonal pathfinding, uncomment out that portion of code
        public List<Node2D> GetNeighbors(Node2D node)
        {
            List<Node2D> neighbors = new List<Node2D>();

            //checks and adds top neighbor
            if (node.GridX >= 0 && node.GridX < gridSize.x && node.GridY + 1 >= 0 && node.GridY + 1 < gridSize.y)
                neighbors.Add(_Grid[node.GridX, node.GridY + 1]);

            //checks and adds bottom neighbor
            if (node.GridX >= 0 && node.GridX < gridSize.x && node.GridY - 1 >= 0 && node.GridY - 1 < gridSize.y)
                neighbors.Add(_Grid[node.GridX, node.GridY - 1]);

            //checks and adds right neighbor
            if (node.GridX + 1 >= 0 && node.GridX + 1 < gridSize.x && node.GridY >= 0 && node.GridY < gridSize.y)
                neighbors.Add(_Grid[node.GridX + 1, node.GridY]);

            //checks and adds left neighbor
            if (node.GridX - 1 >= 0 && node.GridX - 1 < gridSize.x && node.GridY >= 0 && node.GridY < gridSize.y)
                neighbors.Add(_Grid[node.GridX - 1, node.GridY]);

            //checks and adds top right neighbor
            if (node.GridX + 1 >= 0 && node.GridX + 1< gridSize.x && node.GridY + 1 >= 0 && node.GridY + 1 < gridSize.y)
                neighbors.Add(_Grid[node.GridX + 1, node.GridY + 1]);

            //checks and adds bottom right neighbor
            if (node.GridX + 1>= 0 && node.GridX + 1 < gridSize.x && node.GridY - 1 >= 0 && node.GridY - 1 < gridSize.y)
                neighbors.Add(_Grid[node.GridX + 1, node.GridY - 1]);

            //checks and adds top left neighbor
            if (node.GridX - 1 >= 0 && node.GridX - 1 < gridSize.x && node.GridY + 1>= 0 && node.GridY + 1 < gridSize.y)
                neighbors.Add(_Grid[node.GridX - 1, node.GridY + 1]);

            //checks and adds bottom left neighbor
            if (node.GridX - 1 >= 0 && node.GridX - 1 < gridSize.x && node.GridY  - 1>= 0 && node.GridY  - 1 < gridSize.y)
                neighbors.Add(_Grid[node.GridX - 1, node.GridY - 1]);

            return neighbors;
        }

        public Node2D NodeFromWorldPoint(Vector3 worldPosition)
        {
            //difference between bottom corner and current position in question
            float diffX = worldPosition.x - _worldBottomLeft.x;
            float diffY = worldPosition.y - _worldBottomLeft.y;

            //convert difference in worldspace to difference in nodes
            int x = (int)(diffX / _nodeDiameter);
            int y = (int)(diffY / _nodeDiameter);

            //catch out-of-bounds
            x = Mathf.Clamp(x, 0, gridSize.x - 1);
            y = Mathf.Clamp(y, 0, gridSize.y - 1);

            return _Grid[x, y];
        }

        //A* pathfinding algorithm
        public List<Node2D> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            Node2D seekerNode, targetNode;

            //reset path
            List<Node2D> path = new List<Node2D>();

            //get player and target position in grid coords
            seekerNode = NodeFromWorldPoint(startPos);
            targetNode = NodeFromWorldPoint(targetPos);

            //if already at target, skip calculations
            if (seekerNode == targetNode)
            {
                return RetracePath(ref path, seekerNode, targetNode);
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
                    return RetracePath(ref path, seekerNode, targetNode);
                }

                //adds neighbor nodes to openSet
                foreach (Node2D neighbour in GetNeighbors(node))
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
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }

            //No path possible
            return null;
        }

        //reverses calculated path so first node is closest to seeker
        private List<Node2D> RetracePath(ref List<Node2D> path, Node2D startNode, Node2D endNode)
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
        private int GetDistance(Node2D nodeA, Node2D nodeB)
        {
            int dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
            int dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

            if (dstX > dstY)
            {
                return 14 * dstY + 10 * (dstX - dstY);
            }
            return 14 * dstX + 10 * (dstY - dstX);
        }

        //Draws visual representation of grid
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(_gridWorldSize.x, _gridWorldSize.y, 1));

            if (_Grid == null) { return; }

            foreach (Node2D n in _Grid)
            {
                if (n.obstacle)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.white;

                Gizmos.DrawCube(n.worldPosition, Vector3.one * 0.9f * (nodeRadius));
            }
        }

		private void OnValidate()
		{
            //clamps goalPos within valid range
            goalPos = new Vector2(Mathf.Clamp(goalPos.x, 0, gridSize.x), Mathf.Clamp(goalPos.y, 0, gridSize.y));
        }
	}
}