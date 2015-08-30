using UnityEngine;
using System.Collections;

public class MainMenuCamera : MonoBehaviour {

	public float clampX = 10.0f;
	public float clampY = 10.0f;

	public float rotationDelta = 2.0f;

	private Vector3 oldMousePos;
	private Quaternion startCameraRotation;
	private Vector3 rotationAdder;

	void Start () {
		startCameraRotation = transform.rotation;
	}

	void Update () {
		if (Input.GetMouseButton(0)) {
			Vector3 realitiveMosePos = (oldMousePos - Input.mousePosition).normalized;

			rotationAdder += new Vector3(realitiveMosePos.y, -realitiveMosePos.x, 0.0f);
			rotationAdder = new Vector3(Mathf.Clamp(rotationAdder.x, -clampX, clampX), Mathf.Clamp(rotationAdder.y, -clampY, clampY), 0.0f);

			transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(startCameraRotation.eulerAngles + rotationAdder), Time.deltaTime * rotationDelta);

			oldMousePos = Input.mousePosition;
		} else {
			transform.rotation = Quaternion.Lerp(transform.rotation, startCameraRotation, Time.deltaTime * rotationDelta);
		}
	}
}
