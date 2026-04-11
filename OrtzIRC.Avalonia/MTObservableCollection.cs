namespace OrtzIRC.Avalonia;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using global::Avalonia.Threading;

public class MTObservableCollection<T> : ObservableCollection<T>
{
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            base.OnCollectionChanged(e);
        }
        else
        {
            Dispatcher.UIThread.Post(() => OnCollectionChanged(e));
        }
    }
}
