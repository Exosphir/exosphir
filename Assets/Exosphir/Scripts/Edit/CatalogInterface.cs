using System.Linq;
using Edit.Backend;
using UnityEngine;
using UnityEngine.UI;

namespace Edit {
    public class CatalogInterface : MonoBehaviour {
        private float _width;
        private RectTransform _thisRect;
        private ObservableCollection<CatalogItem> _observable;
        private Category _currentCategory;
        private CatalogItem _currentItem;
        private CatalogItemButton _currentItemButton;
        
        public RectTransform ButtonContainer;

        public Category CurrentCategory {
            get { return _currentCategory; }
            set {
                _currentCategory = value;
                _observable = new ObservableCollection<CatalogItem>(_currentCategory.Items);
                _observable.Updated += RecreateButtons;
                RecreateButtons();
            }
        }

        public CatalogItem CurrentItem {
            get { return _currentItem; }
            set {
                if (_currentCategory.Contains(value)) {
                    _currentItemButton.Unselect();
                }
                _currentItem = value;
                CurrentCategory = value.Category;
            }
        }
        
        private void RecreateButtons() {
            var catalog = Catalog.GetInstance();
            foreach (Transform child in ButtonContainer) {
                Destroy(child.gameObject);
            }
            foreach (var item in Catalog.GetInstance()) {
                var buttonObject = Instantiate(catalog.ButtonTemplate.gameObject);
                buttonObject.name = "Button " + item.Name;
                var image = buttonObject.transform.GetChild(0).GetComponent<RawImage>();
                image.texture = item.GetDefaultPreview();
                buttonObject.transform.SetParent(ButtonContainer, false);
                buttonObject.GetComponent<RectTransform>().sizeDelta = Vector2.one * catalog.UiItemWidth;

                var button = buttonObject.GetComponent<CatalogItemButton>();
                button.Click += ButtonClicked;
                button.Item = item;
                if (item == _currentItem) {
                    button.Select();
                }
            }
        }

        private void ButtonClicked(CatalogItemButton button) {
            _currentItemButton = button;
            CurrentItem = button.Item;
        }

        public void Start() {
            _thisRect = GetComponent<RectTransform>();
            CurrentCategory = Category.DefaultCategory;

            _width = Catalog.GetInstance().UiWidth;
            _thisRect.sizeDelta = new Vector2(_width, _thisRect.sizeDelta.y);

            RecreateButtons();
        }
        void Update() {
            var rect = Camera.main.rect;
            rect.xMin = _width / Screen.width;
            Camera.main.rect = rect;
        }
    }
}
