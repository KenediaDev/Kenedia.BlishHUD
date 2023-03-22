using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Core.Models
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public new TValue this[TKey key]
        {
            get => base[key];
            set => OnValueChanged(key, ContainsKey(key) ? base[key] : default, value);
        }

        public ObservableDictionary()
        {

        }

        public event PropertyChangedEventHandler CollectionChanged;

        private void OnValueChanged(TKey key, TValue v, TValue value, [CallerMemberName] string propName = null)
        {
            if (value?.Equals(v) is true)
            {
                return;
            }

            base[key] = value;
            CollectionChanged?.Invoke(this, new(propName));
        }

        public void Wipe()
        {
            foreach (var key in Keys.ToList())
            {
                this[key] = default;
            }
        }
    }
}
