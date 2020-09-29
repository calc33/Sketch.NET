using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Linq;
using System.Text;

namespace Sketch {
    partial class Shape {

        private TransformGeometry[] _hitTestGeometries = null;

        #region 座標計算API
        public double ToDisplayOffsetX(Distance value) {
            return value.Px / Sheet.Scaling;
        }
        public double ToDisplayOffsetY(Distance value) {
            return value.Px / Sheet.Scaling;
        }

        public Size ToDisplaySize(Size2D value) {
            return new Size(ToDisplayOffsetX(value.Width), ToDisplayOffsetY(value.Height));
        }

        public Point ToDisplayPoint(Point2D value) {
            return new Point(ToDisplayOffsetX(value.X), ToDisplayOffsetY(value.Y));
        }

        public Distance FromDisplayOffset(double value) {
            return new Distance(value * Sheet.Scaling, LengthUnit.Pixels);
        }

        public Point2D FromDisplayPoint(Point value) {
            return new Point2D(FromDisplayOffset(value.X), FromDisplayOffset(value.Y));
        }
        #endregion

        private MatrixTransform GetMatrixTransform(Point temporaryDiff) {
            if (IsLine) {
                return null;
            }
            Point p = ToDisplayPoint(Pin);
            p.X += temporaryDiff.X;
            p.Y += temporaryDiff.Y;
            Point lp = ToDisplayPoint(LocPin);
            double sin = Angle.Sin();
            double cos = Angle.Cos();
            return new MatrixTransform(cos, sin, -sin, cos, p.X - (lp.X * cos - lp.Y * sin), p.Y - (lp.X * sin + lp.Y * cos));
        }

        private Matrix GetRotateMatrix() {
            Point p = ToDisplayPoint(Pin);
            Point lp = ToDisplayPoint(LocPin);
            double sin = Angle.Sin();
            double cos = Angle.Cos();
            return new Matrix(cos, sin, -sin, cos, p.X - (lp.X * cos - lp.Y * sin), p.Y - (lp.X * sin + lp.Y * cos));
        }

        private Matrix GetMatrix() {
            if (Parent != null) {
                Matrix m = Parent.GetMatrix();
                m.Append(GetRotateMatrix());
                return m;
            } else {
                Matrix m = GetRotateMatrix();
                return m;
            }
        }

        private Matrix GetRotateInvMatrix() {
            Point p = ToDisplayPoint(Pin);
            Point lp = ToDisplayPoint(LocPin);
            double sin = Angle.Sin();
            double cos = Angle.Cos();
            return new Matrix(cos, -sin, sin, cos, lp.X - (p.X * cos + p.Y * sin), lp.Y - (p.Y * cos - p.X * sin));
        }

        private Matrix GetInvMatrix() {
            if (Parent != null) {
                Matrix m = Parent.GetInvMatrix();
                m.Append(GetRotateInvMatrix());
                return m;
            } else {
                Matrix m = GetRotateInvMatrix();
                return m;
            }
        }

        private MatrixTransform GetMatrixTransform() {
            if (IsLine) {
                return null;
            }
            Point p = ToDisplayPoint(Pin);
            Point lp = ToDisplayPoint(LocPin);
            double sin = Angle.Sin();
            double cos = Angle.Cos();
            return new MatrixTransform(cos, sin, -sin, cos, p.X - (lp.X * cos - lp.Y * sin), p.Y - (lp.X * sin + lp.Y * cos));
        }
        private void RenderCore(DrawingContext drawingContext) {
            PathBuffer buffer = new PathBuffer(drawingContext);
            try {
                foreach (DrawingPath p in DrawingPaths) {
                    p.Render(buffer);
                }
            } finally {
                buffer.Commit();
            }
            _hitTestGeometries = buffer.GetPathGeometries();

            buffer = new PathBuffer(drawingContext);
            try {
                Rectangle2D r = DrawingPaths.BoundsRect;
                buffer.StartPath(ToDisplayPoint(r.TopLeft), new Pen(new SolidColorBrush(Colors.LightGreen), 0.5));
                buffer.LineTo(ToDisplayPoint(r.TopRight));
                buffer.LineTo(ToDisplayPoint(r.BottomRight));
                buffer.LineTo(ToDisplayPoint(r.BottomLeft));
                buffer.LineTo(ToDisplayPoint(r.TopLeft));
                buffer.LineTo(ToDisplayPoint(LocPin));
                buffer.StrokePath();
                //buffer.ClosePath(false, null);
            } finally {
                buffer.Commit();
            }
            foreach (Shape s in Shapes) {
                s.Render(drawingContext);
            }
        }
        public void Render(DrawingContext drawingContext, Point temporaryDiff) {
            MatrixTransform trns = GetMatrixTransform(temporaryDiff);
            if (trns != null) {
                drawingContext.PushTransform(trns);
            }
            try {
                RenderCore(drawingContext);
            } finally {
                if (trns != null) {
                    drawingContext.Pop();
                }
            }
        }
        public void Render(DrawingContext drawingContext) {
            MatrixTransform trns = GetMatrixTransform();
            if (trns != null) {
                drawingContext.PushTransform(trns);
            }
            try {
                RenderCore(drawingContext);
            } finally {
                if (trns != null) {
                    drawingContext.Pop();
                }
            }
        }

        private bool HitTestCore(Point2D point) {
            bool hasFill = false;
            foreach (TransformGeometry g in _hitTestGeometries) {
                foreach (PathFigure f in g.Geometry.Figures) {
                    if (f.IsFilled) {
                        hasFill = true;
                        break;
                    }
                }
            }
            Point p = ToDisplayPoint(point);
            Matrix m = GetInvMatrix();
            p = m.Transform(p);
            if (hasFill) {
                foreach (TransformGeometry g in _hitTestGeometries) {
                    Point lp = g.InvMatrix.Transform(p);
                    if (g.Geometry.FillContains(lp)) {
                        return true;
                    }
                }
            } else {
                Pen pen = new Pen(new SolidColorBrush(), 2.0);
                foreach (TransformGeometry g in _hitTestGeometries) {
                    Point lp = g.InvMatrix.Transform(p);
                    if (g.Geometry.StrokeContains(pen, lp)) {
                        return true;
                    }
                }
            }
            return false;
        }
        private HitTestResultBehavior HitTestResult(HitTestResult result) {
            return HitTestResultBehavior.Stop;
        }

    }
}
