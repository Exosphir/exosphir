using UnityEngine;

namespace EditMode {
    public class ItemCursor : MonoBehaviour {

        private GameObject _oldSelected;
        public GameObject Selected {
            get { return transform.GetChild(0).gameObject; }
            set {
                if (_oldSelected == value) return; //do nothing if it's the same object
                _oldSelected = value;
                foreach (Transform child in transform) {
                    Destroy(child.gameObject);
                }
                if (value == null) return; //dont try to instantiate null
                var visual = (GameObject)Instantiate(value, Vector3.zero, Quaternion.identity);
                visual.transform.SetParent(transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one;
                visual.name = value.name;
            }
        }
    }
}
