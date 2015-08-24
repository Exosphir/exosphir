using UnityEngine;
using System.Collections;

public class VisualizerObject : MonoBehaviour {

	private GameObject oldSelectedObject;

	void Update () {
		GameObject selectedObject = BlockCatalog.SelectedBlock();

		// Make sure to only update visual when selected block has changed
		if (selectedObject != oldSelectedObject) {
			UpdateVisualizer(selectedObject);
		}

		oldSelectedObject = selectedObject;
	}

	void UpdateVisualizer (GameObject obj) {
		if (transform.childCount > 0)
			Destroy(transform.GetChild(0).gameObject);

		GameObject visual = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
		visual.transform.parent = transform;
		visual.transform.localPosition = Vector3.zero;
		visual.name = obj.name;
	}
}
