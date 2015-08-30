using System;
using UnityEngine;

namespace Serialization {
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConverterFor : Attribute {
        public Type Component;

        public ConverterFor(Type component) {
            Component = component;
        }
    }
}
