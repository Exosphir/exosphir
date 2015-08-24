using UnityEngine;
using System.Collections;

public class ConfigurableInput : MonoBehaviour {

	// Global inputs
	public string verticalAxis = "Vertical";
	public string horizontalAxis = "Horizontal";

	// Edit inputs
	public string orbitKey = "Fire2";
	public KeyCode fastMovementKey = KeyCode.LeftShift;
	public KeyCode upFloor = KeyCode.E;
	public KeyCode downFloor = KeyCode.Q;
	public KeyCode fineFloor = KeyCode.V;
	public KeyCode turnOffSnap = KeyCode.F;
	public KeyCode rotateKey = KeyCode.R;
	public KeyCode secondaryRotate = KeyCode.LeftShift;
	public KeyCode scaleKey = KeyCode.C;

	public static ConfigurableInput GetInstance () {
		ConfigurableInput[] cI = (ConfigurableInput[])GameObject.FindObjectsOfType(typeof(ConfigurableInput));
		if (cI != null && cI.Length == 1) {
			return cI[0];
		} else if (cI.Length > 1) {
			Debug.LogError("Too many ConfigurableInputs found");
			return null;
		} else {
			Debug.LogError("No type ConfigurableInput found");
			return null;
		}
	}
}
