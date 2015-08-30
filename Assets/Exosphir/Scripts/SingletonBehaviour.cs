using System;
using UnityEngine;

/// <summary>
/// Centralizes implementation of singleton MonoBehaviours.
/// Classes of this type expect to be present in the scene before being accessed
/// </summary>
/// <typeparam name="T">
/// The same type that extends this. It is recommendable to be a sealed class for extra safety
/// <![CDATA[
/// Example: 
/// sealed class MyBehaviour : SingletonBehaviour<MyBehaviour>  {}
/// ]]></typeparam>
[Serializable]
public class SingletonBehaviour<T> : MonoBehaviour where T: MonoBehaviour {
    private static T _instance;
    public static T GetInstance() {
        if (_instance != null) {
            return _instance;
        }
        var instances = FindObjectsOfType<T>();
        if (instances.Length > 1) {
            Debug.LogWarning("Too many instances of " + typeof(T).Name + " found, returning first.");
        }
        if (instances.Length >= 1) {
            _instance = instances[0];
        } else {
            Debug.LogError("No instances of " + typeof(T).Name + " found in scene!");
        }
        return _instance;
    }
}