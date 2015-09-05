using UnityEngine;
using UnityEngine.EventSystems;

namespace Interface {
    /// <summary>
    /// Allows dragging of the parent UI element
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class WindowHandle : MonoBehaviour, IPointerDownHandler, IDragHandler {
        public bool Draggable = true;

        private RectTransform _window;
        private Vector2 _lastPosition;

        // Use this for initialization
        void Start () {
            _window = transform.parent as RectTransform;
        }
	
        // Update is called once per frame
        void Update () {
	
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (Draggable) {
                _lastPosition = eventData.position;
                _window.SetAsLastSibling(); //move window to top
            }
        }

        public void OnDrag(PointerEventData eventData) {
            if (_window == null || !Draggable) return;

            var position = eventData.position;
            Vector3 delta = position - _lastPosition; //type necessary to upgrade vec2 to vec3
            _window.localPosition += delta;
            _lastPosition = position;
        }
    }
}
