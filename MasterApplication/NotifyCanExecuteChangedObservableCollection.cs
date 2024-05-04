using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MasterApplication
{
    /// <summary>
    /// Custom <see cref="ObservableCollection{T}"/> that takes in a delegate to notify that the collection has changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NotifyCanExecuteChangedObservableCollection<T> : ObservableCollection<T>
    {
        private readonly Action _notifyCanExecuteChanged;

        public NotifyCanExecuteChangedObservableCollection(Action notifyCanExecuteChanged)
        {
            _notifyCanExecuteChanged = notifyCanExecuteChanged ?? throw new ArgumentNullException(nameof(notifyCanExecuteChanged));

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
            _notifyCanExecuteChanged();

            // Reattach the custom event handler
            CollectionChanged += CustomOnCollectionChanged;
        }
    }
}
