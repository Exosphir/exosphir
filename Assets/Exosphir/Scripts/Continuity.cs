using UnityEngine;
using System.Collections;
using Extensions;

public class Continuity : MonoBehaviour {

	public enum LevelStatus {
		Edit, Play, Test
	};

	public LevelStatus currentStatus;
	private LevelStatus oldCurrentStatus;

	public GameObject playerPrefab;
    public GameObject playerCameraPrefab;

	private GameObject player;
	private GameObject playerCamera;

    public GameObject origin;
	public GameObject[] objectsToEnableOnEdit;

    private GameObject startFlag;

	void Awake () {
		player = GameObjectExtensions.PerserveNameOnInstantiate(playerPrefab, Vector3.zero, Quaternion.identity);
		playerCamera = GameObjectExtensions.PerserveNameOnInstantiate(playerCameraPrefab, Vector3.zero, Quaternion.identity);

		playerCamera.GetComponent<CharacterCamera>().target = player.GetComponentInChildren<CharacterLook>().transform;

		DeactivatePlayer();
	}

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

		DeactivatePlayer();
	}

	public void Play () {
		currentStatus = LevelStatus.Play;

		ResetPlayer();
		ActivatePlayer();
    }

	public void Test () {
		currentStatus = LevelStatus.Test;

		EnableScriptsInObject(origin, false);
		foreach (GameObject obj in objectsToEnableOnEdit) {
			obj.SetActive(false);
		}

		ResetPlayer();
		ActivatePlayer();
    }

	void ResetPlayer () {
		Rigidbody playerBody = player.GetComponent<Rigidbody>();

		playerBody.velocity = Vector3.zero;
		playerBody.angularVelocity = Vector3.zero;

        if (GameObject.FindWithTag("StartPoint") != null)
        {
			// Setting a reference to the flag to improve performance and reduce code length
            startFlag = GameObject.FindWithTag("StartPoint");
			// Positioning and rotating the player to be in front and right under the cloth of the flag, and facing where the cloth faces.
            playerBody.position = startFlag.transform.position;
			playerBody.position += startFlag.transform.forward * 1f;
			playerBody.position += startFlag.transform.up * 2f;
			playerBody.rotation = startFlag.transform.rotation;
        }
        else
        {
			// Default starting position.
            playerBody.position = Vector3.zero;
        }
	}

	public void SetCurrentStatus (LevelStatus newStatus) {
		currentStatus = newStatus;
	}

	private static void EnableScriptsInObject (GameObject obj, bool enabledBoolean) {
		foreach (var behaviour in obj.GetComponents<MonoBehaviour>()) {
			behaviour.enabled = enabledBoolean;
		}
	}

	void ActivatePlayer () {
		player.SetActive(true);
		playerCamera.SetActive(true);
	}

	void DeactivatePlayer () {
		player.SetActive(false);
		playerCamera.SetActive(false);
	}
}
