using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditMode {
    [RequireComponent(typeof(Grid))]
    class EditorWorld : SingletonBehaviour<EditorWorld> {
        private const float SquareRoot2 = 1.4142135623730950488016887242097f;

        public Transform Container;
        public float InitialSize = 20f;

        private PointOctree<PlacedItem> _octree;
        private Grid _grid;
        private CatalogItemPool _pool;
        private Catalog _catalog;

        void Start() {
            _catalog = Catalog.GetInstance();
            _octree = new PointOctree<PlacedItem>(InitialSize, Vector3.zero, 1);
            _grid = GetComponent<Grid>();
            
            var obj = new GameObject("Pool");
            obj.transform.parent = transform;
            _pool = obj.AddComponent<CatalogItemPool>();
            _pool.PooledHolder = obj.transform;
            _pool.ModelGenerator = item => item.Model;
            SeedObjectPool();
        }

        private void SeedObjectPool() {
            foreach (var item in _catalog) {
                _pool.FillWithModel(item, item.Category.PooledPerItem);
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
            var obj = _pool.Get(item, item.Category.PoolFillWhenDry);
            obj.transform.SetParent(Container);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = Vector3.one * scale;
            
            var placed = obj.AddComponent<PlacedItem>();
            placed.CatalogEntry = item;
            _octree.Add(placed, position);
            return placed;
        }

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

        public void Remove(PlacedItem item) {
            _octree.Remove(item);
            var obj = item.gameObject;
            var entry = item.CatalogEntry;
            Destroy(item);
            _pool.AddTo(entry, obj);
        }

        void OnDrawGizmosSelected() {
            if (_octree != null) {
                _octree.DrawAllBounds();
                _octree.DrawAllObjects();
            }
        }
    }
}
