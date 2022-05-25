using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Grid2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _gridWorldSize;
        [SerializeField] public Vector2Int gridSize { get; private set; }
        [SerializeField] public float nodeRadius;
        [SerializeField] public Transform goalTransform;
        [SerializeField] private LayerMask _obstacleMask;

        private Vector2 _goalPos;
        private Node2D[,] _Grid;
        private Vector3 _worldBottomLeft;
        private float _nodeDiameter;


        private void Awake()
        {
            _nodeDiameter = nodeRadius * 2;
            _goalPos = goalTransform.position;
            gridSize = new Vector2Int(Mathf.RoundToInt(_gridWorldSize.x / _nodeDiameter),
                                      Mathf.RoundToInt(_gridWorldSize.y / _nodeDiameter));
        }

	    private void Start()
	    {
            CreateGrid();
        }

		private void CreateGrid()
        {
            //defaults to double array of nulls
            _Grid = new Node2D[gridSize.x, gridSize.y];

            _worldBottomLeft = transform.position - Vector3.right * _gridWorldSize.x / 2 - Vector3.up * _gridWorldSize.y / 2;

            Debug.Log("Grid created");
            UpdateGrid();
            Debug.Log("Grid updated");
        }

        public void UpdateGrid()
		{
            UpdateObstacles();
            ComputeDistField();
            ComputeVectorField();
        }

        private void UpdateObstacles()
	    {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 worldPoint = WorldPointFromGridPos(x, y);
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
                n.visited = false;
                n.inOpen = false;
                n.goalVector = Vector2.zero;
			}
		}

        //Vector-goal pathfinding
        //computes distance for each node to goal
        private void ComputeDistField()
		{
            Debug.Log("Begin ComputeDistField");

            ResetGoalDists();

            Node2D goalNode = NodeFromWorldPoint(_goalPos);
            goalNode.goalDist = 0;

            if (goalNode.obstacle)
            {
                //path not possible, grid defaults to max distance for all nodes
                Debug.LogError("Grid goal is on an obstacle node");
                return;
            }

            List<Node2D> open = new List<Node2D>();
            open.Add(goalNode);
            goalNode.inOpen = true;

            Node2D currNode;
            while (open.Count > 0)
			{
                currNode = open[0];

                open.RemoveAt(0);
                currNode.visited = true;

                List<Node2D> neighbors = GetNeighbors(currNode);
                foreach (Node2D n in neighbors)
                {
                    if (n.obstacle)
                    {
                        continue;
                    }

                    if (n.visited)
                    {
                        if (n.goalDist > currNode.goalDist + GetDistance(n,currNode))
						{
                            n.goalDist = currNode.goalDist + GetDistance(n, currNode);
                        }
                    }
                    else if (!n.inOpen)
					{
                        n.goalDist = currNode.goalDist + GetDistance(n, currNode);
                        open.Add(n);
                        n.inOpen = true;
					}
                }
			}
        }

        //Vector-goal pathfinding
        //computes vector field directing towards goal
        private void ComputeVectorField()
		{
            Debug.Log("Begin ComputeVectorField");

            foreach (Node2D n in _Grid)
			{
                //if (n.obstacle) { continue; }

                List<Node2D> neighbors = GetNeighbors(n);

                //does neighbors contain an obstacle?
                bool containsObstacle = neighbors.Any(n => n.obstacle);

                if (containsObstacle)
                {
                    //find min
                    int minDist = int.MaxValue;
                    Vector2 direction = Vector2.zero;
                    foreach (Node2D nbr in neighbors)
                    {
                        if (nbr.goalDist < minDist)
                        {
                            minDist = nbr.goalDist;
                            direction = (nbr.worldPosition - n.worldPosition).normalized;
                            n.goalVector = direction;
                        }
                    }
                }
                else
                {
                    //find lowest gradient
                    Vector2 goalVec = Vector2.zero;
                    foreach (Node2D nbr in neighbors)
                    {
                        Vector2 direction = (nbr.worldPosition - n.worldPosition).normalized;
                        goalVec += direction / nbr.goalDist;
                    }
                    n.goalVector = goalVec.normalized;
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

        public Vector3 WorldPointFromGridPos(int x, int y)
		{
            float scale = _nodeDiameter;
            float diffX = x * scale;
            float diffY = y * scale;
            return _worldBottomLeft + Vector3.right * (diffX + nodeRadius) + Vector3.up * (diffY + nodeRadius);

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
                //draw cubes
                if (n == null) { continue; }

				if (n.obstacle)
				{
					Gizmos.color = Color.red;
				}
				else if (n.goalDist<30)
				{
					Gizmos.color = Color.green;
				}
                else if (n.goalDist < 200)
				{
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.white;
                }

                Gizmos.DrawCube(n.worldPosition, Vector3.one * 0.9f * (nodeRadius));

                //draw lines
                Gizmos.color = Color.black;
                Gizmos.DrawLine(n.worldPosition, n.worldPosition + nodeRadius * (Vector3)n.goalVector);
			}
		}

        private void OnValidate()
		{
            //clamps goalPos within valid range
            _goalPos = new Vector2(Mathf.Clamp(_goalPos.x, 0, _gridWorldSize.x), Mathf.Clamp(_goalPos.y, 0, _gridWorldSize.y));
        }
	}
}