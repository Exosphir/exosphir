using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Edit.Editor {
    [CustomEditor(typeof(Catalog))]
    class CatalogComponentEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            var component = (Catalog) target;
            if (GUILayout.Button("Open Catalog Editor")) {
                CatalogWindow.OpenWindow();
            }
            GUILayout.Label("Item count: " + component.Count());
        }
    }
}
