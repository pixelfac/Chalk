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

		transform.position = endCircle.transform.position;
		//world coords for transforms, line renderer
		Vector3 worldStartPos = startCircle.transform.position;
		Vector3 worldEndPos = endCircle.transform.position;
		//local coords for hitbox
		Vector3 localStartPos = worldStartPos - worldEndPos;
		Vector3 localEndPos = Vector3.zero;

		_direction = (worldEndPos - worldStartPos).normalized;

		//set startCircle pos
		_startCircle = startCircle;
		_startCircle.transform.SetParent(transform);
		_startCircle.transform.position = worldStartPos;
		//set endCircle pos
		_endCircle = endCircle;
		_endCircle.transform.SetParent(transform);
		_endCircle.transform.position = worldEndPos;

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
			_lr.SetPositions(new Vector3[] { worldStartPos, worldEndPos });
			_lr.endWidth = lr.endWidth;
			_lr.startWidth = lr.startWidth;
			_lr.startColor = lr.startColor;
			_lr.endColor = lr.endColor;
			_lr.materials = lr.materials;
		}

		void SetHitbox()
		{
			_hitbox.points = new Vector2[] { localStartPos, localEndPos };
			_hitbox.edgeRadius = lr.startWidth * 2;
		}
	}
}