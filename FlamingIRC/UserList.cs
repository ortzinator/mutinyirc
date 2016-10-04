namespace FlamingIRC
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class UserList : IList<User>, INotifyUpdate
    {
        private List<User> List;
        private EventHandler onUpdate;

        /// <summary>
        /// Specifies whether or not to trigger the Updated event.
        /// </summary>
        /// <remarks>Useful when adding a large number of User objects. 
        /// Be sure to call Refresh to trigger Updated manually.</remarks>
        public bool NotifyUpdate { get; set; }

        public UserList()
        {
            List = new List<User>();
            NotifyUpdate = true;
        }

        public UserList(List<User> userList)
        {
            List = userList;
            NotifyUpdate = true;
        }

        #region IList<User> Members

        public int IndexOf(User item)
        {
            return List.IndexOf(item);
        }

        public void Insert(int index, User item)
        {
            List.Insert(index, item);
            OnUpdate();
        }

        public void RemoveAt(int index)
        {
            List.RemoveAt(index);
            OnUpdate();
        }

        public User this[int index]
        {
            get
            {
                return List[index];
            }
            set
            {
                List[index] = value;
                OnUpdate();
            }
        }

        #endregion

        #region ICollection<User> Members

        public void Add(User item)
        {
            List.Add(item);
            OnUpdate();
        }

        public void Clear()
        {
            List.Clear();
            OnUpdate();
        }

        public bool Contains(User item)
        {
            return List.Contains(item);
        }

        public void CopyTo(User[] array, int arrayIndex)
        {
            List.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return List.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(User item)
        {
            var a = List.Remove(item);
            OnUpdate();
            return a;
        }

        #endregion

        #region IEnumerable<User> Members

        public IEnumerator<User> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
            //TODO: Correct?
        }

        #endregion

        #region INotifyUpdate Members

        public event EventHandler Updated
        {
            add { onUpdate += value; }
            remove { onUpdate -= value; }
        }

        #endregion

        private void OnUpdate()
        {
            if (onUpdate != null && NotifyUpdate == true)
                onUpdate(this, new EventArgs());
        }

        public User GetUser(string nick)
        {
            foreach (var user in List)
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
            List.Sort(comparison);
        }
    }

}
