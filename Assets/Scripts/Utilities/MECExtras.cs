using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MEC;

namespace Utilities
{
	public class MECExtras : MonoBehaviour
	{
		//calls 'action' after 'delay' seconds
		public static void CallDelayed(Action action, float delay)
		{
			Timing.RunCoroutine(CallDelayedRoutine(action, delay));
		}

		private static IEnumerator<float> CallDelayedRoutine(Action action, float delay)
		{
			yield return Timing.WaitForSeconds(delay);
			action?.Invoke();
		}

		//calls 'action' immediately, and then repeatedly on 'delay' second delay
		public static void CallRepeating(Action action, float delay)
		{
			Timing.RunCoroutine(CallRepeatingRoutine(action, delay), Segment.FixedUpdate);
		}

		private static IEnumerator<float> CallRepeatingRoutine(Action action, float delay)
		{
			while (true)
			{
				action?.Invoke();
				yield return Timing.WaitForSeconds(delay);
			}
		}
	}
}