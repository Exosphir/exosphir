using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EditMode.Editor {
    public class CatalogWindow : EditorWindow {
        private readonly Rect GeneralArea = new Rect(0, 0, 360, 600);

        private Rect DetailsArea {
            get {
                var left = GeneralArea.xMax + 20;
                var width = position.width - left;
                return new Rect(left, 0, width, 600);
            }
        }

        private Catalog _catalog;
        private Texture _settingsIcon;

        private readonly Queue<Action> _nextFrameTasks = new Queue<Action>(3);
        private Vector3 _generalScrollPos, _detailsScrollPos;
        private bool _showUiProps;
        private bool _showCategories = true;
        private Action _currentDetailAction;
        private Category _currentCategory;
        private CatalogItem _currentItem;
        private int _currentCategoryIndex;
        private string[] _categoryNames;

        public void OnFocus() {
            titleContent = new GUIContent("Catalog Editor");
            var newCatalogComponent = Catalog.GetInstance();
            if (_catalog != newCatalogComponent) {
                //probably the scene has changed, let's not keep old data
                _currentDetailAction = null;
                _currentCategory = null;
                _currentItem = null;
            }
            _catalog = newCatalogComponent?? Catalog.GetInstance();
            if (_settingsIcon == null) {
                _settingsIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Exosphir/Textures/EditorTextures/Settings.png");
            }
        }

        public void OnGUI() {
            foreach (var task in _nextFrameTasks) {
                try {
                    task();
                } catch (Exception e) {
                    Debug.LogException(e, this);
                }
            }
            if (_catalog == null) {
                EditorGUILayout.HelpBox("No CatalogComponent present in scene, please add one and reopen the catalog editor.", MessageType.Warning);
                return;
            }
            GUILayout.BeginArea(GeneralArea);
            GUILayout.BeginVertical();
            _showUiProps = EditorGUILayout.Foldout(_showUiProps, "Interface Settings");
            if (_showUiProps) {
                EditorGUI.indentLevel++;
                DrawInterfaceSettings();
                EditorGUI.indentLevel--;
            }
            _showCategories = EditorGUILayout.Foldout(_showCategories, "Categories");
            if (_showCategories) {
                EditorGUI.indentLevel++;
                DrawCategoryEditor();
                EditorGUI.indentLevel--;
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            if (_currentDetailAction != null) {
                GUILayout.BeginArea(DetailsArea);
                GUILayout.BeginVertical();
                if (GUILayout.Button("Close details panel")) {
                    _currentDetailAction = null;
                }
                if (_currentDetailAction != null) _currentDetailAction();
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

        private void DrawInterfaceSettings() {
            var newTemplate = (GameObject)EditorGUILayout.ObjectField("Object Button Template", _catalog.ButtonTemplate, typeof(GameObject), false);
            if (newTemplate != _catalog.ButtonTemplate) {
                _catalog.ButtonTemplate = newTemplate;
            }

            var newNullTexture = (Texture2D)EditorGUILayout.ObjectField("Null Preview Image", _catalog.NullTexture, typeof (Texture2D), false);
            if (newNullTexture != _catalog.NullTexture) {
                _catalog.NullTexture = newNullTexture;
            }

            var newWidth = EditorGUILayout.FloatField("Interface Width (px)", _catalog.UiWidth);
            if (Math.Abs(newWidth - _catalog.UiWidth) > 0.001) {
                _catalog.UiWidth = newWidth;
            }

            var newRow = EditorGUILayout.IntField("Items per row", _catalog.UiItemsPerRow);
            if (newRow != _catalog.UiItemsPerRow) {
                _catalog.UiItemsPerRow = newRow;
            }
        }

        private void DrawCategoryEditor() {

            if (GUILayout.Button("Add Category")) {
                Category.GetOrCreate("");
            }
            foreach (var category in _catalog.Categories.Values) {
                GUILayout.BeginHorizontal();
                if (DetailsButton()) {
                    _currentCategory = category;
                    _currentDetailAction = DrawCategoryDetail;
                    RefreshItemCategories();
                }

                var newName = EditorGUILayout.TextField(category.Name);
                if (newName != category.Name) {
                    //dict cant be modified during iteration, so postpone to next frame
                    _nextFrameTasks.Enqueue(RenameCategoryTask(category, newName));
                    RefreshItemCategories();
                }

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(category.Name == Category.DefaultCategoryName);
                if (GUILayout.Button("Remove")) {
                    _nextFrameTasks.Enqueue(category.Destroy);
                    if (_currentCategory == category) {
                        _currentCategory = null;
                        RefreshItemCategories();
                    }
                    RefreshItemCategories();
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();
            }
        }

        private void DrawCategoryDetail() {
            if (_currentCategory == null) {
                return;
            }
            EditorGUILayout.HelpBox("Category Details: " + _currentCategory.Name, MessageType.None, true);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add item")) {
                var item = new CatalogItem {
                    Category = _currentCategory
                };
                _catalog.AddItem(item);

                _currentItem = item;
                _currentDetailAction = DrawItemDetail;
            }

            if (GUILayout.Button("Render all previews")) {
                foreach (var item in _catalog) {
                    PreviewAssetGenerator.GeneratePreviewToFile(item);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            foreach (var item in _currentCategory) {
                GUILayout.BeginHorizontal();
                if (DetailsButton()) {
                    _currentItem = item;
                    _currentDetailAction = DrawItemDetail;
                    RefreshItemCategories();
                }
                GUILayout.Label(item.Id.ToString());
                GUILayout.Label(item.Name ?? "[None]");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete")) {
                    _nextFrameTasks.Enqueue(DeleteItemTask(item));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void RefreshItemCategories() {
            if (_currentCategory != null) {
                var categories = _catalog.Categories.Values.Select(cat => cat.Name).ToList();
                _currentCategoryIndex = categories.IndexOf(_currentCategory.Name);
                _categoryNames = categories.ToArray();
            }
        }

        private void DrawItemDetail() {
            if (_currentItem == null) {
                return;
            }

            if (GUILayout.Button("Back to category")) {
                _currentDetailAction = DrawCategoryDetail;
                return;
            }

            var hasOwnPreview = _currentItem.GetDefaultPreview() != _catalog.NullTexture;
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Current Item:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Item ID", _currentItem.Id.ToString());
            EditorGUILayout.LabelField("Has Preview Image", hasOwnPreview.ToString());
            var newCategoryIndex = EditorGUILayout.Popup("Category", _currentCategoryIndex, _categoryNames);
            if (newCategoryIndex != _currentCategoryIndex) {
                _currentCategoryIndex = newCategoryIndex;
                _currentItem.Category = Category.GetOrCreate(_categoryNames[newCategoryIndex]);
            }
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Object Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var newScalable = EditorGUILayout.Toggle("Scalable", _currentItem.Scalable);
            var newRotatable = EditorGUILayout.Toggle("Rotatable", _currentItem.Rotatable);
            var newModel = (GameObject)EditorGUILayout.ObjectField("Model", _currentItem.Model, typeof(GameObject), false);
            if (newScalable != _currentItem.Scalable || newRotatable != _currentItem.Rotatable ||
                newModel != _currentItem.Model) {
                
                _currentItem.Scalable = newScalable;
                _currentItem.Rotatable = newRotatable;
                _currentItem.Model = newModel;
            }
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Preview Image Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            var preview = _currentItem.PreviewImage;
            var newDistance = EditorGUILayout.FloatField("Distance to Pivot", preview.DistanceToPivot);
            var newPosition = EditorGUILayout.Vector3Field("Pivot Position", preview.PivotPosition);
            var newRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Pivot Rotation", preview.PivotRotation.eulerAngles));
            if (Math.Abs(newDistance - preview.DistanceToPivot) > 0.0001 || newPosition != preview.PivotPosition ||
                newRotation != preview.PivotRotation) {

                preview.DistanceToPivot = newDistance;
                preview.PivotPosition = newPosition;
                preview.PivotRotation = newRotation;
            }
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            if (GUILayout.Button("Render Preview Image")) {
                PreviewAssetGenerator.GeneratePreviewToFile(_currentItem);
            }
        }

        private Action RenameCategoryTask(Category category, string newName) {
            return () => category.Rename(newName);
        }

        private Action DeleteItemTask(CatalogItem item) {
            return () => _catalog.RemoveItem(item);
        }

        public void OnLostFocus() {
            if (_catalog != null) {
                EditorUtility.SetDirty(_catalog);
                EditorApplication.MarkSceneDirty();
            }
        }

        private bool DetailsButton() {
            GUILayout.Space(2);
            GUILayout.BeginVertical();
            var result = GUILayout.Button(_settingsIcon, new GUIStyle());
            GUILayout.EndVertical();
            return result;
        }

        [MenuItem("Exosphir/Catalog Editor %K")]
        public static void OpenWindow() {
            GetWindow<CatalogWindow>().Show();
        }
    }
}
