using UnityEngine;

public sealed class ConfigurableInput : SingletonBehaviour<ConfigurableInput> {

	// Global inputs
	public string verticalAxis = "Vertical";
	public string horizontalAxis = "Horizontal";

    public float vertical {
        get { return Input.GetAxis(verticalAxis); }
    }

    public float horizontal {
        get { return Input.GetAxis(horizontalAxis); }
    }

    public float scroll {
        get {
            var axis = Input.GetAxis("Mouse ScrollWheel");
            //normalize float
            var abs = Mathf.Abs(axis);
            if (abs > 0.01) {
                axis /= abs;
            }
            return axis;
        }
    }

    public Vector2 mouse {
        get {
            return new Vector2 {
                x = Input.GetAxis("Mouse X"),
                y = Input.GetAxis("Mouse Y")
            };
        }
    }

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
	public KeyCode gridManipulation = KeyCode.G;
}
