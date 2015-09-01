using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	public Transform target;

	void FixedUpdate () {
		GetComponent<Rigidbody>().position = target.position;
	}
}
