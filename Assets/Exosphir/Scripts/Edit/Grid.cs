using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour {

	public LineRenderer centerLine1;
	public LineRenderer centerLine2;

	private BlockControl blockControl;

	void Start () {
		blockControl = BlockControl.GetInstance();
	}

	void Update () {
		Vector3 newPosition = transform.position;
		newPosition.y = blockControl.GetLerpedFloorInWorld();
		transform.position = newPosition;

		centerLine1.SetPosition(0, new Vector3(-1250.0f, blockControl.GetLerpedFloorInWorld() - 1.25f, 1.25f));
		centerLine1.SetPosition(1, new Vector3(1250.0f, blockControl.GetLerpedFloorInWorld() - 1.25f, 1.25f));

		centerLine2.SetPosition(0, new Vector3(1.25f, blockControl.GetLerpedFloorInWorld() - 1.25f, -1250.0f));
		centerLine2.SetPosition(1, new Vector3(1.25f, blockControl.GetLerpedFloorInWorld() - 1.25f, 1250.0f));
	}

	static public Vector3 SnapToGrid (Vector3 point, Vector3 gridScale) {
		return new Vector3(Mathf.Round(point.x / gridScale.x) * gridScale.x,
		                   Mathf.Round(point.y / gridScale.y) * gridScale.y,
		                   Mathf.Round(point.z / gridScale.z) * gridScale.z);
	}
}
