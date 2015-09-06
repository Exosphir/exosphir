using UnityEngine;
using System.Collections;

public class Continuity : MonoBehaviour {

	public enum LevelStatus {
		Edit, Play, Test
	};

	public LevelStatus currentStatus;
	private LevelStatus oldCurrentStatus;

	public GameObject player;

	public GameObject origin;
	public GameObject[] objectsToEnableOnEdit;

	void Update () {
		if (oldCurrentStatus != currentStatus) {
			if (currentStatus == LevelStatus.Edit) {
				Edit();
			}
			if (currentStatus == LevelStatus.Test) {
				Test();
			}
			if (currentStatus == LevelStatus.Play) {
				Play();
			}
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Edit();
		}

		oldCurrentStatus = currentStatus;
	}

	public void Edit () {
		currentStatus = LevelStatus.Edit;

		EnableScriptsInObject(origin, true);
		foreach (GameObject obj in objectsToEnableOnEdit) {
			obj.SetActive(true);
		}

		player.SetActive(false);
	}

	public void Play () {
		currentStatus = LevelStatus.Play;

		ResetPlayer();
		player.SetActive(true);
	}

	public void Test () {
		currentStatus = LevelStatus.Test;

		EnableScriptsInObject(origin, false);
		foreach (GameObject obj in objectsToEnableOnEdit) {
			obj.SetActive(false);
		}

		ResetPlayer();
		player.SetActive(true);
	}

	void ResetPlayer () {
		Rigidbody playerBody = player.GetComponent<Rigidbody>();

		playerBody.velocity = Vector3.zero;
		playerBody.angularVelocity = Vector3.zero;

		playerBody.position = Vector3.zero;
	}

	public void SetCurrentStatus (LevelStatus newStatus) {
		currentStatus = newStatus;
	}

	private static void EnableScriptsInObject (GameObject obj, bool enabledBoolean) {
		foreach (var behaviour in obj.GetComponents<MonoBehaviour>()) {
			behaviour.enabled = enabledBoolean;
		}
	}
}
