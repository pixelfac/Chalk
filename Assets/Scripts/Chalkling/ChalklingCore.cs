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
		[Header("HP Parameters")]
		[SerializeField] private float maxHP;
		[SerializeField] private float currHP;

		[Header("Attack Parameters")]
		[SerializeField] private float atkDmg;
		[Range(0.1f, 10)]
		[SerializeField] private float atkSpd;
		[SerializeField] public bool canAttack { get; private set; }

		[Header("Misc Parameters")]
		[SerializeField] private float speed;
		[SerializeField] private float spawnDelay; //delay on creation before attacking
		
		[Header("UI Dependencies")]
		[SerializeField] private Transform hpBarTransform;

		//Component Dependencies
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

		public void Damage(float damage)
		{
			currHP -= damage;

			UpdateState();
		}

		private void UpdateState()
		{
			
			UpdateHP();

			void UpdateHP()
			{
				if (currHP <= 0)
				{
					Destroy(gameObject);
				}
				else if (currHP > maxHP)
				{
					currHP = maxHP;
				}
				UpdateHPBar();
			}

			void UpdateHPBar()
			{
				float hpPercent = currHP / maxHP;
				//set new bar width
				Vector3 newScale = hpBarTransform.localScale;
				newScale.x = hpPercent;
				hpBarTransform.localScale = newScale;
				//set new bar position
				Vector3 newPos = hpBarTransform.localPosition;
				newPos.x = 0.5f - hpPercent / 2; //offset the bar after scaling
				hpBarTransform.localPosition = newPos;
			}
		}

		private void Activate()
		{
			//enable attacking
			Timing.RunCoroutine(AttackRoutine(Attack, 1 / atkSpd).CancelWith(gameObject));
			//enable movement component
			movement.enabled = true;
		}

		private void Attack()
		{
			int lineNodeIndex;
			GameObject nearestLineGO = AttackPossible(out lineNodeIndex);
			if (!canAttack)
			{
				return;
			}

			ChalkLine.LineWard nearestLineWard = nearestLineGO.GetComponent<ChalkLine.LineWard>();
			//damage that node on the line
			nearestLineWard.Damage(atkDmg, lineNodeIndex, transform.position);
		}

		//false if attack not possible, true otherwise
		private GameObject AttackPossible(out int lineNodeIndex)
		{
			canAttack = false;
			//check if line nearby to attack
			Node2D currGridNode = grid.NodeFromWorldPoint(transform.position);
			lineNodeIndex = currGridNode.nearestLineNodeIndex;
			GameObject nearestLine = currGridNode.nearestLine;
			if (nearestLine == null)
			{
				nearestLine = lineOnNeighbors(out lineNodeIndex);
				if (nearestLine == null)
				{
					lineNodeIndex = -1;
					return null;
				}
			}
			Debug.Log("nearby line");

			canAttack = true;
			return nearestLine;

										//temp var needed b/c can't use `out` vars in local func
			GameObject lineOnNeighbors(out int lineNodeIndexTemp)
			{
				List<Node2D> nbrs = grid.GetNeighbors(currGridNode);

				for (int i = 0; i < nbrs.Count; i++)
				{
					if (nbrs[i].nearestLine != null)
					{
						lineNodeIndexTemp = nbrs[i].nearestLineNodeIndex;
						return nbrs[i].nearestLine.gameObject;
					}
				}
				lineNodeIndexTemp = -1;
				return null;
			}
		}

		private IEnumerator<float> AttackRoutine(Action action, float delay)
		{
			while (true)
			{
				//if unable to attack, check again
				int _;
				if (!AttackPossible(out _))
				{
					yield return Timing.WaitForOneFrame;
				}
				//else, do action and wait
				else
				{
					Debug.Log("Attack possible");
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