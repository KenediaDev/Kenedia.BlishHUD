using Blish_HUD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            if (value?.Equals(v) is true) return;

            base[key] = value;
            CollectionChanged?.Invoke(this, new(propName));
        }
    }
}
