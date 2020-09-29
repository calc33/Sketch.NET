using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Sketch {
    public class FormulaIndexAttribute: Attribute {
        private bool _isExtra;
        private int _property;
        public FormulaIndexAttribute(int property) {
            _isExtra = false;
            _property = property;
        }
        public FormulaIndexAttribute(bool isExtra, int property) {
            _isExtra = isExtra;
            _property = property;
        }
        public bool IsExtra { get { return _isExtra; } }
        public int Property { get { return _property; } }
    }
    public class LockingIndexAttribute: Attribute {
        private bool _isExtra;
        private int _property;
        public LockingIndexAttribute(int property) {
            _isExtra = false;
            _property = property;
        }
        public LockingIndexAttribute(bool isExtra, int property) {
            _isExtra = isExtra;
            _property = property;
        }
        public bool IsExtra { get { return _isExtra; } }
        public int Property { get { return _property; } }
    }
    public class EditableIndexAttribute: Attribute {
        private int _section;
        private int _row;
        private int _property;
        public EditableIndexAttribute(int section, int row, int property) {
            _section = section;
            _row = row;
            _property = property;
        }
        public int Section { get { return _section; } }
        public int Row { get { return _row; } }
        public int Property { get { return _property; } }
    }

    [Serializable]
    public class PropertyChangingException: Exception {
        public PropertyChangingException() : base() { }
        public PropertyChangingException(string message) : base(message) { }
    }

    public enum PropertyChangingStatus {
        /// <summary>
        /// 変更を許可する。プロパティ値の変更はない
        /// </summary>
        Allow,
        /// <summary>
        /// 変更を許可する。ただしイベント中でプロパティ値を変更している
        /// </summary>
        Modified,
        /// <summary>
        /// 変更を許可しない。
        /// ErrorMessageプロパティに理由が書かれている。
        /// </summary>
        Rejected
    }

    public class PropertyChangingEventArgs<T>: EventArgs {
        private PropertyChangingStatus _status;
        private string _errorMessage;
        private T _newValue;
        private T _oldValue;
        public PropertyChangingEventArgs(T newValue, T oldValue)
            : base() {
            _status = PropertyChangingStatus.Allow;
            _newValue = newValue;
            _oldValue = oldValue;
        }

        /// <summary>
        /// 変更を許可する場合はtrue,拒否したい場合はfalseに設定する
        /// 初期値はtrue。
        /// falseに変更したい場合は RejectChanged()を呼び出す
        /// </summary>
        public PropertyChangingStatus Status {
            get { return _status; }
            //set { _allowChanging = value; }
        }

        private void BecomeStatus(PropertyChangingStatus newStatus) {
            if (_status < newStatus) {
                _status = newStatus;
            }
        }

        /// <summary>
        /// 変更後の値
        /// </summary>
        public T NewValue {
            get { return _newValue; }
            set {
                if (!object.Equals(_newValue, value)) {
                    _newValue = value;
                    BecomeStatus(PropertyChangingStatus.Modified);
                }
            }
        }

        /// <summary>
        /// 変更前の値
        /// </summary>
        public T OldValue { get { return _oldValue; } }

        /// <summary>
        /// 変更を拒否し、拒否した理由を引数で渡す。
        /// </summary>
        /// <param name="errorMsg"></param>
        public void RejectChanges(string errorMsg) {
            _errorMessage = errorMsg;
            BecomeStatus(PropertyChangingStatus.Rejected);
        }

        /// <summary>
        /// AllowChanging=falseの場合に使用する
        /// 変更を拒否した理由
        /// </summary>
        public string ErrorMessage { get { return _errorMessage; } }

        /// <summary>
        /// このPropertyChangingイベントに対応したPropertyChangedイベントへ渡す
        /// PropertyChangedEventArgsを生成する
        /// </summary>
        /// <returns></returns>
        internal PropertyChangedEventArgs<T> CreatePropertyChangedEventArgs() {
            return new PropertyChangedEventArgs<T>(_newValue, _oldValue);
        }
    }

    public class PropertyChangedEventArgs<T>: EventArgs {
        private T _newValue;
        private T _oldValue;
        public PropertyChangedEventArgs(T newValue, T oldValue)
            : base() {
            _newValue = newValue;
            _oldValue = oldValue;
        }

        public T NewValue { get { return _newValue; } }
        public T OldValue { get { return _oldValue; } }
    }

    public delegate void OnPropertyChanging<T>(PropertyChangingEventArgs<T> e);
    public delegate void OnPropertyChanged<T>(PropertyChangedEventArgs<T> e);
    public class PropertyValueSetter<T> {
        public static void Execute(ref T field, T value, OnPropertyChanging<T> changing, OnPropertyChanged<T> changed) {
            PropertyChangingEventArgs<T> e = new PropertyChangingEventArgs<T>(value, field);
            changing(e);
            if (e.Status == PropertyChangingStatus.Rejected) {
                if (e.ErrorMessage == null) {
                    return;
                }
                throw new PropertyChangingException(e.ErrorMessage);
            }
            field = e.NewValue;
            changed(e.CreatePropertyChangedEventArgs());
        }
    }

    public class PropertyDef {
        private int _index;
        private string _name;
        private Type _requiredType;
        private string _defaultFormula;
        private bool _canLock;
        private int _lockProperty;

        public PropertyDef(string name, Type requiredType, string defaultFormula) {
            _index = -1;
            _name = name;
            _requiredType = requiredType;
            _defaultFormula = defaultFormula;
            _canLock = false;
            _lockProperty = -1;
        }

        public PropertyDef(string name, Type requiredType, string defaultFormula, int lockProperty) {
            _index = -1;
            _name = name;
            _requiredType = requiredType;
            _defaultFormula = defaultFormula;
            _canLock = true;
            _lockProperty = lockProperty;
        }

        public string Name { get { return _name; } }
        public Type RequiredType { get { return _requiredType; } }
        public string DefaultFormula { get { return _defaultFormula; } }
        public bool CanLock { get { return _canLock; } }
        public int LockProperty { get { return _lockProperty; } }
        public int Index { get { return _index; } set { _index = value; } }
    }
    public class PropertyDefCollection {
        private PropertyDef[] _items;
        private Dictionary<string, PropertyDef> _nameToPropertyDef;
        public PropertyDefCollection(PropertyDef[] items) {
            if (items == null) {
                throw new ArgumentNullException("items");
            }
            _items = items;
            for (int i = 0; i < _items.Length; i++) {
                _items[i].Index = i;
            }
            _nameToPropertyDef = new Dictionary<string, PropertyDef>(_items.Length);
            foreach (PropertyDef def in _items) {
                _nameToPropertyDef[def.Name.ToUpper()] = def;
            }
        }
        public PropertyDef this[int index] { get { return _items[index]; } }
        public PropertyDef this[string name] {
            get {
                if (string.IsNullOrEmpty(name)) {
                    throw new ArgumentNullException("name");
                }
                PropertyDef ret;
                if (!_nameToPropertyDef.TryGetValue(name.ToUpper(), out ret)) {
                    throw new ArgumentException("name");
                }
                return ret;
            }
        }
        public int IndexOf(string name) {
            if (string.IsNullOrEmpty(name)) {
                return -1;
            }
            PropertyDef ret;
            if (!_nameToPropertyDef.TryGetValue(name.ToUpper(), out ret)) {
                return -1;
            }
            return ret.Index;
        }
        public int Count { get { return _items.Length; } }
    }
    public interface IExprComponentOwnable {
        void Add(object item);
        void Remove(object item);
        string GetNewName(object target);
    }

    [Serializable]
    public abstract class ExprComponent: IExprObject, IDisposable, ISerializable {
        private IExprComponentOwnable _container;
        private string _name;
        private BuiltinFormulaCollection _formula;
        private ExtraFormulaCollection _extra;
        public abstract PropertyDefCollection GetBuiltinPropertyDefs();

        public IExprComponentOwnable Container { get { return _container; } }

        public FormulaPropertyCollection Formula { get { return _formula; } }
        public FormulaPropertyCollection Extra { get { return _extra; } }

        #region FormulaChanging/FormulaChangedイベント
        public event EventHandler<FormulaChangingEventArgs> FormulaChanging;
        public event EventHandler<FormulaChangedEventArgs> FormulaChanged;
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public virtual void OnFormulaChanging(FormulaChangingEventArgs e) {
            if (FormulaChanging != null) {
                FormulaChanging(this, e);
            }
        }

        public virtual void OnFormulaChanged(FormulaChangedEventArgs e) {
            if (FormulaChanged != null) {
                FormulaChanged(this, e);
            }
        }
        public virtual void OnValueChanged(ValueChangedEventArgs e) {
            if (ValueChanged != null) {
                ValueChanged(this, e);
            }
        }
        #endregion

        protected void SetContainer(IExprComponentOwnable container) {
            _container = container;
            if (_container != null) {
                _container.Add(this);
            }
        }

        protected ExprComponent() {
            _formula = new BuiltinFormulaCollection(this);
            _extra = new ExtraFormulaCollection(this);
        }

        public ExprComponent(IExprComponentOwnable container) {
            _formula = new BuiltinFormulaCollection(this);
            _extra = new ExtraFormulaCollection(this);
            SetContainer(container);
        }

        ~ExprComponent() {
            Dispose(false);
        }

        public virtual object Clone(IExprComponentOwnable container) {
            ExprComponent ret = (ExprComponent)MemberwiseClone();
            ret._container = container;
            ret._formula = new BuiltinFormulaCollection(this);
            ret._extra = new ExtraFormulaCollection(this);
            container.Add(ret);
            for (int i = 0; i < Formula.Count - 1; i++) {
                ret.Formula[i].SetFormula(Formula[i].Formula);
                ret.Formula[i].LockLevelFormula = Formula[i].LockLevelFormula;
            }
            foreach (FormulaProperty prop in Extra) {
                ret.Extra.AddProperty(ret, prop.Name, prop.Formula);
            }
            return ret;
        }

        public event EventHandler<PropertyChangingEventArgs<string>> NameChanging;
        public event EventHandler<PropertyChangedEventArgs<string>> NameChanged;

        protected virtual void OnNameChanging(PropertyChangingEventArgs<string> e) {
            if (NameChanging != null) {
                NameChanging(this, e);
            }
        }

        protected virtual void OnNameChanged(PropertyChangedEventArgs<string> e) {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
        }

        public string Name {
            get { return _name; }
            set {
                PropertyValueSetter<string>.Execute(ref _name, value, OnNameChanging, OnNameChanged);
            }
        }

        public event EventHandler<DisposeEventArgs> Disposed;

        protected virtual void OnDispose(DisposeEventArgs e) {
            if (Disposed != null) {
                Disposed(this, e);
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                OnDispose(new DisposeEventArgs(disposing));
            }
        }

        public void Dispose() {
            Dispose(true);
        }

        public object Eval(string formula) {
            TemporaryFormula f = new TemporaryFormula(this, formula);
            return f.Value;
        }

        #region ISerializableの実装
        protected ExprComponent(SerializationInfo info, StreamingContext context)
            : base() {

        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue()
        }
        #endregion
    }

    public abstract class ExprComponent<TContainer>: ExprComponent where TContainer: class, IExprComponentOwnable {
        public new TContainer Container { get { return base.Container as TContainer; } }
        public virtual object Clone(TContainer container) {
            return Clone(container as IExprComponentOwnable);
        }
        public ExprComponent(TContainer container) : base(container) { }
        protected ExprComponent() : base() { }
    }
}