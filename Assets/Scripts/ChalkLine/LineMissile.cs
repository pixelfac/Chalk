using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Chalkling;

namespace ChalkLine
{
	public class LineMissile : MonoBehaviour
	{
		[SerializeField] private float _speed;
		[SerializeField] private float _baseDmg;
		[SerializeField] private float _lengthScale; //factor to scale length when calc strength

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
		public void Init(GameObject startCircle, GameObject endCircle, float numNodes, LineRenderer lr)
		{
			transform.position = endCircle.transform.position;
			Vector3 startPos = startCircle.transform.position - endCircle.transform.position;
			Vector3 endPos = Vector3.zero;

			float length = (endPos - startPos).magnitude;
			_direction = (endPos - startPos).normalized;

			//set startCircle pos
			_startCircle = startCircle;
			_startCircle.transform.SetParent(transform);
			_startCircle.transform.position = startCircle.transform.position;
			//set endCircle pos
			_endCircle = endCircle;
			_endCircle.transform.SetParent(transform);
			_endCircle.transform.position = endCircle.transform.position;

			_strength = CalcStrength();

			SetLR();
			SetHitbox();

			_rb.velocity = _direction * _speed;

			float CalcStrength()
			{
				return length * _lengthScale;
			}

			//set line renderer values
			void SetLR()
			{
				_lr.endWidth = lr.endWidth;
				_lr.startWidth = lr.startWidth;
				_lr.startColor = lr.startColor;
				_lr.endColor = lr.endColor;
				_lr.materials = lr.materials;

				//number of nodes in line renderer, based on line speed
				int lrNodeLength = (int)(length / (_speed * Time.fixedDeltaTime)) + 1;
				Vector3[] lrNodes = new Vector3[lrNodeLength];

				//Set linerenderer to be array of evenly spaced points, composing a straight line
				for (int i = 0; i < lrNodeLength; i++)
				{
					lrNodes[i] = endPos - (Vector3)_direction * length * i / (float)lrNodeLength;
				}
				lrNodes[lrNodeLength - 1] = startPos;

				_lr.positionCount = lrNodeLength;
				_lr.SetPositions(lrNodes);
			}

			//set edge collider values
			void SetHitbox()
			{
				_hitbox.points = new Vector2[] { startPos, endPos };
				_hitbox.edgeRadius = lr.startWidth * 2;
			}
		}

		private void CollideChalkling(Collider2D collision)
		{
			ChalklingCore chalkling = collision.gameObject.GetComponent<ChalklingCore>();
			chalkling.Damage(CalcDamage());


			float CalcDamage()
			{
				Debug.Log("Missile Strength:" + _strength);
				return _baseDmg + _strength;
			}
		}

		private void CollideWall(Collider2D collision)
		{
			Destroy(gameObject);
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			Debug.Log("collided with " + collision.gameObject.name);

			switch(collision.gameObject.layer)
			{
				case 8: //Chalkling
					CollideChalkling(collision);
					break;
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			Debug.Log("Stopped colliding with " + collision.gameObject.name);

			switch (collision.gameObject.layer)
			{
				case 9: //Wall
					CollideWall(collision);
					break;
			}
		}
	} 
}