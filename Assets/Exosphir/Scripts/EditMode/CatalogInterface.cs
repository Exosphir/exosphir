using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EditMode {
    public class CatalogInterface : MonoBehaviour {
        private float _width;
        private RectTransform _thisRect;
        private ObservableCollection<CatalogItem> _observable;
        private Category _currentCategory;
        
        public RectTransform ButtonContainer;

        public Category CurrentCategory {
            get { return _currentCategory; }
            set {
                _currentCategory = value;
                _observable = new ObservableCollection<CatalogItem>(_currentCategory.Items);
                _observable.Updated += RecreateButtons;
            }
        }

        private void RecreateButtons() {
            var catalogComponent = Catalog.GetInstance();
            //delete all buttons
            foreach (Transform child in ButtonContainer) {
                Destroy(child);
            }
            foreach (var item in Catalog.GetInstance()) {
                var button = Instantiate(catalogComponent.ButtonTemplate);
                button.name = "Button " + item.Name;
                var image = button.transform.GetChild(0).GetComponent<RawImage>();
                image.texture = item.GetDefaultPreview();
                button.transform.SetParent(ButtonContainer, false);
            }
        }

        public void Start() {
            _thisRect = GetComponent<RectTransform>();
            CurrentCategory = Category.DefaultCategory;

            _width = Catalog.GetInstance().UiWidth;
            _thisRect.sizeDelta = new Vector2(_width, _thisRect.sizeDelta.y);

            RecreateButtons();
        }

    }
}
