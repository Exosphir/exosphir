using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(BlockCatalog))]
public class BlockCatalogInspector : Editor {

	BlockCatalogWindow window = null;

	public override void OnInspectorGUI() {
		if (window == null) {
			if (GUILayout.Button("Open Window")) {
				window = (BlockCatalogWindow)EditorWindow.GetWindow(typeof(BlockCatalogWindow));
				window.t = (BlockCatalog)target;
				window.Initialize();
			}
		} else {
			if (GUILayout.Button("Close Window")) {
				window.Close();
				window = null;
			}
		}
	}
}
