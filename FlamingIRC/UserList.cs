using System;
using System.Collections;
using System.Collections.Generic;

namespace FlamingIRC;

public class UserList : IList<User>, INotifyUpdate
{
    private readonly List<User> _list;
    private EventHandler _onUpdate;

    /// <summary>
    /// Specifies whether or not to trigger the Updated event.
    /// </summary>
    /// <remarks>Useful when adding a large number of User objects.
    /// Be sure to call Refresh to trigger Updated manually.</remarks>
    public bool NotifyUpdate { get; set; }

    public UserList()
    {
        _list = new List<User>();
        NotifyUpdate = true;
    }

    public UserList(List<User> userList)
    {
        _list = userList;
        NotifyUpdate = true;
    }

    #region IList<User> Members

    public int IndexOf(User item) => _list.IndexOf(item);

    public void Insert(int index, User item)
    {
        _list.Insert(index, item);
        OnUpdate();
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
        OnUpdate();
    }

    public User this[int index]
    {
        get => _list[index];
        set
        {
            _list[index] = value;
            OnUpdate();
        }
    }

    #endregion

    #region ICollection<User> Members

    public void Add(User item)
    {
        _list.Add(item);
        OnUpdate();
    }

    public void Clear()
    {
        _list.Clear();
        OnUpdate();
    }

    public bool Contains(User item) => _list.Contains(item);

    public void CopyTo(User[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public int Count => _list.Count;

    public bool IsReadOnly => false;

    public bool Remove(User item)
    {
        var a = _list.Remove(item);
        OnUpdate();
        return a;
    }

    #endregion

    #region IEnumerable<User> Members

    public IEnumerator<User> GetEnumerator() => _list.GetEnumerator();

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    #endregion

    #region INotifyUpdate Members

    public event EventHandler Updated
    {
        add => _onUpdate += value; remove => _onUpdate -= value;
    }

    #endregion

    private void OnUpdate()
    {
        if (_onUpdate != null && NotifyUpdate)
            _onUpdate(this, new EventArgs());
    }

    public User GetUser(string nick)
    {
        foreach (var user in _list)
            if (user.Nick.ToUpper() == nick.ToUpper())
                return user;

        return null;
    }

    public virtual User GetUser(User user) => GetUser(user.Nick);

    /// <summary>
    /// Triggers the Updated event
    /// </summary>
    public void Refresh() => OnUpdate();

    public void Sort(Comparison<User> comparison) => _list.Sort(comparison);
}
