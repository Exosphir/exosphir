using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPhysicsSurroundingsCheck : MonoBehaviour {

	public enum AreaToCheck {
		Ground,
		Sides
	};

	public bool enableDebug = false;
	
	public bool setPlayerBoolean = true;
	public AreaToCheck checkArea = AreaToCheck.Ground;

	public CharacterPhysics player;

	[HideInInspector]
	public List<GameObject> collidedObjects = new List<GameObject>();
	
	void Start () {
		if (setPlayerBoolean) {
			if (player == null) {
				player = transform.parent.GetComponent<CharacterPhysics>();
			}
		}
	}
	
	void OnTriggerEnter (Collider other) {
		collidedObjects.Add(other.gameObject);
	}
	
	void OnTriggerExit (Collider other) {
		for (int i = 0; i < collidedObjects.Count; i++) {
			if (collidedObjects[i] == other.gameObject) {
				collidedObjects.RemoveAt(i);
			}
		}
	}
	
	void Update () {
		if (enableDebug) {
			Debug.Log (collidedObjects.Count);
		}

		bool colliding = (collidedObjects.Count > 0);

		if (setPlayerBoolean) {
			switch (checkArea) {
			case AreaToCheck.Ground:
				player.grounded = colliding;

				bool anyBody = false;

				foreach (GameObject obj in collidedObjects) {
					if (obj.GetComponent<Rigidbody>() != null) {
						player.currentPlatform = obj.GetComponent<Rigidbody>();
						anyBody = true;
					}
				}

				if (collidedObjects.Count == 0 || !anyBody) {
					player.currentPlatform = null;
				}

				break;

			case AreaToCheck.Sides:
				player.isThereWall = colliding;
				break;
			}
		}
	}
}
