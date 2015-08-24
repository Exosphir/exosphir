using UnityEngine;
using System.Collections;

public class WorldObjectPooling : MonoBehaviour {

	public GameObject[] preloadObjects;
	public int[] amountOfEachObject;

	private bool allocating = false;

	void OnLevelWasLoaded(int level) {
		if (!allocating)
			PreAllocateObjects();
	}

	void Awake () {
		if (!allocating)
			PreAllocateObjects();
	}

	void PreAllocateObjects () {
		allocating = true;

		if (amountOfEachObject.Length != preloadObjects.Length) {
			Debug.LogError("Amount != PreloadObjects");
			return;
		}
		
		for (int i = 0; i < preloadObjects.Length; i++) {
			GameObject parent = new GameObject(preloadObjects[i].name + "(s)");
			parent.transform.parent = transform;

			int amount = 0;
			while (amount < amountOfEachObject[i]) {
				GameObject newlySpawnedThing = GameObject.Instantiate(preloadObjects[i], transform.TransformPoint(Vector3.zero), Quaternion.identity) as GameObject;
				newlySpawnedThing.name = preloadObjects[i].name;
				newlySpawnedThing.transform.parent = parent.transform;

				amount++;
			}
		}
	}

	public static GameObject GetObject (string name) {
		WorldObjectPooling instance = WorldObjectPooling.GetInstance();

		// Find matching parent in WorldObjectPooling
		GameObject parent = null;

		for (int c = 0; c < instance.transform.childCount; c++) {
			Transform child = instance.transform.GetChild (c);
			if (child.name.Replace("(s)", "") == name) {
				parent = child.gameObject;
			}
		}

		// Make sure the parent is there
		if (parent == null) {
			Debug.LogError("Cannot find parent");
			return null;
		}

		// Make sure the parent has stuff in it
		if (parent.transform.childCount == 0) {
			Debug.LogError("No more " + name + "s");
			return null;
		}

		// Give that crap away
		GameObject go = parent.transform.GetChild(0).gameObject;
		return go;
	}

	public static GameObject InstantiateObject (string name, Vector3 position, Quaternion rotation) {
		GameObject go = WorldObjectPooling.GetObject(name);
		go.transform.parent = null;
		go.transform.position = position;
		go.transform.rotation = rotation;

		return go;
	}

	public static WorldObjectPooling GetInstance () {
		GameObject obj = GameObject.Find ("WorldObjectPooling");
		return obj.GetComponent<WorldObjectPooling>();
	}
}
