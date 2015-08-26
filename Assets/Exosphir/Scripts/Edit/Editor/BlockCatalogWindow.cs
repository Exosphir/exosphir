using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class BlockCatalogWindow : EditorWindow {

	public BlockCatalog t;
	SerializedObject myTarget;
	SerializedProperty categoryList;
	SerializedProperty showList;
	int amountOfCategories;

	Vector2 scrollPosition;
	Vector2 scrollPositionDetails;
	
	bool deleting = false;

	SerializedProperty blockToFocusOn = null;
	BlockCatalog.RenderPreviewSettings blockToFocusOnRenderSettings;
	BlockCatalog.Block blockToFocusOnAsBlock;
	int blockToFocusOnIndex;

	bool editUI;

	public void Initialize () {
		myTarget = new SerializedObject(t);
		categoryList = myTarget.FindProperty("categories"); // Find the List in our script and create a refrence of it
		showList = myTarget.FindProperty("showBlocks");
	}
	
	void OnGUI() {
		// Update our list
		myTarget.Update();

		///////////////////////////////
		/// Main Catalog List

		Rect mainCatalogRect = new Rect(10.0f, 10.0f, 360.0f, position.height);
		GUILayout.BeginArea(mainCatalogRect);
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(mainCatalogRect.width));

		if (GUILayout.Button("Edit UI")) {
			editUI = true;
		}
		
		if (GUILayout.Button("Add New Catagory")) {
			t.categories.Add (new BlockCatalog.Category());
			t.showBlocks.Add (false);
		}
		
		EditorGUILayout.Space();
		
		for (int i = 0; i < categoryList.arraySize; i++) {
			SerializedProperty categoryListRef = categoryList.GetArrayElementAtIndex(i);
			SerializedProperty blocks = categoryListRef.FindPropertyRelative("blocks");
			SerializedProperty name = categoryListRef.FindPropertyRelative("name");
			
			name.stringValue = EditorGUILayout.TextField("Category " + (i + 1) + " Name: ", name.stringValue);
			
			SerializedProperty show = showList.GetArrayElementAtIndex(i);
			show.boolValue = EditorGUILayout.Foldout(show.boolValue, "Blocks");
			
			if (show.boolValue) {

				GUILayout.BeginHorizontal();
				GUILayout.Space(20.0f);
				if (GUILayout.Button("Add Block")){
					t.categories[i].blocks.Add (new BlockCatalog.Block());
				}
				if (blocks.arraySize > 0) {
					if (GUILayout.Button("Render All Images")){
						RenderAllOfCategory(t.categories[i]);
					}
				}
				GUILayout.EndHorizontal();
				
				for (int b = 0; b < blocks.arraySize; b++) {
					// Set block id of block
					int finalID = 1;
					for (int i2 = 0; i2 < i; i2++) {
						finalID += t.categories[i2].blocks.Count;
					}
					finalID += b;
					t.categories[i].blocks[b].id = finalID;

					SerializedProperty blocksRef = blocks.GetArrayElementAtIndex(b);
					SerializedProperty model = blocksRef.FindPropertyRelative("model");

					EditorGUILayout.BeginHorizontal();
					
					if (blocks.arraySize > 1) {
						GUILayout.BeginHorizontal();
						GUILayout.Space(20.0f);
						if (GUILayout.Button ("x", GUILayout.Width(20), GUILayout.Height(20))) {
							if (b == blockToFocusOnIndex) {
								blockToFocusOn = null;
								blockToFocusOnAsBlock = null;
								blockToFocusOnRenderSettings = null;
							}
							blocks.DeleteArrayElementAtIndex(b);
							deleting = true;
						}
						GUILayout.EndHorizontal();
					}
					
					if (!deleting) {
						if (blocks.GetArrayElementAtIndex(b).FindPropertyRelative("model").objectReferenceValue != null) {
							GUILayout.BeginHorizontal();
							if (GUILayout.Button ("->", GUILayout.Width(30), GUILayout.Height(20))) {
								editUI = false;
								blockToFocusOn = blocks.GetArrayElementAtIndex(b);
								blockToFocusOnRenderSettings = t.categories[i].blocks[b].previewImageSettings;
								blockToFocusOnAsBlock = t.categories[i].blocks[b];
								blockToFocusOnIndex = b;
							}
							GUILayout.EndHorizontal();
						}

						model.objectReferenceValue = EditorGUILayout.ObjectField("Block " + (b + 1), model.objectReferenceValue, typeof(GameObject), true, GUILayout.Width(260.0f));
					}
					
					EditorGUILayout.EndHorizontal();
				}
			}

			if (categoryList.arraySize > 1) {
				if (GUILayout.Button("Remove Category '" + name.stringValue + "'")) {
					categoryList.DeleteArrayElementAtIndex(i);
					showList.DeleteArrayElementAtIndex(i);
					blockToFocusOn = null;
				}
			}

			GUILayout.Space(30.0f);
		}

		GUILayout.EndScrollView();
		GUILayout.EndArea();

		///////////////////////////////
		/// Block Details

		if (blockToFocusOn != null || editUI) {
			Rect detailsRect = new Rect(mainCatalogRect.x + mainCatalogRect.width + 10.0f, 10.0f, 360.0f, position.height);
			GUILayout.BeginArea(detailsRect);
			scrollPositionDetails = GUILayout.BeginScrollView(scrollPositionDetails, GUILayout.Width(detailsRect.width));

			if (!editUI) {
				if (blockToFocusOnAsBlock.previewImage != null) {
					// This is not working for some reason
					// EditorGUI.DrawPreviewTexture(new Rect((detailsRect.x + detailsRect.width) - 110.0f, detailsRect.y, 100.0f, 100.0f), blockToFocusOnAsBlock.previewImage);
				}

				GUILayout.Label ("Current block:", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;

				EditorGUILayout.LabelField (blockToFocusOn.FindPropertyRelative("model").objectReferenceValue.name);
				EditorGUILayout.LabelField ("Block ID: " + blockToFocusOnAsBlock.id);
				EditorGUILayout.LabelField ("Preview Image Rendered: " + ((blockToFocusOnAsBlock.previewImage == null)? "No" : "Yes"));

				EditorGUI.indentLevel--;
				GUILayout.Space(20.0f);

				GUILayout.Label ("Object Settings:", EditorStyles.boldLabel);

				EditorGUILayout.PropertyField(blockToFocusOn.FindPropertyRelative("allocationAmount"));
				EditorGUILayout.PropertyField(blockToFocusOn.FindPropertyRelative("rotateable"));
				EditorGUILayout.PropertyField(blockToFocusOn.FindPropertyRelative("scaleable"));

				GUILayout.Space(10.0f);

				GUILayout.Label ("Preview Image Render Settings:", EditorStyles.boldLabel);
				EditorGUI.indentLevel++;

				SerializedProperty settings = blockToFocusOn.FindPropertyRelative("previewImageSettings");

				EditorGUILayout.PropertyField(settings.FindPropertyRelative("distanceFromObject"));
				EditorGUILayout.PropertyField(settings.FindPropertyRelative("positionOfPivot"));
				EditorGUILayout.PropertyField(settings.FindPropertyRelative("rotationOfPivot"));

				GUILayout.Space(5.0f);
				EditorGUILayout.PropertyField(settings.FindPropertyRelative("rayTraceSolution"));

				EditorGUI.indentLevel++;
				EditorGUILayout.LabelField("Enable when the object has any transparent properties");
				EditorGUI.indentLevel--;

				EditorGUI.indentLevel--;

				GUILayout.Space(5.0f);

				if (GUILayout.Button("Render Block Preview Image")) {
					Texture2D previewImage = RenderPreviewImage((GameObject)blockToFocusOn.FindPropertyRelative("model").objectReferenceValue, blockToFocusOnRenderSettings);

					string pathString = "Assets/Exosphir/Textures/BlockPreviewImages/blockCatalog_" + blockToFocusOn.FindPropertyRelative("model").objectReferenceValue.name + ".png";
					byte[] pngData = previewImage.EncodeToPNG();
					
					if (pngData != null)
						File.WriteAllBytes(pathString, pngData);
					
					blockToFocusOn.FindPropertyRelative("previewImage").objectReferenceValue = Object.Instantiate(previewImage);
					DestroyImmediate(previewImage);

					AssetDatabase.Refresh();
				}

				GUILayout.Space(20.0f);
			} else {
				GUILayout.Label ("Block Catalog UI Settings", EditorStyles.boldLabel);

				GUILayout.Space(5.0f);

				EditorGUILayout.PropertyField(myTarget.FindProperty("objectButton"));
				EditorGUILayout.PropertyField(myTarget.FindProperty("nullPreviewTexture"));

				GUILayout.Space(5.0f);

				myTarget.FindProperty("uiWidth").floatValue = EditorGUILayout.FloatField("UI Width (in inches): ", myTarget.FindProperty("uiWidth").floatValue);

				GUILayout.Space(20.0f);
			}

			if (GUILayout.Button("Close Details")) {
				editUI = false;
				blockToFocusOn = null;
				blockToFocusOnAsBlock = null;
				blockToFocusOnRenderSettings = null;
			}

			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		deleting = false;
		
		myTarget.ApplyModifiedProperties();
	}

	void RenderAllOfCategory (BlockCatalog.Category category) {
		for (int i = 0; i < category.blocks.Count; i++) {
			string modelName = category.blocks[i].model.name;
			EditorUtility.DisplayProgressBar("Rendering Preview Images", "Rendering block " + modelName, i / (category.blocks.Count));

			Texture2D previewImage = RenderPreviewImage(category.blocks[i].model, category.blocks[i].previewImageSettings);
			
			string pathString = "Assets/Exosphir/Textures/BlockPreviewImages/blockCatalog_" + modelName + ".png";
			byte[] pngData = previewImage.EncodeToPNG();
			
			if (pngData != null)
				File.WriteAllBytes(pathString, pngData);
			
			category.blocks[i].previewImage = Object.Instantiate(previewImage);
			DestroyImmediate(previewImage);
		}

		EditorUtility.ClearProgressBar();
		AssetDatabase.Refresh();
	}

	Texture2D RenderPreviewImage (GameObject objectToRender, BlockCatalog.RenderPreviewSettings renderPreviewSettings) {
		// Setup

		GameObject camera = new GameObject("blockCatalog_renderCamera");
		GameObject pivot = new GameObject("blockCatalog_pivot");

		camera.hideFlags = HideFlags.DontSave;
		pivot.hideFlags = HideFlags.DontSave;

		camera.transform.parent = pivot.transform;
		camera.AddComponent<Camera>();

		Camera cam = camera.GetComponent<Camera>();
		cam.clearFlags = CameraClearFlags.SolidColor;

		if (renderPreviewSettings.rayTraceSolution) {
			cam.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
		} else {
			cam.backgroundColor = new Color(0.0f, 1.0f, 0.0f);
		}

		cam.cullingMask = 1 << LayerMask.NameToLayer("Ignore Raycast");

		GameObject theObject = (GameObject)GameObject.Instantiate(objectToRender, Vector3.zero, Quaternion.identity);
		theObject.name = "blockCatalog_object";
		theObject.layer = LayerMask.NameToLayer("Ignore Raycast");

		List<GameObject> meshColliders = new List<GameObject>();

		MeshFilter[] allMeshes = theObject.GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter eachMesh in allMeshes) {

			eachMesh.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

			// Setup for Ray Trace Solution

			if (renderPreviewSettings.rayTraceSolution) {
				GameObject meshCollider = new GameObject("meshCollider_" + eachMesh.sharedMesh.name);
				meshCollider.transform.position = eachMesh.transform.position;
				meshCollider.transform.rotation = eachMesh.transform.rotation;
				meshCollider.transform.localScale = eachMesh.transform.localScale;
				meshCollider.hideFlags = HideFlags.DontSave;

				meshCollider.AddComponent<MeshCollider>();
				meshCollider.GetComponent<MeshCollider>().sharedMesh = eachMesh.sharedMesh;

				meshColliders.Add(meshCollider);
			}
		}

		// Adjust

		Vector3 pos = camera.transform.localPosition;
		pos.x = renderPreviewSettings.distanceFromObject;
		camera.transform.localPosition = pos;

		camera.transform.LookAt(pivot.transform.position);

		pivot.transform.position = renderPreviewSettings.positionOfPivot;
		pivot.transform.rotation = Quaternion.Euler(renderPreviewSettings.rotationOfPivot);

		// Render

		int sqr = 1024;

		cam.aspect = 1.0f;

		RenderTexture tempRT = new RenderTexture(sqr,sqr, 24 );
		
		cam.targetTexture = tempRT;
		cam.Render();
		
		RenderTexture.active = tempRT;
		Texture2D image = new Texture2D(sqr,sqr, TextureFormat.ARGB32, false);
		image.ReadPixels( new Rect(0, 0, sqr,sqr), 0, 0);

		image.alphaIsTransparency = true;

		// Replace Green with Transparent if !rayTraceSolution

		if (!renderPreviewSettings.rayTraceSolution) {
			for (int x = 0; x < image.width; x++) {
				for (int y = 0; y < image.height; y++) {
					if (image.GetPixel(x, y) == new Color(0.0f, 1.0f, 0.0f)) {
						image.SetPixel(x, y, new Color(0.0f, 0.0f, 0.0f, 0.0f));
					}
				}
			}
		}

		// Repace Non-RaycastCollision with Transparent if rayTraceSolution

		if (renderPreviewSettings.rayTraceSolution) {
			for (int x = 0; x < image.width; x++) {
				for (int y = 0; y < image.height; y++) {
					Ray ray = cam.ScreenPointToRay(new Vector3(x, y, 0));
					RaycastHit hit;

					if (!Physics.Raycast (ray, out hit, 500)) {
						image.SetPixel(x, y, new Color(0.0f, 0.0f, 0.0f, 0.0f));
					}
				}
			}
		}

		// Clean-up

		RenderTexture.active = null;
		cam.targetTexture = null;
		DestroyImmediate (tempRT);

		DestroyImmediate(camera);
		DestroyImmediate(pivot);
		DestroyImmediate(theObject);

		if (renderPreviewSettings.rayTraceSolution) {
			int count = meshColliders.Count;
			for (int a = 0; a < count; a++) {
				GameObject objectToDestroy = meshColliders[meshColliders.Count - 1];
				meshColliders.RemoveAt(meshColliders.Count - 1);

				DestroyImmediate(objectToDestroy);
			}
		}

		return image;
	}
}

