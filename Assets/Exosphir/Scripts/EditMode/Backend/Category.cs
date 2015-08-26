using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace EditMode {
    /// <summary>
    /// A category is simply a collection of catalog items, organized sequentially.
    /// </summary>
    [Serializable]
    public class Category: IEnumerable<CatalogItem> {
        public static readonly string DefaultCategoryName = "Misc";

        public static Category DefaultCategory {
            get { return GetOrCreate(DefaultCategoryName); }
        }
        [SerializeField]
        private readonly List<CatalogItem> _items;

        public string Name;

        public ReadOnlyCollection<CatalogItem> Items {
            get {
                return new ReadOnlyCollection<CatalogItem>(_items);
            }
        }

        private Category(string name) {
            _items = new List<CatalogItem>();
            Name = name;
        }
        
        /// <summary>
        /// Appends the given item into the category
        /// </summary>
        /// <param name="item"></param>
        public void Add(CatalogItem item) {
            if (!_items.Contains(item)) {
                _items.Add(item);
            }
        }

        public bool Remove(CatalogItem item) {
            if (_items == null) return false;
            return _items.Remove(item);
        }

        /// <summary>
        /// Obtains the category with the given name from the catalog.
        /// If the category doesn't exist, it is created.
        /// See also:
        /// <seealso cref="Catalog.Categories"/>
        /// </summary>
        /// <param name="name">Name of the category to retrieve</param>
        /// <returns>The category</returns>
        public static Category GetOrCreate(string name) {
            var catalog = Catalog.GetInstance();
            if (!catalog.Categories.ContainsKey(name)) {
                var category = new Category(name);
                catalog.Categories[name] = category;
            }
            return catalog.Categories[name];
        }

        /// <summary>
        /// Renames the category, updating the catalog in the process
        /// </summary>
        /// <param name="newName">The new name of this category</param>
        public void Rename(string newName) {
            var catalog = Catalog.GetInstance();
            catalog.Categories.Remove(Name);
            Name = newName;
            catalog.Categories[Name] = this;
        }

        /// <summary>
        /// Destroys the category, removing it from the catalog and moving its contents
        /// to the default category
        /// </summary>
        public void Destroy() {
            Catalog.GetInstance().Categories.Remove(Name);
            foreach (var item in this) {
                item.Category = DefaultCategory;
            }
        }

        public IEnumerator<CatalogItem> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
