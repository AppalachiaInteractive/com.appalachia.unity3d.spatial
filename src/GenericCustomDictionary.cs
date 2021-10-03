#region

using System.Collections.Generic;

#endregion

namespace Appalachia.Spatial
{
    public abstract class GenericCustomDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _lookup = new Dictionary<TKey, TValue>();

        protected Dictionary<TKey, TValue> lookup
        {
            get
            {
                if (_lookup == null)
                {
                    _lookup = new Dictionary<TKey, TValue>();
                }

                return _lookup;
            }
        }

        public int Count => _lookup?.Count ?? 0;

        public bool Contains(TKey key)
        {
            return _lookup.ContainsKey(key);
        }

        public TValue Get(TKey key)
        {
            return _lookup[key];
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            var l = lookup;

            if (l.ContainsKey(key))
            {
                lookup[key] = value;
            }
            else
            {
                lookup.Add(key, value);
            }
        }

        public void Remove(TKey key)
        {
            _lookup.Remove(key);
        }
    }
}
