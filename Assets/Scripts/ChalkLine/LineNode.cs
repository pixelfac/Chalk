using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChalkLine
{
    public class LineNode
    {
        public int health { get; set; }
        public Vector2 nodePos { get; }

        public LineNode(Vector2 pos)
	    {
            nodePos = pos;
            health = -1;
	    }
    }
}