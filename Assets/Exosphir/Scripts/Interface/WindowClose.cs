using UnityEngine;
using UnityEngine.UI;

namespace Interface {
    [RequireComponent(typeof(Button))]
    public class WindowClose : MonoBehaviour {
        public RectTransform Window;

        void Start () {
            var button = GetComponent<Button>();
            button.onClick.AddListener(Close);
        }

        private void Close() {
            Window.gameObject.SetActive(false);
        }
    }
}
