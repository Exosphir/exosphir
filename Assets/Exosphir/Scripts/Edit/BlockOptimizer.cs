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
        /// <summary>
        /// Using MeshTopology.Quads, each face defined by 4 vert index
        /// </summary>
        private const int VertsForQuad = 4;
        private static readonly Vector3[] NeighbourOffsets = {
            Vector3.up, Vector3.down,
            Vector3.left, Vector3.right,
            Vector3.forward, Vector3.back
        };

        private EditorWorld _world;

        void Start() {
            _world = EditorWorld.GetInstance();
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
            foreach (var offset in NeighbourOffsets) {
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

            var verts = new Vector3[VertsPerFace * faceNormals.Count];
            var faces = new int[VertsForQuad * faceNormals.Count];
            for (var i = faceNormals.Count - 1; i >= 0; i--) {
                var baseVertIndex = i * VertsPerFace;
                var baseFaceIndex = i * VertsForQuad;

                var normal = go.transform.InverseTransformDirection(faceNormals[i]); //account for rotation
                var position = normal / 2; //in a 1x1x1 cube, a faces position is 0.5 away from center

                var planar = normal.x + normal.y + normal.z > 0 //remember theres always only 1 nonzero axis, this is basically a or
                    ? Vector3.one - normal //equivalent operations for negative and positive normals
                    : -normal - Vector3.one;
                //source: http://stackoverflow.com/questions/29063139/how-to-get-plane-vertices-by-a-point-and-normal
                var dot = Vector3.Cross(normal, planar); //gets a vector pointing diagonally to one of the square's vertices

                var deltaA = planar / 2;
                var deltaB = dot / 2;
                verts[baseVertIndex    ] = position + deltaA;
                verts[baseVertIndex + 1] = position + deltaB;
                verts[baseVertIndex + 2] = position - deltaA;
                verts[baseVertIndex + 3] = position - deltaB;
                
                faces[baseFaceIndex    ] = baseVertIndex;
                faces[baseFaceIndex + 1] = baseVertIndex + 1;
                faces[baseFaceIndex + 2] = baseVertIndex + 2;
                faces[baseFaceIndex + 3] = baseVertIndex + 3;
            }

            var filter = go.GetComponent<MeshFilter>();
            var mesh = filter.mesh;
            mesh.Clear();
            mesh.vertices = verts;
            mesh.SetIndices(faces, MeshTopology.Quads, 0);
            mesh.RecalculateNormals();
            mesh.Optimize();
        }

        /// <summary>
        /// Returns all neighbours to the given cell that are optimizable
        /// </summary>
        /// <param name="cell">The cell position</param>
        /// <returns>A list of offsets from cell and their associated neighbours</returns>
        private IEnumerable<KeyValuePair<Vector3, PlacedItem>> GetOptimizableNeighbours(Vector3 cell) {
            return NeighbourOffsets
                .Select(offset => new KeyValuePair<Vector3, PlacedItem[]>(offset, _world.GetObjectsInCell(cell + offset).ToArray())) //gets the objects in each offset
                .Where(kvp => kvp.Value.Length > 0 && IsOptimizable(kvp.Value[0])) //filters for objects that are optimizable
                .Select(kvp => new KeyValuePair<Vector3, PlacedItem>(kvp.Key, kvp.Value[0])); //finally return the offset and associated object
        }

        private bool IsOptimizable(PlacedItem item) {
            //uses tolerances because floats!
            var trans = item.transform;

            return item.CatalogEntry.Optimizable
                && item.UniqueInSlot
                && Math.Abs((trans.localScale - Vector3.one).sqrMagnitude) < 0.0001 //scale within a very small variance from the unit scale
                && IsOrthogonalRotation(trans.rotation);
        }

        private static bool IsOrthogonalRotation(Quaternion q) {
            var angles = q.eulerAngles;
            return AlmostMultiple(angles.x, 90)
                && AlmostMultiple(angles.y, 90)
                && AlmostMultiple(angles.z, 90);
        }

        private static bool AlmostMultiple(float a, float b, float precision = 0.0001f) {
            var mod = a % b;
            return mod < precision || b - mod < precision;
        }
    }
}
