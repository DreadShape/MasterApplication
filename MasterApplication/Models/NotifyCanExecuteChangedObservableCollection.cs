using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MasterApplication.Models
{
    /// <summary>
    /// Custom <see cref="ObservableCollection{T}"/> that takes in a delegate to notify when an item inside the collection has changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NotifyCanExecuteChangedObservableCollection<T> : ObservableCollection<T>
    {
        private readonly IList<Action> _notifyCanExecuteChangedList;

        public NotifyCanExecuteChangedObservableCollection(IList<Action> notifyCanExecuteChangedList)
        {
            _notifyCanExecuteChangedList = notifyCanExecuteChangedList ?? throw new ArgumentNullException(nameof(notifyCanExecuteChangedList));

            // Subscribe to the default CollectionChanged event
            CollectionChanged += CustomOnCollectionChanged;
        }

        private void CustomOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Detach the custom event handler temporarily to prevent infinite loop
            CollectionChanged -= CustomOnCollectionChanged;

            // Call the base implementation of the event handler to maintain default behavior
            base.OnCollectionChanged(e);

            // Execute the provided action when the collection changes
            foreach (Action action in _notifyCanExecuteChangedList)
                action();

            // Reattach the custom event handler
            CollectionChanged += CustomOnCollectionChanged;
        }
    }
}
