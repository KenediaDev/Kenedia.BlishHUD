using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Kenedia.Modules.Dev.Models
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        where TValue : INotifyPropertyChanged
    {
        public new TValue this[TKey key]
        {
            get => base[key];
            set => OnValueChanged(key, ContainsKey(key) ? base[key] : default, value);
        }

        public ObservableDictionary()
        {

        }

        private void OnValueChanged(TKey key, TValue v, TValue value)
        {
            if (value?.Equals(v) is true) return;

            base[key] = value;
            Debug.WriteLine($"Changed '[{key}]' from v [{v}] to value [{value}]");
        }
    }
}
