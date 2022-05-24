using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Node2D
    {
        public int gCost, hCost;
        public int goalDist;
        public bool obstacle;
        public bool visited;
        public bool inOpen;
        public Vector2 goalVector;
        public Vector3 worldPosition;

        public int GridX, GridY;
        public Node2D parent;


        public Node2D(bool _obstacle, Vector3 _worldPos, int _gridX, int _gridY)
        {
            obstacle = _obstacle;
            worldPosition = _worldPos;
            GridX = _gridX;
            GridY = _gridY;
            goalDist = int.MaxValue;
            visited = true;
            inOpen = false;
            goalVector = Vector2.zero;
        }

        public int FCost
        {
            get
            {
                return gCost + hCost;
            }

        }
    

        public void SetObstacle(bool isOb)
        {
            obstacle = isOb;
        }
    }
}