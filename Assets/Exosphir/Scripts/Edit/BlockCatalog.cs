using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BlockCatalog : MonoBehaviour {
	
	[System.Serializable]
	public class Category {
		public List<Block> blocks = new List<Block>(1);
		public string name;
	}

	[System.Serializable]
	public class RenderPreviewSettings {
		public float distanceFromObject = 6.0f;
		public Vector3 positionOfPivot = Vector3.zero;
		public Vector3 rotationOfPivot = new Vector3(0.0f, 45.0f, 40.0f);

		public bool rayTraceSolution = false;
	}

	[System.Serializable]
	public class Block {
		public GameObject model;

		public Texture2D previewImage;
		public RenderPreviewSettings previewImageSettings = new RenderPreviewSettings();

		public bool rotateable = true;
		public bool scaleable = true;

		public int id;
	}

	public List<Category> categories = new List<Category>(1);
	public List<bool> showBlocks = new List<bool>(1);

	public int[] selectedBlockID = new int[2]; // Element 0 = category, Element 1 = block index

	public float uiWidth = 1.2f;
	public Texture2D nullPreviewTexture;
	public GameObject objectButton;
	private GameObject[][] objectButtons;

	Vector2 scrollPosition;

	void Start () {
		// Convert inches to pixels if avaliable
		float newUIWidth = 270.0f;
		if (Screen.dpi > 0.0f) {
			newUIWidth = Screen.dpi * uiWidth;
		}

		CreateUI();

		AdjustUI(newUIWidth);

		GetComponent<BlockControl>().catalogRect = new Rect(5.0f, 10.0f, newUIWidth, Screen.height);
	}

	void CreateUI () {
		GameObject canvas = GameObject.Find ("Canvas");

		objectButtons = new GameObject[categories.Count][];
		
		for (int i = 0; i < categories.Count; i++) {
			objectButtons[i] = new GameObject[categories[i].blocks.Count];

			for (int b = 0; b < categories[i].blocks.Count; b++) {
				GameObject newButton = GameObject.Instantiate(objectButton);
				newButton.name = "Button_" + categories[i].blocks[b].model.name;
				newButton.transform.SetParent(canvas.transform, false);

				newButton.GetComponent<BlockCatalogObjectButton>().blockInCategory = b;

				// Set the preview image
				if (categories[i].blocks[b].previewImage != null) {
					newButton.transform.GetChild(0).GetComponent<RawImage>().texture = categories[i].blocks[b].previewImage;
				} else {
					newButton.transform.GetChild(0).GetComponent<RawImage>().texture = nullPreviewTexture;
				}

				objectButtons[i][b] = newButton;
			}
		}
	}

	void AdjustUI (float newUIWidth) {
		int i = selectedBlockID[0];

		for (int b = 0; b < objectButtons[i].Length; b++) {
			RectTransform trans = objectButtons[i][b].GetComponent<RectTransform>();

			Vector2 newPos = trans.anchoredPosition;
			Vector2 newScale = trans.sizeDelta;

			if ((b + 1) % 2 == 0) { // Second button of row
				newScale.x = (newUIWidth - 15.0f) / 2;
				newScale.y = newScale.x;
				
				newPos.x = newUIWidth - ((newScale.x / 2) + 5.0f);
				newPos.y = -(10.0f + (newScale.y / 2) - (-newScale.y * Mathf.FloorToInt(b / 2)));
			} else { // First button of row
				newScale.x = (newUIWidth - 15.0f) / 2;
				newScale.y = newScale.x;

				newPos.x = (newScale.x / 2) + 5.0f;
				newPos.y = -(10.0f + (newScale.y / 2) - (-newScale.y * Mathf.FloorToInt(b / 2)));
			}

			trans.anchoredPosition = newPos;
			trans.sizeDelta = newScale;
		}
	}

	public static GameObject SelectedBlock () {
		BlockCatalog blockCatalog = BlockCatalog.GetInstance();

		GameObject selectedBlock = null;

		int[] selectedBlockID = blockCatalog.selectedBlockID;

		int category = selectedBlockID[0];
		int block = selectedBlockID[1];

		selectedBlock = blockCatalog.categories[category].blocks[block].model;

		return selectedBlock;
	}

	public static Block SelectedBlockAsBlock () {
		BlockCatalog blockCatalog = BlockCatalog.GetInstance();
		
		Block selectedBlock = null;
		
		int[] selectedBlockID = blockCatalog.selectedBlockID;
		
		int category = selectedBlockID[0];
		int block = selectedBlockID[1];
		
		selectedBlock = blockCatalog.categories[category].blocks[block];
		
		return selectedBlock;
	}

	public static string SelectedCategoryName () {
		BlockCatalog blockCatalog = BlockCatalog.GetInstance();

		int[] selectedBlockID = blockCatalog.selectedBlockID;
		string categoryName = blockCatalog.categories[selectedBlockID[0]].name;

		return categoryName;
	}

	public static Block FindBlock (string blockName) {
		BlockCatalog blockCatalog = BlockCatalog.GetInstance();

		for (int i = 0; i < blockCatalog.categories.Count; i++) {
			for (int c = 0; c < blockCatalog.categories[i].blocks.Count; c++) {
				Block currentBlock = blockCatalog.categories[i].blocks[c];
				if (currentBlock.model.name == blockName) {
					return currentBlock;
				}
			}
		}

		return null;
	}

	public static Block GetBlockFromID (int id) {
		BlockCatalog blockCatalog = BlockCatalog.GetInstance();
		
		for (int i = 0; i < blockCatalog.categories.Count; i++) {
			for (int c = 0; c < blockCatalog.categories[i].blocks.Count; c++) {
				Block currentBlock = blockCatalog.categories[i].blocks[c];
				if (currentBlock.id == id) {
					return currentBlock;
				}
			}
		}
		
		return null;
	}

	public static BlockCatalog GetInstance () {
		BlockCatalog[] bC = (BlockCatalog[])GameObject.FindObjectsOfType(typeof(BlockCatalog));
		if (bC != null && bC.Length == 1) {
			return bC[0];
		} else if (bC.Length > 1) {
			Debug.LogError("Too many BlockCatalogs found");
			return null;
		} else {
			Debug.LogError("No type BlockCatalog found");
			return null;
		}
	}
}
