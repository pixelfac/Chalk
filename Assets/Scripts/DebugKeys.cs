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
		Debug.Log("DeleteCollider2D called");
		Vector3 mousePt = Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
		Collider2D collidedLine = Physics2D.OverlapCircle(mousePt, 0.1f);
		Destroy(collidedLine.gameObject);
	}
}
