using System.Collections.Generic;
using System.Collections.Specialized;

namespace PZ20;

public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T> {
    public void ReplaceItem(T prevItem, T newItem) {
        int index = IndexOf(prevItem);
        this[index] = newItem;
        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, prevItem, newItem)
        );
    }

    public ObservableCollection(IEnumerable<T> collection) : base(collection) {
    }

    public ObservableCollection() : base() {
    }

    public ObservableCollection(List<T> list) : base(list) {
    }
}