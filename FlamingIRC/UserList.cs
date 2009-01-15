using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace FlamingIRC
{
    public class UserList : IList<User>, INotifyUpdate
    {
        private List<User> List;
        private EventHandler onUpdate;

        public UserList()
        {
            List = new List<User>();
        }

        #region IList<User> Members

        public int IndexOf(User item)
        {
            return List.IndexOf(item);
        }

        public void Insert(int index, User item)
        {
            List.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            List.RemoveAt(index);
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
            }
        }

        #endregion

        #region ICollection<User> Members

        public void Add(User item)
        {
            List.Add(item);
        }

        public void Clear()
        {
            List.Clear();
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
            return List.Remove(item);
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
            throw new NotImplementedException();
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
            if (onUpdate != null)
                onUpdate(this, new EventArgs());
        }

        /// <summary>
        /// Triggers the Updated event
        /// </summary>
        public void Refresh()
        {
            OnUpdate();
        }
    }

}
