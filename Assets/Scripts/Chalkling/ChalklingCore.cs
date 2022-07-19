using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;
using Utilities;

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
			grid = GetComponent<Grid2D>();
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

		private void Attack()
		{
			if (!canAttack) { return; }
			Debug.Log("Chalkling Attacked");
			//TODO get curr grid node
			Node2D currGridNode = grid.NodeFromWorldPoint(transform.position);
			//get line, if !null
			if (currGridNode.nearestLine == null) { return; }
			Collider2D nearestLine = currGridNode.nearestLine;
			//get closest node on line
			int lineNodeIndex = currGridNode.nearestLineNodeIndex;
			//damage that node on the line
			nearestLine.gameObject.GetComponent<ChalkLine.ChalkLine>().Damage(atkDmg, lineNodeIndex);

		}	


		private void Activate()
		{
			//enable attacking
			MECExtras.CallRepeating(Attack, 1 / atkSpd);
			//enable movement component
			movement.enabled = true;
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