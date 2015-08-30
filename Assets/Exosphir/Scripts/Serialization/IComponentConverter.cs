using System;
using UnityEngine;

namespace Serialization {
    /// <summary>
    /// Converts a Unity component into a series of bytes for serialization purposes.
    /// </summary>
    public interface IComponentConverter {
        /// <summary>
        /// Obtains a component out of a series of bytes and places it on the given object
        /// </summary>
        /// <param name="input">The serialized component</param>
        /// <param name="item">The gameobject to host the component</param>
        /// <returns>A instance of the component</returns>
        Component DeserializeAndAdd(ComponentData input, GameObject item);
        /// <summary>
        /// Converts the component into a series of bytes.
        /// </summary>
        /// <param name="component">The component to serialize</param>
        /// <returns>The serialized representation of input</returns>
        ComponentData Serialize(Component component);
    }

    [Serializable]
    public abstract class ComponentData {}
}
