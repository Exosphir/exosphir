using UnityEngine;

namespace EditMode {
    public class PlacedItem : MonoBehaviour {
        public bool UniqueInSlot;
        public CatalogItem CatalogEntry { get; set; }
    }
}
