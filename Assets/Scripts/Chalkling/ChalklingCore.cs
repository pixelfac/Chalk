using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorUtilities;

namespace Chalkling
{
	public class ChalklingCore : MonoBehaviour
	{
		[SerializeField] private int maxHP;
		[SerializeField] private float speed;
		[SerializeField] private int currHP;

		ChalklingMovement movement;

		private void Awake()
		{
			movement = GetComponent<ChalklingMovement>();
		}

		private void Start()
		{
			currHP = maxHP;
			movement._moveSpeed = speed;
		}

		public void Damage(int damage)
		{
			currHP -= damage;

			UpdateState();
		}

		private void UpdateState()
		{
			if (currHP <= 0)
			{
				Destroy(gameObject);
			}
			else if (currHP > maxHP)
			{
				currHP = maxHP;
			}
		}
	}

}