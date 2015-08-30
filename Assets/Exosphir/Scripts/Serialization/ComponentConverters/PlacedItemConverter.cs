using System;
using Edit;
using UnityEngine;

namespace Serialization.ComponentConverters {
    [ConverterFor(typeof(PlacedItem))]
    public class PlacedItemConverter : IComponentConverter {
        [Serializable]
        public class PlacedItemData : ComponentData {
            public int Id;
            public bool Unique;
        }
        public Component DeserializeAndAdd(ComponentData input, GameObject item) {
            var data = (PlacedItemData) input;
            var catalogItem = Catalog.GetInstance().GetItemById(data.Id);
            var component = item.AddComponent<PlacedItem>();
            component.CatalogEntry = catalogItem;
            component.UniqueInSlot = data.Unique;
            return component;
        }

        public ComponentData Serialize(Component component) {
            var placedItem = (PlacedItem) component;
            return new PlacedItemData {
                Id = placedItem.CatalogEntry.Id,
                Unique = placedItem.UniqueInSlot
            };
        }
    }
}
