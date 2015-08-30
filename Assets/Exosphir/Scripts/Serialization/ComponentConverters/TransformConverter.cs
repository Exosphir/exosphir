using System;
using UnityEngine;

namespace Serialization.ComponentConverters {
    /// <summary>
    /// Converts a object's Transform into serializable form and back.
    /// Also serves as a reference example to implementing other IComponentConverters
    /// </summary>
    [ConverterFor(typeof(Transform))]
    public class TransformConverter : IComponentConverter {
        [Serializable]
        public class TransformData : ComponentData {
            public float PositionX, PositionY, PositionZ;
            public float RotationX, RotationY, RotationZ;
            public float ScaleX, ScaleY, ScaleZ;
        }
        public Component DeserializeAndAdd(ComponentData input, GameObject item) {
            //NOTE: other components should first be added to the gameobject,
            //Transform is an exception because it is intrinsic to the object
            //Other implementations should always add the component first:
            //  var component = item.AddComponent<TheComponent>();
            //  //do stuff with component
            var data = (TransformData) input;
            item.transform.position = new Vector3(data.PositionX, data.PositionY, data.PositionZ);
            item.transform.rotation = Quaternion.Euler(data.RotationX, data.RotationY, data.RotationZ);
            item.transform.localScale = new Vector3(data.ScaleX, data.ScaleY, data.ScaleZ);
            return item.transform;
        }

        public ComponentData Serialize(Component input) {
            var component = (Transform)input;
            var pos = component.position;
            var rot = component.rotation.eulerAngles;
            var scale = component.localScale;
            return new TransformData {
                PositionX = pos.x,
                PositionY = pos.y,
                PositionZ = pos.z,
                RotationX = rot.x,
                RotationY = rot.y,
                RotationZ = rot.z,
                ScaleX = scale.x,
                ScaleY = scale.y,
                ScaleZ = scale.z
            };
        }
    }
}
