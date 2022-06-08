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
        [Range(0.1f, 1f)]
        [SerializeField] private float nodeOverlapRadius; //how big the OverlapBox radius is relative to nodeRadius

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
            UpdateObstacles();    //set which nodes are obstacles
            ComputeDistField();   //calc distance to goal around obstacles
            ComputeVectorField(); //calc vectors to goal from distance
        }

        private void UpdateObstacles()
	    {
            //for loop faster than foreach
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 worldPoint = WorldPointFromGridPos(x, y);
                    _Grid[x, y] = new Node2D(false, worldPoint, x, y);

                    //using ContactFilter2D for more performance
                    ContactFilter2D overlapFilter = new ContactFilter2D();
                    overlapFilter.layerMask = _obstacleMask;
                    overlapFilter.useLayerMask = true;
                    Collider2D[] results = new Collider2D[1];

                    if (Physics2D.OverlapBox(worldPoint, Vector2.one * nodeRadius * nodeOverlapRadius, 0f, overlapFilter, results) != 0) //0 == no collision
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
            //for loop faster than foreach
            for (int x = 0; x < _Grid.GetLength(0); x++)
            {
                for (int y = 0; y < _Grid.GetLength(1); y++)
                {
                    Node2D n = _Grid[x, y];

                    n.goalDist = int.MaxValue;
                    n.visited = false;
                    n.inOpen = false;
                    n.goalVector = Vector2.zero;
                }
			}
		}

        //Vector-goal pathfinding
        //computes distance for each node to goal
        private void ComputeDistField()
		{
            ResetGoalDists();

            Node2D goalNode = NodeFromWorldPoint(_goalPos);
            goalNode.goalDist = 0;

            if (goalNode.obstacle)
            {
                //path not possible, grid defaults to max distance for all nodes
                Debug.LogError("Grid goal is on an obstacle node");
                return;
            }

            Queue<Node2D> open = new Queue<Node2D>();
            open.Enqueue(goalNode);
            goalNode.inOpen = true;

            //Dijkstra's-esque iterate through all nodes and assign distance from goal
            Node2D currNode;
            while (open.Count > 0)
			{
                currNode = open.Dequeue();
                currNode.visited = true;

                List<Node2D> neighbors = GetNeighbors(currNode);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Node2D n = neighbors[i];

                    if (n.obstacle) { continue; }

                    int nodeDist = currNode.goalDist + GetDistance(n, currNode);
                    if (n.visited)
                    {
                        if (n.goalDist > nodeDist)
						{
                            n.goalDist = nodeDist;
                        }
                    }
                    else if (!n.inOpen)
					{
                        n.goalDist = nodeDist;
                        open.Enqueue(n);
                        n.inOpen = true;
					}
                }
			}
        }

        //Vector-goal pathfinding
        //computes vector field directing towards goal
        private void ComputeVectorField()
        {
            //for loop faster than foreach
            for (int x = 0; x < _Grid.GetLength(0); x++)
			{
                for (int y = 0; y < _Grid.GetLength(1); y++)
                {
                    Node2D n = _Grid[x, y];
                    List<Node2D> neighbors = GetNeighbors(n);

                    //does neighbors contain an obstacle?
                    bool containsObstacle = neighbors.Any(n => n.obstacle);

                    if (containsObstacle)
                    {
                        VectorMin(n, neighbors);
                    }
                    else
                    {
                        VectorGradient(n, neighbors);
                    }
                }
			}

            //sets node vector to be towards min of neighbor dist
            void VectorMin(Node2D n, List<Node2D> neighbors)
			{
                int minDist = int.MaxValue;
                Vector2 direction = Vector2.zero;
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Node2D nbr = neighbors[i];

                    if (nbr.goalDist < minDist)
                    {
                        minDist = nbr.goalDist;
                        direction = (nbr.worldPosition - n.worldPosition).normalized;
                        n.goalVector = direction;
                    }
                }
            }

            //sets node vector to be weighted gradient of neighbor dist
            void VectorGradient(Node2D n, List<Node2D> neighbors)
			{
                //find lowest gradient
                Vector2 goalVec = Vector2.zero;
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Node2D nbr = neighbors[i];

                    Vector2 direction = (nbr.worldPosition - n.worldPosition).normalized;
                    goalVec += direction / nbr.goalDist;
                }
                n.goalVector = goalVec.normalized;
            }


        }

        //gets the neighboring nodes in the 4 cardinal directions. If you would like to enable diagonal pathfinding, uncomment out that portion of code
        public List<Node2D> GetNeighbors(Node2D node)
        {
            List<Node2D> neighbors = new List<Node2D>();

            //top neighbor
            AddNeighbor(0, 1);
            //bottom neighbor
            AddNeighbor(0, -1);
            //right neighbor
            AddNeighbor(1, 0);
            //left neighbor
            AddNeighbor(-1, 0);
            //topleft neighbor
            AddNeighbor(-1, +1);
            //bottomleft neighbor
            AddNeighbor(-1, -1);
            //bottomright neighbor
            AddNeighbor(+1, -1);
            //topright neighbor
            AddNeighbor(+1, +1);

            return neighbors;

            //Adds neighbor node, if valid position in Grid
            void AddNeighbor(int x, int y)
			{
                if (CoordsInGrid(node.GridX + x, node.GridY + y))
                {
                    neighbors.Add(_Grid[node.GridX + x, node.GridY + y]);
                }
            }

            //return true if parameters are in Grid, false otherwise
            bool CoordsInGrid(int x, int y)
            {
                return (x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y);
            }
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

            //for loop faster than foreach
            for (int x = 0; x < _Grid.GetLength(0); x++)
            {
                for (int y = 0; y < _Grid.GetLength(1); y++)
                {
                    Node2D n = _Grid[x, y];
                    //draw cubes
                    if (n == null) { continue; }

                    if (n.obstacle)
                    {
                        Gizmos.color = Color.red;
                    }
                    else if (n.goalDist % 30 < 10)
                    {
                        Gizmos.color = Color.green;
                    }
                    else if (n.goalDist % 30 < 20)
                    {
                        Gizmos.color = Color.cyan;
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                    }

                    Gizmos.DrawCube(n.worldPosition, Vector3.one * 0.9f * (nodeRadius));

                    //draw lines
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(n.worldPosition, n.worldPosition + nodeRadius * (Vector3)n.goalVector);
                }
            }
        }

        private void OnValidate()
		{
            //clamps goalPos within valid range
            _goalPos = new Vector2(Mathf.Clamp(_goalPos.x, 0, _gridWorldSize.x), Mathf.Clamp(_goalPos.y, 0, _gridWorldSize.y));
        }
	}
}