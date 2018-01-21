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
    public class MultiMap<TKey, TValue>
    {
        private readonly Dictionary<TKey, List<TValue>> storage = new Dictionary<TKey, List<TValue>>();

        public IEnumerable<TKey> Keys => storage.Keys;

        public int Count => storage.Count;

        public MultiMap()
        {
        }

        public MultiMap(IEnumerable<KeyValuePair<TKey, TValue>> data)
        {
            foreach (KeyValuePair<TKey, TValue> pair in data)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (!storage.TryGetValue(key, out List<TValue> storageValue))
            {
                storageValue = new List<TValue>();
                storage[key] = storageValue;
            }
            storageValue.Add(value);
        }

        public void Clear()
        {
            storage.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return storage.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out List<TValue> value) => storage.TryGetValue(key, out value);

        public List<TValue> this[TKey key] =>
            storage.TryGetValue(key, out List<TValue> storageValue)
            ? storageValue
            : new List<TValue>();
    }
}
