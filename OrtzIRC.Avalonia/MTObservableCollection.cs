namespace OrtzIRC.Avalonia;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using global::Avalonia.Threading;

public class MTObservableCollection<T> : ObservableCollection<T>
{
    protected override void InsertItem(int index, T item)
    {
        if (Dispatcher.UIThread.CheckAccess())
            base.InsertItem(index, item);
        else
            Dispatcher.UIThread.Post(() => base.InsertItem(index, item));
    }

    protected override void RemoveItem(int index)
    {
        if (Dispatcher.UIThread.CheckAccess())
            base.RemoveItem(index);
        else
            Dispatcher.UIThread.Post(() => base.RemoveItem(index));
    }

    protected override void ClearItems()
    {
        if (Dispatcher.UIThread.CheckAccess())
            base.ClearItems();
        else
            Dispatcher.UIThread.Post(() => base.ClearItems());
    }

    protected override void SetItem(int index, T item)
    {
        if (Dispatcher.UIThread.CheckAccess())
            base.SetItem(index, item);
        else
            Dispatcher.UIThread.Post(() => base.SetItem(index, item));
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        if (Dispatcher.UIThread.CheckAccess())
            base.MoveItem(oldIndex, newIndex);
        else
            Dispatcher.UIThread.Post(() => base.MoveItem(oldIndex, newIndex));
    }
}
