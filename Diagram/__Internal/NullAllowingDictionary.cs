using System.Collections.Generic;

namespace Excubo.Blazor.Diagrams.__Internal
{
    internal class NullAllowingDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private bool has_null;
        private TValue value;
        public new void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                has_null = true;
                this.value = value;
            }
            else
            {
                base.Add(key, value);
            }
        }
        public new TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    if (has_null)
                    {
                        return value;
                    }
                    throw new KeyNotFoundException();
                }
                else
                {
                    return base[key];
                }
            }
            set
            {
                if (key == null)
                {
                    has_null = true;
                    this.value = value;
                }
                else
                {
                    base[key] = value;
                }
            }
        }
    }
}