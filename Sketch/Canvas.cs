using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Sketch {
    partial class Canvas {
        private Sheet _sheet;
        private Layer _activeLayer;
        private double _scaling = 1.0;
        private Point2D _visibleCenter;
        private Angle _rotateAngle = Angle.Zero;
        private bool _isVisibleRectValid = false;
        private Rectangle2D _visibleRect;
        private Selection _selection = null;
        private static object _selectionLock = new object();
        public Sheet Sheet {
            get { return _sheet; }
            set {
                if (_sheet != value) {
                    _sheet = value;
                    _activeLayer = (_sheet != null) ? _sheet.CurrentLayer : null;
                    InvalidateVisibleRect();
                }
            }
        }

        public Layer ActiveLayer {
            get {
                return _activeLayer;
            }
            set {
                if (_activeLayer != null && _activeLayer.Container != _sheet) {
                    throw new ArgumentException("ActiveLayer");
                }
                _activeLayer = value;
            }
        }

        [DefaultValue(1.0)]
        public double Scaling {
            get { return _scaling; }
            set {
                if (_scaling != value) {
                    _scaling = value;
                    InvalidateVisibleRect();
                }
            }
        }

        public Angle RotatingAngle {
            get { return _rotateAngle; }
            set {
                if (_rotateAngle != value) {
                    _rotateAngle = value;
                    InvalidateVisibleRect();
                }
            }
        }

        protected void InvalidateVisibleRect() {
            _isVisibleRectValid = false;
            InvalidateVisual();
        }

        protected void UpdateVisibleRect() {
            if (!_isVisibleRectValid) {
                double sc = Scaling;
                if (_sheet != null) {
                    sc = sc * _sheet.Scaling;
                }
                Size2D s = new Size2D(ActualWidth * sc, ActualHeight * sc, LengthUnit.Pixels);
                _visibleRect = new Rectangle2D(_visibleCenter - s / 2, s);
                UpdateVisibleRectCore(); // Implementation for framework dependent
                _isVisibleRectValid = true;
            }
        }

        /// <summary>
        /// 表示領域の矩形
        /// </summary>
        protected Rectangle2D VisibleRect {
            get {
                UpdateVisibleRect();
                return _visibleRect;
            }
        }

        /// <summary>
        /// 表示領域の中心座標
        /// </summary>
        public Point2D VisibleCenter {
            get {
                return _visibleCenter;
            }
            set {
                if (_visibleCenter != value) {
                    _visibleCenter = value;
                    InvalidateVisibleRect();
                }
            }
        }

        public Selection Selection {
            get {
                lock (_selectionLock) {
                    if (_selection == null) {
                        _selection = new Selection(this);
                    }
                }
                return _selection;
            }
        }

        private List<KnobControl> _knobControls = new List<KnobControl>();

        public event EventHandler<SelectionChangingEventArgs> SelectionChanging;
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        protected internal virtual void OnSelectionChanging(object sender, SelectionChangingEventArgs e) {
            if (SelectionChanging != null) {
                SelectionChanging(this, e);
            }
        }

        protected internal virtual void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (SelectionChanged != null) {
                SelectionChanged(this, e);
            }
            InvalidateVisual();
            UpdateKnob();
        }

        //private KnobControl[] GetSelectionKnob() {
        //    Distance x0 = new Distance(double.MaxValue, Distance.DefaultUnit);
        //    Distance x1 = new Distance(double.MaxValue, Distance.DefaultUnit);
        //    Distance y0 = new Distance(double.MinValue, Distance.DefaultUnit);
        //    Distance y1 = new Distance(double.MinValue, Distance.DefaultUnit);
        //    foreach (Shape sh in Selection) {
        //        Rectangle2D r = sh.GetBounds(Angle.Zero);
        //        x0 = Distance.Min(x0, r.Left);
        //        x1 = Distance.Max(x1, r.Right);
        //        y0 = Distance.Min(y0, r.Top);
        //        y1 = Distance.Max(y1, r.Bottom);
        //    }

        //}

        public void UpdateKnob() {
            List<KnobControl> l = new List<KnobControl>();
            if (_behavior == null || _behavior.SelectionVisible) {
                switch (Selection.Count) {
                    case 0:
                        break;
                    case 1:
                        Shape sel = Selection[0];
                        if (sel != null) {
                            l.AddRange(sel.GetKnobControls(this));
                        }
                        break;
                    default:

                        break;
                }
            }
            for (int i = _knobControls.Count - 1; 0 <= i; i--) {
                KnobControl c = _knobControls[i];
                if (!l.Contains(c)) {
                    c.DisposeControl();
                }
            }
        }

        //public void UpdateKnobPosition() {
        //    foreach (KnobControl c in _knobControls) {
        //        c.UpdatePosition();
        //    }
        //}
    }
}
