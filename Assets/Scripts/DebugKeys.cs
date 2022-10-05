using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugKeys : MonoBehaviour
{
    public Controls _controls;

	private void Awake()
	{
		_controls = new Controls();
	}

	private void OnEnable()
	{
		_controls.Debug.Delete_Line.performed += DeleteCollider2D;
		_controls.Debug.Delete_Line.Enable();
	}

	private void OnDisable()
	{
		_controls.Debug.Delete_Line.performed -= DeleteCollider2D;
		_controls.Debug.Delete_Line.Disable();
	}

	//when called, detects for a collider2d under cursor. If founds, destroys the object
	private void DeleteCollider2D(InputAction.CallbackContext ctx)
	{
		Vector3 mousePt = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue(), Camera.MonoOrStereoscopicEye.Mono);
		Collider2D collidedLine = Physics2D.OverlapCircle(mousePt, 0.1f);
		if (collidedLine)
		{
			Debug.Log("Collider2D Destroyed");
			Destroy(collidedLine.gameObject);
		}
	}
}
