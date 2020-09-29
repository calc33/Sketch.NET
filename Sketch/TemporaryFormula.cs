using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Sketch {
    public class TemporaryFormula : IExpressionOwnable {
        private WeakReference _owner;
        private Expression _formula;

        public TemporaryFormula(IExprObject owner, string formula) {
            _owner = new WeakReference(owner);
            _formula = new Expression(this, formula, true);
        }

        public IExprObject Owner {
            get {
                if (!_owner.IsAlive) {
                    return null;
                }
                return _owner.Target as IExprObject;
            }
        }

        public string Formula {
            get { return _formula.Formula; }
            //set {
            //    _formula.Formula = value;
            //}
        }
        protected internal void SetFormula(string value) {
            _formula.Formula = value;
        }

        public object Value {
            get { return _formula.Value; }
        }

        protected internal void SetValue(object value, EditingLevel editingLevel) {
            if (!object.Equals(_formula.Value, value)) {
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

        protected Color ColorValue {
            get {
                return ToColorValue(Value);
            }
        }

        public void ReplaceComponents(Dictionary<ExprComponent<Document>, ExprComponent<Document>> shapeMapping) {
            //_formula.ReplaceComponents
        }

        private void DependentFormulaChanged(object sender, ValueChangedEventArgs e) {
            if (_formula != null) {
                _formula.Invalidate();
            }
        }

        private List<FormulaProperty> _dependencies = new List<FormulaProperty>();

        public void AddDependency(DependencyAddedEevntArgs e) { }

        public void ClearDependencies() { }

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
}
