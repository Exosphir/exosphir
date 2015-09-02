using System;
using System.Collections.Generic;
using System.Linq;
using Edit.Backend;
using UnityEditor;
using UnityEngine;

namespace Edit.Editor {
    public class CatalogWindow : EditorWindow {
        private const float WindowWidth = 900;

        private Catalog _catalog;
        private SerializedProperty _buttonTemplate;
        private SerializedProperty _nullTexture;
        private SerializedProperty _uiWidth;
        private SerializedProperty _uiItemWidth;
        private SerializedProperty _categories;

        private Texture _settingsIcon;

        private readonly Queue<Action> _nextFrameTasks = new Queue<Action>(3);
        private Vector3 _generalScrollPos, _detailsScrollPos;
        private bool _showUiProps;
        private bool _showCategories = true;
        private Category _currentCategory;
        private CatalogItem _currentItem;
        private int _currentCategoryIndex;
        private string[] _categoryNames;
        private SerializedObject _serializedCatalog;

        public void OnFocus() {
            titleContent = new GUIContent("Catalog Editor");
            var newCatalogComponent = Catalog.GetInstance();
            if (_catalog != newCatalogComponent) {
                //probably the scene has changed, let's not keep old data
                _currentCategory = null;
                _currentItem = null;
            }
            _catalog = newCatalogComponent?? Catalog.GetInstance();
            _serializedCatalog = new SerializedObject(_catalog);
            _buttonTemplate = _serializedCatalog.FindProperty("ButtonTemplate");
            _nullTexture = _serializedCatalog.FindProperty("NullTexture");
            _uiWidth = _serializedCatalog.FindProperty("UiWidth");
            _uiItemWidth = _serializedCatalog.FindProperty("UiItemWidth");
            _categories = _serializedCatalog.FindProperty("Categories");
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
            GUILayout.BeginHorizontal(GUILayout.Width(WindowWidth));
            GUILayout.BeginVertical(GUILayout.Width(WindowWidth / 3));
            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Fix Catalog Leaks",
                                                "Saves the scene and reloads it, cleaning up leaks."))) {
                EditorApplication.SaveCurrentSceneIfUserWantsTo();
                if (!EditorApplication.isSceneDirty) {
                    //only executes if scene has saved
                    EditorApplication.OpenScene(EditorApplication.currentScene);
                    OnFocus();
                    EditorApplication.SaveScene(EditorApplication.currentScene);
                } else {
                    Debug.LogWarning("Leak cleanup aborted because scene is not saved");
                }
            }
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

            GUILayout.BeginVertical(GUILayout.Width(WindowWidth / 3));
            EditorGUILayout.Space();
            if (_currentCategory != null) {
                DrawCategoryDetail();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(WindowWidth / 3));
            EditorGUILayout.Space();
            if (_currentItem != null) {
                DrawItemDetail();
            }
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
            _serializedCatalog.ApplyModifiedProperties();
        }


        private void DrawInterfaceSettings() {
            EditorGUILayout.PropertyField(_buttonTemplate, new GUIContent("Object Button Template"));
            EditorGUILayout.PropertyField(_nullTexture, new GUIContent("Null Preview Image"));
            EditorGUILayout.PropertyField(_uiWidth, new GUIContent("Interface Width (px)"));
            EditorGUILayout.PropertyField(_uiItemWidth, new GUIContent("Item Width (px)"));
        }

        private void DrawCategoryEditor() {

            if (GUILayout.Button("Add Category")) {
                var newCategory = CreateInstance<Category>();
                var newName = _categories.arraySize > 0
                               ? "Category " + _categories.arraySize
                               : Category.DefaultCategoryName;
                newCategory.Name = newName;
                _categories.arraySize++;
                _categories.GetArrayElementAtIndex(_categories.arraySize - 1).objectReferenceValue = newCategory;
                _currentCategory = newCategory;
            }
            _generalScrollPos = EditorGUILayout.BeginScrollView(_generalScrollPos);
            foreach (SerializedProperty categoryProp in _categories) {
                var category = (Category)categoryProp.objectReferenceValue;
                if (category == null) {
                    continue;
                }
                GUILayout.BeginHorizontal();
                if (DetailsButton()) {
                    _currentCategory = category;
                    RefreshItemCategories();
                }

                var categorySerial = new SerializedObject(category);
                EditorGUILayout.PropertyField(categorySerial.FindProperty("Name"), GUIContent.none, GUILayout.ExpandWidth(true));
                categorySerial.ApplyModifiedProperties();

                EditorGUI.BeginDisabledGroup(category.Name == Category.DefaultCategoryName);
                if (GUILayout.Button("Remove")) {
                    category.Destroy();
                    if (_currentCategory == category) {
                        _currentCategory = null;
                    }
                    RefreshItemCategories();
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawCategoryDetail() {
            if (_currentCategory == null) {
                return;
            }
            EditorGUILayout.HelpBox("Category Details: " + _currentCategory.Name, MessageType.None, true);
            var currentCategorySerial = new SerializedObject(_currentCategory);
            EditorGUILayout.PropertyField(currentCategorySerial.FindProperty("PooledPerItem"),
                                          new GUIContent("Amount Pooled for Each", "The initial pool size for each object of this category"));
            EditorGUILayout.PropertyField(currentCategorySerial.FindProperty("PoolFillWhenDry"),
                                          new GUIContent("Pool Fill when Dry", "How many objects to add to pool when it runs empties"));
            currentCategorySerial.ApplyModifiedProperties();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add item")) {
                var item = CreateInstance<CatalogItem>();
                item.Category = _currentCategory;
                _catalog.AddItem(item);

                _currentItem = item;
            }

            if (GUILayout.Button("Render all previews")) {
                foreach (var item in _catalog) {
                    PreviewAssetGenerator.GeneratePreviewToFile(item);
                }
            }
            GUILayout.EndHorizontal();

            _detailsScrollPos = EditorGUILayout.BeginScrollView(_detailsScrollPos);
            GUILayout.BeginVertical();
            foreach (var item in _currentCategory) {
                GUILayout.BeginHorizontal();
                if (DetailsButton()) {
                    _currentItem = item;
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
            EditorGUILayout.EndScrollView();
        }

        private void RefreshItemCategories() {
            OnFocus();
            if (_currentCategory != null) {
                var categories = _catalog.Categories.Select(cat => cat.Name).ToList();
                if (_currentItem != null) {
                    _currentCategoryIndex = categories.IndexOf(_currentItem.Category.Name);
                }
                _categoryNames = categories.ToArray();
            }
        }

        private void DrawItemDetail() {
            if (_currentItem == null) {
                return;
            }
            var currentItemSerial = new SerializedObject(_currentItem);
            var previewSerial = new SerializedObject(_currentItem.PreviewImage);

            var hasOwnPreview = _currentItem.GetDefaultPreview() != _catalog.NullTexture;
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Current Item:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Item ID", _currentItem.Id.ToString());
            EditorGUILayout.LabelField("Has Preview Image", hasOwnPreview.ToString());
            var newCategoryIndex = EditorGUILayout.Popup(_currentCategoryIndex, _categoryNames);
            if (newCategoryIndex != _currentCategoryIndex) {
                _currentItem.Category = Category.GetOrCreate(_categoryNames[newCategoryIndex]);
                _currentCategoryIndex = newCategoryIndex;
            }
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();
            GUILayout.Label("Object Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(currentItemSerial.FindProperty("Scalable"), new GUIContent("Scalable"));
            EditorGUILayout.PropertyField(currentItemSerial.FindProperty("Rotatable"), new GUIContent("Rotatable"));
            EditorGUILayout.PropertyField(currentItemSerial.FindProperty("Optimizable"),
                                          new GUIContent("Optimizable", "If enabled, model mesh will be substituted by optimized version during map load."));
            EditorGUILayout.PropertyField(currentItemSerial.FindProperty("Model"), new GUIContent("Model"));
            EditorGUILayout.PropertyField(currentItemSerial.FindProperty("Groups"),
                                          new GUIContent("Groups", "Internal groupings to determine various many-to-many relations"), true);
            EditorGUILayout.PropertyField(currentItemSerial.FindProperty("OptimizationGroups"),
                                          new GUIContent("Optimize With", "Which groups can this object optimize with. Optimizable objects always optimize with themselves"), true);
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Preview Image Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(previewSerial.FindProperty("DistanceToPivot"), new GUIContent("Distance to Pivot"));
            EditorGUILayout.PropertyField(previewSerial.FindProperty("PivotPosition"), new GUIContent("Pivot Position"));
            //we need to use vector3field because quaternion property drawer is awkward.
            var rotationProperty = previewSerial.FindProperty("PivotRotation");
            rotationProperty.quaternionValue = Quaternion.Euler(
                EditorGUILayout.Vector3Field("Pivot Rotation", rotationProperty.quaternionValue.eulerAngles));
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            if (GUILayout.Button("Render Preview Image")) {
                PreviewAssetGenerator.GeneratePreviewToFile(_currentItem);
            }
            previewSerial.ApplyModifiedProperties();
            currentItemSerial.ApplyModifiedProperties();
        }

        private Action DeleteItemTask(CatalogItem item) {
            if (_currentItem == item) {
                _currentItem = null;
            }
            return () => _catalog.RemoveItem(item);
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
