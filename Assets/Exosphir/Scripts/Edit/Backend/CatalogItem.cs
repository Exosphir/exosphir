using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Edit.Backend {
    /// <summary>
    /// Represents a entry in the object catalog.
    /// </summary>
    [Serializable]
    public class CatalogItem : ScriptableObject {
        public static readonly Resolution DefaultPreviewResolution = new Resolution(256, 256);
        [Serializable]
        public class Comparer : IComparer<CatalogItem> {
            public int Compare(CatalogItem x, CatalogItem y) {
                return x.Id.CompareTo(y.Id);
            }
        }
        
        public GameObject Model;
		public float AnchorHeight = 1.25f;
        public PreviewImage PreviewImage;
        public string[] Groups;
        public bool Rotatable = true;
        public bool Scalable = true;
		public bool Optimizable = false;
        public string[] OptimizationGroups;
        public int Id = -1;
        
        [SerializeField]
        private  Category _category;
        public Category Category {
            get { return _category; }
            set {
                if (_category != null) {
                    _category.Remove(this);
                }
                value.Add(this);
                _category = value;
            }
        }

        public string Name {
            get {
                return Model != null ? Model.name : null;
            }
            set {
                if (Model != null) {
                    Model.name = value;
                }
            }
        }

        public Texture2D GetDefaultPreview() {
            var cache = PreviewCache.GetForResolution(DefaultPreviewResolution.Width, DefaultPreviewResolution.Height);
            var preview = cache.GetImageFor(this);
            return preview ?? Catalog.GetInstance().NullTexture;
        }

        public bool OptimizableWith(CatalogItem other) {
            return OptimizationGroups.Intersect(other.Groups).Any();
        }

        void OnEnable() {
            if (PreviewImage == null) {
                PreviewImage = PreviewImage.Create(this);
            }
        }
    }
}
