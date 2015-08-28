using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Edit.Backend;
using UnityEngine;

namespace Edit {
    internal class DummyItem: CatalogItem {
        public DummyItem(int id) {
            Id = id;
        }

        public static DummyItem Create(int id) {
            var dummy = CreateInstance<DummyItem>();
            dummy.Id = id;
            return dummy;
        }
    }
    /// <summary>
    /// Manages a list of edit mode objects.
    /// </summary>
    [Serializable]
    public sealed class Catalog : SingletonBehaviour<Catalog>, IEnumerable<CatalogItem>, ISerializationCallbackReceiver {
        private static Catalog _instance;

        public CatalogItemButton ButtonTemplate;
        public Texture2D NullTexture;
        public float UiWidth = 215f;
        public int UiItemWidth = 100;

        private static readonly CatalogItem.Comparer Comparer = new CatalogItem.Comparer();
        /// <summary>
        /// All registered catalog items, must be always sorted by ID.
        /// </summary>
        [SerializeField]
        private List<CatalogItem> _items;

        public List<Category> Categories;

        void OnEnable() {
            if (Categories == null) {
                Categories = new List<Category>();
            }
            if (_items == null) {
                _items = new List<CatalogItem>();
            }
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
            var index = _items.BinarySearch(new DummyItem(id), Comparer);
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
            var index = _items.BinarySearch(DummyItem.Create(item.Id), Comparer);
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
            var index = _items.BinarySearch(new DummyItem(id), Comparer);
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
