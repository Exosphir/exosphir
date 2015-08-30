using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic multiple object pool, stores model objects and instantiates them when
/// necessary. Calls sends a "Reset" message upon stashing a object!
/// </summary>
/// <typeparam name="TKey">The type of key to categorize the pools</typeparam>
public abstract class GameObjectPool<TKey> : MonoBehaviour {
    private class PoolStorage : Queue<GameObject> {
        public GameObject Model;

        public PoolStorage(int capacity): base(capacity) { }
    }

    private const string ResetMessage = "Reset";

    /// <summary>
    /// Parent of all pooled objects
    /// </summary>
    public Transform PooledHolder;
    /// <summary>
    /// Function to be called ONCE if a model has not been provided for the key.
    /// Must return a model to be associated with the key.
    /// </summary>
    public Func<TKey, GameObject> ModelGenerator;

    private readonly Dictionary<TKey, PoolStorage> _objectStorage;

    protected GameObjectPool() {
        _objectStorage = new Dictionary<TKey, PoolStorage>();
    }

    /// <summary>
    /// Sets the model of a specific key to the given GameObject
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="model">The associated model</param>
    public void SetModel(TKey key, GameObject model) {
        GetStorageFor(key).Model = model;
    }

    /// <summary>
    /// Inserts a object into the pool, use this to return unused objects.
    /// </summary>
    /// <param name="key">The key this object belongs to</param>
    /// <param name="obj">The object to be inserted</param>
    public void AddTo(TKey key, GameObject obj) {
        AddToStorage(GetStorageFor(key), obj);
    }

    /// <summary>
    /// Creates /amount/ copies of the model for the given key and adds them to the pool
    /// </summary>
    /// <param name="key">The key to be filled</param>
    /// <param name="amount">The amount of items to fill</param>
    public void FillWithModel(TKey key, int amount) {
        var storage = GetStorageFor(key);
        FillWithModel(key, storage, amount);
    }

    /// <summary>
    /// Grabs a object from the pool, and generates more if necessary
    /// </summary>
    /// <param name="key">The key to get</param>
    /// <param name="dryFillAmount">How many to create if the key empties</param>
    /// <returns>The object removed from the pool</returns>
    public GameObject Get(TKey key, int dryFillAmount = 1) {
        dryFillAmount = Math.Max(dryFillAmount, 1); // always add at least one
        var storage = GetStorageFor(key);

        if (storage.Count > 0) {
            var obj = storage.Dequeue();
            obj.transform.SetParent(null);
            obj.SetActive(true);
            return obj;
        }
        if (storage.Count == 0) {
            FillWithModel(key, storage, dryFillAmount);
            return Get(key, 0);
        }
        Debug.LogError("WTF: Pool empty after filling!");
        return null;
    }

    private void FillWithModel(TKey key, PoolStorage storage, int amount) {
        if (storage.Model == null) {
            storage.Model = ModelGenerator(key);
        }
        for (int i = 0; i < amount; i++) {
            AddToStorage(storage, Instantiate(storage.Model));
        }
    }

    private void AddToStorage(PoolStorage storage, GameObject obj) {
        obj.SendMessage(ResetMessage, SendMessageOptions.DontRequireReceiver);
        obj.SetActive(false);
        obj.transform.SetParent(PooledHolder);
        storage.Enqueue(obj);
    }

    private PoolStorage GetStorageFor(TKey key, int estimatedCount = 0) {
        if (!_objectStorage.ContainsKey(key)) {
            _objectStorage[key] = new PoolStorage(estimatedCount);
        }
        return _objectStorage[key];
    }
}