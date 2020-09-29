using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace Sketch {
    public interface IExprObject {
        FormulaPropertyCollection Formula { get; }
        FormulaPropertyCollection Extra { get; }
        PropertyDefCollection GetBuiltinPropertyDefs();
        event EventHandler<FormulaChangingEventArgs> FormulaChanging;
        event EventHandler<FormulaChangedEventArgs> FormulaChanged;
        event EventHandler<ValueChangedEventArgs> ValueChanged;
        void OnFormulaChanging(FormulaChangingEventArgs e);
        void OnFormulaChanged(FormulaChangedEventArgs e);
        void OnValueChanged(ValueChangedEventArgs e);
    }

    public class DisposeEventArgs : EventArgs {
        private bool _disposing;
        public bool Disposing { get { return _disposing; } }

        public DisposeEventArgs(bool disposing) {
            _disposing = disposing;
        }
    }

    public abstract class ExprObject: IExprObject, IDisposable {
        private string _name;
        private BuiltinFormulaCollection _formula;
        private ExtraFormulaCollection _extra;

        public abstract PropertyDefCollection GetBuiltinPropertyDefs();

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

        public ExprObject() {
            _formula = new BuiltinFormulaCollection(this);
            _extra = new ExtraFormulaCollection(this);
        }

        ~ExprObject() {
            Dispose(false);
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

        public virtual object Clone() {
            ExprObject ret = (ExprObject)MemberwiseClone();
            return ret;
        }

        FormulaPropertyCollection IExprObject.Formula {
            get { throw new NotImplementedException(); }
        }

        FormulaPropertyCollection IExprObject.Extra {
            get { throw new NotImplementedException(); }
        }
    }
}
