using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace Sketch {
    /// <summary>
    /// プロパティの編集制御のレベルを定義
    /// 編集制御は
    /// ・GUIからの編集
    /// ・プログラム上でValueプロパティによる変更
    /// ・プログラム上でFormulaプロパティによる変更
    /// の三段階がある
    /// </summary>
    public enum LockLevel {
        /// <summary>
        /// 編集制御しない
        /// </summary>
        Enabled = 3,
        /// <summary>
        /// GUIからの変更不可
        /// (Valueプロパティ、Formulaプロパティによる変更は許可)
        /// </summary>
        KnobDisabled = 2,
        /// <summary>
        /// Valueプロパティの変更不可
        /// (Formulaプロパティによる変更は可)
        /// </summary>
        ValueDisabled = 1,
        /// <summary>
        /// 一切の変更不可
        /// </summary>
        Disabled = 0,
    }
    public enum EditingLevel {
        /// <summary>
        /// Formulaの変更
        /// </summary>
        EditByFormula = 0,
        /// <summary>
        /// プログラムによる編集
        /// </summary>
        EditByValue = 1,
        /// <summary>
        /// GUI(主にKnob)からの編集
        /// </summary>
        EditByKnob = 2,
    }
    public class FormulaProperty: IExpressionOwnable {
        private WeakReference _owner;
        private string _name;
        //private IExprObject _owner;
        private Expression _formula;
        private Expression _lockLevelFormula;
        //private Expression _editableFormula;

        public event EventHandler<FormulaChangingEventArgs> FormulaChanging;
        public event EventHandler<FormulaChangedEventArgs> FormulaChanged;
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected internal virtual void OnFormulaChanging(FormulaChangingEventArgs e) {
            if (FormulaChanging != null) {
                FormulaChanging(this, e);
            }
            Owner.OnFormulaChanging(e);
        }

        protected internal virtual void OnFormulaChanged(FormulaChangedEventArgs e) {
            if (FormulaChanged != null) {
                FormulaChanged(this, e);
            }
            Owner.OnFormulaChanged(e);
        }

        protected internal virtual void OnValueChanged(ValueChangedEventArgs e) {
            if (ValueChanged != null) {
                ValueChanged(this, e);
            }
            Owner.OnValueChanged(e);
        }

        private void DependenyValueChanged(object sender, EventArgs e) {
            OnValueChanged(new ValueChangedEventArgs(Owner, this));
        }

        public FormulaProperty(IExprObject owner, string name, string formula) {
            _owner = new WeakReference(owner);
            _name = name;
            _formula = new Expression(this, formula);
            _formula.PropertyChanged += DependenyValueChanged;
            _lockLevelFormula = new Expression(this, "3");
        }

        public IExprObject Owner {
            get {
                if (!_owner.IsAlive) {
                    return null;
                }
                return _owner.Target as IExprObject;
            }
        }

        public string Name { get { return _name; } }

        public string Formula {
            get { return _formula.Formula; }
            //set {
            //    _formula.Formula = value;
            //}
        }
        protected internal void SetFormula(string value) {
            // FormulaChangingEvent/FormulaChangedEventの実装が必要
            FormulaChangingEventArgs e = new FormulaChangingEventArgs(Owner, this, value);
            OnFormulaChanging(e);
            if (e.Status == PropertyChangingStatus.Rejected) {
                throw new ApplicationException(e.ErrorMessage);
            }
            _formula.Formula = e.NewFormula;
            OnFormulaChanged(new FormulaChangedEventArgs(e));
        }

        public string LockLevelFormula {
            get { return _lockLevelFormula.Formula; }
            set { _lockLevelFormula.Formula = value; }
        }

        public LockLevel LockLevel {
            get {
                object v = _lockLevelFormula.Value;
                return (v != null) ? (LockLevel)Convert.ToInt32(v) : LockLevel.Enabled;
            }
            set {
                if (LockLevel != value) {
                    _lockLevelFormula.Formula = value.ToString();
                }
            }
        }

        /// <summary>
        /// GUI(主にKnob)による値の変更を許可する場合はtrueを返す
        /// </summary>
        /// <returns></returns>
        public bool CanEditByKnob() {
            return LockLevel == LockLevel.Enabled;
        }

        public bool CanModifyValue() {
            return (LockLevel.ValueDisabled < LockLevel);
        }

        public bool CanModify(EditingLevel level) {
            return (int)level < (int)LockLevel;
        }

        public object Value {
            get { return _formula.Value; }
            //set {
            //    if (!object.Equals(_formula.Eval(), value)) {
            //        _formula.Formula = '=' + value.ToString();
            //    }
            //}
        }
        protected internal void SetValue(object value, EditingLevel editingLevel) {
            if (!object.Equals(_formula.Value, value) && CanModify(editingLevel)) {
                _formula.Value = value;
            }
        }

        public bool BooleanValue {
            get { return Convert.ToBoolean(Value); }
        }

        public Byte ByteValue {
            get { return Convert.ToByte(Value); }
        }
        public SByte SByteValue {
            get { return Convert.ToSByte(Value); }
        }
        public Int16 Int16Value {
            get { return Convert.ToInt16(Value); }
        }
        public UInt16 UInt16Value {
            get { return Convert.ToUInt16(Value); }
        }
        public Int32 Int32Value {
            get { return Convert.ToInt32(Value); }
        }
        public UInt32 UInt32Value {
            get { return Convert.ToUInt32(Value); }
        }
        public Int64 Int64Value {
            get { return Convert.ToInt64(Value); }
        }
        public UInt64 UInt64Value {
            get { return Convert.ToUInt64(Value); }
        }
        public Single SingleValue {
            get { return Convert.ToSingle(Value); }
        }
        public Double DoubleValue {
            get { return Convert.ToDouble(Value); }
        }
        public Decimal DecimalValue {
            get { return Convert.ToDecimal(Value); }
        }
        public Distance DistanceValue {
            get { return UnitConvert.ToDistance(Value); }
        }
        public Angle AngleValue {
            get { return UnitConvert.ToAngle(Value); }
        }
        public String StringValue {
            get { return Value.ToString(); }
        }

        public static Color ToColorValue(object v) {
            if (v is string) {
                return (Color)(ColorConverter.ConvertFromString((string)v));
            } else if (v is Color) {
                return (Color)v;
            }
            throw new InvalidCastException();
        }

        public Color ColorValue {
            get {
                return ToColorValue(Value);
            }
        }

        public void ReplaceComponents(Dictionary<ExprComponent<Document>, ExprComponent<Document>> shapeMapping) {
            //_formula.ReplaceComponents
        }

        //private static EvalSpec GetEvalSpecFromAttribute(MemberInfo member) {
        //    foreach (EvalSpecAttribute a in member.GetCustomAttributes(typeof(EvalSpecAttribute), false)) {
        //        return a.Spec;
        //    }
        //    foreach (EvalSpecAttribute a in member.GetCustomAttributes(typeof(EvalSpecAttribute), true)) {
        //        return a.Spec;
        //    }
        //    if (member is PropertyInfo) {
        //        bool extra;
        //        int prop;
        //        if (GetFormulaIndex(member, out extra, out prop)) {
        //            return EvalSpec.PropertyDependent;
        //        } else {
        //            return EvalSpec.Variable;
        //        }
        //    } else if (member is MethodInfo) {
        //        return EvalSpec.FunctionalDependent;
        //    }
        //    return EvalSpec.Variable;
        //}

        private static bool GetFormulaIndex(MemberInfo member, out bool isExtra, out int property) {
            foreach (FormulaIndexAttribute a in member.GetCustomAttributes(typeof(FormulaIndexAttribute), false)) {
                isExtra = a.IsExtra;
                property = a.Property;
                return true;
            }
            foreach (FormulaIndexAttribute a in member.GetCustomAttributes(typeof(FormulaIndexAttribute), true)) {
                isExtra = a.IsExtra;
                property = a.Property;
                return true;
            }
            isExtra = false;
            property = -1;
            return false;
        }
        private static bool GetLockingIndex(MemberInfo member, out bool isExtra, out int property) {
            foreach (LockingIndexAttribute a in member.GetCustomAttributes(typeof(LockingIndexAttribute), false)) {
                isExtra = a.IsExtra;
                property = a.Property;
                return true;
            }
            foreach (LockingIndexAttribute a in member.GetCustomAttributes(typeof(LockingIndexAttribute), true)) {
                isExtra = a.IsExtra;
                property = a.Property;
                return true;
            }
            isExtra = false;
            property = -1;
            return false;
        }
        private static bool GetEditableIndex(MemberInfo member, out int section, out int row, out int property) {
            foreach (EditableIndexAttribute a in member.GetCustomAttributes(typeof(EditableIndexAttribute), false)) {
                section = a.Section;
                row = a.Row;
                property = a.Property;
                return true;
            }
            foreach (EditableIndexAttribute a in member.GetCustomAttributes(typeof(EditableIndexAttribute), true)) {
                section = a.Section;
                row = a.Row;
                property = a.Property;
                return true;
            }
            section = -1;
            row = -1;
            property = -1;
            return false;
        }

        private void DependentFormulaChanged(object sender, ValueChangedEventArgs e) {
            if (_formula != null) {
                _formula.Invalidate();
            }
        }

        private List<FormulaProperty> _dependencies = new List<FormulaProperty>();

        public void AddDependency(DependencyAddedEevntArgs e) {
            EvalSpec spec = Expression.GetEvalSpecFromAttribute(e.Member);
            //if (spec == EvalSpec.FunctionalDependent || spec == EvalSpec.PropertyDependent) {
            if (spec == EvalSpec.PropertyDependent) {
                bool x;
                int p;
                if (GetFormulaIndex(e.Member, out x, out p)) {
                    IExprObject c = e.Target as IExprObject;
                    if (c != null) {
                        FormulaProperty prop = c.Formula[x, p];
                        if (prop != null) {
                            prop.ValueChanged += DependentFormulaChanged;
                            _dependencies.Add(prop);
                        }
                    }
                }
            }
        }

        public void ClearDependencies() {
            foreach (FormulaProperty p in _dependencies) {
                if (p != null) {
                    p.ValueChanged -= DependentFormulaChanged;
                }
            }
            _dependencies.Clear();
        }

        public override string ToString() {
            StringBuilder buf = new StringBuilder();
            buf.Append('[');
            try {
                if (Value != null) {
                    buf.Append(Value.ToString());
                } else {
                    buf.Append("(null)");
                }
            } catch (Exception t) {
                buf.Append(t.Message);
            }
            buf.Append(": ");
            if (Formula != null) {
                buf.Append(Formula);
            }
            buf.Append(']');
            return buf.ToString();
        }
    }
    public class FormulaChangingEventArgs: EventArgs {
        private PropertyChangingStatus _status;
        private IExprObject _owner;
        private FormulaProperty _property;
        private string _errorMessage;
        private string _newFormula;
        private string _oldFormula;
        public FormulaChangingEventArgs(IExprObject owner, FormulaProperty property, string newFormula)
            : base() {
            _owner = owner;
            _status = PropertyChangingStatus.Allow;
            _property = property;
            _newFormula = newFormula;
            _oldFormula = (_property != null) ? _property.Formula : null;
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

        public IExprObject Owner { get { return _owner; } }

        public FormulaProperty Property { get { return _property; } }

        /// <summary>
        /// 変更後の式
        /// </summary>
        public string NewFormula {
            get { return _newFormula; }
            set {
                if (_newFormula != value) {
                    _newFormula = value;
                    BecomeStatus(PropertyChangingStatus.Modified);
                }
            }
        }

        /// <summary>
        /// 変更前の式
        /// </summary>
        public string OldFormula { get { return _oldFormula; } }

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
        internal FormulaChangedEventArgs CreateFormulaChangedEventArgs() {
            return new FormulaChangedEventArgs(this);
        }
        internal ValueChangedEventArgs CreateDependencyChangedEventArgs() {
            return new ValueChangedEventArgs(this);
        }
    }

    public abstract class FormulaPropertyCollection: IEnumerable<FormulaProperty> {
        protected IExprObject _owner;
        protected IList<FormulaProperty> _items;
        protected internal FormulaPropertyCollection(IExprObject owner) {
            _owner = owner;
        }
        public FormulaProperty this[bool isExtra, int index] {
            get {
                if (_owner == null) {
                    return null;
                }
                return isExtra ? _owner.Extra[index] : _owner.Formula[index];
            }
        }
        public FormulaProperty this[int index] { get { return _items[index]; } }
        public FormulaProperty this[string name] { get { return GetPropertyByName(name); } }
        public int Count { get { return _items.Count; } }

        protected internal IList<FormulaProperty> Properties { get { return _items; } }
        protected internal abstract FormulaProperty GetPropertyByName(string name);
        protected internal abstract int AddProperty(IExprObject owner, string name, string formula);
        protected internal abstract void RemoveAt(int index);
        protected internal abstract void RemoveByName(string name);

        public IEnumerator<FormulaProperty> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
    }

    public class BuiltinFormulaCollection: FormulaPropertyCollection {
        protected internal BuiltinFormulaCollection(IExprObject owner)
            : base(owner) {
            PropertyDefCollection defs = _owner.GetBuiltinPropertyDefs();
            _items = new FormulaProperty[defs.Count];
            for (int i = 0; i < defs.Count; i++) {
                _items[i] = new FormulaProperty(owner, defs[i].Name, defs[i].DefaultFormula);
            }
        }

        protected internal override FormulaProperty GetPropertyByName(string name) {
            PropertyDefCollection defs = _owner.GetBuiltinPropertyDefs();
            int i = defs.IndexOf(name);
            return (0 <= i) ? _items[i] : null;
        }

        protected internal override int AddProperty(IExprObject owner, string name, string formula) {
            throw new NotImplementedException();
        }

        protected internal override void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        protected internal override void RemoveByName(string name) {
            throw new NotImplementedException();
        }
    }

    public class ExtraFormulaCollection: FormulaPropertyCollection {
        Dictionary<string, FormulaProperty> _nameToProperty;
        protected internal ExtraFormulaCollection(IExprObject owner)
            : base(owner) {
            _items = new List<FormulaProperty>();
            _nameToProperty = new Dictionary<string, FormulaProperty>();
        }

        //public void Add(FormulaProperty item) {
        //    _items.Add(item);
        //}

        //public void Clear() {
        //    _items.Clear();
        //}

        //public bool Contains(FormulaProperty item) {
        //    return _items.Contains(item);
        //}

        //public void CopyTo(FormulaProperty[] array, int arrayIndex) {
        //    _items.CopyTo(array, arrayIndex);
        //}

        //public bool IsReadOnly {
        //    get { return _items.IsReadOnly; }
        //}

        //public bool Remove(FormulaProperty item) {
        //    return _items.Remove(item);
        //}

        protected internal override FormulaProperty GetPropertyByName(string name) {
            if (!string.IsNullOrEmpty(name)) {
                return null;
            }
            FormulaProperty ret;
            if (!_nameToProperty.TryGetValue(name.ToUpper(), out ret)) {
                return null;
            }
            return ret;
        }

        protected internal override int AddProperty(IExprObject owner, string name, string formula) {
            FormulaProperty p = new FormulaProperty(owner, name, formula);
            _nameToProperty.Add(name.ToUpper(), p);
            _items.Add(p);
            return _items.Count - 1;
        }

        protected internal override void RemoveAt(int index) {
            
        }

        protected internal override void RemoveByName(string name) {
            throw new NotImplementedException();
        }
    }

    public class FormulaChangedEventArgs: EventArgs {
        private IExprObject _owner;
        private FormulaProperty _property;
        //private bool _isExtra;
        //private int _property;
        private string _newFormula;
        private string _oldFormula;
        internal FormulaChangedEventArgs(FormulaChangingEventArgs e)
            : base() {
            _owner = e.Owner;
            _property = e.Property;
            _newFormula = e.NewFormula;
            _oldFormula = e.OldFormula;
        }

        public IExprObject Owner { get { return _owner; } }
        public FormulaProperty Property { get { return _property; } }

        public string NewFormula { get { return _newFormula; } }
        public string OldFormula { get { return _oldFormula; } }
    }

    public class ValueChangedEventArgs: EventArgs {
        private IExprObject _owner;
        private FormulaProperty _property;
        internal ValueChangedEventArgs(FormulaChangingEventArgs e)
            : base() {
            _owner = e.Owner;
            _property = e.Property;
        }
        internal ValueChangedEventArgs(IExprObject owner, FormulaProperty property):base() {
            _owner = owner;
            _property = property;
        }

        public IExprObject Owner { get { return _owner; } }
        public FormulaProperty FormulaProperty { get { return _property; } }
    }

    public class PointFormula: ExprObject {
        public const int PROP_X = 0;
        public const int PROP_Y = 1;
        public const int PROP_LOCKX = 2;
        public const int PROP_LOCKY = 3;

        private static PropertyDefCollection _builtinPropertyDefs = new PropertyDefCollection(
            new PropertyDef[] {
                new PropertyDef("X", typeof(Distance), "0px", PROP_LOCKX),
                new PropertyDef("Y", typeof(Distance), "0px", PROP_LOCKY),
                new PropertyDef("LockX", typeof(bool), "false"),
                new PropertyDef("LockY", typeof(bool), "false"),
            }
        );
        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinPropertyDefs;
        }

        private Shape _shape;
        public Shape Shape { get { return _shape; } }

        public FormulaProperty FormulaX { get { return Formula[PROP_X]; } }
        public FormulaProperty FormulaY { get { return Formula[PROP_Y]; } }
        [FormulaIndex(PROP_X)]
        public Distance X {
            get { return Formula[PROP_X].DistanceValue; }
            set { Formula[PROP_X].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_Y)]
        public Distance Y {
            get { return Formula[PROP_Y].DistanceValue; }
            set { Formula[PROP_Y].SetValue(value, EditingLevel.EditByValue); }
        }
        public Point2D Point2D { get { return new Point2D(X, Y); } }
        public Point ToPointPx() {
            return new Point(X.Px, Y.Px);
        }
        public PointFormula(IExprObject owner, string x, string y)
            : base() {
            if (owner is DrawingPath) {
                _shape = ((DrawingPath)owner).Shape;
            } else if (owner is Shape) {
                _shape = (Shape)owner;
            }
            Formula[PROP_X].SetFormula(x);
            Formula[PROP_Y].SetFormula(y);
        }
    }

    public class PointFormulaCollection: ICollection<PointFormula> {
        private List<PointFormula> _items;
        protected internal PointFormulaCollection() {
            _items = new List<PointFormula>();
        }
        //protected internal PointFormulaCollection(ExprComponent owner) {
        //    _items = new List<PointFormula>();
        //}

        public PointFormula this[int index] { get { return _items[index]; } }
        public int Count { get { return _items.Count; } }

        public void RemoveAt(int index) {
            _items.RemoveAt(index);
        }

        public void Add(PointFormula item) {
            _items.Add(item);
        }

        public void Clear() {
            _items.Clear();
        }

        public bool Contains(PointFormula item) {
            return _items.Contains(item);
        }

        public void CopyTo(PointFormula[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(PointFormula item) {
            return _items.Remove(item);
        }

        public IEnumerator<PointFormula> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
    }
}
