using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace Sketch {
    public interface IExpressionOwnable {
        IExprObject Owner { get; }
        void AddDependency(DependencyAddedEevntArgs e);
        void ClearDependencies();
    }

    public delegate void AddDependencyCallback(DependencyAddedEevntArgs e);
    public partial class Expression: IConvertible {
        private static List<object> _globals = new List<object>();

        private WeakReference _owner;

        private string _formula;
        private bool _isFormulaCompiled;
        private bool _temporary;
        private bool _isValueValid = false;
        private bool _isValueCachable = true;
        private object _value;
        private Node _topNode;
        public event EventHandler PropertyChanged;
        protected void OnPropertyChanged(EventArgs e) {
            if (PropertyChanged != null) {
                PropertyChanged(this, e);
            }
        }

        protected void InvalidateFormula() {
            _isFormulaCompiled = false;
            Invalidate();
        }

        public void Invalidate() {
            _value = null;
            _isValueValid = false;
            OnPropertyChanged(EventArgs.Empty);
        }

        public void Invalidate(object sender, EventArgs e) {
            Invalidate();
        }

        //private TokenTree _topToken;

        private void AddDependency(DependencyAddedEevntArgs e) {
            IExpressionOwnable o = Owner;
            if (o != null) {
                o.AddDependency(e);
            }
        }

        private string GetImmediateEvalFormula(string formula, ref int index, out int start, out int length) {
            if (string.IsNullOrEmpty(formula)) {
                start = 0;
                length = 0;
                return null;
            }
            if (formula.Length <= index) {
                start = formula.Length;
                length = 0;
                return null;
            }
            bool inQuote = false;
            int p0 = -1;
            for (int i = index; i < formula.Length; i++) {
                char c = formula[i];
                switch (c) {
                    case '"':
                        inQuote = !inQuote;
                        break;
                    case '`':
                        if (!inQuote) {
                            if (p0 == -1) {
                                p0 = i;
                            } else {
                                index = i + 1;
                                start = p0;
                                length = i - p0;
                                return formula.Substring(start + 1, length - 1);
                            }
                        }
                        break;
                    default:
                        if (char.IsSurrogate(c)) {
                            i++;
                        }
                        break;
                }
            }
            index = formula.Length;
            start = formula.Length;
            length = 0;
            return null;
        }

        private void Precompile() {
            if (string.IsNullOrEmpty(Formula)) {
                return;
            }
            if (Formula.IndexOf('`') == -1) {
                return;
            }
            int p = 0;
            int l = 0;
            int n = Formula.Length;
            StringBuilder buf = new StringBuilder(n);
            for (int i = 0; i < n; ) {
                int i0 = i;
                string s = GetImmediateEvalFormula(Formula, ref i, out p, out l);
                buf.Append(Formula.Substring(i0, p - i0));
                if (s == null) {
                    break;
                }
                Expression temp = new Expression(Owner, s, true);
                buf.Append(Expression.ToExpression(temp.Value));
            }
            Formula = buf.ToString();
        }

        protected void Compile() {
            Precompile();
            IExpressionOwnable o = Owner;
            if (o != null) {
                o.ClearDependencies();
            }
            Token[] tokens = LexicalAnalyze(Formula);
            TokenTree topToken = Parse(tokens);
            Context ctx = GetCurrentContext();
            AddDependencyCallback proc = AddDependency;
            if (_temporary) {
                proc = null;
            }
            _topNode = null;
            if (topToken != null) {
                Node node = topToken.GetNode();
                node.Eval(ctx, proc);
                _topNode = node.Reduce(ctx);
                _isValueCachable = (node.Style != EvalSpec.Variable);
            }
            _isFormulaCompiled = true;
        }

        private void TryCompile() {
            if (!_isFormulaCompiled) {
                Compile();
            }
        }

        public string ToDebugString() {
            //TryCompile();
            //if (_topToken != null) {
            //    return _topToken.ToString();
            //}
            TryCompile();
            if (_topNode != null) {
                return _topNode.ToDebugString(GetCurrentContext());
            }
            return null;
        }

        public object Eval() {
            TryCompile();
            if (_topNode == null) {
                return null;
            }
            object oldValue = _value;
            Context ctx = GetCurrentContext();
            _value = _topNode.Eval(ctx, AddDependency);
            if (_value != oldValue) {
                OnPropertyChanged(EventArgs.Empty);
            }
            return _value;
        }

        public Expression(IExpressionOwnable owner) {
            _owner = new WeakReference(owner);
            _temporary = false;
        }

        public Expression(IExpressionOwnable owner, string formula) {
            _owner = new WeakReference(owner);
            Formula = formula;
            _temporary = false;
        }

        protected internal Expression(IExpressionOwnable owner, string formula, bool temporary) {
            _owner = new WeakReference(owner);
            Formula = formula;
            _temporary = temporary;
        }

        public string Formula {
            get {
                return _formula;
            }
            set {
                if (_formula != value) {
                    _formula = value;
                    InvalidateFormula();
                }
            }
        }

        public object Value {
            get {
                if (!_isValueCachable) {
                    Eval();
                } else if (!_isValueValid) {
                    Eval();
                    _isValueValid = true;
                }
                return _value;
            }
            set {
                DataSourceNode ds = _topNode as DataSourceNode;
                if (ds != null) {
                    Context ctx = GetCurrentContext();
                    ds.SetValue(value, ctx);
                } else {
                    Formula = ToExpression(value);
                }
            }
        }

        public IExpressionOwnable Owner {
            get {
                if (!_owner.IsAlive) {
                    return null;
                }
                return _owner.Target as IExpressionOwnable;
            }
        }

        public bool BooleanValue {
            get { return Convert.ToBoolean(Value); }
            set { Value = value; }
        }

        public Byte ByteValue {
            get { return Convert.ToByte(Value); }
            set { Value = value; }
        }
        public SByte SByteValue {
            get { return Convert.ToSByte(Value); }
            set { Value = value; }
        }
        public Int16 Int16Value {
            get { return Convert.ToInt16(Value); }
            set { Value = value; }
        }
        public UInt16 UInt16Value {
            get { return Convert.ToUInt16(Value); }
            set { Value = value; }
        }
        public Int32 Int32Value {
            get { return Convert.ToInt32(Value); }
            set { Value = value; }
        }
        public UInt32 UInt32Value {
            get { return Convert.ToUInt32(Value); }
            set { Value = value; }
        }
        public Int64 Int64Value {
            get { return Convert.ToInt64(Value); }
            set { Value = value; }
        }
        public UInt64 UInt64Value {
            get { return Convert.ToUInt64(Value); }
            set { Value = value; }
        }
        public Single SingleValue {
            get { return Convert.ToSingle(Value); }
            set { Value = value; }
        }
        public Double DoubleValue {
            get { return Convert.ToDouble(Value); }
            set { Value = value; }
        }
        public Decimal DecimalValue {
            get { return Convert.ToDecimal(Value); }
            set { Value = value; }
        }
        public Distance DistanceValue {
            get { return UnitConvert.ToDistance(Value); }
            set { Value = value; }
        }
        public Angle AngleValue {
            get { return UnitConvert.ToAngle(Value); }
            set { Value = value; }
        }
        public String StringValue {
            get { return Value.ToString(); }
            set { Value = value; }
        }

        public static Color ToColorValue(object v) {
            if (v is string) {
                return (Color)(ColorConverter.ConvertFromString((string)v));
            } else if (v is Color) {
                return (Color)v;
            }
            throw new InvalidCastException();
        }

        protected Color ColorValue {
            get {
                return ToColorValue(Value);
            }
        }

        #region IConvertibleの実装
        public TypeCode GetTypeCode() {
            IConvertible v = Value as IConvertible;
            if (v != null) {
                return v.GetTypeCode();
            }
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider) {
            return Convert.ToBoolean(Value, provider);
        }

        public byte ToByte(IFormatProvider provider) {
            return Convert.ToByte(Value, provider);
        }

        public char ToChar(IFormatProvider provider) {
            return Convert.ToChar(Value, provider);
        }

        public DateTime ToDateTime(IFormatProvider provider) {
            return Convert.ToDateTime(Value, provider);
        }

        public decimal ToDecimal(IFormatProvider provider) {
            return Convert.ToDecimal(Value, provider);
        }

        public double ToDouble(IFormatProvider provider) {
            return Convert.ToDouble(Value, provider);
        }

        public short ToInt16(IFormatProvider provider) {
            return Convert.ToInt16(Value, provider);
        }

        public int ToInt32(IFormatProvider provider) {
            return Convert.ToInt32(Value, provider);
        }

        public long ToInt64(IFormatProvider provider) {
            return Convert.ToInt64(Value, provider);
        }

        public sbyte ToSByte(IFormatProvider provider) {
            return Convert.ToSByte(Value, provider);
        }

        public float ToSingle(IFormatProvider provider) {
            return Convert.ToSingle(Value, provider);
        }

        public string ToString(IFormatProvider provider) {
            return Convert.ToString(Value, provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider) {
            IConvertible v = Value as IConvertible;
            if (v != null) {
                return v.ToType(conversionType, provider);
            }
            throw new InvalidCastException();
        }

        public ushort ToUInt16(IFormatProvider provider) {
            return Convert.ToUInt16(Value, provider);
        }

        public uint ToUInt32(IFormatProvider provider) {
            return Convert.ToUInt32(Value, provider);
        }

        public ulong ToUInt64(IFormatProvider provider) {
            return Convert.ToUInt64(Value, provider);
        }
        #endregion

        public static string ToExpression(string value) {
            StringBuilder buf = new StringBuilder(value.Length * 2);
            buf.Append('"');
            foreach (char c in value) {
                buf.Append(c);
                if (c == '"') {
                    buf.Append(c);
                }
            }
            buf.Append('"');
            return buf.ToString();
        }

        public static string ToExpression(DateTime value) {
            if (value.TimeOfDay == TimeSpan.Zero) {
                return string.Format("Date({0},{1},{2})", value.Year, value.Month, value.Day);
            } else {
                return string.Format("Date({0},{1},{2},{3},{4},{5})", value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
            }
        }

        public static string ToExpression(TimeSpan value) {
            return string.Format("Time({0},{1},{2},{3})", value.Days, value.Hours, value.Minutes, value.Seconds);
        }

        public static string ToExpression(object value) {
            if (value is string) {
                return ToExpression((string)value);
            }
            if (value is DateTime) {
                return ToExpression((DateTime)value);
            }
            if (value is TimeSpan) {
                return ToExpression((TimeSpan)value);
            }
            return value.ToString();
        }

        public static string ToFormula(object value) {
            return ToExpression(value);
        }
    }
}
