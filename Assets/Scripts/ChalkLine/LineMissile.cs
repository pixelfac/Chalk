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
	private EdgeCollider2D _hitbox;
	private Rigidbody2D _rb;

	private void Awake()
	{
		_lr = GetComponent<LineRenderer>();
		_hitbox = GetComponent<EdgeCollider2D>();
		_rb = GetComponent<Rigidbody2D>();
	}

	//basically a constructor, but since can't call constructor
	//on gameobject prefab component, this is the best alternative
	public void Init(GameObject startCircle, GameObject endCircle, float length, LineRenderer lr)
	{
		Vector3 startPos = startCircle.transform.position;
		Vector3 endPos = endCircle.transform.position;

		_direction = (endPos - startPos).normalized;

		_lr.SetPositions(new Vector3[] { startPos, endPos });

		//set startCircle pos
		_startCircle = startCircle;
		_startCircle.transform.SetParent(transform);
		_startCircle.transform.position = startPos;
		//set endCircle pos
		_endCircle = endCircle;
		_endCircle.transform.SetParent(transform);
		_endCircle.transform.position = endPos;

		_strength = CalcStrength();

		SetLR();
		SetHitbox();

		float CalcStrength()
		{
			return length;
		}

		//copy relevant line renderer values
		void SetLR()
		{
			_lr.endWidth = lr.endWidth;
			_lr.startWidth = lr.startWidth;
			_lr.startColor = lr.startColor;
			_lr.endColor = lr.endColor;
			_lr.materials = lr.materials;
		}

		void SetHitbox()
		{
			_hitbox.points = new Vector2[] { startPos, endPos };
			_hitbox.edgeRadius = lr.startWidth * 2;
		}
	}
}