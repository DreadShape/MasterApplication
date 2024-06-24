using System.Collections.ObjectModel;
using System.Windows;

namespace MasterApplication.Extensions;

/// <summary>
/// Extension methods for an <see cref="ObservableCollection{T}"/>.
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Extension method to safely update the <see cref="ObservableCollection{T}"/> from a different thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"><see cref="ObservableCollection{T}"/> to update safely.</param>
    /// <param name="updateAction"><see cref="Action"/> to update the <see cref="ObservableCollection{T}"/>.</param>
    /// <exception cref="ArgumentNullException">Gets thrown if <see cref="ObservableCollection{T}"/> or the <see cref="Action"/> are null.</exception>
    public static async Task SafeUpdateAsync<T>(this ObservableCollection<T> collection, Action updateAction)
    {
        if (collection == null) 
            throw new ArgumentNullException(nameof(collection));

        if (updateAction == null) 
            throw new ArgumentNullException(nameof(updateAction));

        if (Application.Current.Dispatcher.CheckAccess())
        {
            updateAction();
            return;
        }

        await Application.Current.Dispatcher.BeginInvoke(updateAction);
    }
}
