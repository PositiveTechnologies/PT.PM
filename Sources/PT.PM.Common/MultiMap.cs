using System.Collections;
using System.Collections.Generic;

namespace PT.PM.Common
{
    /// <summary>
    /// An alternative to .NET Dictionary class that allows duplicate keys,
    /// and is still mutable (unlike the Lookup class).
    /// </summary>
    /// <remarks>
    /// Inspired by http://stackoverflow.com/questions/146204/duplicate-keys-in-net-dictionaries
    /// </remarks>
    public class MultiMap<TKey, TValue> : ICollection<KeyValuePair<TKey, List<TValue>>>, IEnumerable<KeyValuePair<TKey, List<TValue>>>, IEnumerable
    {
        private readonly Dictionary<TKey, List<TValue>> storage;

        public ICollection<TKey> Keys => storage.Keys;

        public int Count => storage.Count;

        public ICollection<List<TValue>> Values => storage.Values;

        public bool IsReadOnly => false;

        public MultiMap(IEqualityComparer<TKey> comparer)
        {
            storage = new Dictionary<TKey, List<TValue>>(comparer);
        }

        public MultiMap(IEnumerable<KeyValuePair<TKey, TValue>> data, int capacity = 0)
            : this(capacity)
        {
            foreach (KeyValuePair<TKey, TValue> pair in data)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public MultiMap(int capacity = 0)
        {
            storage = capacity == 0
                ? new Dictionary<TKey, List<TValue>>()
                : new Dictionary<TKey, List<TValue>>(capacity);
        }

        public void Add(TKey key, TValue value)
        {
            if (!storage.TryGetValue(key, out List<TValue> storageValue))
            {
                storageValue = new List<TValue>();
                storage.Add(key, storageValue);
            }
            storageValue.Add(value);
        }

        public void Clear() => storage.Clear();

        public bool ContainsKey(TKey key) => storage.ContainsKey(key);

        public bool TryGetValue(TKey key, out List<TValue> value) => storage.TryGetValue(key, out value);

        public void Add(KeyValuePair<TKey, List<TValue>> items)
        {
            List<TValue> value;
            if (!storage.TryGetValue(items.Key, out value))
            {
                value = new List<TValue>();
                storage.Add(items.Key, value);
            }
            value.AddRange(items.Value);
        }

        public bool Contains(KeyValuePair<TKey, List<TValue>> items)
        {
            List<TValue> values;
            if (!storage.TryGetValue(items.Key, out values))
            {
                return false;
            }

            foreach (TValue item in items.Value)
            {
                if (!values.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        public void CopyTo(KeyValuePair<TKey, List<TValue>>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, List<TValue>> items)
        {
            List<TValue> values;
            if (!storage.TryGetValue(items.Key, out values))
            {
                return false;
            }

            foreach (TValue item in items.Value)
            {
                values.Remove(item);
            }

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => storage.GetEnumerator();

        public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator() => storage.GetEnumerator();

        public List<TValue> this[TKey key] => storage[key];
    }
}
