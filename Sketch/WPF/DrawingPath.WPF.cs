using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Linq;
using System.Text;

namespace Sketch {
    public class TransformGeometry {
        private Matrix _matrix;
        private Matrix _invMatrix;
        private PathGeometry _geometry;
        public TransformGeometry(PathGeometry geometry) {
            _matrix = new Matrix(1, 0, 0, 1, 0, 0);
            _invMatrix = new Matrix(1, 0, 0, 1, 0, 0);
            _geometry = geometry;
        }
        public TransformGeometry(Matrix matrix, Matrix invMatrix, PathGeometry geometry) {
            _matrix = matrix;
            _invMatrix = invMatrix;
            _geometry = geometry;
        }

        /// <summary>
        /// 座標変換のマトリックス
        /// </summary>
        public Matrix Matrix { get { return _matrix; } }
        /// <summary>
        /// Matrixと逆方向の座標変換を行うためのマトリックス
        /// </summary>
        public Matrix InvMatrix { get { return _invMatrix; } }
        public PathGeometry Geometry { get { return _geometry; } }

        /// <summary>
        /// (centerX, centerY)を中心にangleだけ回転する座標変換を行うための
        /// MatrixとInvMatrixを生成する
        /// </summary>
        /// <param name="angle">回転する角度(単位:度)</param>
        /// <param name="centerX">回転の中心のX座標</param>
        /// <param name="centerY">回転の中心のX座標</param>
        public void SetRotateMatrix(double angle, double centerX, double centerY) {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            _matrix = new Matrix(cos, sin, -sin, cos, centerX * (1 - cos) - centerY * sin, centerX * sin + centerY * (1 - cos));
            _invMatrix = new Matrix(cos, -sin, sin, cos, centerX * (1 - cos) + centerY * sin, -centerX * sin + centerY * (1 - cos));
        }
    }
    public class PathBuffer {
        private DrawingContext _drawingContext;
        private bool _stroke;
        private Pen _strokePen;
        private Brush _fillBrush;
        private List<PathFigure> _figures;
        private PathFigure _current;
        private List<TransformGeometry> _geometries = new List<TransformGeometry>();
        private Point _startPoint;
        private List<Point> _polyLines = new List<Point>();
        private List<Point[]> _polyBeziers = new List<Point[]>();

        public PathBuffer(DrawingContext context) {
            _drawingContext = context;
            _figures = new List<PathFigure>();
            _startPoint = new Point(0, 0);
        }

        public void StartPath(Point point, Pen pen) {
            _startPoint = point;
            _strokePen = pen;
            _stroke = (_strokePen != null);
            PathFigure f = GetCurrentFigure(true);
            f.StartPoint = _startPoint;
        }

        public void ClosePath(bool fill, Brush brush) {
            _fillBrush = fill ? brush : null;
            Flush();
            PathFigure f = GetCurrentFigure(false);
            if (f != null) {
                f.IsClosed = true;
                f.IsFilled = fill;
            }
            Commit();
        }
        public void StrokePath() {
            PathFigure f = GetCurrentFigure(false);
            if (f != null) {
                f.IsClosed = false;
                f.IsFilled = false;
            }
            Commit();
        }

        public void LineTo(Point point) {
            FlushPolyBezier();
            _polyLines.Add(point);
        }

        public void BezierTo(Point point1, Point point2, Point point3) {
            FlushPolyLine();
            _polyBeziers.Add(new Point[] { point1, point2, point3 });
        }

        public void ArcTo(Point endPoint, Size radius, double rotatingAngle, bool isLargeArc, bool isClockwise) {
            Flush();
            PathFigure f = GetCurrentFigure(true);
            f.Segments.Add(new ArcSegment(endPoint, radius, rotatingAngle, isLargeArc,
                isClockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true));
        }

        public void DrawEllipse(Point center, Size radius, double rotatingAngle, Brush fillBrush, bool stroke, Pen strokePen) {
            Commit();
            EllipseGeometry g = new EllipseGeometry(center, radius.Width, radius.Height);

            Transform trns = null;
            if (rotatingAngle != 0) {
                trns = new RotateTransform(rotatingAngle, center.X, center.Y);
            }
            
            if (trns != null) {
                _drawingContext.PushTransform(trns);
            }
            try {
                _drawingContext.DrawEllipse(fillBrush, strokePen, center, radius.Width, radius.Height);
            } finally {
                if (trns != null) {
                    _drawingContext.Pop();
                }
            }
        }

        public PathFigure GetCurrentFigure(bool forceCreate) {
            if (forceCreate && _current == null) {
                _current = new PathFigure();
                _figures.Add(_current);
            }
            return _current;
        }

        private void FlushPolyLine() {
            if (_polyLines == null || _polyLines.Count == 0) {
                return;
            }
            PathFigure f = GetCurrentFigure(true);
            if (_polyLines.Count == 1) {
                f.Segments.Add(new LineSegment(_polyLines[0], _stroke));
            } else {
                f.Segments.Add(new PolyLineSegment(_polyLines, _stroke));
            }
            _polyLines.Clear();
        }

        private void FlushPolyBezier() {
            if (_polyBeziers == null || _polyBeziers.Count == 0) {
                return;
            }
            PathFigure f = GetCurrentFigure(true);
            if (_polyBeziers.Count == 1) {
                Point[] p = _polyBeziers[0];
                f.Segments.Add(new BezierSegment(p[0], p[1], p[2], _stroke));
            } else {
                List<Point> points = new List<Point>();
                foreach (Point[] pts in _polyBeziers) {
                    foreach (Point p in pts) {
                        points.Add(p);
                    }
                }
                f.Segments.Add(new PolyBezierSegment(points, _stroke));
            }
            _polyBeziers.Clear();
        }
        private void Flush() {
            FlushPolyBezier();
            FlushPolyLine();
        }

        public void Commit() {
            Flush();
            if (_figures != null && 0 < _figures.Count) {
                PathGeometry g = new PathGeometry(_figures);
                _geometries.Add(new TransformGeometry(g));
                _drawingContext.DrawGeometry(_fillBrush, _strokePen, g);
                _figures.Clear();
            }
            _current = null;
        }

        public TransformGeometry[] GetPathGeometries() {
            return _geometries.ToArray();
        }
    }

    partial class DrawingPath {
        public abstract void Render(PathBuffer buffer);
        //public virtual bool HitTest(Point2D point) {
        //    return true;
        //}
        //public abstract PathSegment GetPathSegment();
        public override void OnValueChanged(ValueChangedEventArgs e) {
            base.OnValueChanged(e);
            if (Shape != null) {
                Shape.DrawingPaths.InvalidateVisual();
            }
        }
    }

    partial class DrawingPathCollection {
        public void Render(DrawingContext drawingContext) {
            //drawingContext.DrawDrawing
        }
    }

    partial class StartPath {
        public override void Render(PathBuffer buffer) {
            Pen pen = null;
            if (Stroke) {
                pen = new Pen(new SolidColorBrush(LineColor), LineWidth.Px);
            }
            buffer.StartPath(Shape.ToDisplayPoint(Point), pen);
        }
    }

    partial class LineToPath {
        public override void Render(PathBuffer buffer) {
            buffer.LineTo(Shape.ToDisplayPoint(Point));
        }
    }

    partial class ArcToPath {
        public override void Render(PathBuffer buffer) {
            buffer.ArcTo(Shape.ToDisplayPoint(Point1), Shape.ToDisplaySize(Radius), RotatingAngle.Deg, IsLargeArc, IsClockwise);
        }
    }

    partial class BezierPath {
        public override void Render(PathBuffer buffer) {
            buffer.BezierTo(Shape.ToDisplayPoint(Point1), Shape.ToDisplayPoint(Point2), Shape.ToDisplayPoint(Point3));
        }
    }

    partial class ClosePath {
        public Brush FillBrush() {
            if (!Fill) {
                return null;
            }
            return new SolidColorBrush(FillColor);
        }

        public override void Render(PathBuffer buffer) {
            buffer.ClosePath(Fill, FillBrush());
        }
    }

    partial class StrokePath {
        public override void Render(PathBuffer buffer) {
            buffer.StrokePath();
        }
    }

    partial class ArrowPath {
        public override void Render(PathBuffer buffer) {
            Point st = new Point(Container.ToDisplayOffsetX(StartX), Container.ToDisplayOffsetY(StartY));
            Point ed = new Point(Container.ToDisplayOffsetX(EndX), Container.ToDisplayOffsetY(EndY));
            double dx = st.X - ed.X;
            double dy = st.Y - ed.Y;
            double l = ArrowSize.Px / Math.Sqrt(dx * dx + dy + dy);
            dx = dx * l;
            dy = dy * l;
            double a = 30.0 / 2 / Math.PI;
            Point p1 = new Point(dx * Math.Cos(a) - dy * Math.Sin(a) + st.X, dx * Math.Sin(a) + dy * Math.Cos(a) + st.Y);
            a = -a;
            Point p2 = new Point(dx * Math.Cos(a) - dy * Math.Sin(a) + st.X, dx * Math.Sin(a) + dy * Math.Cos(a) + st.Y);
            Point p3 = new Point(dx / 2 + st.X, dy / 2 + st.Y);
            Brush br = new SolidColorBrush(LineColor);
            Pen pen = new Pen(br, LineWidth.Px);
            buffer.StartPath(p1, pen);
            buffer.LineTo(st);
            buffer.LineTo(p2);
            buffer.LineTo(p3);
            buffer.ClosePath(true, br);
        }
    }
    partial class EllipsePath {
        public Brush FillBrush() {
            if (!Fill) {
                return null;
            }
            return new SolidColorBrush(FillColor);
        }
        public Pen StrokePen() {
            if (!Stroke) {
                return null;
            }
            return new Pen(new SolidColorBrush(LineColor), LineWidth.Px);
        }
        public override void Render(PathBuffer buffer) {
            buffer.DrawEllipse(Shape.ToDisplayPoint(Center), Shape.ToDisplaySize(Radius), RotatingAngle.Deg, FillBrush(), Stroke, StrokePen());
        }
    }
}
