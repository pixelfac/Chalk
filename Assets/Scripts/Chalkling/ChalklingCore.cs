using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorUtilities;
using Pathfinding;
using MEC;

namespace Chalkling
{
	public class ChalklingCore : MonoBehaviour
	{
		[SerializeField] private int maxHP;
		[SerializeField] private float speed;
		[SerializeField] private int currHP;
		[SerializeField] private float spawnDelay; //delay on creation before attacking

		[SerializeField] private float atkDmg;
		[Range(0.1f, 10)]
		[SerializeField] private float atkSpd;
		public bool canAttack { get; private set; }

		ChalklingMovement movement;
		Grid2D grid;

		private void Awake()
		{
			movement = GetComponent<ChalklingMovement>();
			grid = GetComponent<Grid2D>();
		}

		private void Start()
		{
			currHP = maxHP;
			movement._moveSpeed = speed;
			canAttack = true;

			StartAttacking();
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

		private IEnumerator<float> AttackRoutine()
		{
			yield return Timing.WaitForSeconds(spawnDelay);
			while (true)
			{
				if (!canAttack) { continue; }
				Debug.Log("Chalkling Attacked");
				//TODO get curr grid node
				//get line, if !null
				//get closest node on line
				//damage that node on the line

				yield return Timing.WaitForSeconds(1 / atkSpd);
			}
		}

		private void StartAttacking()
		{
			Debug.Log("Start Attacking");
			Timing.RunCoroutine(AttackRoutine(), Segment.FixedUpdate);
		}


		private Node2D GetCurrentNode2D()
		{
			if (grid == null)
			{
				Debug.LogError("Grid Reference is null in chalkling");
			}

			return grid.NodeFromWorldPoint(transform.position);
		}

		private void OnValidate()
		{
			atkSpd = Mathf.Max(atkSpd, 0.1f);
		}
	}
}