using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Edit {
    public class BlockOptimizer : SingletonBehaviour<BlockOptimizer> {
        /// <summary>
        /// A cube's face is formed by 4 vertices
        /// </summary>
        private const int VertsPerFace = 4;
        private const int VertsForQuad = 6;
        /// <summary>
        /// Keys are both plane normals and offset from center.
        /// Values are the UVs for that face
        /// </summary>
        private static readonly Dictionary<Vector3, Vector2[]> OffsetsToUvs = new Dictionary<Vector3, Vector2[]> {
            {Vector3.up, new[] {
                Vector2.one, Vector2.up,
                Vector2.right, Vector2.zero
            }},
            {Vector3.down, new [] {
                Vector2.up, Vector2.zero,
                Vector2.one, Vector2.right
            }},
            {Vector3.left, new [] {
                Vector2.one, Vector2.up,
                Vector2.right, Vector2.zero
            }},
            {Vector3.right, new [] {
                Vector2.right, Vector2.one,
                Vector2.zero, Vector2.up
            }},
            {Vector3.forward, new [] {
                Vector2.one, Vector2.up,
                Vector2.right, Vector2.zero
            }},
            {Vector3.back, new [] {
                Vector2.right, Vector2.one,
                Vector2.zero, Vector2.up
            }}
        };

        private List<Vector3> _offsets;
        private EditorWorld _world;
        private List<Mesh> _meshCache;

        void Start() {
            _world = EditorWorld.GetInstance();
            _meshCache = new List<Mesh>();
            _offsets = OffsetsToUvs.Keys.ToList();
            
            //bitmask trick. each bit represents existence of a face
            foreach (var mask in Enumerable.Range(0, 2 << _offsets.Count - 1)) {
                var normals = new List<Vector3>();
                for (int i = 0, length = _offsets.Count; i < length; i++) {
                    if (((1 << i) & mask) != 0) {
                        normals.Add(_offsets[i]);
                    }
                }
                _meshCache.Insert(mask, MakeMeshForNormals(normals));
            }
        }

        public void OptimizeAll() {
            var all = _world.Container.GetComponentsInChildren<PlacedItem>();
            foreach (var item in all) {
                OptimizeInternal(item, false);
            }
        }

        /// <summary>
        /// Optimizes the meshes of the given item and its neighbours
        /// </summary>
        /// <param name="item">The item to optimize</param>
        public void Optimize(PlacedItem item) {
            OptimizeInternal(item, true);
        }

        /// <summary>
        /// Optimizes the neighbouring cells of the given point.
        /// </summary>
        /// <param name="cell">A coordinate inside the cell you want</param>
        public void OptimizeNeighbours(Vector3 cell) {
            foreach (var kvp in GetOptimizableNeighbours(_world.Grid.Snap(cell))) {
                OptimizeInternal(kvp.Value, false);
            }
        }

        private void OptimizeInternal(PlacedItem item, bool spread) {
            if (!IsOptimizable(item)) return;

            var go = item.gameObject;
            var cell = _world.Grid.Snap(go.transform.position);
            //collect normals for faces without optimizable neighbours
            var faceNormals = new List<Vector3>(6);
            foreach (var kvp in OffsetsToUvs) {
                var offset = kvp.Key;
                var optimizableNeighbours = _world.GetObjectsInCell(cell + offset).Where(IsOptimizable).ToList();
                var entries = optimizableNeighbours.Select(n => n.CatalogEntry);
                if (entries.Any(n => n.Id == item.CatalogEntry.Id || item.CatalogEntry.OptimizableWith(n))) {
                    if (spread) {
                        foreach (var neighbour in optimizableNeighbours) {
                            OptimizeInternal(neighbour, false); //update neighbouring objects
                        }
                    }
                } else {
                    faceNormals.Add(offset);
                }
            }
            var mask = GetMaskForNormals(faceNormals.Select(n => go.transform.InverseTransformDirection(n)));
            go.GetComponent<MeshFilter>().mesh = _meshCache[mask];
        }

        private int GetMaskForNormals(IEnumerable<Vector3> normals) {
            return normals
                .Select(n => 1 << _offsets.IndexOf(n))
                .Aggregate(0, (mask, o) => mask | o);
        }

        private Mesh MakeMeshForNormals(IList<Vector3> normals) {
            var verts = new Vector3[VertsPerFace * normals.Count];
            var uvs = new Vector2[VertsPerFace * normals.Count];
            var faces = new int[VertsForQuad * normals.Count];
            for (var i = normals.Count - 1; i >= 0; i--) {
                var baseVertIndex = i * VertsPerFace;
                var baseFaceIndex = i * VertsForQuad;
                var normal = normals[i];
                var position = normal / 2; //in a 1x1x1 cube, a faces position is 0.5 away from center

                var planar = normal.x + normal.y + normal.z > 0 //remember theres always only 1 nonzero axis, this is basically a or
                    ? Vector3.one - normal //equivalent operations for negative and positive normals
                    : Vector3.one + normal;
                //source: http://stackoverflow.com/questions/29063139/how-to-get-plane-vertices-by-a-point-and-normal
                var dot = Vector3.Cross(normal, planar); //gets a vector pointing diagonally to one of the square's vertices

                var deltaA = planar / 2;
                var deltaB = dot / 2;
                verts[baseVertIndex] = position + deltaB;
                verts[baseVertIndex + 1] = position + deltaA;
                verts[baseVertIndex + 2] = position - deltaA;
                verts[baseVertIndex + 3] = position - deltaB;

                OffsetsToUvs[normals[i]].CopyTo(uvs, baseVertIndex);

                faces[baseFaceIndex] = baseVertIndex + 2;
                faces[baseFaceIndex + 1] = baseVertIndex + 1;
                faces[baseFaceIndex + 2] = baseVertIndex;
                faces[baseFaceIndex + 3] = baseVertIndex + 2;
                faces[baseFaceIndex + 4] = baseVertIndex + 3;
                faces[baseFaceIndex + 5] = baseVertIndex + 1;
            }
            
            var mesh = new Mesh();
            mesh.Clear();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.SetIndices(faces, MeshTopology.Triangles, 0);
            mesh.RecalculateNormals();
            mesh.Optimize();
            return mesh;
        }

        /// <summary>
        /// Returns all neighbours to the given cell that are optimizable
        /// </summary>
        /// <param name="cell">The cell position</param>
        /// <returns>A list of offsets from cell and their associated neighbours</returns>
        private IEnumerable<KeyValuePair<Vector3, PlacedItem>> GetOptimizableNeighbours(Vector3 cell) {
            return OffsetsToUvs
                .Select(kvp => new KeyValuePair<Vector3, PlacedItem[]>(kvp.Key, _world.GetObjectsInCell(cell + kvp.Key).ToArray())) //gets the objects in each offset
                .Where(kvp => kvp.Value.Length > 0 && IsOptimizable(kvp.Value[0])) //filters for objects that are optimizable
                .Select(kvp => new KeyValuePair<Vector3, PlacedItem>(kvp.Key, kvp.Value[0])); //finally return the offset and associated object
        }



        /// <summary>
        /// Verifies that an item is marked optimizable, is evenly rotated and not scaled.
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <returns>True if the item is optimizable</returns>
        private bool IsOptimizable(PlacedItem item) {
            var trans = item.transform;

            return item.CatalogEntry.Optimizable
                && item.UniqueInSlot
                && AlmostEqual(trans.localScale, Vector3.one)
                && IsOrthogonalRotation(trans.rotation);
        }

        /// <summary>
        /// Returns true if the given quaternion represents a orthogonal direction within an error margin
        /// </summary>
        /// <param name="rotation">The rotation to check</param>
        /// <returns>True if all axes' rotations are multiples of 90 degrees</returns>
        private static bool IsOrthogonalRotation(Quaternion rotation) {
            var angles = rotation.eulerAngles;
            return AlmostMultiple(angles.x, 90)
                && AlmostMultiple(angles.y, 90)
                && AlmostMultiple(angles.z, 90);
        }
        

        private static bool AlmostMultiple(float a, float b, float precision = 0.0001f) {
            var mod = a % b;
            return mod < precision || b - mod < precision;
        }

        private static bool AlmostEqual(Vector3 a, Vector3 b) {
            return Math.Abs((a - b).sqrMagnitude) < 0.0001;
        }
    }
}
