using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Collection that reports changes to an event.
/// Very similar to .NET 4.5 class, except for the event's name,
/// and the fact it doesn't support PropertyChanged.
/// </summary>
/// <typeparam name="T">Type of the values of the collection</typeparam>
class ObservableCollection<T> : ICollection<T> {
    public delegate void CollectionChangedEvent();

    public event CollectionChangedEvent Updated;
    private readonly ICollection<T> _items;
    
    public ObservableCollection(ICollection<T> collection) {
        _items = collection;
    }

    public IEnumerator<T> GetEnumerator() {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Add(T item) {
        _items.Add(item);
        OnUpdated();
    }

    public void Clear() {
        _items.Clear();
        OnUpdated();
    }

    public bool Contains(T item) {
        return _items.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        _items.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item) {
        var result = _items.Remove(item);
        OnUpdated();
        return result;
    }

    public int Count {
        get { return _items.Count; }
    }

    public bool IsReadOnly {
        get { return _items.IsReadOnly; }
    }

    protected virtual void OnUpdated() {
        var handler = Updated;
        if (handler != null) {
            handler();
        }
    }
}