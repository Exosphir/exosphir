using Edit.Backend;
using UnityEngine;
using UnityEngine.UI;

namespace Edit {
    public class CatalogItemButton : MonoBehaviour {
        public delegate void ClickHandler(CatalogItemButton button);
        public Button Button;
        [HideInInspector]
        public CatalogItem Item;
        [HideInInspector]
        public CatalogInterface Interface;

        public event ClickHandler Click;
        
        void Start() {
            if (Button != null) {
                Button.onClick.AddListener(Select);
                Button.onClick.AddListener(OnClick);
            }
        }

        public void Select() {
            Button.enabled = false;
        }

        public void Unselect() {
            Button.enabled = true;
        }

        protected virtual void OnClick() {
            var handler = Click;
            if (handler != null) {
                handler(this );
            }
        }
    }
}
