using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EditMode {
    [Serializable]
    public struct Resolution {
        public int Width;
        public int Height;

        public Resolution(int width, int height) {
            Width = width;
            Height = height;
        }
    }

    public class PreviewCache {
        private const string PreviewResourcePath = "BlockPreviewImages";
        private static readonly Dictionary<Resolution, PreviewCache> CachesByResolution = new Dictionary<Resolution, PreviewCache>();

        private readonly Resolution _resolution;
        private readonly Dictionary<string, Texture2D> _imageCache = new Dictionary<string, Texture2D>();

        private PreviewCache(Resolution resolution) {
            _resolution = resolution;
        }

        public bool ContainsImageFor(CatalogItem item) {
            return GetImageFor(item) != null;
        }

        public bool TryGetImageFor(CatalogItem item, out Texture2D image) {
            image = GetImageFor(item);
            return image != null;
        }

        public Texture2D GetImageFor(CatalogItem item) {
            var path = GetPathFor(_resolution, item);
            if (path == null) {
                return null;
            }
            path = path.Substring(0, path.LastIndexOf(".", StringComparison.Ordinal)); // remove extension
            if (_imageCache.ContainsKey(path)) {
                return _imageCache[path];
            }
            var texture = Resources.Load<Texture2D>(path);
            if (texture != null) {
                _imageCache[path] = texture;
            }
            return texture;
        }

        public static string GetFolderForResolution(Resolution resolution) {
            return Path.Combine(PreviewResourcePath, string.Format("{0}x{1}", resolution.Width, resolution.Height));
        }

        public static string GetPathFor(Resolution resolution, CatalogItem item) {
            var folder = GetFolderForResolution(resolution);
            if (item.Model == null) {
                return null;
            }
            var path = Path.Combine(folder, string.Format("{0}.png", item.Model.name));
            return path.Replace('\\', '/');
        }

        public static PreviewCache GetForResolution(int width, int height) {
            if (width <= 0) throw new ArgumentOutOfRangeException("width");
            if (height <= 0) throw new ArgumentOutOfRangeException("height");
            var resolution = new Resolution(width, height);
            PreviewCache result;
            if (CachesByResolution.TryGetValue(resolution, out result)) {
                return result;
            }
            var cache = new PreviewCache(resolution);
            CachesByResolution[resolution] = cache;
            return cache;
        }
    }
}
