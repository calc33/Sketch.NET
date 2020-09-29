using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
//using System.Threading;

namespace Sketch {
    public class GetGluePointArgs : EventArgs {
        private Shape _shape;
        private Point2D _moveFrom;
        private Point2D _moveTo;
        private bool _isGlued;
        private Point2D _glueTo;
        private Distance _distance;
        private string _glueFormulaX;
        private string _glueFormulaY;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="shapeBase">ドラッグ開始時のシェイプの位置</param>
        /// <param name="moveFrom">マウスのドラッグ開始位置</param>
        /// <param name="moveTo">マウスの現在の位置</param>
        public GetGluePointArgs(Shape shape, Point2D shapeBase, Point2D moveFrom, Point2D moveTo) {
            _shape = shape;
            _moveFrom = shapeBase;
            _moveTo = shapeBase + moveTo - moveFrom;
            _isGlued = false;
            _glueTo = _moveTo;
            _distance = Distance.Zero;
        }

        public Shape Shape { get { return _shape; } }
        public Point2D MoveFrom { get { return _moveFrom; } }
        public Point2D MoveTo { get { return _moveTo; } }
        public bool IsGlued { get { return _isGlued; } }
        public Point2D GlueTo { get { return _glueTo; } set { _glueTo = value; } }
        public Distance Distance { get { return _distance; } set { _distance = value; } }
        public string GlueFormulaX { get { return _glueFormulaX; } set { _glueFormulaX = value; } }
        public string GlueFormulaY { get { return _glueFormulaY; } set { _glueFormulaY = value; } }
    }

    public partial class Canvas : System.Windows.Controls.Canvas {
        private Matrix _baseMatrix;
        //private Matrix _controlMatrix;
        private Matrix _invMatrix;
        private IMouseBehavior _behavior;
        //private object _mouseOperationLock = new object();
        private IMouseBehavior Behavior {
            get {
                return _behavior;
            }
        }

        public void PushMouseBehavior(IMouseBehavior behavior) {
            if (behavior == null) {
                throw new ArgumentNullException("behavior");
            }
            behavior.SetPrior(_behavior);
            _behavior = behavior;
        }

        public void PopMouseBehavior(IMouseBehavior behavior) {
            if (behavior == null) {
                throw new ArgumentNullException("behavior");
            }
            if (_behavior != behavior) {
                throw new ArgumentException("behavior");
            }
            if (_behavior != null && _behavior == behavior) {
                _behavior = _behavior.GetPrior();
            }
            InvalidateVisual();
        }

        private WeakReference _selectionControl;
        public DraggingControl SelectionControl {
            get {
                return (_selectionControl != null && _selectionControl.IsAlive) ? _selectionControl.Target as DraggingControl : null;
            }
        }

        public void RequireSelectionControl() {
            if (_selectionControl == null) {
                _selectionControl = new WeakReference(new DraggingControl(this));
            } else {
                _selectionControl.Target = new DraggingControl(this);
            }
            DraggingControl c = SelectionControl;
            //c.
        }

        public void ReleaseSelectionControl() {

        }

        public DependencyObject GetVisualParent() {
            return VisualParent;
        }

        private static readonly TimeSpan SCROLL_TIMER = new TimeSpan(500000);
        private DispatcherTimer _trackingTimer = null;
        public event EventHandler MouseTrack;

        public Canvas()
            : base() {
            Background = new SolidColorBrush(Colors.White);
            //ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Auto);
            //ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Auto);
            ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Visible);
            ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Visible);
            _selectionControl = null;
            _trackingTimer = new DispatcherTimer(SCROLL_TIMER, DispatcherPriority.Normal, ExecuteTrackingTimer, Dispatcher);
            _trackingTimer.Stop();
            PushMouseBehavior(new BasicMouseBehavior(this));
            AllowDrop = true;
        }

        protected void UpdateVisibleRectCore() {
            double sc = Scaling;
            double dx = 0;
            double dy = 0;
            if (_sheet != null) {
                sc = sc * _sheet.Scaling;
                dx = _sheet.OffsetX.Px;
                dy = _sheet.OffsetY.Px;
            }
            Size2D s = new Size2D(ActualWidth * sc, ActualHeight * sc, LengthUnit.Pixels);
            double sin = RotatingAngle.Sin();
            double cos = RotatingAngle.Cos();
            _baseMatrix = new Matrix(sc * cos, sc * sin, -sc * sin, sc * cos, sc * dx, sc * dy);
            _invMatrix = new Matrix(cos / sc, -sin / sc, sin / sc, cos / sc, -dx / sc, -dy / sc);
            _isVisibleRectValid = true;
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

        internal void AddKnobControl(KnobControl control) {
            _knobControls.Add(control);
            //AddVisualChild(control);
            Children.Add(control);
        }

        internal void RemoveKnobControl(KnobControl control) {
            _knobControls.Remove(control);
            //RemoveVisualChild(control);
            Children.Remove(control);
        }

        internal void UpdateKnobPosition() {
            foreach (KnobControl c in _knobControls) {
                c.UpdatePosition();
            }
            InvalidateVisual();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
            InvalidateVisibleRect();
        }
        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            if (Sheet != null) {
                MatrixTransform tfm = new MatrixTransform(BaseMatrix);
                drawingContext.PushTransform(tfm);
                try {
                    Sheet.RenderBackground(drawingContext);
                    if (Behavior != null) {
                        Behavior.PreRender(drawingContext);
                    }
                    if (Selection != null) {
                        Selection.PreRender(drawingContext);
                    }
                    Sheet.Render(drawingContext);
                    if (Selection != null) {
                        Selection.PostRender(drawingContext);
                    }
                    if (Behavior != null) {
                        Behavior.PostRender(drawingContext);
                    }
                } finally {
                    drawingContext.Pop();
                }
            }
        }

        protected override bool IsEnabledCore {
            get {
                return base.IsEnabledCore;
                //retruen
            }
        }

        public event EventHandler<GetGluePointArgs> GetGluePoint;

        public void GetGluePointFor(GetGluePointArgs e) {
            if (GetGluePoint != null) {
                GetGluePoint(this, e);
            }
        }

        #region マウスイベント

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            if (Behavior != null) {
                Behavior.OnMouseDown(e, true);
            }
            if (Selection != null) {
                Selection.OnMouseDown(e, true);
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (Behavior != null) {
                Behavior.OnPreviewMouseMove(e, true);
            }
            if (Selection != null) {
                Selection.OnPreviewMouseMove(e, true);
            }
        }
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (Behavior != null) {
                Behavior.OnMouseMove(e, true);
            }
            if (Selection != null) {
                Selection.OnMouseMove(e, true);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            base.OnMouseUp(e);
            if (Behavior != null) {
                Behavior.OnMouseUp(e, true);
            }
            if (Selection != null) {
                Selection.OnMouseUp(e, true);
            }
            EndTrackingMouse();
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);
            if (Behavior != null) {
                Behavior.OnMouseLeave(e, true);
            }
            if (Selection != null) {
                Selection.OnMouseLeave(e, true);
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            base.OnMouseEnter(e);
            if (Behavior != null) {
                Behavior.OnMouseEnter(e, true);
            }
            if (Selection != null) {
                Selection.OnMouseEnter(e, true);
            }
        }

        protected override void OnDragEnter(DragEventArgs e) {
            base.OnDragEnter(e);
            if (Behavior != null) {
                Behavior.OnDragEnter(e, true);
            }
        }

        protected override void OnDragLeave(DragEventArgs e) {
            base.OnDragLeave(e);
            if (Behavior != null) {
                Behavior.OnDragLeave(e, true);
            }
        }

        protected override void OnDragOver(DragEventArgs e) {
            base.OnDragOver(e);
            if (Behavior != null) {
                Behavior.OnDragOver(e, true);
            }
        }
        protected override void OnDrop(DragEventArgs e) {
            base.OnDrop(e);
            if (Behavior != null) {
                Behavior.OnDrop(e, true);
            }
        }

        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e) {
            base.OnQueryContinueDrag(e);
            if (Behavior != null) {
                Behavior.OnQueryContinueDrag(e, true);
            }
        }

        private void StopCapture() {
            if (Mouse.Captured == this) {
                Mouse.Capture(null);
            }
            if (_trackingTimer.IsEnabled) {
                _trackingTimer.Stop();
            }
        }

        protected void MouseTrackCore(EventArgs e) {
            ScrollContentPresenter v = GetVisualParent() as ScrollContentPresenter;
            if (v == null) {
                return;
            }
            //Point p = Mouse.GetPosition(v);
            Point p = GetMousePosition(v);
            if (v.ActualWidth < p.X) {
                p.X -= v.ActualWidth;
            } else if (0 < p.X) {
                p.X = 0;
            }
            if (v.ActualHeight < p.Y) {
                p.Y -= v.ActualHeight;
            } else if (0 < p.Y) {
                p.Y = 0;
            }
            if (p.X == 0 && p.Y == 0) {
                return;
            }
            v.SetHorizontalOffset(v.HorizontalOffset + p.X);
            v.SetVerticalOffset(v.VerticalOffset + p.Y);
        }
        
        protected virtual void OnMouseTrack(EventArgs e) {
            // ドラッグ&ドロップ中はMouse.LeftButtonでマウスが押されているかどうか判定できない
            //if (Mouse.LeftButton == MouseButtonState.Released) {
            if (!IsLeftButtonDown()) {
                EndTrackingMouse();
                return;
            }
            MouseTrackCore(e);
            if (MouseTrack != null) {
                MouseTrack(this, e);
            }
        }

        private void ExecuteTrackingTimer(object sender, EventArgs e) {
            OnMouseTrack(e);
        }

        public bool StartTrackingMouse() {
            if (Mouse.Captured != this) {
                if (!Mouse.Capture(this)) {
                    if (_trackingTimer.IsEnabled) {
                        _trackingTimer.Stop();
                    }
                    return false;
                }
            }
            if (!_trackingTimer.IsEnabled) {
                _trackingTimer.Start();
            }
            return true;
        }

        public void EndTrackingMouse() {
            //MouseTrack = null;
            StopCapture();
            InvalidateVisual();
        }

        #endregion

        #region マウスの状態を取得(ドラッグ＆ドロップ中はWPF側でマウス情報を取れない問題を回避するため)
        [DllImport("user32.dll")]
        private static extern void GetCursorPos(out POINT pt);

        private struct POINT {
            public UInt32 X;
            public UInt32 Y;
        }

        /// <summary>
        /// マウスの現在位置を取得
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Point GetMousePosition(Visual v) {
            POINT p;
            GetCursorPos(out p);
            Point ret = new Point(p.X, p.Y);
            ret = v.PointFromScreen(ret);
            return ret;
        }

        /// <summary>
        /// マウスの右ボタンが押されているかどうかを取得
        /// </summary>
        /// <param name="nVirtKey"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        private const int VK_LBUTTON = 1;

        public static bool IsLeftButtonDown() {
            return (GetKeyState(VK_LBUTTON) & 0x80) != 0;
        }
        #endregion
    }
}
