﻿using UnityEngine;

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

        public float Step(float value) {
            return Mathf.Floor(value / CellSize) * CellSize;
        }

        public Vector3 Snap(Vector3 position) {
            var rect = GetHorizontalPlaneRect();
            var snapX = Step(position.x);
            var snapY = Step(position.y);
            var snapZ = Step(position.z);
            return new Vector3 {
                x = Mathf.Clamp(snapX, rect.xMin, rect.xMax) + CellSize / 2f,
                y = snapY + CellSize / 2f,
                z = Mathf.Clamp(snapZ, rect.yMin, rect.yMax) + CellSize / 2f
            };
        }

        public bool SharingCell(Vector3 a, Vector3 b) {
            var halfSize = CellSize / 2;
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
