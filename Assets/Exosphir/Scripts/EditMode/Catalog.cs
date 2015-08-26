using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EditMode {
    internal class DummyItem: CatalogItem {
        public DummyItem(int id) {
            Id = id;
        }
    }
    /// <summary>
    /// Manages a list of edit mode objects.
    /// </summary>
    [Serializable]
    public class Catalog : MonoBehaviour, IEnumerable<CatalogItem>, ISerializationCallbackReceiver {
        private static Catalog _instance;

        public GameObject ButtonTemplate;
        public Texture2D NullTexture;
        public float UiWidth = 3f;
        public int UiItemsPerRow = 2;

        private readonly CatalogItem.Comparer _comparer;
        /// <summary>
        /// All registered catalog items, must be always sorted by ID.
        /// </summary>
        [SerializeField]
        private readonly List<CatalogItem> _items;

        public Dictionary<string, Category> Categories;

        private Catalog() {
            Categories = new Dictionary<string, Category>();
            _items = new List<CatalogItem>();
            _comparer = new CatalogItem.Comparer();
        }


        /// <summary>
        /// Returns the <see cref="CatalogItem"/> at the specified index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The object</returns>
        public CatalogItem GetItemAt(int index) {
            return _items[index];
        }

        /// <summary>
        /// Obtains the <see cref="CatalogItem"/> with the given ID from the catalog
        /// </summary>
        /// <param name="id">The ID of the item to be fetched</param>
        /// <returns>The item</returns>
        public CatalogItem GetItemById(int id) {
            var index = _items.BinarySearch(new DummyItem(id), _comparer);
            return _items[index];
        }

        /// <summary>
        /// Obtains the <see cref="CatalogItem"/> with the given name from the catalog.
        /// <b>This method is slow, always consider using <see cref="GetItemById"/> or <see cref="GetItemAt"/></b>.
        /// </summary>
        /// <param name="name">The name of the item to be returned</param>
        /// <returns>The item with a corresponding name</returns>
        public CatalogItem GetItemByName(string name) {
            return _items.FirstOrDefault(item => item.Model.name == name);
        }

        /// <summary>
        /// Inserts a new item to the catalog, only if it doesn't exist yet
        /// </summary>
        /// <param name="item">The item to be inserted</param>
        public void AddItem(CatalogItem item) {
            var index = _items.BinarySearch(new DummyItem(item.Id), _comparer);
            //"otherwise, a negative number that is the bitwise complement of the 
            //index of the next element that is larger than item or, if there is
            //no larger element, the bitwise complement of Count."
            if (index < 0) {
                item.Id = _items.Count;
                _items.Add(item);
                if (item.Category == null) {
                    item.Category = Category.DefaultCategory;
                }
            }
            //do nothing if the catalog already contains the item
        }

        /// <summary>
        /// Removes the specified item from the catalog
        /// </summary>
        /// <param name="item">The item to remove</param>
        public void RemoveItem(CatalogItem item) {
            item.Category.Remove(item);
            _items.Remove(item);
        }

        /// <summary>
        /// Removes the item with the specified ID from the catalog
        /// </summary>
        /// <param name="id">ID of the item to remove</param>
        public void RemoveItemById(int id) {
            var index = _items.BinarySearch(new DummyItem(id), _comparer);
            if (index > 0) {
                var item = _items[index];
                item.Category.Remove(item);
                _items.RemoveAt(index);
            }
        }

        /// <summary>
        /// Creates a shallow copy of the catalog's contents into the given List
        /// </summary>
        /// <param name="target">List which will contain the contents of the catalog</param>
        public void CopyTo(List<CatalogItem> target) {
            target.Clear();
            target.AddRange(_items);
        }

        /// <summary>
        /// Populates the catalog with the contents of the given enumerable, automatically detecting categories.
        /// </summary>
        /// <param name="source">The source of catalog items</param>
        public void LoadFrom(IEnumerable<CatalogItem> source) {
            Categories.Clear();
            _items.Clear();
            _items.AddRange(source);
            RefreshCategories();
        }

        private void RefreshCategories() {
            Categories.Clear();
            foreach (var item in _items) {
                var catName = Category.DefaultCategoryName;
                if (item.Category != null) {
                    catName = item.Category.Name;
                }
                item.Category = Category.GetOrCreate(catName);
            }
        }

        public static Catalog GetInstance() {
            var catalogs = FindObjectsOfType<Catalog>();
            if (catalogs.Length == 1) {
                return catalogs[0];
            }
            if (catalogs.Length > 1) {
                Debug.LogError("Too many catalogs in scene, unpredictable behaviour! Returning first.");
                return catalogs[0];
            }
            Debug.LogError("No catalogs found in scene!");
            return null;
        }

        public IEnumerator<CatalogItem> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void OnBeforeSerialize() {
            
        }

        public void OnAfterDeserialize() {
            
        }
    }
}
