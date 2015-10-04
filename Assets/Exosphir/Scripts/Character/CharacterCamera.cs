using UnityEngine;
using System.Collections;

public class CharacterCamera : MonoBehaviour {
	
	public Transform target;

	public float rotationDamp = 2.0f;

	private Quaternion oldRotation;

	private Rigidbody body;

	void Start () {
		body = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate () {
		Vector3 rotationDelta = target.rotation.eulerAngles - oldRotation.eulerAngles;
		Vector3 relativeDelta = target.transform.InverseTransformVector(rotationDelta);

		Vector3 rot = Quaternion.Lerp (body.rotation, target.rotation, Time.fixedDeltaTime * rotationDamp).eulerAngles;
		rot.z = 0.0f;
		body.rotation = Quaternion.Euler(rot);
		body.position = target.position;

		oldRotation = target.rotation;
	}
}
