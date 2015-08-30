using UnityEngine;

namespace Edit {
    public class Grid : MonoBehaviour {
        private static readonly Color GizmoColor = new Color(255, 64, 0);
        public float CellSize = 1f;
        public float BaseSize = 1f;
        private Renderer _renderer;

        void Start() {
            _renderer = GetComponent<Renderer>();
        }

        void Update() {
            _renderer.material.SetFloat("_CellSize", CellSize);
        }

        public Rect GetHorizontalPlaneRect() {
            var scale = transform.localScale;
            var center = transform.position;
            var sizeX = scale.x * BaseSize;
            var sizeZ = scale.z * BaseSize;
            return new Rect(center.x - sizeX / 2, center.z - sizeZ / 2, sizeX, sizeZ);
        }

        public Vector3 Snap(Vector3 position, float gridVerticalOffset = 0) {
            var rect = GetHorizontalPlaneRect();
            var snapX = Mathf.Round(position.x / CellSize) * CellSize;
            var snapZ = Mathf.Round(position.z / CellSize) * CellSize;
            return new Vector3 {
                x = Mathf.Clamp(snapX, rect.xMin, rect.xMax),
                y = transform.position.y + gridVerticalOffset,
                z = Mathf.Clamp(snapZ, rect.yMin, rect.yMax)
            };
        }

        public bool SharingCell(Vector3 a, Vector3 b) {
            var halfSize = CellSize;
            var delta = Snap(b) - Snap(a);
            return delta.x < halfSize
                   && delta.y < halfSize
                   && delta.z < halfSize;
        }

        public bool Contains(Vector3 point) {
            return GetHorizontalPlaneRect().Contains(new Vector2(point.x, point.z));
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = GizmoColor;
            var scale = transform.localScale * BaseSize;
            scale.y = 0;
            Gizmos.DrawWireCube(transform.position, scale);
        }
    }
}
