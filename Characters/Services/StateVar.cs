using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Kenedia.Modules.Characters.Services
{
    public sealed class StateVarChangedEventArgs<T> : EventArgs
    {
        public StateVarChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; }

        public T NewValue { get; }
    }

    public sealed class StateVar<T>
    {
        private T _value;
        private INotifyPropertyChanged _observedObject;
        private INotifyCollectionChanged _observedCollection;
        private readonly HashSet<INotifyPropertyChanged> _observedCollectionItems = [];

        public event EventHandler<StateVarChangedEventArgs<T>> Changed;

        public T Value
        {
            get => _value;
            set => Set(value);
        }

        public bool Set(T value, bool forceNotify = false)
        {
            if (!forceNotify && EqualityComparer<T>.Default.Equals(_value, value))
            {
                return false;
            }

            var oldValue = _value;
            UnobserveCurrentValue();
            _value = value;
            ObserveCurrentValue();
            Changed?.Invoke(this, new StateVarChangedEventArgs<T>(oldValue, _value));
            return true;
        }

        private void ObserveCurrentValue()
        {
            if (_value is INotifyPropertyChanged notifyPropertyChanged)
            {
                _observedObject = notifyPropertyChanged;
                _observedObject.PropertyChanged += ObservedObject_PropertyChanged;
            }

            if (_value is INotifyCollectionChanged notifyCollectionChanged)
            {
                _observedCollection = notifyCollectionChanged;
                _observedCollection.CollectionChanged += ObservedCollection_CollectionChanged;

                if (_value is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        AttachObservedItem(item as INotifyPropertyChanged);
                    }
                }
            }
        }

        private void UnobserveCurrentValue()
        {
            if (_observedObject is not null)
            {
                _observedObject.PropertyChanged -= ObservedObject_PropertyChanged;
                _observedObject = null;
            }

            if (_observedCollection is not null)
            {
                _observedCollection.CollectionChanged -= ObservedCollection_CollectionChanged;
                _observedCollection = null;
            }

            foreach (var item in _observedCollectionItems.ToArray())
            {
                item.PropertyChanged -= ObservedItem_PropertyChanged;
            }

            _observedCollectionItems.Clear();
        }

        private void ObservedObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Changed?.Invoke(this, new StateVarChangedEventArgs<T>(_value, _value));
        }

        private void ObservedCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems)
                {
                    DetachObservedItem(item as INotifyPropertyChanged);
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems)
                {
                    AttachObservedItem(item as INotifyPropertyChanged);
                }
            }

            Changed?.Invoke(this, new StateVarChangedEventArgs<T>(_value, _value));
        }

        private void ObservedItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Changed?.Invoke(this, new StateVarChangedEventArgs<T>(_value, _value));
        }

        private void AttachObservedItem(INotifyPropertyChanged item)
        {
            if (item is null) return;
            if (!_observedCollectionItems.Add(item)) return;

            item.PropertyChanged += ObservedItem_PropertyChanged;
        }

        private void DetachObservedItem(INotifyPropertyChanged item)
        {
            if (item is null) return;
            if (!_observedCollectionItems.Remove(item)) return;

            item.PropertyChanged -= ObservedItem_PropertyChanged;
        }
    }
}
