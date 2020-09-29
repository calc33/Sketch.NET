using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sketch {
    //public class NameChangingEventArgs: EventArgs {
    //    private string _oldName;
    //    private string _newName;
    //    private bool _allowChange;

    //    public string NewName {
    //        get { return _newName; }
    //        set { _newName = value; }
    //    }
        
    //    public string OldName { get { return _oldName; } }

    //    public bool AllowChange {
    //        get { return _allowChange; }
    //        set { _allowChange = value; }
    //    }

    //    public NameChangingEventArgs(string newName, string oldName)
    //        : base() {
    //        _newName = newName;
    //        _oldName = oldName;
    //        _allowChange = true;
    //    }
    //}

    public interface INamedCollectionOwnable<TItem> {
        void OnCollectionItemAdded(object collection, TItem item);
        void OnCollectionItemRemoved(object collection, TItem item);
    }

    public class NamedCollection<TItem, TOwner>: ICollection<TItem>
        where TItem: ExprComponent<TOwner>
        where TOwner: class, IExprComponentOwnable, INamedCollectionOwnable<TItem>
    {
        private TOwner _owner;
        private List<TItem> _items = new List<TItem>();
        private Dictionary<string, TItem> _nameToItems = new Dictionary<string, TItem>();

        internal NamedCollection(TOwner owner) {
            _owner = owner;
        }

        public TOwner Owner { get { return _owner; } }

        public TItem this[int index] { get { return _items[index]; } }
        public TItem this[string name] {
            get {
                string key = ToDictionaryKey(name);
                if (string.IsNullOrEmpty(key)) {
                    throw new ArgumentNullException("name");
                }
                TItem ret;
                if (_nameToItems.TryGetValue(key, out ret)) {
                    return ret;
                }
                return null;
            }
        }

        public int IndexOf(TItem item) {
            return _items.IndexOf(item);
        }

        public int LastIndexOf(TItem item) {
            return _items.LastIndexOf(item);
        }

        /// <summary>
        /// _nameToItemsへ登録する際に使用する名前
        /// (アルファベットを大文字にする)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual string ToDictionaryKey(string name) {
            if (string.IsNullOrEmpty(name)) {
                return null;
            }
            return name.ToUpper();
        }

        public virtual string ValidNameDescription {
            get {
                return "制御文字は使用できません。";
            }
        }

        public virtual bool IsValidName(string name) {
            if (string.IsNullOrEmpty(name)) {
                return true;
            }
            foreach (char c in name) {
                if (char.IsControl(c)) {
                    return false;
                }
            }
            return true;
        }

        private string _newNameBase;
        //private int _newNameIndex = 1;
        public string NewNameBase {
            get { return _newNameBase; }
            set {
                if (!IsValidName(value)) {
                    throw new ArgumentException(ValidNameDescription);
                }
                _newNameBase = value;
            }
        }

        public virtual string GetNewName(string baseName) {
            if (this[baseName] == null) {
                return baseName;
            }
            int i = baseName.Length - 1;
            for (; 0 <= i && char.IsDigit(baseName[i]); i--) ;
            string basePart = baseName.Substring(0, i + 1);
            string numPart = baseName.Substring(i + 1);
            int n = 0;
            int.TryParse(numPart, out n);
            n++;
            string newName = basePart + n.ToString();
            while (this[newName] != null) {
                n++;
                newName = basePart + n.ToString();
            }
            return newName;
        }

        public virtual string GetNewName() {
            return GetNewName(NewNameBase);
        }

        //public abstract TItem NewItem();
        //public abstract TItem NewItem(string baseName);
        //public abstract TItem NewItem(string baseName, ref int index);

        //public virtual TItem NewItem() {
        //    return NewItem(NewNameBase, ref _newNameIndex);
        //}

        //public virtual TItem NewItem(string baseName) {
        //    if (_nameToItems.ContainsKey(ToDictionaryKey(baseName))) {
        //        int i = 2;
        //        return NewItem(baseName, ref i);
        //    }
        //    return new TItem() { Name = baseName };
        //}

        //public virtual TItem NewItem(string baseName, ref int index) {
        //    int i = index;
        //    string s1 = Path.GetFileNameWithoutExtension(baseName);
        //    string s2 = Path.GetExtension(baseName);
        //    string s;
        //    do {
        //        s = ToDictionaryKey(s1 + i.ToString() + s2);
        //        i++;
        //    } while (_nameToItems.ContainsKey(s));
        //    return new TItem() { Name = s };
        //}

        private void UnlinkItemEvent(TItem item) {
            item.NameChanging -= ItemNameChanging;
            item.NameChanged -= ItemNameChanged;
            item.Disposed -= ItemDisposed;
        }

        #region Itemのイベント処理
        private void ItemNameChanging(object sender, PropertyChangingEventArgs<string> e) {
            if (e.Status == PropertyChangingStatus.Rejected) {
                return;
            }
            if (string.IsNullOrEmpty(e.NewValue)) {
                return;
            }
            if (!IsValidName(e.NewValue)) {
                e.RejectChanges(ValidNameDescription);
                return;
            }
            string s = ToDictionaryKey(e.NewValue);
            if (!string.IsNullOrEmpty(s) && _nameToItems.ContainsKey(s)) {
                e.RejectChanges(string.Format("名前が重複しています: {0}", e.NewValue));
            }
        }

        private void ItemNameChanged(object sender, PropertyChangedEventArgs<string> e) {
            string vOld = ToDictionaryKey(e.OldValue);
            if (!string.IsNullOrEmpty(vOld)) {
                _nameToItems.Remove(vOld);
            }
            string vNew = ToDictionaryKey(e.NewValue);
            if (!string.IsNullOrEmpty(vNew)) {
                _nameToItems.Add(vNew, sender as TItem);
            }
        }

        private void ItemDisposed(object sender, DisposeEventArgs e) {
            Remove(sender as TItem);
        }
        #endregion

        #region ICollection の実装
        public void Add(TItem item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            if (!IsValidName(item.Name)) {
                throw new ArgumentException(ValidNameDescription);
            }
            string s = ToDictionaryKey(item.Name);
            if (!string.IsNullOrEmpty(s) && _nameToItems.ContainsKey(s)) {
                throw new ArgumentException(string.Format("名前が重複しています: {0}", item.Name));
            }
            _items.Add(item);
            if (!string.IsNullOrEmpty(s)) {
                _nameToItems.Add(s, item);
            }
            item.NameChanging += ItemNameChanging;
            item.NameChanged += ItemNameChanged;
            item.Disposed += ItemDisposed;
        }

        public void Clear() {
            foreach (TItem item in _items) {
                UnlinkItemEvent(item);
            }
            _items.Clear();
            _nameToItems.Clear();
        }

        public bool Contains(TItem item) {
            return _items.Contains(item);
        }

        public void CopyTo(TItem[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _items.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(TItem item) {
            bool ret = _items.Remove(item);
            if (ret) {
                string key = ToDictionaryKey(item.Name);
                if (!string.IsNullOrEmpty(key)) {
                    _nameToItems.Remove(key);
                }
                UnlinkItemEvent(item);
            }
            return ret;
        }

        public IEnumerator<TItem> GetEnumerator() {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
        #endregion

        public void Insert(int index, TItem item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            if (!IsValidName(item.Name)) {
                throw new ArgumentException(ValidNameDescription);
            }
            string s = ToDictionaryKey(item.Name);
            if (!string.IsNullOrEmpty(s) && _nameToItems.ContainsKey(s)) {
                throw new ArgumentException(string.Format("名前が重複しています: {0}", item.Name));
            }
            _items.Insert(index, item);
            _nameToItems.Add(s, item);
            item.NameChanging += ItemNameChanging;
            item.NameChanged += ItemNameChanged;
            item.Disposed += ItemDisposed;
            OnItemAdded(new ItemEventArgs(item));
        }

        public class ItemEventArgs: EventArgs {
            private TItem _item;

            public ItemEventArgs(TItem item):base() {
                _item = item;
            }

            public TItem Item { get { return _item; } }
        }

        public event EventHandler<ItemEventArgs> ItemAdded;
        public event EventHandler<ItemEventArgs> ItemRemoved;
        protected virtual void OnItemAdded(ItemEventArgs e) {
            if (ItemAdded != null) {
                ItemAdded(this, e);
            }
        }

        protected virtual void OnItemRemoved(ItemEventArgs e) {
            if (ItemRemoved != null) {
                ItemRemoved(this, e);
            }
        }
    }

    [Serializable]
    public class ShapeCollection: ICollection<Shape> {
        private IShapeParentable _parent;
        private List<Shape> _items = new List<Shape>();
        //private Dictionary<string, Shape> _nameToShape = null;

        internal ShapeCollection(IShapeParentable parent) {
            _parent = parent;
        }

        public Shape this[int index] { get { return _items[index]; } }

        //public virtual Shape NewItem() {
        //}

        private void UnlinkItemEvent(Shape item) {
            item.Disposed -= ItemDisposed;
        }

        private void ItemDisposed(object sender, DisposeEventArgs e) {
            Remove(sender as Shape);
        }

        #region ICollection の実装
        public void Add(Shape item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            _items.Add(item);
            item.Disposed += ItemDisposed;
        }

        public void Clear() {
            foreach (Shape item in _items) {
                UnlinkItemEvent(item);
            }
            _items.Clear();
        }

        public bool Contains(Shape item) {
            return _items.Contains(item);
        }

        public void CopyTo(Shape[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _items.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(Shape item) {
            bool ret = _items.Remove(item);
            if (ret) {
                UnlinkItemEvent(item);
            }
            return ret;
        }

        public IEnumerator<Shape> GetEnumerator() {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
        #endregion

        public void Insert(int index, Shape item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            _items.Insert(index, item);
            item.Disposed += ItemDisposed;
        }
        public void BringToTop(Shape shape) {
            if (_items.Remove(shape)) {
                _items.Add(shape);
            }
        }
        public void BringToFront(Shape shape) {
            int i = _items.IndexOf(shape);
            if (0 <= i && i < _items.Count - 1) {
                _items.RemoveAt(i);
                _items.Insert(i + 1, shape);
            }
        }
        public void BringToFront(Shape shape, Shape target) {
            if (shape == target) {
                return;
            }
            int i = _items.IndexOf(target);
            if (i != -1) {
                if (_items.Remove(shape)) {
                    i = _items.IndexOf(target);
                    _items.Insert(i + 1, shape);
                }
            }
        }
        public void SendToBottom(Shape shape) {
            if (_items.Remove(shape)) {
                _items.Insert(0, shape);
            }
        }
        public void SendToBack(Shape shape) {
            int i = _items.IndexOf(shape);
            if (0 < i) {
                _items.RemoveAt(i);
                _items.Insert(i - 1, shape);
            }
        }
        public void SendToBack(Shape shape, Shape target) {
            if (shape == target) {
                return;
            }
            int i = _items.IndexOf(target);
            if (i != -1) {
                if (_items.Remove(shape)) {
                    i = _items.IndexOf(target);
                    _items.Insert(i, shape);
                }
            }
        }
    }
}
