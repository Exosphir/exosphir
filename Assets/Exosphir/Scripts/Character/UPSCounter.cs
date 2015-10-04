using UnityEngine;
using System.Collections;

public class UPSCounter : MonoBehaviour {

	public enum Units {
		MetersPerSecond
	}

	public enum DisplayArea {
		TopLeft,
		Center
	}

	public Rigidbody body;
	public Units unitToUse = Units.MetersPerSecond;

	// X and Y values are ignored
	public Rect displayArea = new Rect(0, 0, 200, 100);
	public DisplayArea displayAreaLocation = DisplayArea.TopLeft;

	public int amountOfDecimals = 2;

	[HideInInspector]
	public float finalOutput;

	void Start () {
		if (GetComponent<Rigidbody>() != null) {
			body = GetComponent<Rigidbody>();
		}
	}

	void Update () {
		float bodySpeed = body.velocity.magnitude;

		switch (unitToUse) {
		case Units.MetersPerSecond:
			finalOutput = bodySpeed;
			break;
		}

		switch (displayAreaLocation) {
		case DisplayArea.TopLeft:
			displayArea.x = 10.0f;
			displayArea.y = 10.0f;
			break;

		case DisplayArea.Center:
			displayArea.x = (Screen.width / 2) - (displayArea.width / 2);
			displayArea.y = (Screen.height / 2) - (displayArea.height / 2);
			break;
		}
	}

	void OnGUI () {
		GUILayout.BeginArea(displayArea);

		GUILayout.Label(finalOutput.ToString("F" + amountOfDecimals.ToString()) + " " + unitToUse.ToString());

		GUILayout.EndArea();
	}
}
