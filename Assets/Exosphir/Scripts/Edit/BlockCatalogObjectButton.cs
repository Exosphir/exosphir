using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlockCatalogObjectButton : MonoBehaviour {

	public int blockInCategory = -1;

	private BlockCatalog theCatalog;

	void Start () {
		theCatalog = BlockCatalog.GetInstance();
	}

	public void ButtonPressed () {
		if (blockInCategory != -1) {
			theCatalog.selectedBlockID[1] = blockInCategory;
		}
		if (blockInCategory == -1) {
			Debug.LogError("blockInCategory not set");
		}
	}
}
