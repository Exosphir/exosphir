using System;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization {
    [Serializable]
    public class SerializedGameObject {
        public string Name;
        public int CatalogId;
        public IList<SerializedComponent> Components;
    }
    [Serializable]
    public class SerializedComponent {
        public ulong Id;
        public ComponentData RawData;
    }
    [Serializable]
    public class SerializedWorld {
        public byte FileVersion;
        public Dictionary<string, string> Header;
        public IList<SerializedGameObject> Objects;

        public SerializedWorld(byte version) {
            FileVersion = version;
            Header = new Dictionary<string, string>();
            Objects = new List<SerializedGameObject>();
        }
    }
}
