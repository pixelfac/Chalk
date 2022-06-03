using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMissile : MonoBehaviour
{
	private GameObject _startCircle;
	private GameObject _endCircle;

	private float _strength;
	private Vector2 _direction;
	private LineRenderer _lr;

	private void Awake()
	{
		_lr = GetComponent<LineRenderer>();
	}

	//basically a constructor, but since can't call constructor
	//on gameobject prefab component, this is the best alternative
	public void Init(GameObject startCircle, GameObject endCircle, float length, LineRenderer lr)
	{
		Vector3 startPos = startCircle.transform.position;
		Vector3 endPos = endCircle.transform.position;

		_direction = (endPos - startPos).normalized;

		_lr.SetPositions(new Vector3[] { startPos, endPos });

		_startCircle = startCircle;
		_endCircle = endCircle;
		_startCircle.transform.position = startPos;
		_endCircle.transform.position = endPos;

		_strength = CalcStrength();

		CopyLR();

		float CalcStrength()
		{
			return length;
		}

		//copy relevant line renderer values
		void CopyLR()
		{
			_lr.endWidth = lr.endWidth;
			_lr.startWidth = lr.startWidth;
			_lr.startColor = lr.startColor;
			_lr.endColor = lr.endColor;
			_lr.materials = lr.materials;
		}
	}
}