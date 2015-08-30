using System.Collections.Generic;
using Edit.Backend;
using UnityEngine;

namespace Edit {
    [RequireComponent(typeof(Grid))]
    public class EditorWorld : SingletonBehaviour<EditorWorld> {
        private const float SquareRoot2 = 1.4142135623730950488016887242097f;

        public Transform Container;
        public float InitialSize = 20f;
        [HideInInspector]
        public CatalogItemPool Pool;

        private PointOctree<PlacedItem> _octree;
        private Grid _grid;
        private Catalog _catalog;

        void Start() {
            _catalog = Catalog.GetInstance();
            _octree = new PointOctree<PlacedItem>(InitialSize, Vector3.zero, 1);
            _grid = GetComponent<Grid>();
            
            var obj = new GameObject("Pool");
            obj.transform.parent = transform;
            Pool = obj.GetComponent<CatalogItemPool>();
            if (Pool == null) {
                Pool = obj.AddComponent<CatalogItemPool>();
            }
            Pool.PooledHolder = obj.transform;
            Pool.ModelGenerator = item => item.Model;
            SeedObjectPool();
        }

        private void SeedObjectPool() {
            foreach (var item in _catalog) {
                var category = item.Category;
                var amount = category == null ? 1 : category.PooledPerItem;
                Pool.FillWithModel(item, amount);
            }
        }

        /// <summary>
        /// Places a item with the given transformations
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <param name="position">Position of the new item</param>
        /// <param name="rotation">Rotation of the item</param>
        /// <param name="scale">Uniform scaling factor of the item</param>
        /// <returns>A PlacedItem denoting the just-placed item</returns>
        public PlacedItem PlaceItemAt(CatalogItem item, Vector3 position, Quaternion rotation, float scale) {
            var obj = Pool.Get(item, item.Category.PoolFillWhenDry);
            obj.transform.SetParent(Container);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = Vector3.one * scale;
            
            var placed = obj.AddComponent<PlacedItem>();
            placed.CatalogEntry = item;
            _octree.Add(placed, position);
            return placed;
        }

        /// <summary>
        /// Registers an existing PlacedItem to the world.
        /// </summary>
        /// <param name="placed">The item to be registered</param>
        public void RegisterExistingItem(PlacedItem placed) {
            placed.transform.SetParent(Container, true);
            _octree.Add(placed, placed.transform.position);
        }

        /// <summary>
        /// Gets all placed objects residing in the cell which
        /// <see cref="pos"/> belongs to.
        /// </summary>
        /// <param name="pos">The position of the cell to search</param>
        /// <returns>All items inside that cell</returns>
        public IEnumerable<PlacedItem> GetObjectsInCell(Vector3 pos) {
            // get objects in nearby sphere covering whole cell
            var octNear = _octree.GetNearby(pos, _grid.CellSize / SquareRoot2);

            var cellmates = new List<PlacedItem>(octNear.Count);
            foreach (var near in octNear) {
                //filter objects in sphere to cell
                if (_grid.SharingCell(pos, near.transform.position)) {
                    cellmates.Add(near);
                }
            }
            return cellmates;
        }

        /// <summary>
        /// Removes a item from the world and destroys it.
        /// Returns the object to the object pool.
        /// </summary>
        /// <param name="item">The item to remove</param>
        public void Remove(PlacedItem item) {
            _octree.Remove(item);
            var obj = item.gameObject;
            var entry = item.CatalogEntry;
            Destroy(item);
            Pool.AddTo(entry, obj);
        }

        void OnDrawGizmosSelected() {
            if (_octree != null) {
                _octree.DrawAllBounds();
                _octree.DrawAllObjects();
            }
        }
    }
}
