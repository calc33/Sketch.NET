using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Sketch {
    public partial class DraggingControl : Thumb {
        private Canvas _owner;
        //private Shape[] _shapes;
        private Point2D _visibleCenter;

        public DraggingControl(Canvas owner) {
            if (owner == null) {
                throw new ArgumentNullException("owner");
            }
            if (owner.Selection == null || owner.Selection.Count == 0) {
                throw new ArgumentException("No selection");
            }
            _owner = owner;
            Rectangle2D r = _owner.Selection.Bounds;
            _visibleCenter = r.Center;
            Point p = _owner.Sheet.ToDisplayPoint(r.TopLeft);
            p = _owner.PointToScreen(p);
            VisualOffset = new Vector(p.X, p.Y);
        }

        public static DraggingControl RequireDraggingControl(Canvas owner) {
            if (owner == null) {
                return null;
            }
            if (owner.Selection == null) {
                return null;
            }
            if (owner.Selection.Count == 0) {
                return null;
            }
            return new DraggingControl(owner);
        }

        private bool _isVisibleRectValid = false;
        private Rectangle2D _visibleRect;
        private Matrix _baseMatrix;
        private Matrix _invMatrix;

        protected void UpdateVisibleRectCore() {
            if (_owner == null) {
                return;
            }
            double sc = _owner.Scaling;
            double dx = 0;
            double dy = 0;
            Sheet sh = _owner.Sheet;
            if (sh != null) {
                sc = sc * sh.Scaling;
                dx = sh.OffsetX.Px;
                dy = sh.OffsetY.Px;
            }
            Size2D s = new Size2D(ActualWidth * sc, ActualHeight * sc, LengthUnit.Pixels);
            _visibleRect = new Rectangle2D(_visibleCenter - s / 2, s);
            double sin = _owner.RotatingAngle.Sin();
            double cos = _owner.RotatingAngle.Cos();
            _baseMatrix = new Matrix(sc * cos, sc * sin, -sc * sin, sc * cos, sc * dx, sc * dy);
            _invMatrix = new Matrix(cos / sc, -sin / sc, sin / sc, cos / sc, -dx / sc, -dy / sc);
            _isVisibleRectValid = true;
        }

        protected void InvalidateVisibleRect() {
            _isVisibleRectValid = false;
            InvalidateVisual();
        }

        protected void UpdateVisibleRect() {
            if (!_isVisibleRectValid) {
                UpdateVisibleRectCore(); // Implementation for framework dependent
                _isVisibleRectValid = true;
            }
        }

        protected internal Matrix BaseMatrix {
            get {
                UpdateVisibleRect();
                return _baseMatrix;
            }
        }

        protected internal Matrix ControlMatrix {
            get {
                UpdateVisibleRect();
                return _baseMatrix;
            }
        }

        protected internal Matrix InvMatrix {
            get {
                UpdateVisibleRect();
                return _invMatrix;
            }
        }

        private MatrixTransform GetMatrixTransform() {
            double sc = (_owner != null && _owner.Sheet != null) ? _owner.Sheet.Scaling : 1.0;
            if (sc == 0.0) {
                sc = 1.0;
            }
            //return new MatrixTransform(1, 0, 0, 1, _offset.X.Px / sc, _offset.Y.Px / sc);
            return new MatrixTransform(1, 0, 0, 1, 0, 0);
        }
        
        protected override void OnRender(DrawingContext drawingContext) {
            MatrixTransform trns = GetMatrixTransform();
            drawingContext.PushTransform(trns);
            try {
                //base.OnRender(drawingContext);
                foreach (Shape sh in _owner.Selection) {
                    sh.Render(drawingContext);
                }
            } finally {
                drawingContext.Pop();
            }
        }
    }
}
