using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChalkLine
{
    public class LineNode
    {
        public int strongHealth;
        public int weakHealth;
        public Vector2 nodePos;

        public Vector2 strongSideNormal; //normal vector to strong side of linenode
        public Vector2 weakSideNormal;   //normal vector to weak side of linenode

        public LineNode(Vector2 pos)
	    {
            nodePos = pos;
            strongHealth = 1;
            weakHealth = 1;
        }
    }
}