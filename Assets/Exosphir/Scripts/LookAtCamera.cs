using UnityEngine;
using System.Collections;

public class LookAtCamera : MonoBehaviour {

	public Camera theCamera;

	private Camera cameraToLookAt;

	void Start () {
		cameraToLookAt = theCamera;

		if (theCamera == null) {
			cameraToLookAt = Camera.main;
		}
	}
	
	void Update () {
		transform.LookAt(cameraToLookAt.transform.position);
	}
}
