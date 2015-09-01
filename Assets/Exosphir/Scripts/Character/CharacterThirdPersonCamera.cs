using UnityEngine;
using System.Collections;

public class CharacterThirdPersonCamera : MonoBehaviour {

	public Transform target;
	
	void FixedUpdate () {
		transform.position = target.position;
		transform.rotation = target.rotation;
	}
}
