using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Core.Models
{
#nullable enable
    public class NotifyPropertyChangedDictionary<TKey, TValue> : Dictionary<TKey, TValue?>, IDisposable
        where TValue : INotifyPropertyChanged
        where TKey : notnull
    {

        public new TValue? this[TKey key]
        {
            get => base[key];
            set => OnValueChanged(key, ContainsKey(key) ? base[key] : default, value);
        }

        private void OnValueChanged(TKey key, TValue? oldValue, TValue? newValue, [CallerMemberName] string? propName = null)
        {
            if (oldValue?.Equals(newValue) is true) return;

            if (oldValue != null)
            {
                oldValue.PropertyChanged -= ItemProperty_Changed;
            }

            if (newValue != null)
            {
                newValue.PropertyChanged += ItemProperty_Changed;
            }

            bool exists = ContainsKey(key);

            base[key] = newValue;
            ItemChanged?.Invoke(this, new(key, oldValue, newValue));

            if (!exists)
            {
                CollectionChanged?.Invoke(this, new(propName));
            }
        }

        private void ItemProperty_Changed(object? sender, PropertyChangedEventArgs e)
        {
            ItemPropertyChanged?.Invoke(sender, e);
        }

        public NotifyPropertyChangedDictionary()
        {

        }

        public event EventHandler<PropertyChangedEventArgs>? CollectionChanged;
        public event EventHandler<PropertyChangedEventArgs>? ItemPropertyChanged;
        public event EventHandler<DictionaryItemChangedEventArgs<TKey, TValue?>>? ItemChanged;

        public new void Add(TKey key, TValue? value)
        {
            if (ContainsKey(key)) return;
            if (value != null)
            {
                value.PropertyChanged += ItemProperty_Changed;
            }

            base.Add(key, value);
            ItemChanged?.Invoke(this, new(key, default, value));
            CollectionChanged?.Invoke(this, new("Collection"));
        }

        public new bool Remove(TKey key)
        {
            if (ContainsKey(key))
            {
                if (this[key] != null)
                {
                    this[key].PropertyChanged -= ItemProperty_Changed;
                }
            }

            return base.Remove(key);
        }

        public void Dispose()
        {

        }

        public virtual void Wipe()
        {
            foreach (var key in Keys.ToList())
            {
                this[key] = default;
            }
        }

        protected virtual void OnItemChanged()
        {

        }
    }
#nullable disable

#nullable enable
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue?>
    {
        public new TValue? this[TKey key]
        {
            get => base[key];
            set => OnValueChanged(key, ContainsKey(key) ? base[key] : default, value);
        }

        public ObservableDictionary()
        {

        }

        public event PropertyChangedEventHandler? CollectionChanged;
        public event EventHandler<DictionaryItemChangedEventArgs<TKey, TValue?>>? ItemChanged;

        private void OnValueChanged(TKey key, TValue? oldValue, TValue? newValue, [CallerMemberName] string? propName = null)
        {
            if (oldValue?.Equals(newValue) is true) return;
                        
            bool exists = ContainsKey(key);

            base[key] = newValue;
            ItemChanged?.Invoke(this, new(key, oldValue, newValue));

            if (!exists)
            {
                CollectionChanged?.Invoke(this, new(propName));
            }
        }

        public new void Add(TKey key, TValue? value)
        {
            if (ContainsKey(key)) return;

            base.Add(key, value);
            ItemChanged?.Invoke(this, new(key, default, value));
            CollectionChanged?.Invoke(this, new("Collection"));
        }

        public new bool Remove(TKey key)
        {
            bool deleted = base.Remove(key);
            if (deleted)
            {
                CollectionChanged?.Invoke(this, new("Collection"));
            }

            return deleted;
        }

        public void Wipe()
        {
            foreach (var key in Keys.ToList())
            {
                this[key] = default;
            }
        }
    }
#nullable disable
}
