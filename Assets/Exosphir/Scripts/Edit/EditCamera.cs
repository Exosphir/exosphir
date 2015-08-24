using UnityEngine;
using System.Collections;

public class EditCamera : MonoBehaviour {

	private Transform cam;
	private ConfigurableInput input;

	public float xMoveSpeed = 10.0f;
	public float yMoveSpeed = 10.0f;

	public float fastMoveMulitplier = 2.0f;

	public float xSpeed = 80.0f;
	public float ySpeed = 80.0f;

	public float yMinLimit = -90.0f;
	public float yMaxLimit = 90.0f;

	public float distanceMin = 0.5f;
	public float distanceMax = 10.0f;

	private float x = 40.0f;
	private float y = 40.0f;

	private float distance = 0.0f;

	private bool disableZoom = false;

	void Start () {
		cam = transform.GetComponentInChildren<Camera>().transform;
		distance = cam.localPosition.z;
		input = ConfigurableInput.GetInstance();

		x = transform.rotation.eulerAngles.x;
		y = transform.rotation.eulerAngles.y;
	}

	void LateUpdate () {
		// Move the camera pivot around using WASD or Arrows
		MoveCamera();

		// Rotate pivot when option and LMB is down or MMB is down
		RotateCamera();

		// Zoom in and out using scroll wheel if not disabled
		if (!disableZoom) {
			ZoomCamera();
		}
	}

	private void MoveCamera () {
		float inputX = Input.GetAxis(input.horizontalAxis);
		float inputY = Input.GetAxis(input.verticalAxis);
		
		float tempXMoveSpeed = xMoveSpeed * (Input.GetKey(input.fastMovementKey)? fastMoveMulitplier : 1.0f);
		float tempYMoveSpeed = yMoveSpeed * (Input.GetKey(input.fastMovementKey)? fastMoveMulitplier : 1.0f);
		Vector3 worldDir = cam.TransformDirection(new Vector3(inputX * tempXMoveSpeed * 0.02f, 0.0f, inputY * tempYMoveSpeed * 0.02f));
		worldDir.y = 0.0f;
		
		Vector3 newPosition = transform.position;
		newPosition += worldDir;
		BlockControl blockControl = GetComponent<BlockControl>();
		newPosition.y = blockControl.GetLerpedFloorInWorld();
		transform.position = newPosition;
	}

	private void RotateCamera () {
		if ((Input.GetButton(input.orbitKey) && Input.GetMouseButton(0)) || Input.GetMouseButton(2)) {
			x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			
			transform.rotation = Quaternion.Euler(new Vector3(y, x, 0.0f));
		}
	}

	private void ZoomCamera () {
		distance += Input.GetAxis("Mouse ScrollWheel");
		distance = Mathf.Clamp (distance, -distanceMax, -distanceMin);
		Vector3 newLocalPosition = cam.transform.localPosition;
		newLocalPosition.z = distance;
		cam.transform.localPosition = newLocalPosition;
	}

	public void DisableZoom () {
		disableZoom = true;
	}

	public void EnableZoom () {
		disableZoom = false;
	}

	static private float ClampAngle (float angle, float min, float max) {
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
}
