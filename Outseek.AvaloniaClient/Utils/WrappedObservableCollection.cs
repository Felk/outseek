using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Outseek.AvaloniaClient.Utils
{
    /// <summary>
    /// An observable collection that stays in sync with the observable collection passed in,
    /// but maps the collection's items to another type.
    /// </summary>
    public sealed class WrappedObservableCollection<T, T2> : ObservableCollection<T>, IDisposable
    {
        private readonly ObservableCollection<T2> _wrappedCollection;
        private readonly Func<T2, T> _wrap;
        private readonly Func<T, T2> _unwrap;

        public WrappedObservableCollection(ObservableCollection<T2> wrappedCollection, Func<T2, T> wrap, Func<T, T2> unwrap)
        {
            _wrappedCollection = wrappedCollection;
            _wrap = wrap;
            _unwrap = unwrap;

            _wrappedCollection.CollectionChanged += OnWrappedCollectionChanged;
        }

        private void OnWrappedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Clear();
                foreach (T2 item in _wrappedCollection)
                    Add(_wrap(item));
            }
            else
            {
                if (e.OldItems != null)
                {
                    var oldItems = e.OldItems;
                    List<T> oldChildren = this.Where(c => oldItems.Contains(_unwrap(c))).ToList();
                    foreach (T oldChild in oldChildren)
                        Remove(oldChild);
                }
                if (e.NewItems != null)
                {
                    foreach (T2 newNode in e.NewItems)
                        Add(_wrap(newNode));
                }
            }
        }

        public void Dispose()
        {
            _wrappedCollection.CollectionChanged -= OnWrappedCollectionChanged;
        }
    }
}
