using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Edit.Backend {
    /// <summary>
    /// A preview image, with a subject and camera positioning.
    /// 
    /// The camera position is defined to be DistanceToPivot units in front of the pivot, which is at the same location as the object.
    /// Then the camera is set to face the subject.
    /// Finally, the pivot is offset and rotated.
    /// 
    /// To obtain a image of this composition, see <see cref="RenderPreview"/>
    /// </summary>
    [Serializable]
    public class PreviewImage: ScriptableObject {
        /// <summary>
        /// A distant position to take the render, for minimal interference with whatever else is on the scene.
        /// </summary>
        private static readonly Vector3 RenderSetupPosition = new Vector3(10000, -10000, 10000);
        /// <summary>
        /// Overscaling is a simple solution to reduce aliasing.
        /// The render is taken at this many times greater resolution, then
        /// downscaled to the target resolution using bilinear filtering
        /// </summary>
        private const int OverscaleRender = 4;
        private static readonly Color MatteColor = new Color(0, 0, 0, 0);
        
        [SerializeField] CatalogItem _subject;

        public float DistanceToPivot = 5;
        public Vector3 PivotPosition = Vector3.zero;
        public Quaternion PivotRotation = Quaternion.Euler(-45, 45, 0);

        /// <summary>
        /// Sets up a copy of the subject, and creates a image from a camera positioned according to this instance's values
        /// </summary>
        /// <param name="width">The output image width, in pixels</param>
        /// <param name="height">The output image height, in pixels</param>
        /// <returns></returns>
        public Texture2D RenderPreview(int width, int height) {
            //validation
            if (width <= 0) { throw new ArgumentOutOfRangeException("width"); }
            if (height <= 0) { throw new ArgumentOutOfRangeException("height"); }


            var cameraObject = new GameObject("__catalog_preview_camera");
            var pivot = new GameObject("__catalog_preview_pivot");
            cameraObject.hideFlags = HideFlags.DontSave;
            pivot.hideFlags = HideFlags.DontSave;

            cameraObject.transform.parent = pivot.transform;
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = MatteColor;

            //must record active state to be able to instantiate disabled object
            //otherwise undesired scripts might run
            var wasActive = _subject.Model.activeSelf;
            _subject.Model.SetActive(false);
            var objectToRender = (GameObject) Object.Instantiate(_subject.Model, Vector3.zero, Quaternion.identity);
            objectToRender.name = "__catalog_preview_subject";
            _subject.Model.SetActive(wasActive);
            DisableScriptsInHierarchy(objectToRender);
            objectToRender.SetActive(true);

            //position camera and pivot
            objectToRender.transform.position = RenderSetupPosition;
            pivot.transform.position = RenderSetupPosition;
            camera.transform.localPosition = Vector3.forward * DistanceToPivot;
            camera.transform.LookAt(objectToRender.transform);
            pivot.transform.position += PivotPosition;
            pivot.transform.rotation = PivotRotation;
            camera.aspect = width / (float)height;

            //do render
            int ow = width * OverscaleRender,
                oh = height * OverscaleRender;
            var output = new RenderTexture(ow, oh, 32);
            RenderTexture.active = output;
            camera.targetTexture = output;
            camera.Render();

            //export image
            var image = new Texture2D(ow, oh, TextureFormat.ARGB32, false);
            image.ReadPixels(new Rect(0, 0, ow, oh), 0, 0);
            TextureScale.Bilinear(image, width, height);
            image.alphaIsTransparency = true;

            //cleanup
            RenderTexture.active = null;
            camera.targetTexture = null;
            DestroyImmediate(output);
            DestroyImmediate(cameraObject);
            DestroyImmediate(pivot);
            DestroyImmediate(objectToRender);


            return image;
        }

        public static PreviewImage Create(CatalogItem subject) {
            var preview = CreateInstance<PreviewImage>();
            preview._subject = subject;
            return preview;
        }

        /// <summary>
        /// Disables all scripts on the tree for obj.
        /// </summary>
        /// <param name="obj">The root object</param>
        private static void DisableScriptsInHierarchy(GameObject obj) {
            foreach (GameObject child in obj.transform) {
                DisableScriptsInHierarchy(child);
            }
            foreach (var behaviour in obj.GetComponents<MonoBehaviour>()) {
                behaviour.enabled = false;
            }
        }
    }
}
