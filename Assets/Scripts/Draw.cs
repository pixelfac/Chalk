using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Draw : MonoBehaviour
{
    Controls controls;
	[SerializeField] bool isDrawing = false;
	[SerializeField] float maxNodeDistance;

	[SerializeField] GameObject chalkLinePrefab;
	GameObject currentLine;
	LineRenderer lr;
	List<Vector2> lineNodes;


	private void Awake()
	{
		controls = new Controls();
		lineNodes = new List<Vector2>();
	}

	private void Update()
	{
		if (isDrawing)
		{
			UpdateLine();
		}
	}

	public void OnEnable()
	{
		controls.Draw.Draw.performed += StartDrawing;
		controls.Draw.Draw.canceled += StopDrawing;
		controls.Draw.Draw.Enable();
	}

	public void OnDisable()
	{
		controls.Draw.Draw.performed -= StartDrawing;
		controls.Draw.Draw.canceled -= StopDrawing;
		controls.Draw.Draw.Disable();
	}

	private void StartDrawing(InputAction.CallbackContext ctx)
	{
		Debug.Log("Started Drawing");
		CreateChalkLine();
		isDrawing = true;
	}

	private void StopDrawing(InputAction.CallbackContext ctx)
	{
		Debug.Log("Stopped Drawing");
		isDrawing = false;
	}

	private void CreateChalkLine()
	{
		currentLine = Instantiate(chalkLinePrefab, Vector3.zero, Quaternion.identity);
		lr = currentLine.GetComponent<LineRenderer>();

		lineNodes.Clear();
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		lineNodes.Add(mousePos);
		lineNodes.Add(mousePos);
		lr.SetPosition(0, lineNodes[0]);
		lr.SetPosition(1, lineNodes[1]);
	}

	private void UpdateLine()
	{
		Debug.Log("Drawing");
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		lineNodes.Add(mousePos);
		lr.positionCount++;
		lr.SetPosition(lr.positionCount-1, mousePos);
	}
}

