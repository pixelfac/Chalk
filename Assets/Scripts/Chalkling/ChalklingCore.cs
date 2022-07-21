using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;
using Utilities;
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
			movement.enabled = false;
			grid = FindObjectOfType<Grid2D>();
		}

		private void Start()
		{
			currHP = maxHP;
			movement._moveSpeed = speed;
			canAttack = true;

			MECExtras.CallDelayed(Activate, spawnDelay);
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

		private void Activate()
		{
			//enable attacking
			Timing.RunCoroutine(AttackRoutine(Attack, 1 / atkSpd));
			//enable movement component
			movement.enabled = true;
		}

		private void Attack()
		{
			Debug.Log("Chalkling Attack Attempt");
			if (!AttackPossible())
			{
				return;
			}
			//get closest node on line
			Node2D currGridNode = grid.NodeFromWorldPoint(transform.position);
			int lineNodeIndex = currGridNode.nearestLineNodeIndex;
			ChalkLine.ChalkLine nearestLineNode = currGridNode.nearestLine.gameObject.GetComponent<ChalkLine.ChalkLine>();
			//damage that node on the line
			nearestLineNode.Damage(atkDmg, lineNodeIndex, transform.position);
		}

		//false if attack not possible, true otherwise
		private bool AttackPossible()
		{
			canAttack = false;
			//check if line nearby to attack
			Node2D currGridNode = grid.NodeFromWorldPoint(transform.position);
			Collider2D nearestLine = currGridNode.nearestLine;
			if (nearestLine == null) { return false; }

			canAttack = true;
			return true;
		}


		private IEnumerator<float> AttackRoutine(Action action, float delay)
		{
			while (true)
			{
				//if unable to attack, check again
				if (!AttackPossible())
				{
					Debug.Log("Attack not possible");
					yield return Timing.WaitForOneFrame;
				}
				//else, do action and wait
				else
				{
					action?.Invoke();
					yield return Timing.WaitForSeconds(delay);
				}
			}
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