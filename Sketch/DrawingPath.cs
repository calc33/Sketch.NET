using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;

namespace Sketch {
    //public enum PathSpec {
    //    /// <summary>
    //    /// 一本の直線
    //    /// </summary>
    //    Line,
    //    /// <summary>
    //    /// 円弧
    //    /// </summary>
    //    Arc,
    //    /// <summary>
    //    /// 複数の直線の組み合わせ
    //    /// </summary>
    //    PolyLine,
    //    /// <summary>
    //    /// 多角形
    //    /// </summary>
    //    Polygon,
    //    /// <summary>
    //    /// 楕円/円
    //    /// </summary>
    //    Oval,
    //    /// <summary>
    //    /// 扇形
    //    /// </summary>
    //    OvalFan,
    //    /// <summary>
    //    /// 複合図形
    //    /// </summary>
    //    Path,
    //}

    /// <summary>
    /// 描画情報の抽象クラス
    /// </summary>
    public abstract partial class DrawingPath: ExprComponent<Shape> {
        private WeakReference _prior;
        public Shape Shape { get { return Container; } }
        public DrawingPath(Shape container) : base(container) { }
        public DrawingPath Prior {
            get {
                if (_prior != null && !_prior.IsAlive) {
                    _prior = null;
                }
                return (_prior != null) ? _prior.Target as DrawingPath : null;
            }
            set {
                if (value == null) {
                    _prior = null;
                } else {
                    if (_prior == null || !_prior.IsAlive || _prior.Target != value) {
                        _prior = new WeakReference(value);
                    }
                }
            }
        }

        public abstract Distance EndX { get; }
        public abstract Distance EndY { get; }
        public Point2D EndPoint { get { return new Point2D(EndX, EndY); } }
        
        /// <summary>
        /// 図形に外接する矩形を返す
        /// </summary>
        /// <returns></returns>
        public abstract Rectangle2D GetContainingRect();
        public abstract Rectangle2D GetContainingRect(Point2D basePoint, Angle rotatingAngle);

        ///// <summary>
        ///// basePointとtoPointを結ぶ直線と直交する直線のうち、
        ///// この図形と接点を持ち、basePointからの距離が最大のものの距離を返す。
        ///// ただし、basePointからtoPointと逆方向の場合、距離はマイナス値とする。
        ///// この図形の中心点(basePoint)からtoPoint方向の外縁までの距離を求める用途に使用する
        ///// </summary>
        ///// <param name="basePoint"></param>
        ///// <param name="toPoint"></param>
        ///// <returns></returns>
        //public abstract Distance GetTangentDistance(Point2D basePoint, Point2D toPoint);
        //public abstract IKnobInfo[] GetKnobInfos();
    }

    public partial class StartPath: DrawingPath {
        private const int PROP_X = 0;
        private const int PROP_Y = 1;
        private const int PROP_STROKE = 2;
        private const int PROP_LINECOLOR = 3;
        private const int PROP_LINEWIDTH = 4;

        private PropertyDefCollection _builtinDefs = new PropertyDefCollection(new PropertyDef[] {
                new PropertyDef("X", typeof(Distance), "0cm"),
                new PropertyDef("Y", typeof(Distance), "0cm"),
                new PropertyDef("Stroke", typeof(bool), "Shape.Stroke"),
                new PropertyDef("LineColor", typeof(Color), "Shape.LineColor"),
                new PropertyDef("LineWidth", typeof(Distance), "Shape.LineWidth"),
            });
        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinDefs;
        }

        public StartPath(Shape container) : base(container) { }
        public StartPath(Shape container, Point2D startPoint)
            : base(container) {
            X = startPoint.X;
            Y = startPoint.Y;
        }

        public StartPath(Shape container, Distance startX, Distance startY)
            : base(container) {
            X = startX;
            Y = startY;
        }

        public StartPath(Shape container, string formulaX, string formulaY)
            : base(container) {
            FormulaX.SetFormula(formulaX);
            FormulaY.SetFormula(formulaY);
        }

        public FormulaProperty FormulaX { get { return Formula[PROP_X]; } }
        public FormulaProperty FormulaY { get { return Formula[PROP_Y]; } }
        public FormulaProperty FormulaStroke { get { return Formula[PROP_STROKE]; } }
        public FormulaProperty FormulaLineColor { get { return Formula[PROP_LINECOLOR]; } }
        public FormulaProperty FormulaLineWidth { get { return Formula[PROP_LINEWIDTH]; } }

        [FormulaIndex(PROP_X)]
        public Distance X {
            get { return Formula[PROP_X].DistanceValue; }
            set { Formula[PROP_X].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_Y)]
        public Distance Y {
            get { return Formula[PROP_Y].DistanceValue; }
            set { Formula[PROP_Y].SetValue(value, EditingLevel.EditByValue); }
        }

        public Point2D Point { get { return new Point2D(X, Y); } }

        [FormulaIndex(PROP_STROKE)]
        public bool Stroke {
            get { return Formula[PROP_STROKE].BooleanValue; }
            set { Formula[PROP_STROKE].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_LINECOLOR)]
        public Color LineColor {
            get { return Formula[PROP_LINECOLOR].ColorValue; }
            set { Formula[PROP_LINECOLOR].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_LINECOLOR)]
        public Distance LineWidth {
            get { return Formula[PROP_LINEWIDTH].DistanceValue; }
            set { Formula[PROP_LINEWIDTH].SetValue(value, EditingLevel.EditByValue); }
        }

        public override Distance EndX { get { return X; } }
        public override Distance EndY { get { return Y; } }

        public override Rectangle2D GetContainingRect() {
            return new Rectangle2D(X, Y, X, Y, false);
        }

        public override Rectangle2D GetContainingRect(Point2D basePoint, Angle rotatingAngle) {
            Point2D p = Point.Rotate(basePoint, rotatingAngle);
            return new Rectangle2D(p.X, p.Y, p.X, p.Y, false);
        }

        public override string ToString() {
            return string.Format("StartPath(X={0}, Y={1}, Stroke={2}, LineColor={3}, LineWidth={4})",
                FormulaX, FormulaY, FormulaStroke, FormulaLineColor, FormulaLineWidth);
        }
    }
    public partial class LineToPath: DrawingPath {
        public const int PROP_X = 0;
        public const int PROP_Y = 1;
        private PropertyDefCollection _builtinDefs = new PropertyDefCollection(new PropertyDef[] {
                new PropertyDef("X", typeof(Distance), "0cm"),
                new PropertyDef("Y", typeof(Distance), "0cm"),
            });
        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinDefs;
        }

        public LineToPath(Shape container) : base(container) { }
        public LineToPath(Shape container, Point2D point)
            : base(container) {
            X = point.X;
            Y = point.Y;
        }

        public LineToPath(Shape container, Distance x, Distance y)
            : base(container) {
            X = x;
            Y = y;
        }

        public LineToPath(Shape container, string formulaX, string formulaY)
            : base(container) {
            FormulaX.SetFormula(formulaX);
            FormulaY.SetFormula(formulaY);
        }

        public FormulaProperty FormulaX { get { return Formula[PROP_X]; } }
        public FormulaProperty FormulaY { get { return Formula[PROP_Y]; } }

        [FormulaIndex(PROP_X)]
        public Distance X {
            get { return Formula[PROP_X].DistanceValue; }
            set { Formula[PROP_X].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_Y)]
        public Distance Y {
            get { return Formula[PROP_Y].DistanceValue; }
            set { Formula[PROP_Y].SetValue(value, EditingLevel.EditByValue); }
        }

        public Point2D Point { get { return new Point2D(X, Y); } }

        public override Rectangle2D GetContainingRect() {
            return new Rectangle2D(X, Y, X, Y, false);
        }

        public override Rectangle2D GetContainingRect(Point2D basePoint, Angle rotatingAngle) {
            Point2D p = Point.Rotate(basePoint, rotatingAngle);
            return new Rectangle2D(p.X, p.Y, p.X, p.Y, false);
        }

        public override Distance EndX { get { return X; } }
        public override Distance EndY { get { return Y; } }

        public override string ToString() {
            return string.Format("LineTo(X={0}, Y={1})", FormulaX, FormulaY);
        }
    }

    public partial class ArcToPath: DrawingPath {
        private const int PROP_X1 = 0;
        private const int PROP_Y1 = 1;
        private const int PROP_RADIUSX = 2;
        private const int PROP_RADIUSY = 3;
        private const int PROP_ROTATINGANGLE = 4;
        private const int PROP_ISLARGEARC = 5;
        private const int PROP_ISCLOCKWISE = 6;
        private PropertyDefCollection _builtinDefs = new PropertyDefCollection(new PropertyDef[] {
                new PropertyDef("X1", typeof(Distance), "0cm"),
                new PropertyDef("Y1", typeof(Distance), "0cm"),
                new PropertyDef("RadiusX", typeof(Distance), "0cm"),
                new PropertyDef("RadiusY", typeof(Distance), "0cm"),
                new PropertyDef("RotatingAngle", typeof(Angle), "0deg"),
                new PropertyDef("IsLargeArc", typeof(bool), "false"),
                new PropertyDef("IsClockwise", typeof(bool), "true"),
            });
        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinDefs;
        }

        public ArcToPath(Shape container) : base(container) { }
        public ArcToPath(Shape container, Point2D point1, Size2D radius, Angle rotatingAngle, bool isLargeArc, bool isClockwise)
            : base(container) {
            X1 = point1.X;
            Y1 = point1.Y;
            RadiusX = radius.Width;
            RadiusY = radius.Height;
            RotatingAngle = rotatingAngle;
            IsLargeArc = isLargeArc;
            IsClockwise = isClockwise;
        }
        public ArcToPath(Shape container, Distance x1, Distance y1, Distance radiusX, Distance radiusY, Angle rotatingAngle, bool isLargeArc, bool isClockwise)
            : base(container) {
            X1 = x1;
            Y1 = y1;
            RadiusX = radiusX;
            RadiusY = radiusY;
            RotatingAngle = rotatingAngle;
            IsLargeArc = isLargeArc;
            IsClockwise = isClockwise;
        }
        public ArcToPath(Shape container, string formulaX1, string formulaY1, string formulaRadiusX, string formulaRadiusY, string formulaRotatingAngle, string formulaIsLargeArc, string formulaIsClockwise)
            : base(container) {
            FormulaX1.SetFormula(formulaX1);
            FormulaY1.SetFormula(formulaY1);
            FormulaRadiusX.SetFormula(formulaRadiusX);
            FormulaRadiusY.SetFormula(formulaRadiusY);
            FormulaRotatingAngle.SetFormula(formulaRotatingAngle);
            FormulaIsLargeArc.SetFormula(formulaIsLargeArc);
            FormulaIsClockwise.SetFormula(formulaIsClockwise);
        }

        public FormulaProperty FormulaX1 { get { return Formula[PROP_X1]; } }
        public FormulaProperty FormulaY1 { get { return Formula[PROP_Y1]; } }
        public FormulaProperty FormulaRadiusX { get { return Formula[PROP_RADIUSX]; } }
        public FormulaProperty FormulaRadiusY { get { return Formula[PROP_RADIUSY]; } }
        public FormulaProperty FormulaRotatingAngle { get { return Formula[PROP_ROTATINGANGLE]; } }
        public FormulaProperty FormulaIsLargeArc { get { return Formula[PROP_ISLARGEARC]; } }
        public FormulaProperty FormulaIsClockwise { get { return Formula[PROP_ISCLOCKWISE]; } }

        [FormulaIndex(PROP_X1)]
        public Distance X1 {
            get { return Formula[PROP_X1].DistanceValue; }
            set { Formula[PROP_X1].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_Y1)]
        public Distance Y1 {
            get { return Formula[PROP_Y1].DistanceValue; }
            set { Formula[PROP_Y1].SetValue(value, EditingLevel.EditByValue); }
        }

        public Point2D Point1 { get { return new Point2D(X1, Y1); } }

        [FormulaIndex(PROP_RADIUSX)]
        public Distance RadiusX {
            get { return Formula[PROP_RADIUSX].DistanceValue; }
            set { Formula[PROP_RADIUSX].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_RADIUSY)]
        public Distance RadiusY {
            get { return Formula[PROP_RADIUSY].DistanceValue; }
            set { Formula[PROP_RADIUSY].SetValue(value, EditingLevel.EditByValue); }
        }

        public Size2D Radius { get { return new Size2D(RadiusX, RadiusY); } }

        [FormulaIndex(PROP_ROTATINGANGLE)]
        public Angle RotatingAngle {
            get { return Formula[PROP_ROTATINGANGLE].AngleValue; }
            set { Formula[PROP_ROTATINGANGLE].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_ISLARGEARC)]
        public bool IsLargeArc {
            get { return Formula[PROP_ISLARGEARC].BooleanValue; }
            set { Formula[PROP_ISLARGEARC].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_ISCLOCKWISE)]
        public bool IsClockwise {
            get { return Formula[PROP_ISCLOCKWISE].BooleanValue; }
            set { Formula[PROP_ISCLOCKWISE].SetValue(value, EditingLevel.EditByValue); }
        }

        public override Distance EndX { get { return X1; } }
        public override Distance EndY { get { return Y1; } }

        private bool _isEllipseCenterValid = false;
        private Point2D _ellipseCenter;

        private void InvalidateEllipseCenter() {
            _isEllipseCenterValid = false;
        }

        public static Point2D GetEllipseCenter(Point2D startPoint, Point2D endPoint, Distance radiusX, Distance radiusY, Angle rotatingAngle, bool isLargeArc, bool isClockwise) {
            LengthUnit unit = endPoint.X.Unit;
            Point2D p0 = (-rotatingAngle).Rotate(startPoint);
            Point2D p1 = (-rotatingAngle).Rotate(endPoint);
            // Distance同士の演算は単位の次数を意識するため
            double x0 = p0.X[unit];
            double y0 = p0.Y[unit];
            double x1 = p1.X[unit];
            double y1 = p1.Y[unit];
            double dx = x1 - x0;
            double dy = y1 - y0;
            double rx = radiusX[unit];
            double ry = radiusY[unit];
            double k = Math.Sqrt(1 / (rx * rx * dy * dy + ry * ry * dx * dx) - 1 / (4 * rx * rx * ry * ry));

            double cxA = (x0 + x1) / 2;
            double cxB = -rx * rx * dy * k;
            double cyA = (y0 + y1) / 2;
            double cyB = ry * ry * dx * k;

            if (!isClockwise ^ isLargeArc) {
                cxB = -cxB;
                cyB = -cyB;
            }
            Point2D center = new Point2D(cxA + cxB, cyA + cyB, unit);
            return (rotatingAngle).Rotate(center);

        }
        private void UpdateEllipseCenter() {
            if (_isEllipseCenterValid) {
                return;
            }
            if (Prior == null) {
                _ellipseCenter = EndPoint;
                _isEllipseCenterValid = true;
                return;
            }
            _ellipseCenter = GetEllipseCenter(Prior.EndPoint, EndPoint, RadiusX, RadiusY, RotatingAngle, IsClockwise, IsLargeArc);
            _isEllipseCenterValid = true;
        }

        /// <summary>
        /// ArcToのパラメータから求められる楕円の中心を返す
        /// </summary>
        /// <returns></returns>
        public Point2D EllipseCenter {
            get {
                UpdateEllipseCenter();
                return _ellipseCenter;
            }
        }

        public override void OnValueChanged(ValueChangedEventArgs e) {
            base.OnValueChanged(e);
            InvalidateEllipseCenter();
        }

        private static readonly Angle DEGREE_180 = new Angle(180, AngleUnit.Degree);
        private static readonly Angle DEGREE_360 = new Angle(360, AngleUnit.Degree);

        private static Angle[] GetBoundaryAngle(Point2D center, Distance radiusX, Distance radiusY, Angle rotatingAngle) {
            LengthUnit unit = radiusX.Unit;
            double rx = radiusX[unit];
            double ry = radiusY[unit];
            double cos = rotatingAngle.Cos();
            double sin = rotatingAngle.Sin();
            double rx2 = rx * rx;
            double ry2 = ry * ry;
            double cos2 = cos * cos;
            double sin2 = sin * sin;
            List<Angle> l = new List<Angle>(12);
            l.Add(new Angle(Math.Asin(ry * sin / Math.Sqrt(rx2 * cos2 + ry2 * sin2)) * 180.0 / Math.PI, AngleUnit.Degree).Normalize(false));
            l.Add(new Angle(Math.Asin(ry * cos / Math.Sqrt(rx2 * sin2 + ry2 * cos2)) * 180.0 / Math.PI, AngleUnit.Degree).Normalize(false));
            l.Add(new Angle(Math.Asin(-ry * sin / Math.Sqrt(rx2 * cos2 + ry2 * sin2)) * 180.0 / Math.PI, AngleUnit.Degree).Normalize(false));
            l.Add(new Angle(Math.Asin(-ry * cos / Math.Sqrt(rx2 * sin2 + ry2 * cos2)) * 180.0 / Math.PI, AngleUnit.Degree).Normalize(false));
            l.Add((l[0] + DEGREE_180).Normalize(false));
            l.Add((l[1] + DEGREE_180).Normalize(false));
            l.Add((l[2] + DEGREE_180).Normalize(false));
            l.Add((l[3] + DEGREE_180).Normalize(false));
            l.Sort();
            int n = l.Count;
            for (int i = 0; i < n; i++) {
                l.Add(l[i] + DEGREE_360);
            }
            l.Insert(0, l[n - 1] - DEGREE_360);
            return l.ToArray();
        }

        private static Angle GetAngleParam(Point2D point, Point2D center, Distance radiusX, Distance radiusY, Angle rotatingAngle) {
            Point2D p0 = center;
            Point2D p = (-rotatingAngle).Rotate(point - p0);
            double x = p.X / radiusX;
            double y = p.Y / radiusY;
            double t = Math.Asin(y / (x * x + y * y));
            Angle ret = new Angle(t, AngleUnit.Radian);
            if (x < 0) {
                ret = DEGREE_180 - ret;
            }
            ret = ret.Normalize(false);
            return ret;
        }

        private Angle[] GetBoundaryAngle(Angle angle) {
            return GetBoundaryAngle(EllipseCenter, RadiusX, RadiusY, RotatingAngle + angle);
        }

        private Angle[] GetBoundaryAngle() {
            return GetBoundaryAngle(EllipseCenter, RadiusX, RadiusY, RotatingAngle);
        }

        private Angle GetAngleParam(Point2D point) {
            return GetAngleParam(point, EllipseCenter, RadiusX, RadiusY, RotatingAngle);
        }

        private static Point2D GetPointFromAngleParam(Angle angle, Point2D center, Distance radiusX, Distance radiusY, Angle rotatingAngle) {
            Point2D p = new Point2D(radiusX * angle.Cos(), radiusY * angle.Sin());
            return rotatingAngle.Rotate(p) + center;
        }

        private Point2D GetPointFromAngleParam(Angle angle) {
            return GetPointFromAngleParam(angle, EllipseCenter, RadiusX, RadiusY, RotatingAngle);
        }

        public static Rectangle2D GetContainingRect(Point2D startPoint, Point2D endPoint, Distance radiusX, Distance radiusY, Angle rotatingAngle, bool isLargeArc, bool isClockwise) {
            List<Point2D> l = new List<Point2D>();
            l.Add(startPoint);
            l.Add(endPoint);
            Point2D center = GetEllipseCenter(startPoint, endPoint, radiusX, radiusY, rotatingAngle, isLargeArc, isClockwise);
            Angle a0;
            Angle a1;
            if (isClockwise) {
                a0 = GetAngleParam(startPoint, center, radiusX, radiusY, rotatingAngle);
                a1 = GetAngleParam(endPoint, center, radiusX, radiusY, rotatingAngle);
            } else {
                a0 = GetAngleParam(endPoint, center, radiusX, radiusY, rotatingAngle);
                a1 = GetAngleParam(startPoint, center, radiusX, radiusY, rotatingAngle);
            }
            if (a1 < a0) {
                a1 += DEGREE_360;
            }
            foreach (Angle a in GetBoundaryAngle(center, radiusX, radiusX, rotatingAngle)) {
                if (a0 < a && a < a1) {
                    Point2D p = GetPointFromAngleParam(a, center, radiusX, radiusY, rotatingAngle);
                    l.Add(p);
                }
            }
            return Rectangle2D.GetContainingRect(l);
        }

        /// <summary>
        /// 図形に外接する矩形を取得
        /// </summary>
        /// <returns></returns>
        public override Rectangle2D GetContainingRect() {
            if (Prior == null) {
                return new Rectangle2D(EndPoint, EndPoint, false);
            }
            List<Point2D> l = new List<Point2D>();
            l.Add(Prior.EndPoint);
            l.Add(EndPoint);

            Angle a0;
            Angle a1;
            if (IsClockwise) {
                a0 = GetAngleParam(Prior.EndPoint);
                a1 = GetAngleParam(EndPoint);
            } else {
                a0 = GetAngleParam(EndPoint);
                a1 = GetAngleParam(Prior.EndPoint);
            }
            if (a1 < a0) {
                a1 += DEGREE_360;
            }
            foreach (Angle a in GetBoundaryAngle()) {
                if (a0 < a && a < a1) {
                    Point2D p = GetPointFromAngleParam(a);
                    l.Add(p);
                }
            }
            return Rectangle2D.GetContainingRect(l);
        }

        /// <summary>
        /// 回転した図形に外接する矩形を取得
        /// </summary>
        /// <param name="rotatingAngle"></param>
        /// <returns></returns>
        public override Rectangle2D GetContainingRect(Point2D basePoint, Angle rotatingAngle) {
            if (Prior == null) {
                return new Rectangle2D(EndPoint, EndPoint, false);
            }
            List<Point2D> l = new List<Point2D>();
            l.Add(Prior.EndPoint);
            l.Add(EndPoint);

            Angle a0;
            Angle a1;
            if (IsClockwise) {
                a0 = GetAngleParam(Prior.EndPoint);
                a1 = GetAngleParam(EndPoint);
            } else {
                a0 = GetAngleParam(EndPoint);
                a1 = GetAngleParam(Prior.EndPoint);
            }
            if (a1 < a0) {
                a1 += DEGREE_360;
            }
            foreach (Angle a in GetBoundaryAngle(rotatingAngle)) {
                if (a0 < a && a < a1) {
                    Point2D p = GetPointFromAngleParam(a);
                    l.Add(p);
                }
            }
            for (int i = 0; i < l.Count; i++) {
                l[i] = l[i].Rotate(basePoint, rotatingAngle);
            }
            return Rectangle2D.GetContainingRect(l);
        }

        public override string ToString() {
            return string.Format("ArcTo(X1={0}, Y1={1}, RotatingAngle={2}, RadiusX={3}, RadiusY={4}, IsLargeArc={5}, IsClockwise={6})",
                FormulaX1, FormulaY1, FormulaRotatingAngle, FormulaRadiusX, FormulaRadiusY, FormulaIsLargeArc, FormulaIsClockwise);
        }
    }
    public partial class BezierPath: DrawingPath {
        private const int PROP_X1 = 0;
        private const int PROP_Y1 = 1;
        private const int PROP_X2 = 2;
        private const int PROP_Y2 = 3;
        private const int PROP_X3 = 4;
        private const int PROP_Y3 = 5;
        private PropertyDefCollection _builtinDefs = new PropertyDefCollection(new PropertyDef[] {
                new PropertyDef("X1", typeof(Distance), "0cm"),
                new PropertyDef("Y1", typeof(Distance), "0cm"),
                new PropertyDef("X2", typeof(Distance), "0cm"),
                new PropertyDef("Y2", typeof(Distance), "0cm"),
                new PropertyDef("X3", typeof(Distance), "0cm"),
                new PropertyDef("Y3", typeof(Distance), "0cm"),
            });
        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinDefs;
        }

        public BezierPath(Shape container) : base(container) { }
        public BezierPath(Shape container, Point2D point1, Point2D point2, Point2D point3)
            : base(container) {
            X1 = point1.X;
            Y1 = point1.Y;
            X2 = point2.X;
            Y2 = point2.Y;
            X3 = point3.X;
            Y3 = point3.Y;
        }

        public BezierPath(Shape container, Distance x1, Distance y1, Distance x2, Distance y2, Distance x3, Distance y3)
            : base(container) {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            X3 = x3;
            Y3 = y3;
        }

        public BezierPath(Shape container, string formulaX1, string formulaY1, string formulaX2, string formulaY2, string formulaX3, string formulaY3)
            : base(container) {
            FormulaX1.SetFormula(formulaX1);
            FormulaY1.SetFormula(formulaY1);
            FormulaX2.SetFormula(formulaX2);
            FormulaY2.SetFormula(formulaY2);
            FormulaX3.SetFormula(formulaX3);
            FormulaY3.SetFormula(formulaY3);
        }

        public FormulaProperty FormulaX1 { get { return Formula[PROP_X1]; } }
        public FormulaProperty FormulaY1 { get { return Formula[PROP_Y1]; } }
        public FormulaProperty FormulaX2 { get { return Formula[PROP_X2]; } }
        public FormulaProperty FormulaY2 { get { return Formula[PROP_Y2]; } }
        public FormulaProperty FormulaX3 { get { return Formula[PROP_X3]; } }
        public FormulaProperty FormulaY3 { get { return Formula[PROP_Y3]; } }

        [FormulaIndex(PROP_X1)]
        public Distance X1 {
            get { return Formula[PROP_X1].DistanceValue; }
            set { Formula[PROP_X1].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_Y1)]
        public Distance Y1 {
            get { return Formula[PROP_Y1].DistanceValue; }
            set { Formula[PROP_Y1].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_X2)]
        public Distance X2 {
            get { return Formula[PROP_X2].DistanceValue; }
            set { Formula[PROP_X2].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_Y2)]
        public Distance Y2 {
            get { return Formula[PROP_Y2].DistanceValue; }
            set { Formula[PROP_Y2].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_X3)]
        public Distance X3 {
            get { return Formula[PROP_X3].DistanceValue; }
            set { Formula[PROP_X3].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_Y3)]
        public Distance Y3 {
            get { return Formula[PROP_Y3].DistanceValue; }
            set { Formula[PROP_Y3].SetValue(value, EditingLevel.EditByValue); }
        }

        public Point2D Point1 { get { return new Point2D(X1, Y1); } }
        public Point2D Point2 { get { return new Point2D(X2, Y2); } }
        public Point2D Point3 { get { return new Point2D(X3, Y3); } }

        public override Distance EndX { get { return X3; } }
        public override Distance EndY { get { return Y3; } }

        public override Rectangle2D GetContainingRect() {
            throw new NotImplementedException();
        }

        public override Rectangle2D GetContainingRect(Point2D basePoint, Angle rotatingAngle) {
            throw new NotImplementedException();
        }

        public override string ToString() {
            return string.Format("BezierTo(X1={0}, Y1={1}, X2={2}, Y2={3}, X3={4}, Y3={5})",
                FormulaX1, FormulaY1, FormulaX2, FormulaY2, FormulaX3, FormulaY3);
        }
    }
    public partial class ClosePath: DrawingPath {
        //private const int PROP_STROKE = 0;
        //private const int PROP_LINECOLOR = 1;
        //private const int PROP_LINEWIDTH = 2;
        //private const int PROP_FILL = 3;
        //private const int PROP_FILLCOLOR = 4;
        private Distance _endX;
        private Distance _endY;
        private const int PROP_FILL = 0;
        private const int PROP_FILLCOLOR = 1;
        private PropertyDefCollection _builtinDefs = new PropertyDefCollection(new PropertyDef[] {
                //new PropertyDef("Stroke", typeof(bool), "Shape.Stroke"),
                //new PropertyDef("LineColor", typeof(Color), "Shape.LineColor"),
                //new PropertyDef("LineWidth", typeof(Distance), "Shape.LineWidth"),
                new PropertyDef("Fill", typeof(bool), "Shape.Fill"),
                new PropertyDef("FillColor", typeof(Color), "Shape.FillColor"),
            });
        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinDefs;
        }

        public ClosePath(Shape container)
            : base(container) {
            StartPath st = null;
            foreach (DrawingPath p in container.DrawingPaths) {
                if (p is StartPath) {
                    st = (StartPath)p;
                }
            }
            if (st != null) {
                _endX = st.X;
                _endY = st.Y;
            } else {
                _endX = Distance.Zero;
                _endY = Distance.Zero;
            }
        }

        //public FormulaProperty FormulaStroke { get { return Formula[PROP_STROKE]; } }
        //public FormulaProperty FormulaLineColor { get { return Formula[PROP_LINECOLOR]; } }
        //public FormulaProperty FormulaLineWidth { get { return Formula[PROP_LINEWIDTH]; } }
        public FormulaProperty FormulaFill { get { return Formula[PROP_FILL]; } }
        public FormulaProperty FormulaFillColor { get { return Formula[PROP_FILLCOLOR]; } }

        //public bool Stroke {
        //    get { return Formula[PROP_STROKE].BooleanValue; }
        //    set { Formula[PROP_STROKE].SetValue(value, EditingLevel.EditByValue); }
        //}
        //public Color LineColor {
        //    get { return Formula[PROP_LINECOLOR].ColorValue; }
        //    set { Formula[PROP_LINECOLOR].SetValue(value, EditingLevel.EditByValue); }
        //}
        //public Distance LineWidth {
        //    get { return Formula[PROP_LINEWIDTH].DistanceValue; }
        //    set { Formula[PROP_LINEWIDTH].SetValue(value, EditingLevel.EditByValue); }
        //}
        public bool Fill {
            get { return Formula[PROP_FILL].BooleanValue; }
            set { Formula[PROP_FILL].SetValue(value, EditingLevel.EditByValue); }
        }
        public Color FillColor {
            get { return Formula[PROP_FILLCOLOR].ColorValue; }
            set { Formula[PROP_FILLCOLOR].SetValue(value, EditingLevel.EditByValue); }
        }

        public override Distance EndX { get { return _endX; } }
        public override Distance EndY { get { return _endY; } }

        public override Rectangle2D GetContainingRect() {
            Distance x0 = (Prior != null) ? Prior.EndX : EndX;
            Distance y0 = (Prior != null) ? Prior.EndY : EndY;
            Distance x1 = EndX;
            Distance y1 = EndY;
            return new Rectangle2D(x0, y0, x1, y1, true);
        }

        public override Rectangle2D GetContainingRect(Point2D basePoint, Angle rotatingAngle) {
            Point2D p1 = EndPoint.Rotate(basePoint, rotatingAngle);
            Point2D p0 = Prior != null ? Prior.EndPoint.Rotate(basePoint, rotatingAngle) : p1;
            return new Rectangle2D(p0.X, p0.Y, p1.X, p1.Y, false);
        }

        public override string ToString() {
            return string.Format("ClosePath(Fill={0}, FillColor={1})", FormulaFill, FormulaFillColor);
        }
    }
    public partial class StrokePath: DrawingPath {
        private PropertyDefCollection _builtinDefs = new PropertyDefCollection(new PropertyDef[0]);
        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinDefs;
        }

        public StrokePath(Shape container) : base(container) { }

        public override Distance EndX { get { return Distance.Zero; } }
        public override Distance EndY { get { return Distance.Zero; } }

        public override Rectangle2D GetContainingRect() {
            return new Rectangle2D(Prior.EndX, Prior.EndY, Prior.EndX, Prior.EndY, false);
        }

        public override Rectangle2D GetContainingRect(Point2D basePoint, Angle rotatingAngle) {
            Point2D p = Prior.EndPoint.Rotate(basePoint, rotatingAngle);
            return new Rectangle2D(p.X, p.Y, p.X, p.Y, false);
        }
        public override string ToString() {
            return "StrokePath()";
        }
    }

    public enum ArrowStyle {
        None = 0,
        Line = 1,
        Arrow = 2,
        Triangle = 3
    }
    public partial class ArrowPath: DrawingPath {
        private const int PROP_STARTX = 0;
        private const int PROP_STARTY = 1;
        private const int PROP_ARROWTOX = 2;
        private const int PROP_ARROWTOY = 3;
        private const int PROP_ARROWSIZE = 4;
        private const int PROP_ARROWSTYLE = 5;
        private const int PROP_LINECOLOR = 6;
        private const int PROP_LINEWIDTH = 7;

        private PropertyDefCollection _builtinDefs = new PropertyDefCollection(new PropertyDef[] {
                new PropertyDef("StartX", typeof(Distance), "0cm"),
                new PropertyDef("StartY", typeof(Distance), "0cm"),
                new PropertyDef("ArrowToX", typeof(Distance), "1cm"),
                new PropertyDef("ArrowToY", typeof(Distance), "1cm"),
                new PropertyDef("ArrowSize", typeof(Distance), "5px"),
                new PropertyDef("Style", typeof(ArrowStyle), "Shape.Stroke"),
                new PropertyDef("LineColor", typeof(Color), "Shape.LineColor"),
                new PropertyDef("LineWidth", typeof(Distance), "Shape.LineWidth"),
            });
        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinDefs;
        }

        public ArrowPath(Shape container) : base(container) { }
        public ArrowPath(Shape container, Point2D start, Point2D arrowTo, Distance arrowSize, ArrowStyle style)
            : base(container) {
            StartX = start.X;
            StartY = start.Y;
            ArrowToX = arrowTo.Y;
            ArrowToY = arrowTo.Y;
            ArrowSize = arrowSize;
            ArrowStyle = style;
        }

        public FormulaProperty FormulaStartX { get { return Formula[PROP_STARTX]; } }
        public FormulaProperty FormulaStartY { get { return Formula[PROP_STARTY]; } }
        public FormulaProperty FormulaArrowToX { get { return Formula[PROP_ARROWTOX]; } }
        public FormulaProperty FormulaArrowToY { get { return Formula[PROP_ARROWTOY]; } }
        public FormulaProperty FormulaArrowSize { get { return Formula[PROP_ARROWSIZE]; } }
        public FormulaProperty FormulaArrowStyle { get { return Formula[PROP_ARROWSTYLE]; } }
        public FormulaProperty FormulaLineColor { get { return Formula[PROP_LINECOLOR]; } }
        public FormulaProperty FormulaLineWidth { get { return Formula[PROP_LINEWIDTH]; } }

        [FormulaIndex(PROP_STARTX)]
        public Distance StartX {
            get { return Formula[PROP_STARTX].DistanceValue; }
            set { Formula[PROP_STARTX].SetValue(value, EditingLevel.EditByValue); }
        }
        
        [FormulaIndex(PROP_STARTY)]
        public Distance StartY {
            get { return Formula[PROP_STARTY].DistanceValue; }
            set { Formula[PROP_STARTY].SetValue(value, EditingLevel.EditByValue); }
        }
        
        [FormulaIndex(PROP_ARROWTOX)]
        public Distance ArrowToX {
            get { return Formula[PROP_ARROWTOX].DistanceValue; }
            set { Formula[PROP_ARROWTOX].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_ARROWTOY)]
        public Distance ArrowToY {
            get { return Formula[PROP_ARROWTOY].DistanceValue; }
            set { Formula[PROP_ARROWTOY].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_ARROWSIZE)]
        public Distance ArrowSize {
            get { return Formula[PROP_ARROWSIZE].DistanceValue; }
            set { Formula[PROP_ARROWSIZE].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_ARROWSTYLE)]
        public ArrowStyle ArrowStyle {
            get { return (ArrowStyle)Formula[PROP_ARROWSTYLE].Int16Value; }
            set { Formula[PROP_ARROWSTYLE].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LINECOLOR)]
        public Color LineColor {
            get { return Formula[PROP_LINECOLOR].ColorValue; }
            set { Formula[PROP_LINECOLOR].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LINEWIDTH)]
        public Distance LineWidth {
            get { return Formula[PROP_LINEWIDTH].DistanceValue; }
            set { Formula[PROP_LINEWIDTH].SetValue(value, EditingLevel.EditByValue); }
        }

        public override Distance EndX {
            get { return StartX; }
        }

        public override Distance EndY {
            get { return StartY; }
        }

        public override Rectangle2D GetContainingRect() {
            return new Rectangle2D(StartX, StartY, StartX, StartY, false);
        }

        public override Rectangle2D GetContainingRect(Point2D basePoint, Angle rotatingAngle) {
            Point2D p = new Point2D(StartX, StartY).Rotate(basePoint, rotatingAngle);
            return new Rectangle2D(p.X, p.Y, p.X, p.Y, false);
        }

        public override string ToString() {
            return string.Format("ArrowPath(Start=({0}, {1}), ArrowTo=({2}, {3}), ArrowSize={4}, ArrowStyle={5}, LineColor={6}, LineWidth={7})",
                FormulaStartX, FormulaStartY, FormulaArrowToX, FormulaArrowToY, FormulaArrowSize, FormulaArrowStyle,
                FormulaLineColor, FormulaLineWidth);
        }
    }

    public partial class EllipsePath: DrawingPath {
        private const int PROP_CENTERX = 0;
        private const int PROP_CENTERY = 1;
        private const int PROP_RADIUSX = 2;
        private const int PROP_RADIUSY = 3;
        private const int PROP_ROTATINGANGLE = 4;
        private const int PROP_STROKE = 5;
        private const int PROP_LINECOLOR = 6;
        private const int PROP_LINEWIDTH = 7;
        private const int PROP_FILL = 8;
        private const int PROP_FILLCOLOR = 9;

        private PropertyDefCollection _builtinDefs = new PropertyDefCollection(new PropertyDef[] {
                new PropertyDef("CenterX", typeof(Distance), "0cm"),
                new PropertyDef("CenterY", typeof(Distance), "0cm"),
                new PropertyDef("RadiusX", typeof(Distance), "0cm"),
                new PropertyDef("RadiusY", typeof(Distance), "0cm"),
                new PropertyDef("RotatingAngle", typeof(Angle), "0deg"),
                new PropertyDef("Stroke", typeof(bool), "Shape.Stroke"),
                new PropertyDef("LineColor", typeof(Color), "Shape.LineColor"),
                new PropertyDef("LineWidth", typeof(Distance), "Shape.LineWidth"),
                new PropertyDef("Fill", typeof(bool), "Shape.Fill"),
                new PropertyDef("FillColor", typeof(Color), "Shape.FillColor"),
            });
        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinDefs;
        }

        public EllipsePath(Shape container) : base(container) { }
        public EllipsePath(Shape container, Point2D center, Size2D radius, Angle rotatingAngle)
            : base(container) {
            CenterX = center.X;
            CenterY = center.Y;
            RadiusX = radius.Width;
            RadiusY = radius.Height;
            RotatingAngle = rotatingAngle;
        }
        public EllipsePath(Shape container, Distance centerX, Distance centerY, Distance radiusX, Distance radiusY, Angle rotatingAngle)
            : base(container) {
            CenterX = centerX;
            CenterY = centerY;
            RadiusX = radiusX;
            RadiusY = radiusY;
            RotatingAngle = rotatingAngle;
        }
        public EllipsePath(Shape container, string formulaCenterX, string formulaCenterY, string formulaRadiusX, string formulaRadiusY, string formulaRotatingAngle)
            : base(container) {
            FormulaCenterX.SetFormula(formulaCenterX);
            FormulaCenterY.SetFormula(formulaCenterY);
            FormulaRadiusX.SetFormula(formulaRadiusX);
            FormulaRadiusY.SetFormula(formulaRadiusY);
            FormulaRotatingAngle.SetFormula(formulaRotatingAngle);
        }

        public FormulaProperty FormulaCenterX { get { return Formula[PROP_CENTERX]; } }
        public FormulaProperty FormulaCenterY { get { return Formula[PROP_CENTERY]; } }
        public FormulaProperty FormulaRadiusX { get { return Formula[PROP_RADIUSX]; } }
        public FormulaProperty FormulaRadiusY { get { return Formula[PROP_RADIUSY]; } }
        public FormulaProperty FormulaRotatingAngle { get { return Formula[PROP_ROTATINGANGLE]; } }
        public FormulaProperty FormulaStroke { get { return Formula[PROP_STROKE]; } }
        public FormulaProperty FormulaLineColor { get { return Formula[PROP_LINECOLOR]; } }
        public FormulaProperty FormulaLineWidth { get { return Formula[PROP_LINEWIDTH]; } }
        public FormulaProperty FormulaFill { get { return Formula[PROP_FILL]; } }
        public FormulaProperty FormulaFillColor { get { return Formula[PROP_FILLCOLOR]; } }

        [FormulaIndex(PROP_CENTERX)]
        public Distance CenterX {
            get { return Formula[PROP_CENTERX].DistanceValue; }
            set { Formula[PROP_CENTERX].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_CENTERY)]
        public Distance CenterY {
            get { return Formula[PROP_CENTERY].DistanceValue; }
            set { Formula[PROP_CENTERY].SetValue(value, EditingLevel.EditByValue); }
        }

        public Point2D Center { get { return new Point2D(CenterX, CenterY); } }

        [FormulaIndex(PROP_RADIUSX)]
        public Distance RadiusX {
            get { return Formula[PROP_RADIUSX].DistanceValue; }
            set { Formula[PROP_RADIUSX].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_RADIUSY)]
        public Distance RadiusY {
            get { return Formula[PROP_RADIUSY].DistanceValue; }
            set { Formula[PROP_RADIUSY].SetValue(value, EditingLevel.EditByValue); }
        }

        public Size2D Radius { get { return new Size2D(RadiusX, RadiusY); } }

        [FormulaIndex(PROP_ROTATINGANGLE)]
        public Angle RotatingAngle {
            get { return Formula[PROP_ROTATINGANGLE].AngleValue; }
            set { Formula[PROP_ROTATINGANGLE].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_STROKE)]
        public bool Stroke {
            get { return Formula[PROP_STROKE].BooleanValue; }
            set { Formula[PROP_STROKE].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_LINECOLOR)]
        public Color LineColor {
            get { return Formula[PROP_LINECOLOR].ColorValue; }
            set { Formula[PROP_LINECOLOR].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_LINEWIDTH)]
        public Distance LineWidth {
            get { return Formula[PROP_LINEWIDTH].DistanceValue; }
            set { Formula[PROP_LINEWIDTH].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_FILL)]
        public bool Fill {
            get { return Formula[PROP_FILL].BooleanValue; }
            set { Formula[PROP_FILL].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_FILLCOLOR)]
        public Color FillColor {
            get { return Formula[PROP_FILLCOLOR].ColorValue; }
            set { Formula[PROP_FILLCOLOR].SetValue(value, EditingLevel.EditByValue); }
        }

        public override Distance EndX { get { return CenterX + RadiusX; } }
        public override Distance EndY { get { return CenterY; } }

        public static Rectangle2D GetContainingRect(Distance centerX, Distance centerY, Distance radiusX, Distance radiusY, Angle rotatingAngle) {
            LengthUnit u = radiusX.Unit;
            double a = radiusX[u];
            double b = radiusY[u];
            double cos = rotatingAngle.Cos();
            double sin = rotatingAngle.Sin();
            double rx = Math.Sqrt(a * a * cos * cos + b * b * sin * sin);
            double ry = Math.Sqrt(b * b * cos * cos + a * a * sin * sin);
            double cx = centerX[u];
            double cy = centerY[u];
            return new Rectangle2D(cx - rx, cy - ry, cx + rx, cy + ry, u, false);
        }

        public override Rectangle2D GetContainingRect() {
            return EllipsePath.GetContainingRect(CenterX, CenterY, RadiusX, RadiusY, RotatingAngle);
        }

        public override Rectangle2D GetContainingRect(Point2D basePoint, Angle rotatingAngle) {
            Rectangle2D r = EllipsePath.GetContainingRect(CenterX, CenterY, RadiusX, RadiusY, RotatingAngle + rotatingAngle);
            Point2D p = r.Center.Rotate(basePoint, rotatingAngle);
            return new Rectangle2D(p.X - r.Width / 2, p.Y - r.Height / 2, p.X + r.Width / 2, p.Y + r.Height / 2);
        }

        public override string ToString() {
            return string.Format("Elipse(Center=({0}, {1}), Radius=({2}, {3}), RotatingAngle={4}, Stroke={5}, LineColor={6}, LineWidth={7}, Fill={8}, FillColor={9})",
                FormulaCenterX, FormulaCenterY, FormulaRadiusX, FormulaRadiusY, FormulaRotatingAngle, FormulaStroke,
                FormulaLineColor, FormulaLineWidth, FormulaFill, FormulaFillColor);
        }
    }

    public partial class DrawingPathCollection: ICollection<DrawingPath> {
        //private Shape _parent;
        private List<DrawingPath> _items = new List<DrawingPath>();

        internal DrawingPath this[int index] { get { return _items[index]; } }

        internal DrawingPathCollection() { }
        //internal DrawingPathCollection(Shape parent) {
        //    _parent = parent;
        //}

        private void UpdateTailPrior() {
            int i = _items.Count - 2;
            if (0 <= i) {
                _items[i + 1].Prior = _items[i];
            }
        }
        private void UpdatePrior() {
            DrawingPath prior = null;
            foreach (DrawingPath item in _items) {
                item.Prior = prior;
                prior = item;
            }
        }

        private void UnlinkItemEvent(DrawingPath item) {
            item.Disposed -= ItemDisposed;
            InvalidateVisual();
        }

        private void ItemDisposed(object sender, DisposeEventArgs e) {
            Remove(sender as DrawingPath);
            InvalidateVisual();
        }

        #region ICollection の実装
        public void Add(DrawingPath item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            _items.Add(item);
            UpdateTailPrior();
            item.Disposed += ItemDisposed;
            InvalidateVisual();
        }

        public void Clear() {
            foreach (DrawingPath item in _items) {
                UnlinkItemEvent(item);
            }
            _items.Clear();
            InvalidateVisual();
        }

        public bool Contains(DrawingPath item) {
            return _items.Contains(item);
        }

        public void CopyTo(DrawingPath[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
            UpdateTailPrior();
        }

        public int Count {
            get { return _items.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(DrawingPath item) {
            bool ret = _items.Remove(item);
            if (ret) {
                UpdatePrior();
                UnlinkItemEvent(item);
                InvalidateVisual();
            }
            return ret;
        }

        public IEnumerator<DrawingPath> GetEnumerator() {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
        #endregion

        public void Insert(int index, DrawingPath item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            _items.Insert(index, item);
            item.Disposed += ItemDisposed;
            InvalidateVisual();
        }

        private Rectangle2D _boundsRect = Rectangle2D.Empty;
        private Rectangle2D _boundsRectOnSheet = Rectangle2D.Empty;
        private bool _isBoundsRectValid = false;

        private void UpdateBoundsRect() {
            if (_items.Count == 0) {
                _boundsRect = Rectangle2D.Empty;
                _isBoundsRectValid = true;
                return;
            }
            Rectangle2D rect = new Rectangle2D(double.MaxValue, double.MaxValue, double.MinValue, double.MinValue, Distance.DefaultUnit, false);
            foreach (DrawingPath path in _items) {
                Rectangle2D r = path.GetContainingRect();
                rect = Rectangle2D.Union(rect, r);
            }
            if (rect.Right < rect.Left || rect.Bottom < rect.Top) {
                _boundsRect = Rectangle2D.Empty;
                _isBoundsRectValid = true;
                return;
            }
            _boundsRect = rect;
            _isBoundsRectValid = true;
            return;
        }

        public void InvalidateVisual() {
            _isBoundsRectValid = false;
        }

        public Rectangle2D BoundsRect {
            get {
                if (!_isBoundsRectValid) {
                    UpdateBoundsRect();
                }
                return _boundsRect;
            }
        }

        public Rectangle2D GetBoundsRect(Point2D basePoint, Angle angle) {
            List<Rectangle2D> l = new List<Rectangle2D>();
            foreach(DrawingPath p in _items){
                l.Add(p.GetContainingRect(basePoint, angle));
            }
            return Rectangle2D.Union(l);
        }
        
        public IKnobInfo[] GetKnobInfos() {
            throw new NotImplementedException();
        }
    }
}
