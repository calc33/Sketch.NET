using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Sketch {
    partial interface IKnobInfo {
        Point ToPoint();
    }

    partial class KnobInfo {
        public Point ToPoint() {
            return Shape.ToDisplayPoint(ToPoint2D());
        }

        public void Render(DrawingContext drawingContext) { }
    }

    partial class BorderKnobInfo {
        public Point ToPoint() {
            Point p = Shape.ToDisplayPoint(ToPoint2D());
            return p;
        }
    }

    public partial class KnobControl: Thumb {
        private const double KNOB_SIZE = 8.0;
        private IKnobInfo _knobInfo;

        private static readonly Brush KNOB_BACKGROUND = new LinearGradientBrush(Color.FromRgb(128, 255, 128), Color.FromRgb(96, 192, 96), 45);
        private static readonly Pen KNOB_BORDER = new Pen(new SolidColorBrush(Color.FromRgb(0, 160, 0)), 1.0);
        public IKnobInfo KnobInfo { get { return _knobInfo; } set { _knobInfo = value; } }

        internal KnobControl(Canvas canvas, IKnobInfo knobInfo)
            : base() {
            if (canvas == null) {
                throw new ArgumentNullException();
            }
            if (knobInfo == null) {
                throw new ArgumentNullException();
            }
            _knobInfo = knobInfo;
            canvas.AddKnobControl(this);
            Width = KNOB_SIZE;
            Height = KNOB_SIZE;
            UpdatePosition();
            DragDelta += DoDragDelta;
            Template = null;
        }

        public void DoDragDelta(object sender, DragDeltaEventArgs e) {
            Point p = KnobInfo.ToPoint();
            p.X += e.HorizontalChange;
            p.Y += e.VerticalChange;
            e.Handled = true;
            KnobInfo.SetValueFromPoint(KnobInfo.Shape.FromDisplayPoint(p));
            p = KnobInfo.ToPoint();
            //p = (Parent as Canvas).ControlMatrix.Transform(p);
            Canvas.SetLeft(this, p.X - Width / 2);
            Canvas.SetTop(this, p.Y - Height / 2);
            Canvas c = Parent as Canvas;
            if (c != null) {
                c.UpdateKnobPosition();
            }
        }

        public void UpdatePosition() {
            Point p = _knobInfo.ToPoint();
            p.X -= KNOB_SIZE / 2;
            p.Y -= KNOB_SIZE / 2;
            //p = (Parent as Canvas).ControlMatrix.Transform(p);
            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Y);
        }

        protected override void OnRender(DrawingContext drawingContext) {
            double x = Width / 2;
            double y = Height / 2;
            drawingContext.DrawEllipse(KNOB_BACKGROUND, KNOB_BORDER, new Point(x, y), x, y);
        }

        protected void DoHideControl() {
            Visibility = Visibility.Hidden;
        }

        protected void DoDisposeControl() {
            (VisualParent as Canvas).RemoveKnobControl(this);
        }
    }
}
