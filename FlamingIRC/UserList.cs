namespace FlamingIRC
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class UserList : IList<User>, INotifyUpdate
    {
        private List<User> list;
        private EventHandler onUpdate;

        /// <summary>
        /// Specifies whether or not to trigger the Updated event.
        /// </summary>
        /// <remarks>Useful when adding a large number of User objects. 
        /// Be sure to call Refresh to trigger Updated manually.</remarks>
        public bool NotifyUpdate { get; set; }

        public UserList()
        {
            list = new List<User>();
            NotifyUpdate = true;
        }

        public UserList(List<User> userList)
        {
            list = userList;
            NotifyUpdate = true;
        }

        #region IList<User> Members

        public int IndexOf(User item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, User item)
        {
            list.Insert(index, item);
            OnUpdate();
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
            OnUpdate();
        }

        public User this[int index]
        {
            get => list[index];
            set
            {
                list[index] = value;
                OnUpdate();
            }
        }

        #endregion

        #region ICollection<User> Members

        public void Add(User item)
        {
            list.Add(item);
            OnUpdate();
        }

        public void Clear()
        {
            list.Clear();
            OnUpdate();
        }

        public bool Contains(User item)
        {
            return list.Contains(item);
        }

        public void CopyTo(User[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public bool Remove(User item)
        {
            var a = list.Remove(item);
            OnUpdate();
            return a;
        }

        #endregion

        #region IEnumerable<User> Members

        public IEnumerator<User> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region INotifyUpdate Members

        public event EventHandler Updated
        { add => onUpdate += value; remove => onUpdate -= value;
        }

        #endregion

        private void OnUpdate()
        {
            if (onUpdate != null && NotifyUpdate)
                onUpdate(this, new EventArgs());
        }

        public User GetUser(string nick)
        {
            foreach (var user in list)
                if (user.Nick.ToUpper() == nick.ToUpper())
                    return user;

            return null;
        }

        public virtual User GetUser(User user)
        {
            return GetUser(user.Nick);
        }

        /// <summary>
        /// Triggers the Updated event
        /// </summary>
        public void Refresh()
        {
            OnUpdate();
        }

        public void Sort(Comparison<User> comparison)
        {
            list.Sort(comparison);
        }
    }

}
