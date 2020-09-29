using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch {
    public partial interface IKnobInfo {
        Shape Shape { get; }
        bool Enabled { get; set; }
        bool Visible { get; set; }
        Point2D ToPoint2D();
        void SetValueFromPoint(Point2D point);
        KnobControl RequireKnobControl(Canvas canvas);
        event EventHandler<DisposeEventArgs> Disposed;
    }

    public abstract partial class KnobInfo: ExprObject, IKnobInfo {
        private WeakReference _target;
        private bool _enabled;
        private bool _visible;
        public IExprObject Target {
            get {
                if (_target == null) {
                    return null;
                }
                if (!_target.IsAlive) {
                    return null;
                }
                return _target.Target as IExprObject;
            }
        }
        private WeakReference _shape;
        public Shape Shape {
            get {
                if (_shape == null) {
                    return null;
                }
                if (!_shape.IsAlive) {
                    return null;
                }
                return _shape.Target as Shape;   
            }
        }
        public virtual bool Enabled { get { return _enabled; } set { _enabled = value; } }
        public virtual bool Visible { get { return _visible; } set { _visible = value; } }
        public abstract Point2D ToPoint2D();
        public abstract void SetValueFromPoint(Point2D point);
        public abstract KnobControl RequireKnobControl(Canvas canvas);

        public KnobInfo(Shape shape, IExprObject target) {
            if (shape == null) {
                throw new ArgumentNullException("shape");
            }
            if (target == null) {
                throw new ArgumentNullException("target");
            }
            _shape = new WeakReference(shape);
            _target = new WeakReference(target);
            _visible = true;
            _enabled = true;
            //_cells = InitCells();
            shape.Knobs.Add(this);
        }
    }

    public partial class KnobInfoCollection: ICollection<IKnobInfo> {
        //private Shape _parent;
        private List<IKnobInfo> _items = new List<IKnobInfo>();

        internal IKnobInfo this[int index] { get { return _items[index]; } }

        internal KnobInfoCollection() { }
        //internal DrawingPathCollection(Shape parent) {
        //    _parent = parent;
        //}

        private void UnlinkItemEvent(IKnobInfo item) {
            item.Disposed -= ItemDisposed;
        }

        private void ItemDisposed(object sender, DisposeEventArgs e) {
            Remove(sender as IKnobInfo);
        }

        #region ICollection の実装
        public void Add(IKnobInfo item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            _items.Add(item);
            item.Disposed += ItemDisposed;
        }

        public void Clear() {
            foreach (IKnobInfo item in _items) {
                UnlinkItemEvent(item);
            }
            _items.Clear();
        }

        public bool Contains(IKnobInfo item) {
            return _items.Contains(item);
        }

        public void CopyTo(IKnobInfo[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _items.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(IKnobInfo item) {
            bool ret = _items.Remove(item);
            if (ret) {
                UnlinkItemEvent(item);
            }
            return ret;
        }

        public IEnumerator<IKnobInfo> GetEnumerator() {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
        #endregion

        public void Insert(int index, IKnobInfo item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            _items.Insert(index, item);
            item.Disposed += ItemDisposed;
        }
    }

    public class PointKnobInfo: KnobInfo {
        private const int PROP_X = 0;
        private const int PROP_Y = 1;
        private static PropertyDefCollection _builtinPropertyDefs = new PropertyDefCollection(
            new PropertyDef[] {
                new PropertyDef("X", typeof(Distance), "0px"),
                new PropertyDef("Y", typeof(Distance), "0px"),
            }
        );

        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinPropertyDefs;
        }

        public override Point2D ToPoint2D() {
            return Shape.PointFromSheet(new Point2D(Formula[PROP_X].DistanceValue, Formula[PROP_Y].DistanceValue));
        }
        public override void SetValueFromPoint(Point2D point) {
            Formula[PROP_X].SetValue(point.X, EditingLevel.EditByKnob);
            Formula[PROP_Y].SetValue(point.Y, EditingLevel.EditByKnob);
        }

        public PointKnobInfo(Shape shape, ExprObject target, string propertyX, string propertyY)
            : base(shape, target) {
            Formula[PROP_X].SetFormula("@" + propertyX);
            Formula[PROP_Y].SetFormula("@" + propertyY);
        }

        private object _knobContorlLock = new object();
        private Dictionary<Canvas, KnobControl> _knobControls;
        public override KnobControl RequireKnobControl(Canvas canvas) {
            lock (_knobContorlLock) {
                if (_knobControls == null) {
                    _knobControls = new Dictionary<Canvas, KnobControl>();
                }
                KnobControl c;
                if (!_knobControls.TryGetValue(canvas, out c)) {
                    c = null;
                }
                if (c == null) {
                    c = new KnobControl(canvas, this);
                    c.ControlDisposed += KnobControlDisposed;
                    _knobControls.Add(canvas, c);
                }
                return c;
            }
        }

        public void KnobControlDisposed(object sender, DisposeEventArgs e) {
            lock (_knobContorlLock) {
                foreach (Canvas key in _knobControls.Keys) {
                    if (_knobControls[key] == sender) {
                        _knobControls.Remove(key);
                    }
                }
            }
        }
    }

    public abstract partial class BorderKnobInfo: IKnobInfo {
        internal const double SIDEKNOB_THRESHOLD = 10.0;
        private WeakReference _shape;

        public BorderKnobInfo(Shape owner) {
            if (owner == null) {
                throw new ArgumentNullException();
            }
            _shape = new WeakReference(owner);
            owner.Knobs.Add(this);
        }

        ~BorderKnobInfo() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (Disposed != null) {
                Disposed(this, new DisposeEventArgs(disposing));
            }
        }

        public void Dispose() {
            Dispose(true);
        }

        public Shape Shape { get { return _shape.IsAlive ? (_shape.Target as Shape) : null; } }
        public virtual bool Visible { get { return true; } set { } }

        public virtual bool Enabled {
            get {
                //return Shape.FormulaPinX.CanEditByKnob() && Shape.FormulaWidth.CanEditByKnob()
                //    && Shape.FormulaPinY.CanEditByKnob() && Shape.FormulaHeight.CanEditByKnob();
                return Shape.FormulaWidth.CanEditByKnob() && Shape.FormulaHeight.CanEditByKnob();
            }
            set { }
        }
        public abstract Point2D ToPoint2D();
        public abstract void SetValueFromPoint(Point2D point);

        public virtual KnobControl RequireKnobControl(Canvas canvas) {
            KnobControl c = new KnobControl(canvas, this);
            return c;
        }

        public event EventHandler<DisposeEventArgs> Disposed; 
    }

    public class TopLeftKnobInfo: BorderKnobInfo {
        public TopLeftKnobInfo(Shape owner) : base(owner) { }
        
        public override Point2D ToPoint2D() {
            return Shape.PointToCanvas(Point2D.Zero);
        }

        public override void SetValueFromPoint(Point2D point) {
            Point2D pDiff = point - ToPoint2D();
            Point2D pRDiff = Angle.Rotate(-Shape.Angle, pDiff);
            if (Shape.LockAspect) {
                Size2D s = Shape.Size;
                if (s.Width == Distance.Zero || s.Height == Distance.Zero) {
                    return;
                }
                double rX = pRDiff.X / s.Width;
                double rY = pRDiff.Y / s.Height;
                double r;
                if (0 <= rX) {
                    r = Math.Max(rX, rY);
                } else {
                    r = Math.Min(rX, rY);
                }
                pRDiff.X = r * s.Width;
                pRDiff.Y = r * s.Height;
                pDiff = Angle.Rotate(Shape.Angle, pRDiff);
            }
            Shape.FormulaPinX.SetValue(Shape.PinX + (pDiff.X / 2), EditingLevel.EditByKnob);
            Shape.FormulaPinY.SetValue(Shape.PinY + (pDiff.Y / 2), EditingLevel.EditByKnob);
            Shape.FormulaWidth.SetValue(Shape.Width - pRDiff.X, EditingLevel.EditByKnob);
            Shape.FormulaHeight.SetValue(Shape.Height - pRDiff.Y, EditingLevel.EditByKnob);
        }
    }

    public class TopRightKnobInfo: BorderKnobInfo {
        public TopRightKnobInfo(Shape owner) : base(owner) { }

        public override Point2D ToPoint2D() {
            return Shape.PointToCanvas(new Point2D(Shape.Width, Distance.Zero));
        }

        public override void SetValueFromPoint(Point2D point) {
            Point2D pDiff = point - ToPoint2D();
            Point2D pRDiff = Angle.Rotate(-Shape.Angle, pDiff);
            if (Shape.LockAspect) {
                Size2D s = Shape.Size;
                if (s.Width == Distance.Zero || s.Height == Distance.Zero) {
                    return;
                }
                double rX = pRDiff.X / s.Width;
                double rY = pRDiff.Y / s.Height;
                double r;
                if (0 <= rX) {
                    r = Math.Max(rX, -rY);
                } else {
                    r = Math.Min(rX, -rY);
                }
                pRDiff.X = r * s.Width;
                pRDiff.Y = -r * s.Height;
                pDiff = Angle.Rotate(Shape.Angle, pRDiff);
            }
            Shape.FormulaPinX.SetValue(Shape.PinX + pDiff.X / 2, EditingLevel.EditByKnob);
            Shape.FormulaPinY.SetValue(Shape.PinY + pDiff.Y / 2, EditingLevel.EditByKnob);
            Shape.FormulaWidth.SetValue(Shape.Width + pRDiff.X, EditingLevel.EditByKnob);
            Shape.FormulaHeight.SetValue(Shape.Height - pRDiff.Y, EditingLevel.EditByKnob);
        }
    }

    public class BottomLeftKnobInfo: BorderKnobInfo {
        public BottomLeftKnobInfo(Shape owner) : base(owner) { }

        public override Point2D ToPoint2D() {
            return Shape.PointToCanvas(new Point2D(Distance.Zero, Shape.Height));
        }

        public override void SetValueFromPoint(Point2D point) {
            Point2D pDiff = point - ToPoint2D();
            Point2D pRDiff = Angle.Rotate(-Shape.Angle, pDiff);
            if (Shape.LockAspect) {
                Size2D s = Shape.Size;
                if (s.Width == Distance.Zero || s.Height == Distance.Zero) {
                    return;
                }
                double rX = pRDiff.X / s.Width;
                double rY = pRDiff.Y / s.Height;
                double r;
                if (0 <= rX) {
                    r = Math.Max(rX, -rY);
                } else {
                    r = Math.Min(rX, -rY);
                }
                pRDiff.X = r * s.Width;
                pRDiff.Y = -r * s.Height;
                pDiff = Angle.Rotate(Shape.Angle, pRDiff);
            }
            Shape.FormulaPinX.SetValue(Shape.PinX + pDiff.X / 2, EditingLevel.EditByKnob);
            Shape.FormulaPinY.SetValue(Shape.PinY + pDiff.Y / 2, EditingLevel.EditByKnob);
            Shape.FormulaWidth.SetValue(Shape.Width - pRDiff.X, EditingLevel.EditByKnob);
            Shape.FormulaHeight.SetValue(Shape.Height + pRDiff.Y, EditingLevel.EditByKnob);
        }
    }

    public class BottomRightKnobInfo: BorderKnobInfo {
        public BottomRightKnobInfo(Shape owner) : base(owner) { }

        public override Point2D ToPoint2D() {
            return Shape.PointToCanvas(new Point2D(Shape.Width, Shape.Height));
        }

        public override void SetValueFromPoint(Point2D point) {
            Point2D pDiff = point - ToPoint2D();
            Point2D pRDiff = Angle.Rotate(-Shape.Angle, pDiff);
            if (Shape.LockAspect) {
                Size2D s = Shape.Size;
                if (s.Width == Distance.Zero || s.Height == Distance.Zero) {
                    return;
                }
                double rX = pRDiff.X / s.Width;
                double rY = pRDiff.Y / s.Height;
                double r;
                if (0 <= rX) {
                    r = Math.Max(rX, rY);
                } else {
                    r = Math.Min(rX, rY);
                }
                pRDiff.X = r * s.Width;
                pRDiff.Y = r * s.Height;
                pDiff = Angle.Rotate(Shape.Angle, pRDiff);
            }
            Shape.FormulaPinX.SetValue(Shape.PinX + pDiff.X / 2, EditingLevel.EditByKnob);
            Shape.FormulaPinY.SetValue(Shape.PinY + pDiff.Y / 2, EditingLevel.EditByKnob);
            Shape.FormulaWidth.SetValue(Shape.Width + pRDiff.X, EditingLevel.EditByKnob);
            Shape.FormulaHeight.SetValue(Shape.Height + pRDiff.Y, EditingLevel.EditByKnob);
        }
    }

    public class LeftKnobInfo: BorderKnobInfo {
        public LeftKnobInfo(Shape owner) : base(owner) { }

        public override bool Visible {
            get {
                return base.Visible && (SIDEKNOB_THRESHOLD < Shape.ToDisplayOffsetX(Shape.Width));
            }
            set {
                base.Visible = value;
            }
        }

        public override Point2D ToPoint2D() {
            return Shape.PointToCanvas(new Point2D(Distance.Zero, Shape.Height / 2));
        }

        public override void SetValueFromPoint(Point2D point) {
            Point2D pDiff = point - ToPoint2D();
            Point2D pRDiff = Angle.Rotate(-Shape.Angle, pDiff);
            pRDiff.Y = Distance.Zero;
            pDiff = Angle.Rotate(Shape.Angle, pRDiff);
            if (Shape.LockAspect) {
                Size2D s = Shape.Size;
                if (s.Width != Distance.Zero) {
                    double r = pRDiff.X / s.Width;
                    pRDiff.Y = r * s.Height;
                }
            }
            Shape.FormulaPinX.SetValue(Shape.PinX + pDiff.X / 2, EditingLevel.EditByKnob);
            Shape.FormulaPinY.SetValue(Shape.PinY + pDiff.Y / 2, EditingLevel.EditByKnob);
            Shape.FormulaWidth.SetValue(Shape.Width - pRDiff.X, EditingLevel.EditByKnob);
            Shape.FormulaHeight.SetValue(Shape.Height - pRDiff.Y, EditingLevel.EditByKnob);
        }
    }

    public class TopKnobInfo: BorderKnobInfo {
        public TopKnobInfo(Shape owner) : base(owner) { }

        public override bool Visible {
            get {
                return base.Visible && (SIDEKNOB_THRESHOLD < Shape.ToDisplayOffsetY(Shape.Height));
            }
            set {
                base.Visible = value;
            }
        }

        public override Point2D ToPoint2D() {
            return Shape.PointToCanvas(new Point2D(Shape.Width / 2, Distance.Zero));
        }
        
        public override void SetValueFromPoint(Point2D point) {
            Point2D pDiff = point - ToPoint2D();
            Point2D pRDiff = Angle.Rotate(-Shape.Angle, pDiff);
            pRDiff.X = Distance.Zero;
            pDiff = Angle.Rotate(Shape.Angle, pRDiff);
            if (Shape.LockAspect) {
                Size2D s = Shape.Size;
                if (s.Height != Distance.Zero) {
                    double r = pRDiff.Y / s.Height;
                    pRDiff.X = r * s.Width;
                }
            }
            Shape.FormulaPinX.SetValue(Shape.PinX + pDiff.X / 2, EditingLevel.EditByKnob);
            Shape.FormulaPinY.SetValue(Shape.PinY + pDiff.Y / 2, EditingLevel.EditByKnob);
            Shape.FormulaWidth.SetValue(Shape.Width - pRDiff.X, EditingLevel.EditByKnob);
            Shape.FormulaHeight.SetValue(Shape.Height - pRDiff.Y, EditingLevel.EditByKnob);
        }
    }

    public class RightKnobInfo: BorderKnobInfo {
        public RightKnobInfo(Shape owner) : base(owner) { }

        public override bool Visible {
            get {
                return base.Visible && (SIDEKNOB_THRESHOLD < Shape.ToDisplayOffsetX(Shape.Width));
            }
            set {
                base.Visible = value;
            }
        }

        public override Point2D ToPoint2D() {
            return Shape.PointToCanvas(new Point2D(Shape.Width, Shape.Height / 2));
        }
        
        public override void SetValueFromPoint(Point2D point) {
            Point2D pDiff = point - ToPoint2D();
            Point2D pRDiff = Angle.Rotate(-Shape.Angle, pDiff);
            pRDiff.Y = Distance.Zero;
            pDiff = Angle.Rotate(Shape.Angle, pRDiff);
            if (Shape.LockAspect) {
                Size2D s = Shape.Size;
                if (s.Width != Distance.Zero) {
                    double r = pRDiff.X / s.Width;
                    pRDiff.Y = r * s.Height;
                }
            }
            Shape.FormulaPinX.SetValue(Shape.PinX + pDiff.X / 2, EditingLevel.EditByKnob);
            Shape.FormulaPinY.SetValue(Shape.PinY + pDiff.Y / 2, EditingLevel.EditByKnob);
            Shape.FormulaWidth.SetValue(Shape.Width + pRDiff.X, EditingLevel.EditByKnob);
            Shape.FormulaHeight.SetValue(Shape.Height + pRDiff.Y, EditingLevel.EditByKnob);
        }
    }

    public class BottomKnobInfo: BorderKnobInfo {
        public BottomKnobInfo(Shape owner) : base(owner) { }

        public override bool Visible {
            get {
                return base.Visible && (SIDEKNOB_THRESHOLD < Shape.ToDisplayOffsetY(Shape.Height));
            }
            set {
                base.Visible = value;
            }
        }

        public override Point2D ToPoint2D() {
            return Shape.PointToCanvas(new Point2D(Shape.Width / 2, Shape.Height));
        }
        public override void SetValueFromPoint(Point2D point) {
            Point2D pDiff = point - ToPoint2D();
            Point2D pRDiff = Angle.Rotate(-Shape.Angle, pDiff);
            pRDiff.X = Distance.Zero;
            pDiff = Angle.Rotate(Shape.Angle, pRDiff);
            if (Shape.LockAspect) {
                Size2D s = Shape.Size;
                if (s.Height != Distance.Zero) {
                    double r = pRDiff.Y / s.Height;
                    pRDiff.X = r * s.Width;
                }
            }
            Shape.FormulaPinX.SetValue(Shape.PinX + pDiff.X / 2, EditingLevel.EditByKnob);
            Shape.FormulaPinY.SetValue(Shape.PinY + pDiff.Y / 2, EditingLevel.EditByKnob);
            Shape.FormulaWidth.SetValue(Shape.Width + pRDiff.X, EditingLevel.EditByKnob);
            Shape.FormulaHeight.SetValue(Shape.Height + pRDiff.Y, EditingLevel.EditByKnob);
        }
    }

    public class AngleKnobInfo: KnobInfo {
        private static double _crankLength = 100;
        /// <summary>
        /// 角度変更ノブと回転の中心点の間の距離
        /// 単位はピクセル、縮尺の影響を受けない
        /// </summary>
        public static double CrankLength { get { return _crankLength; } set { _crankLength = value; } }

        private const int PROP_PINX = 0;
        private const int PROP_PINY = 1;
        private const int PROP_ANGLE = 2;
        private static PropertyDefCollection _builtinPropertyDefs = new PropertyDefCollection(
            new PropertyDef[] {
                new PropertyDef("PinX", typeof(Distance), "0px"),
                new PropertyDef("PinY", typeof(Distance), "0px"),
                new PropertyDef("Angle", typeof(Angle), "0deg"),
            }
        );

        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinPropertyDefs;
        }

        private double _displayScale;

        public FormulaProperty FormulaPinX { get { return Formula[PROP_PINX]; } }
        public FormulaProperty FormulaPinY { get { return Formula[PROP_PINY]; } }
        public FormulaProperty FormulaAngle { get { return Formula[PROP_ANGLE]; } }

        [FormulaIndex(PROP_PINX)]
        public Distance PinX {
            get { return Formula[PROP_PINX].DistanceValue; }
            set { Formula[PROP_PINX].SetValue(value, EditingLevel.EditByKnob); }
        }

        [FormulaIndex(PROP_PINY)]
        public Distance PinY {
            get { return Formula[PROP_PINY].DistanceValue; }
            set { Formula[PROP_PINY].SetValue(value, EditingLevel.EditByKnob); }
        }

        [FormulaIndex(PROP_ANGLE)]
        public Angle Angle {
            get { return Formula[PROP_ANGLE].AngleValue; }
            set { Formula[PROP_ANGLE].SetValue(value, EditingLevel.EditByKnob); }
        }

        public override Point2D ToPoint2D() {
            Angle a = FormulaAngle.AngleValue - new Angle(90, AngleUnit.Degree);
            Distance l = new Distance(_displayScale * CrankLength, LengthUnit.Pixels);
            Point2D p = new Point2D(PinX + l * a.Cos(), PinY + l * a.Sin());
            p = Shape.ParentPointToSheet(p);
            return p;
        }
        public override void SetValueFromPoint(Point2D point) {
            Point2D p = Shape.ParentPointFromSheet(point) - new Point2D(PinX, PinY);
            Point2D baseLine = new Point2D(0, -CrankLength, LengthUnit.Pixels);
            Angle a = p.Angle(baseLine);
            Distance l = p.GetDistance();
            if (l.Px < CrankLength * 2) {
                a = new Angle(Math.Round(a.Deg / 15) * 15, AngleUnit.Degree);
            } else {
                a = new Angle(Math.Round(a.Deg), AngleUnit.Degree);
            }
            FormulaAngle.SetValue(a, EditingLevel.EditByKnob);
        }

        public override KnobControl RequireKnobControl(Canvas canvas) {
            KnobControl c = new KnobControl(canvas, this);
            return c;
        }

        public AngleKnobInfo(Shape shape, IExprObject target, string propertyPinX, string propertyPinY, string propertyAngle, double displayScale)
            : base(shape, target) {
            Formula[PROP_PINX].SetFormula(propertyPinX);
            Formula[PROP_PINY].SetFormula(propertyPinY);
            Formula[PROP_ANGLE].SetFormula("@" + propertyAngle);
            _displayScale = displayScale;
        }
    }

    public class SizeKnobInfo: KnobInfo {
        private const int PROP_X = 0;
        private const int PROP_Y = 1;
        private static PropertyDefCollection _builtinPropertyDefs = new PropertyDefCollection(
            new PropertyDef[] {
                new PropertyDef("X", typeof(Distance), "0px"),
                new PropertyDef("Y", typeof(Distance), "0px"),
            }
        );

        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinPropertyDefs;
        }

        public FormulaProperty FormulaX { get { return Formula[PROP_X]; } }
        public FormulaProperty FormulaY { get { return Formula[PROP_Y]; } }

        [FormulaIndex(PROP_X)]
        public Distance X {
            get { return Formula[PROP_X].DistanceValue; }
            set { Formula[PROP_X].SetValue(value, EditingLevel.EditByKnob); }
        }

        [FormulaIndex(PROP_Y)]
        public Distance Y {
            get { return Formula[PROP_Y].DistanceValue; }
            set { Formula[PROP_Y].SetValue(value, EditingLevel.EditByKnob); }
        }

        public override Point2D ToPoint2D() {
            return Shape.PointFromSheet(new Point2D(Formula[PROP_X].DistanceValue, Formula[PROP_Y].DistanceValue));

        }
        public override void SetValueFromPoint(Point2D point) {
            Formula[PROP_X].SetValue(point.X, EditingLevel.EditByKnob);
            Formula[PROP_Y].SetValue(point.Y, EditingLevel.EditByKnob);
        }

        public override KnobControl RequireKnobControl(Canvas canvas) {
            KnobControl c = new KnobControl(canvas, this);
            return c;
        }

        public SizeKnobInfo(Shape shape, ExprObject target, string propertyX, string propertyY)
            : base(shape, target) {
            Formula[PROP_X].SetFormula("@" + propertyX);
            Formula[PROP_Y].SetFormula("@" + propertyY);
        }
    }
    public partial class KnobControl {
        public event EventHandler<DisposeEventArgs> ControlDisposed;

        public void HideControl() {
            DoHideControl();    // WPF依存部分になるためKnob.WPF.csで実装
        }
        public void DisposeControl() {
            DoDisposeControl(); // WPF依存部分になるためKnob.WPF.csで実装
            if (ControlDisposed != null) {
                ControlDisposed(this, new DisposeEventArgs(true));
            }
        }
    }
}
