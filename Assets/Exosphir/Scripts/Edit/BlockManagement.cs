using UnityEngine;
using System.Collections;

public class BlockManagement : MonoBehaviour {

	public Transform placedBlocksParent;
	public Transform pooledBlocksParent;

	void Awake () {
		// Allocate the objects for placement
		PreAllocateAllObjects();
	}

	void Update () {
		// Allocate more objects if the amount of certain objects drops to or below 1
		for (int c = 0; c < pooledBlocksParent.childCount; c++) {
			Transform category = pooledBlocksParent.GetChild (c);
			for (int b = 0; b < category.childCount; b++) {
				Transform blockParent = category.GetChild (b);
				if (blockParent.childCount <= 1) {
					AllocateMoreObjects(blockParent.GetChild(0).gameObject, BlockCatalog.FindBlock(blockParent.GetChild(0).gameObject.name));
				}
			}
		}
	}

	//////////////////////////////
	// Block Pooling Functions

	void PreAllocateAllObjects () {
		BlockCatalog blockCatalog = BlockCatalog.GetInstance();

		// Go through each cateogry
		for (int i = 0; i < blockCatalog.categories.Count; i++) {

			GameObject categoryParent = new GameObject(blockCatalog.categories[i].name);
			categoryParent.transform.parent = pooledBlocksParent;
			categoryParent.transform.localPosition = Vector3.zero;

			// Go through each block in category
			for (int j = 0; j < blockCatalog.categories[i].blocks.Count; j++) {
				GameObject parent = new GameObject(blockCatalog.categories[i].blocks[j].model.name + "(s)");
				parent.transform.parent = categoryParent.transform;
				parent.transform.localPosition = Vector3.zero;

				// Spawn the amount of each block
				int amount = 0;
				while (amount < blockCatalog.categories[i].blocks[j].allocationAmount) {
					GameObject newlySpawnedThing = GameObject.Instantiate(blockCatalog.categories[i].blocks[j].model, Vector3.zero, Quaternion.identity) as GameObject;
					newlySpawnedThing.name = blockCatalog.categories[i].blocks[j].model.name;
					newlySpawnedThing.transform.parent = parent.transform;
					newlySpawnedThing.transform.localPosition = Vector3.zero;
					
					amount++;
				}
			}
		}
	}

	void AllocateMoreObjects (GameObject otherObject, BlockCatalog.Block obj) {
		int amount = 0;
		while (amount < obj.allocationAmount) {
			GameObject newlySpawnedThing = GameObject.Instantiate(obj.model, Vector3.zero, Quaternion.identity) as GameObject;
			newlySpawnedThing.name = obj.model.name;
			newlySpawnedThing.transform.parent = otherObject.transform.parent.transform;
			newlySpawnedThing.transform.localPosition = Vector3.zero;
			
			amount++;
		}
	}

	public static GameObject GetObject (string name) {
		BlockManagement instance = BlockManagement.GetInstance();
		
		// Find matching parent in BlockManagement
		GameObject parent = null;

		// Cycle through each category
		for (int c = 0; c < instance.pooledBlocksParent.childCount; c++) {
			Transform category = instance.pooledBlocksParent.GetChild (c);
			// Check to see if the block parent equals the block the function wants
			for (int b = 0; b < category.childCount; b++) {
				Transform blockParent = category.GetChild (b);
				if (blockParent.name.Replace("(s)", "") == name) {
					parent = blockParent.gameObject;
				}
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
		
		// Give one block away
		// Index of child does not matter because all the children are the same
		GameObject go = parent.transform.GetChild(0).gameObject;

		return go;
	}

	public static GameObject InstantiateObject (string name, Vector3 position, Quaternion rotation) {
		GameObject go = BlockManagement.GetObject(name);
		go.transform.parent = null;
		go.transform.position = position;
		go.transform.rotation = rotation;
		
		return go;
	}

	//////////////////////////////
	// Block Placement Functions

	public static GameObject PlaceBlock (GameObject obj, Vector3 position, Quaternion rotation, Vector3 scale) {
		if (obj == null)
			return null;

		BlockManagement blockManagement = BlockManagement.GetInstance();

		// Check to see if there isnt already a block there
		for (int a = 0; a < blockManagement.placedBlocksParent.childCount; a++) {
			if (blockManagement.placedBlocksParent.GetChild(a).position == position) {
				return null;
			}
		}

		GameObject returnObject = BlockManagement.InstantiateObject(obj.name, position, rotation) as GameObject;
		returnObject.name = obj.name;
		returnObject.transform.parent = blockManagement.placedBlocksParent;
		returnObject.transform.position = position;
		returnObject.transform.rotation = rotation;
		returnObject.transform.localScale = scale;
		returnObject.tag = "ActiveBlock";

		return returnObject;
	}

	public static GameObject PlaceBlockNoPooling (GameObject obj, Vector3 position, Quaternion rotation, Vector3 scale) {
		if (obj == null)
			return null;
		
		BlockManagement blockManagement = BlockManagement.GetInstance();
		
		GameObject returnObject = GameObject.Instantiate(obj, position, rotation) as GameObject;
		returnObject.name = obj.name;
		returnObject.transform.parent = blockManagement.placedBlocksParent;
		returnObject.transform.position = position;
		returnObject.transform.rotation = rotation;
		returnObject.transform.localScale = scale;
		returnObject.tag = "ActiveBlock";
		
		return returnObject;
	}

	public static void RemoveBlocksAtPosition (Vector3 position, float checkingRaduis) {

		BlockManagement blockManagement = BlockManagement.GetInstance();

		Collider[] colliders;
		colliders = Physics.OverlapSphere(position, checkingRaduis);

		for (int c = 0; c < colliders.Length; c++) {
			for (int a = 0; a < blockManagement.placedBlocksParent.childCount; a++) {
				if (blockManagement.placedBlocksParent.GetChild(a).gameObject == colliders[c].gameObject) {
					Destroy (blockManagement.placedBlocksParent.GetChild(a).gameObject);
				}
			}
		}
	}

	//////////////////////////////
	// Return Functions

	public static BlockManagement GetInstance () {
		BlockManagement[] bM = (BlockManagement[])GameObject.FindObjectsOfType(typeof(BlockManagement));
		if (bM != null && bM.Length == 1) {
			return bM[0];
		} else if (bM.Length > 1) {
			Debug.LogError("Too many BlockManagements found");
			return null;
		} else {
			Debug.LogError("No type BlockManagement found");
			return null;
		}
	}
}
