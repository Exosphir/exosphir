using UnityEngine;
using System.Collections;

public class CharacterLook : MonoBehaviour {

	public GameObject characterBody;
	private Rigidbody charRigidbody;

	public Vector2 sensitivity = new Vector2(2.0f, 2.0f);
	public Vector2 smoothing = new Vector2(3.0f, 3.0f);

	public float clampXMin = -90.0f;
	public float clampXMax = 90.0f;

	private Vector2 mouseAbsolute; // Axis are in character space not mouse space
	private Vector2 smoothMouse;
	
	void Start () {
		charRigidbody = characterBody.GetComponent<Rigidbody>();
	}

	void Update () {
		Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		mouseInput = Vector2.Scale(mouseInput, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

		smoothMouse.x = Mathf.Lerp(smoothMouse.x, mouseInput.y, 1f / smoothing.x);
		smoothMouse.y = Mathf.Lerp(smoothMouse.y, mouseInput.x, 1f / smoothing.y);

		mouseAbsolute += smoothMouse;

		mouseAbsolute.x = ClampAngle(mouseAbsolute.x, clampXMin, clampXMax);
		mouseAbsolute.y = InterpolateAngle(mouseAbsolute.y);

		// Apply rotation to...

		// ... the camera:
		transform.localRotation = Quaternion.Euler(new Vector3(-mouseAbsolute.x, 0.0f, 0.0f));

		// ... the character;
		charRigidbody.rotation = Quaternion.Euler(new Vector3(0.0f, mouseAbsolute.y, 0.0f));
	}

	static public float ClampAngle (float angle, float min, float max) {
		InterpolateAngle(angle);
		return Mathf.Clamp (angle, min, max);
	}

	static public float InterpolateAngle (float angle) {
		if (angle < -360f)
			angle += 360f;
		if (angle > 360f)
			angle -= 360f;
		return angle;
	}
}
