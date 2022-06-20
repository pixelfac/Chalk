using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChalkLine
{
    public class LineNode
    {
        public int strongHealth;
        public int weakHealth;
        public Vector2 worldPos;

        public Vector2 strongSideNormal; //normal vector to strong side of linenode
        public Vector2 weakSideNormal;   //normal vector to weak side of linenode

        public LineNode(Vector2 pos)
	    {
            worldPos = pos;
            strongHealth = 1;
            weakHealth = 1;
        }

        //returns appropriate health value (strong/weak) based on which side origin is
        public int GetDirectionalHealth(Vector3 origin)
		{
            float strongSideDist = ((Vector2)origin - strongSideNormal).magnitude;
            float weakSideDist = ((Vector2)origin - weakSideNormal).magnitude;

            if (strongSideDist < weakSideDist)
			{
                return strongHealth;
			}
            else
			{
                return weakHealth;
			}
        }
    }
}