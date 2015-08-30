using System.IO;
using Edit.Backend;
using UnityEngine;
using Resolution = Edit.Backend.Resolution;

namespace Edit.Editor {
    public static class PreviewAssetGenerator {
        private static readonly string PreviewDestinationRoot = "Assets/Exosphir/Textures/Resources";
        public static void GeneratePreviewToFile(CatalogItem item, Resolution resolution = new Resolution()) {
            if (resolution.Width == 0 || resolution.Height == 0) {
                resolution = CatalogItem.DefaultPreviewResolution;
            }
            if (item.Model == null) {
                return;
            }
            var resourcePath = PreviewCache.GetPathFor(resolution, item);
            var fullPath = Path.Combine(PreviewDestinationRoot, resourcePath);
            var folder = Path.GetDirectoryName(fullPath);

            if (folder != null) {
                Directory.CreateDirectory(folder);
            }

            var texture = item.PreviewImage.RenderPreview(resolution.Width, resolution.Height);
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            Object.DestroyImmediate(texture);
        }
    }
}
