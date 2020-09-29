using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Sketch {
    partial interface IMouseBehavior {
        IMouseBehavior GetPrior();
        void SetPrior(IMouseBehavior prior);
        void OnMouseDown(MouseButtonEventArgs e, bool isFirstResponder);
        void OnMouseMove(MouseEventArgs e, bool isFirstResponder);
        void OnPreviewMouseMove(MouseEventArgs e, bool isFirstResponder);
        void OnMouseUp(MouseButtonEventArgs e, bool isFirstResponder);
        void OnMouseLeave(MouseEventArgs e, bool isFirstResponder);
        void OnMouseEnter(MouseEventArgs e, bool isFirstResponder);
        void OnDragEnter(DragEventArgs e, bool isFirstResponder);
        void OnDragLeave(DragEventArgs e, bool isFirstResponder);
        void OnDragOver(DragEventArgs e, bool isFirstResponder);
        void OnDrop(DragEventArgs e, bool isFirstResponder);
        void OnQueryContinueDrag(QueryContinueDragEventArgs e, bool isFirstResponder);
        void BeginDragging(MouseEventArgs e);
        void PreRender(DrawingContext drawingContext);
        void PostRender(DrawingContext drawingContext);
    }

    public abstract class MouseBehavior : IMouseBehavior {
        public MouseBehavior(Canvas owner) {
            if (owner != null) {
                _owner = new WeakReference(owner);
            }
        }
        
        private WeakReference _owner;

        public Canvas Owner {
            get { return (_owner != null && _owner.IsAlive) ? _owner.Target as Canvas : null; }
        }

        IMouseBehavior _prior = null;
        public IMouseBehavior GetPrior() {
            return _prior;
        }
        public void SetPrior(IMouseBehavior prior) {
            _prior = prior;
        }

        public virtual bool SelectionVisible { get { return true; } }

        public virtual void OnMouseDown(MouseButtonEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnMouseDown(e, false);
            }
        }

        public virtual void OnMouseMove(MouseEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnMouseMove(e, false);
            }
        }

        public virtual void OnPreviewMouseMove(MouseEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnPreviewMouseMove(e, false);
            }
        }

        public virtual void OnMouseUp(MouseButtonEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnMouseUp(e, false);
            }
        }

        public virtual void OnMouseLeave(MouseEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnMouseLeave(e, false);
            }
        }

        public virtual void OnMouseEnter(MouseEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnMouseEnter(e, false);
            }
        }

        public virtual void OnDragEnter(DragEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnDragEnter(e, false);
            }
        }

        public virtual void OnDragLeave(DragEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnDragLeave(e, false);
            }
        }
        
        public virtual void OnDragOver(DragEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnDragOver(e, false);
            }
        }
        
        public virtual void OnDrop(DragEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnDrop(e, false);
            }
        }
        
        public virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e, bool isFirstResponder) {
            if (_prior != null) {
                _prior.OnQueryContinueDrag(e, false);
            }
        }
        public virtual void BeginDragging(MouseEventArgs e) { }
        public virtual void PreRender(DrawingContext drawingContext) { }
        public virtual void PostRender(DrawingContext drawingContext) { }
    }

    public class SelectionDraggingAdorner : Adorner {
        public class DraggingCanvas : System.Windows.Controls.Canvas {
            private SelectionDraggingAdorner _owner;
            private Selection _selection;

            public DraggingCanvas(SelectionDraggingAdorner owner)
                : base() {
                if (owner == null){
                    throw new ArgumentNullException("owner");
                }
                _owner = owner;
                Canvas c = _owner.AdornedElement as Canvas;
                if (c != null) {
                    _selection = c.Selection;
                    if (_selection != null && _selection.Count != 0) {
                        Rectangle2D r = _selection.Bounds;
                        Size s = c.Sheet.ToDisplaySize(r.Size);
                        Width = s.Width;
                        Height = s.Height;
                    }
                }
            }

            protected override void OnRender(DrawingContext drawingContext) {
                drawingContext.DrawEllipse(new SolidColorBrush(Colors.Blue), RangeSelectingMouseBehavior.RangePen, new Point(0, 0), 1000, 1000);
                if (_selection != null) {
                    //MatrixTransform trns = new MatrixTransform(_owner.BaseMatrix);
                    //drawingContext.PushTransform(trns);
                    try {
                        _selection.PreRender(drawingContext);
                        foreach (Shape sh in _selection) {
                            sh.Render(drawingContext);
                        }
                        _selection.PostRender(drawingContext);
                    } finally {
                        //drawingContext.Pop();
                    }
                }
            }
        }

        //private DraggingCanvas _draggingCanvas;
        public SelectionDraggingAdorner(Canvas target, Point mouse)
            : base(target) {
            if (target == null) {
                throw new ArgumentNullException("target");
            }
            Sheet sh = target.Sheet;
            if (sh == null) {
                return;
            }
            //_mousePoint = target.PointToScreen(mouse);
            _mousePoint = mouse;
            //_visibleCenter = sh.ToPoint2D(mouse);

            Selection sel = target.Selection;
            if (sel != null && sel.Count != 0) {
                Rectangle2D r = target.Selection.Bounds;
                Size s = sh.ToDisplaySize(r.Size);
                Width = s.Width;
                Height = s.Height;
            }

            //_draggingCanvas = new DraggingCanvas(this);
        }

        //protected override Visual GetVisualChild(int index) {
        //    return _draggingCanvas;
        //}

        //protected override int VisualChildrenCount { get { return 1; } }

        //protected override Size MeasureOverride(Size finalSize) {
        //    _draggingCanvas.Measure(finalSize);
        //    return _draggingCanvas.DesiredSize;
        //}
        //protected override Size ArrangeOverride(Size finalSize) {
        //    _draggingCanvas.Arrange(new Rect(_draggingCanvas.DesiredSize));
        //    return finalSize;
        //}

        private bool _isVisibleRectValid = false;
        private Point _pin;
        private Point _mousePoint;
        //private Point2D _visibleCenter;
        //private Rectangle2D _visibleRect;
        private Matrix _baseMatrix;

        public double PinX {
            get { return _pin.X; }
            set {
                _pin.X = value;
                InvalidateVisibleRect();
            }
        }
        public double PinY {
            get { return _pin.Y; }
            set {
                _pin.Y = value;
                InvalidateVisibleRect();
            }
        }

        public void SetPin(Point value) {
            Canvas c = AdornedElement as Canvas;
            if (c != null) {
                //_pin = c.InvMatrix.Transform(value);
                _pin = value;
                InvalidateVisibleRect();
            }
        }

        protected void UpdateVisibleRectCore() {
            Canvas c = AdornedElement as Canvas;
            if (c == null) {
                return;
            }
            double sc = c.Scaling;
            double dx = 0;
            double dy = 0;
            Sheet sh = c.Sheet;
            if (sh != null) {
                sc = sc * sh.Scaling;
                dx = sh.OffsetX.Px;
                dy = sh.OffsetY.Px;
            }
            Size2D s = new Size2D(ActualWidth * sc, ActualHeight * sc, LengthUnit.Pixels);
            //_visibleRect = new Rectangle2D(_visibleCenter - s / 2, s);
            double sin = c.RotatingAngle.Sin();
            double cos = c.RotatingAngle.Cos();
            _baseMatrix = new Matrix(sc * cos, sc * sin, sc * sin, sc * cos, sc * dx + _pin.X - _mousePoint.X, sc * dy + _pin.Y - _mousePoint.Y);
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

        //public override GeneralTransform GetDesiredTransform(GeneralTransform transform) {
        //    var trns = new GeneralTransformGroup();
        //    trns.Children.Add(base.GetDesiredTransform(transform));
        //    trns.Children.Add(new MatrixTransform(BaseMatrix));
        //    return trns;
        //}

        protected override void OnRender(DrawingContext drawingContext) {
            Canvas c = AdornedElement as Canvas;
            if (c != null) {
                Selection sel = c.Selection;
                if (sel != null) {
                    MatrixTransform trns = new MatrixTransform(BaseMatrix);
                    drawingContext.PushTransform(trns);
                    try {
                        //drawingContext.DrawEllipse(new SolidColorBrush(Colors.Blue), RangeSelectingMouseBehavior.RangePen, new Point(0, 0), 1000, 1000);
                        sel.PreRender(drawingContext);
                        foreach (Shape sh in sel) {
                            sh.Render(drawingContext);
                        }
                        sel.PostRender(drawingContext);
                    } finally {
                        drawingContext.Pop();
                    }
                }
            }
        }
    }

    public sealed class BasicMouseBehavior : MouseBehavior {
        private bool _leftButtonDown = false;
        private bool _isDragging = false;
        private SelectionDraggingAdorner _adorner;

        public BasicMouseBehavior(Canvas owner) : base(owner) { }

        public override bool SelectionVisible { get { return true; } }

        public void ShapeBecomeSelected(Shape shape, bool multiple) {
            Canvas c = Owner;
            if (c == null) {
                return;
            }
            if (multiple) {
                if (shape != null) {
                    if (c.Selection.Contains(shape)) {
                        shape.Deselect(c);
                    } else {
                        shape.Select(c);
                    }
                }
            } else {
                c.Selection.Clear();
                if (shape != null) {
                    shape.Select(c);
                }
            }
        }

        public override void OnMouseDown(MouseButtonEventArgs e, bool isFirstResponder) {
            Canvas c = Owner;
            if (c == null) {
                return;
            }
            if (c.Sheet == null || c.ActiveLayer == null) {
                return;
            }

            Point pos = e.GetPosition(c);
            pos = c.InvMatrix.Transform(pos);
            Point2D p = c.Sheet.ToPoint2D(pos);
            Shape sh = c.ActiveLayer.GetTopShapeAt(p);
            switch (Keyboard.Modifiers) {
                case ModifierKeys.Shift:    // Shiftキーのみ
                    ShapeBecomeSelected(sh, true);
                    break;
                case ModifierKeys.None:     // 何も押さない
                    if (sh == null || !sh.IsSelected(c)) {
                        ShapeBecomeSelected(sh, false);
                    }
                    break;
            }
            c.InvalidateVisual();
            _leftButtonDown = (e.LeftButton == MouseButtonState.Pressed);
        }

        public override void BeginDragging(MouseEventArgs e) {
            try {
                if (_isDragging) {
                    return;
                }
                Canvas c = Owner;
                if (c == null) {
                    return;
                }
                if (c.Sheet == null || c.ActiveLayer == null) {
                    return;
                }
                Point pos = e.GetPosition(c);
                pos = c.InvMatrix.Transform(pos);
                Point2D p = c.Sheet.ToPoint2D(pos);
                Shape sh = c.ActiveLayer.GetTopShapeAt(p);
                IMouseBehavior b = null;
                if (sh == null) {
                    b = new RangeSelectingMouseBehavior(Owner);
                } else {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) {
                        // コピー&ドラッグ
                        IDataObject obj = new DataObject(DataFormats.Serializable, Owner.Selection);
                        AdornerLayer l = AdornerLayer.GetAdornerLayer(Owner);
                        pos = Canvas.GetMousePosition(c);
                        _adorner = new SelectionDraggingAdorner(Owner, pos);
                        _isDragging = true;
                        l.Add(_adorner);
                        c.StartTrackingMouse();
                        try {
                            DragDrop.DoDragDrop(Owner, obj, DragDropEffects.All);
                        } finally {
                            c.EndTrackingMouse();
                            l.Remove(_adorner);
                            _isDragging = false;
                        }
                    } else {
                        b = new DraggingMouseBehavior(Owner);
                    }
                }
                if (b != null) {
                    Owner.PushMouseBehavior(b);
                    b.BeginDragging(e);
                }
            } finally {
                Owner.UpdateKnob();
            }
        }

        //public override void PreRender(DrawingContext drawingContext) { }
        //public override void PostRender(DrawingContext drawingContext) { }

        public override void OnPreviewMouseMove(MouseEventArgs e, bool isFirstResponder) {
            _leftButtonDown &= (e.LeftButton == MouseButtonState.Pressed);
            if (isFirstResponder && _leftButtonDown) {
                // マウス左ボタンを押しながらマウスを移動した場合
                // ドラッグモードor範囲選択モードへ移行
                BeginDragging(e);
            }
            base.OnPreviewMouseMove(e, isFirstResponder);
        }

        public override void OnMouseUp(MouseButtonEventArgs e, bool isFirstResponder) {
            _isDragging = false;
            Canvas c = Owner;
            if (c == null) {
                return;
            }
            if (c.Sheet == null || c.ActiveLayer == null) {
                return;
            }

            Point pos = e.GetPosition(c);
            pos = c.InvMatrix.Transform(pos);
            Point2D p = c.Sheet.ToPoint2D(pos);
            Shape sh = c.ActiveLayer.GetTopShapeAt(p);
            if (Keyboard.Modifiers == ModifierKeys.None) {
                ShapeBecomeSelected(sh, false);
            }
            c.InvalidateVisual();
            _leftButtonDown = (e.LeftButton == MouseButtonState.Pressed);
        }

        public override void OnDragOver(DragEventArgs e, bool isFirstResponder) {
            base.OnDragOver(e, isFirstResponder);
            e.Effects = e.AllowedEffects & DragDropEffects.Copy;
            e.Handled = true;
        }

        public override void OnQueryContinueDrag(QueryContinueDragEventArgs e, bool isFirstResponder) {
            base.OnQueryContinueDrag(e, isFirstResponder);
            Canvas c = Owner;
            if (_adorner != null){
                Point p = Canvas.GetMousePosition(c);
                _adorner.SetPin(p);
            }
            e.Handled = true;
        }

        //public override void OnMouseLeave(MouseEventArgs e, bool isFirstResponder) { }

        //public override void OnMouseEnter(MouseEventArgs e, bool isFirstResponder) { }
    }


    public sealed class DraggingMouseBehavior : MouseBehavior {
        private Selection _draggingSelection;
        private Point _mouseStart;
        private Point2D _selectionStart;
        private Dictionary<Shape, Point2D> _shapeStart = new Dictionary<Shape, Point2D>();

        public override bool SelectionVisible { get { return false; } }

        public DraggingMouseBehavior(Canvas owner) : base(owner) { }

        public override void OnMouseDown(MouseButtonEventArgs e, bool isFirstResponder) {
            base.OnMouseDown(e, isFirstResponder);
        }

        private bool GetStartPoint(Shape shape, out Point2D point) {
            if (shape == null) {
                point = Point2D.Zero;
                return false;
            }
            if (_shapeStart == null) {
                point = shape.Pin;
                return false;
            }
            if (!_shapeStart.TryGetValue(shape, out point)) {
                point = shape.Pin;
                return false;
            }
            return true;
        }

        public override void BeginDragging(MouseEventArgs e) {
            try {
                Canvas c = Owner;
                if (c == null) {
                    return;
                }
                if (c.Sheet == null || c.ActiveLayer == null) {
                    return;
                }
                Point pos = e.GetPosition(c);
                pos = c.InvMatrix.Transform(pos);
                Point2D p = c.Sheet.ToPoint2D(pos);
                Shape sh = c.ActiveLayer.GetTopShapeAt(p);
                _mouseStart = pos;
                _selectionStart = p;
                if (sh != null && (Keyboard.Modifiers & ModifierKeys.Control) == 0) {
                    _shapeStart = null;
                    _draggingSelection = c.Selection;
                    if (_draggingSelection != null) {
                        _shapeStart = _draggingSelection.GetShapePoints();
                    }
                }
                c.StartTrackingMouse();
            } finally {
                Owner.UpdateKnob();
            }
        }

        //public override void PreRender(DrawingContext drawingContext) { }
        //public override void PostRender(DrawingContext drawingContext) { }

        private void DoDragging(MouseEventArgs e) {
            Canvas c = Owner;
            if (c == null) {
                return;
            }
            if (_draggingSelection == null || _draggingSelection.Count == 0) {
                c.PopMouseBehavior(this);
                return;
            }
            if (c.Sheet == null || c.ActiveLayer == null) {
                return;
            }
            Point pos = e.GetPosition(c);
            pos = c.InvMatrix.Transform(pos);
            Point2D p = c.Sheet.ToPoint2D(pos);
            GetGluePointArgs selected = null;
            // Altキーを押しながらドラッグしている場合にはグリッド線等を無視
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == 0) {
                foreach (Shape sh in _draggingSelection) {
                    Point2D pSh;
                    if (GetStartPoint(sh, out pSh)) {
                        GetGluePointArgs e2 = new GetGluePointArgs(sh, pSh, _selectionStart, p);
                        c.GetGluePointFor(e2);
                        if (e2.IsGlued) {
                            if (selected == null || (e2.Distance < selected.Distance)) {
                                selected = e2;
                            }
                        }
                    }
                }
            }
            Point2D diff = (selected != null) ? (selected.GlueTo - selected.MoveFrom) : (p - _selectionStart);
            if (_draggingSelection != null) {
                foreach (Shape sh in _draggingSelection) {
                    Point2D pBase;
                    if (GetStartPoint(sh, out pBase)) {
                        sh.Pin = pBase + diff;
                    }
                }
            }
            Owner.InvalidateVisual();
        }

        private void EndDragging(MouseEventArgs e) {
            try {
                Canvas c = Owner;
                if (c == null) {
                    return;
                }
                if (c.Sheet == null || c.ActiveLayer == null) {
                    return;
                }
                _shapeStart = null;
                _draggingSelection = null;
            } finally {
                Owner.UpdateKnob();
            }
        }

        public override void OnMouseMove(MouseEventArgs e, bool isFirstResponder) {
            DoDragging(e);
        }

        public override void OnMouseUp(MouseButtonEventArgs e, bool isFirstResponder) {
            EndDragging(e);
        }

        //public void OnMouseLeave(MouseEventArgs e, bool isFirstResponder) { }

        //public void OnMouseEnter(MouseEventArgs e, bool isFirstResponder) { }
    }

    public class RangeSelectingMouseBehavior : MouseBehavior {
        private Selection _selection;
        private Point _mouseStart;
        private Point2D _selectionStart;
        private Dictionary<Shape, Point2D> _shapeStart = new Dictionary<Shape, Point2D>();

        public override bool SelectionVisible { get { return true; } }

        public RangeSelectingMouseBehavior(Canvas owner) : base(owner) { }

        public override void OnMouseDown(MouseButtonEventArgs e, bool isFirstResponder) {
            base.OnMouseDown(e, isFirstResponder);
        }

        public override void BeginDragging(MouseEventArgs e) {
            try {
                Canvas c = Owner;
                if (c == null) {
                    return;
                }
                if (c.Sheet == null || c.ActiveLayer == null) {
                    return;
                }
                Point pos = e.GetPosition(c);
                pos = c.InvMatrix.Transform(pos);
                Point2D p = c.Sheet.ToPoint2D(pos);
                Shape sh = c.ActiveLayer.GetTopShapeAt(p);
                _mouseStart = pos;
                _selectionStart = p;
                if (sh != null && (Keyboard.Modifiers & ModifierKeys.Control) == 0) {
                    _shapeStart = null;
                    _selection = c.Selection;
                    if (_selection != null) {
                        _shapeStart = _selection.GetShapePoints();
                    }
                }
                c.StartTrackingMouse();
            } finally {
                Owner.UpdateKnob();
            }
        }

        //private static readonly Brush RANGE_BRUSH = new SolidColorBrush(Colors.LightGreen);
        private static Brush _rangeBrush = new SolidColorBrush(Colors.Transparent);

        /// <summary>
        /// マウスにより範囲選択する際の選択範囲の塗りつぶしに使用するBrushを指定する
        /// 初期値は透明(塗りつぶしなし)
        /// </summary>
        public static Brush RangeBrush { get { return _rangeBrush; } set { _rangeBrush = value; } }

        private static Pen _rangePen = new Pen(new SolidColorBrush(Colors.LimeGreen), 0.5);
        
        /// <summary>
        /// マウスにより範囲選択する際の選択範囲の枠を描画するPenを指定する
        /// 初期値はLimeGreenの0.5ptのペン
        /// </summary>
        public static Pen RangePen { get { return _rangePen; } set { _rangePen = value; } }

        public override void PreRender(DrawingContext drawingContext) {
            Canvas c = Owner;
            if (c == null) {
                return;
            }
            if (c.Sheet == null || c.ActiveLayer == null) {
                return;
            }
            Point pos = Mouse.GetPosition(c);
            pos = c.InvMatrix.Transform(pos);
            Rect r = new Rect(_mouseStart, pos);
            drawingContext.DrawRectangle(RangeBrush, RangePen, r);
        }

        //public override void PostRender(DrawingContext drawingContext) { }

        public override void OnMouseMove(MouseEventArgs e, bool isFirstResponder) {
            base.OnMouseMove(e, isFirstResponder);
            Canvas c = Owner;
            if (c != null) {
                c.InvalidateVisual();
            }
        }

        public override void OnMouseUp(MouseButtonEventArgs e, bool isFirstResponder) {
            base.OnMouseUp(e, isFirstResponder);
            Canvas c = Owner;
            if (c != null) {
                c.Selection.Clear();
                Point pos = e.GetPosition(c);
                pos = c.InvMatrix.Transform(pos);
                Point2D p = c.Sheet.ToPoint2D(pos);
                Rectangle2D r = new Rectangle2D(_selectionStart, p, true);
                //List<Shape> l = new List<Shape>();
                switch (SketchSettings.Settings.RangeSelectionStyle){
                    case RangeSelectionStyle.Full:
                        foreach (Shape sh in c.Sheet.Shapes) {
                            if (r.Contains(sh.GetBounds(Angle.Zero))) {
                                sh.Select(c);
                                //l.Add(sh);
                            }
                        }
                        break;
                    case RangeSelectionStyle.Partial:
                        foreach (Shape sh in c.Sheet.Shapes) {
                            if (r.Intersects(sh.GetBounds(Angle.Zero))) {
                                sh.Select(c);
                                //l.Add(sh);
                            }
                        }
                        break;
                }
                c.InvalidateVisual();
                c.PopMouseBehavior(this);
            }
        }

        //public override void OnMouseLeave(MouseEventArgs e, bool isFirstResponder) { }

        //public override void OnMouseEnter(MouseEventArgs e, bool isFirstResponder) { }
    }
}
