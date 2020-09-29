using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Text;

namespace Sketch {
    /// <summary>
    /// パスの終端処理
    /// </summary>
    public enum PathTermination {
        /// <summary>
        /// 終端処理をしない。後でパスを更に追加する場合に使う
        /// </summary>
        Continue,
        /// <summary>
        /// 終点までの線を引く
        /// </summary>
        Stroke,
        /// <summary>
        /// 終点と始点を結んで閉じた図形を作る
        /// </summary>
        Close,
    }
    
    public interface IShapeParentable: IExprComponentOwnable {
        Document Document { get; }
        Sheet Sheet { get; }
        Layer Layer { get; }
        Shape Parent { get; }
        ShapeCollection Shapes { get; }
        //void AddShape(Shape obj);
        //void RemoveShape(Shape obj);
        void BringToTop(Shape shape);
        void BringToFront(Shape shape);
        void BringToFront(Shape shape, Shape target);
        void SendToBottom(Shape shape);
        void SendToBack(Shape shape);
        void SendToBack(Shape shape, Shape target);
        Shape[] GetShapesAt(Point2D point);
        Shape GetTopShapeAt(Point2D point);
        Shape DrawLine(Point2D startPoint, Point2D endPoint);
        Shape DrawLine(Distance startX, Distance startY, Distance endX, Distance endY);
        //Shape DrawLine(string startX, string startY, string endX, string endY);

        Shape DrawEllipse(Point2D center, Distance radiusX, Distance radiusY);
        Shape DrawEllipse(Point2D center, Distance radiusX, Distance radiusY, Angle rotatingAngle);
        Shape DrawArc(Point2D startPoint, Point2D endPoint, Distance radiusX, Distance radiusY, Angle rotatingAngle, bool isLargeArc, bool isClockwise, PathTermination termination);
        //Shape DrawImage(ImageSource imageSource, Rect rectangle);
        Shape DrawRectangle(Rectangle2D rectangle);
        Shape DrawRoundedRectangle(Rectangle2D rectangle, Distance radiusX, Distance radiusY);
        Shape DrawText(string formattedText, Point2D origin);
        //Shape DrawVideo(MediaPlayer player, Rect rectangle);
        //Shape DrawVideo(MediaPlayer player, Rect rectangle, AnimationClock rectangleAnimations);
    }

    //public enum LockTarget {
    //    Aspect
    //}

    [Serializable]
    public sealed partial class Shape : ExprComponent<Sheet>, IShapeParentable {
        //private Document _owner;
        private IShapeParentable _parent;
        private ShapeCollection _shapes;

        private DrawingPathCollection _drawingPaths;
        private KnobInfoCollection _knobs;
        private Master _master;
        private bool _temporary;
        //private bool _isFlipped;

        //private Rectangle2D _bounds;
        //private bool _isDrawingFill;
        //private Color _fillColor;
        //private bool _isDrawingStroke;
        //private Color _strokeColor;
        //private float _strokeWidth;

        //public Shape(Document owner, IShapeParentable parent): base(owner) {
        public Shape(IShapeParentable parent, Point2D pin, bool temporary = false)
            : base(parent != null ? parent.Sheet : null) {
            _parent = parent;
            _temporary = temporary;
            _shapes = new ShapeCollection(this);
            _drawingPaths = new DrawingPathCollection();
            _knobs = new KnobInfoCollection();
            Pin = pin;
            if (_parent != null) {
                _parent.Shapes.Add(this);
            }

            new LeftKnobInfo(this);
            new TopLeftKnobInfo(this);
            new TopKnobInfo(this);
            new TopRightKnobInfo(this);
            new RightKnobInfo(this);
            new BottomRightKnobInfo(this);
            new BottomKnobInfo(this);
            new BottomLeftKnobInfo(this);
            new AngleKnobInfo(this, this, "Shape.PinX", "Shape.PinY", "Shape.Angle", 1.0);
        }

        public Shape(IShapeParentable parent, Master master, Point2D pin, bool temporary = false)
            : base(parent != null ? parent.Sheet : null) {
            _parent = parent;
            _temporary = temporary;
            _master = master;
            _shapes = new ShapeCollection(this);
            _drawingPaths = new DrawingPathCollection();
            _knobs = new KnobInfoCollection();
            Pin = pin;
        }

        protected override void OnNameChanged(PropertyChangedEventArgs<string> e) {
            base.OnNameChanged(e);
            //Container.InvalidateNameToShape();
        }

        public Document Document {
            get {
                return Container != null ? Container.Document : null;
            }
        }

        public Sheet Sheet { get { return Container; } }

        public Layer Layer {
            get {
                return _parent != null ? _parent.Layer : null;
            }
        }

        public Shape Parent { get { return _parent as Shape; } }

        public ShapeCollection Shapes { get { return _shapes; } }

        public DrawingPathCollection DrawingPaths { get { return _drawingPaths; } }

        public KnobInfoCollection Knobs { get { return _knobs; } }

        public Master Master { get { return _master; } }

        public Rectangle2D Bounds {
            get {
                Rectangle2D r = DrawingPaths.BoundsRect;
                r.Location += (Pin - LocPin);
                return r;
            }
        }

        public Rectangle2D GetBounds(Angle angle) {
            Rectangle2D r = DrawingPaths.GetBoundsRect(Pin, angle);
            r.Location += (Pin - LocPin);
            return r;
        }

        private Shape CloneRecursive(IShapeParentable parent, Dictionary<Shape, Shape> shapeMapping, Point2D pin) {
            Shape ret = new Shape(parent, pin);
            Clone(parent);
            //ret.Assign(this);
            shapeMapping.Add(this, ret);
            foreach (Shape sh in _shapes) {
                ret.Shapes.Add(sh.CloneRecursive(parent, shapeMapping, pin));
            }
            ret.ReplaceShapeMapping(shapeMapping);
            return ret;
        }

        private void ReplaceShapeMapping(Dictionary<Shape, Shape> shapeMapping) {

            throw new NotImplementedException();
        }

        public object Clone(Layer container, Point2D pin) {
            Dictionary<Shape, Shape> shapeMapping = new Dictionary<Shape, Shape>();
            Shape ret = CloneRecursive(container, shapeMapping, pin);
            ret.ReplaceShapeMapping(shapeMapping);
            return ret;
        }

        /* You can override these class methods in your subclass of SKTShape, but it would be a waste of time, because no one invokes these on any class other than SKTShape itself. Really these could just be functions if we didn't have such a syntactic sweet tooth. */

        /// <summary>
        /// Move each shape in the array by the same amount.
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public static void TranslateShapes(IEnumerable<Shape> shapes, Point2D delta) {
            foreach (Shape g in shapes) {
                g.Pin += delta;
            }
        }

        #region FormulaProperty
        private static PropertyDefCollection _builtinPropertyDefs = new PropertyDefCollection(
            new PropertyDef[] {
                new PropertyDef("IsLine", typeof(bool), "false"),
                new PropertyDef("PinX", typeof(Distance), "0cm", PROP_LOCKPINX),
                new PropertyDef("PinY", typeof(Distance), "0cm", PROP_LOCKPINY),
                new PropertyDef("Width", typeof(Distance), "0cm", PROP_WIDTH),
                new PropertyDef("Height", typeof(Distance), "0cm", PROP_HEIGHT),
                new PropertyDef("LocPinX", typeof(Distance), "Width/2", PROP_LOCKLOCPINX),
                new PropertyDef("LocPinY", typeof(Distance), "Height/2", PROP_LOCKLOCPINY),
                new PropertyDef("Angle", typeof(Angle), "0deg", PROP_LOCKANGLE),
                new PropertyDef("Flipped", typeof(bool), "false", PROP_LOCKFLIPPED),
                new PropertyDef("IgnoreScaling", typeof(bool), "false"),
                new PropertyDef("Stroke", typeof(bool), "true"),
                new PropertyDef("LineColor", typeof(Color), "Black"),
                new PropertyDef("LineWidth", typeof(Distance), "1px"),
                new PropertyDef("Fill", typeof(bool), "true"),
                new PropertyDef("FillColor", typeof(Color), "White"),
                new PropertyDef("LockPinX", typeof(bool), "false"),
                new PropertyDef("LockPinY", typeof(bool), "false"),
                new PropertyDef("LockWidth", typeof(bool), "false"),
                new PropertyDef("LockHeight", typeof(bool), "false"),
                new PropertyDef("LockAspect", typeof(bool), "false"),
                new PropertyDef("LockLocPinX", typeof(bool), "false"),
                new PropertyDef("LockLocPinY", typeof(bool), "false"),
                new PropertyDef("LockAngle", typeof(bool), "false"),
                new PropertyDef("LockFlipped", typeof(bool), "false"),
                new PropertyDef("LockText", typeof(bool), "false"),
                new PropertyDef("FontName", typeof(string), null),
                new PropertyDef("FontSize", typeof(double), null),
                new PropertyDef("Bold", typeof(bool), "false"),
                new PropertyDef("Italic", typeof(bool), "false"),
                new PropertyDef("Text", typeof(string), "\"\"", PROP_LOCKTEXT),
                // 汎用拡張プロパティ
                new PropertyDef("RadiusX", typeof(Distance), "0cm"),
                new PropertyDef("RadiusY", typeof(Distance), "0cm"),
                new PropertyDef("RotatingAngle", typeof(Angle), "0deg"),
                new PropertyDef("LineCap", typeof(LineCap), "CapFlat"),
                new PropertyDef("LineJoin", typeof(LineJoin), "JoinMiter"),
                new PropertyDef("MiterLimit", typeof(double), "3"),
                new PropertyDef("FillRule", typeof(FillRule), "NonZero"),
                new PropertyDef("CreatedDate", typeof(DateTime), "`Now`"),
            }
        );

        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinPropertyDefs;
        }

        public const int PROP_ISLINE = 0;
        public const int PROP_PINX = 1;
        public const int PROP_PINY = 2;
        public const int PROP_WIDTH = 3;
        public const int PROP_HEIGHT = 4;
        public const int PROP_LOCPINX = 5;
        public const int PROP_LOCPINY = 6;
        public const int PROP_ANGLE = 7;
        public const int PROP_FLIPPED = 8;
        public const int PROP_IGNORESCALING = 9;
        public const int PROP_STROKE = 10;
        public const int PROP_LINECOLOR = 11;
        public const int PROP_LINEWIDTH = 12;
        public const int PROP_FILL = 13;
        public const int PROP_FILLCOLOR = 14;
        public const int PROP_LOCKPINX = 15;
        public const int PROP_LOCKPINY = 16;
        public const int PROP_LOCKWIDTH = 17;
        public const int PROP_LOCKHEIGHT = 18;
        public const int PROP_LOCKASPECT = 19;
        public const int PROP_LOCKLOCPINX = 20;
        public const int PROP_LOCKLOCPINY = 21;
        public const int PROP_LOCKANGLE = 22;
        public const int PROP_LOCKFLIPPED = 23;
        public const int PROP_LOCKTEXT = 24;
        public const int PROP_FONTNAME = 25;
        public const int PROP_FONTSIZE = 26;
        public const int PROP_BOLD = 27;
        public const int PROP_ITALIC = 28;
        public const int PROP_TEXT = 29;
        public const int PROP_RADIUSX = 30;
        public const int PROP_RADIUSY = 31;
        public const int PROP_ROTATINGANGLE = 32;
        public const int PROP_LINECAP = 33;
        public const int PROP_LINEJOIN = 34;
        public const int PROP_MITERLIMIT = 35;
        public const int PROP_FILLRULE = 36;
        public const int PROP_CREATEDDATE = 37;
        public FormulaProperty FormulaIsLine { get { return Formula[PROP_ISLINE]; } }
        public FormulaProperty FormulaPinX { get { return Formula[PROP_PINX]; } }
        public FormulaProperty FormulaPinY { get { return Formula[PROP_PINY]; } }
        public FormulaProperty FormulaWidth { get { return Formula[PROP_WIDTH]; } }
        public FormulaProperty FormulaHeight { get { return Formula[PROP_HEIGHT]; } }
        public FormulaProperty FormulaLocPinX { get { return Formula[PROP_LOCPINX]; } }
        public FormulaProperty FormulaLocPinY { get { return Formula[PROP_LOCPINY]; } }
        public FormulaProperty FormulaAngle { get { return Formula[PROP_ANGLE]; } }
        public FormulaProperty FormulaFlipped { get { return Formula[PROP_FLIPPED]; } }
        public FormulaProperty FormulaIgnoreScaling { get { return Formula[PROP_IGNORESCALING]; } }
        public FormulaProperty FormulaLockPinX { get { return Formula[PROP_LOCKPINX]; } }
        public FormulaProperty FormulaLockPinY { get { return Formula[PROP_LOCKPINY]; } }
        public FormulaProperty FormulaLockWidth { get { return Formula[PROP_LOCKWIDTH]; } }
        public FormulaProperty FormulaLockHeight { get { return Formula[PROP_LOCKHEIGHT]; } }
        public FormulaProperty FormulaLockAspect { get { return Formula[PROP_LOCKASPECT]; } }
        public FormulaProperty FormulaLockLocPinX { get { return Formula[PROP_LOCKLOCPINX]; } }
        public FormulaProperty FormulaLockLocPinY { get { return Formula[PROP_LOCKLOCPINY]; } }
        public FormulaProperty FormulaLockAngle { get { return Formula[PROP_LOCKANGLE]; } }
        public FormulaProperty FormulaLockFlipped { get { return Formula[PROP_LOCKFLIPPED]; } }
        public FormulaProperty FormulaLockText { get { return Formula[PROP_LOCKTEXT]; } }
        public FormulaProperty FormulaFontName { get { return Formula[PROP_FONTNAME]; } }
        public FormulaProperty FormulaFontSize { get { return Formula[PROP_FONTSIZE]; } }
        public FormulaProperty FormulaBold { get { return Formula[PROP_BOLD]; } }
        public FormulaProperty FormulaItalic { get { return Formula[PROP_ITALIC]; } }
        public FormulaProperty FormulaText { get { return Formula[PROP_TEXT]; } }
        public FormulaProperty FormulaRadiusX { get { return Formula[PROP_RADIUSX]; } }
        public FormulaProperty FormulaRadiusY { get { return Formula[PROP_RADIUSY]; } }
        public FormulaProperty FormulaRotatingAngle { get { return Formula[PROP_ROTATINGANGLE]; } }
        public FormulaProperty FormulaLineCap { get { return Formula[PROP_LINECAP]; } }
        public FormulaProperty FormulaLineJoin { get { return Formula[PROP_LINEJOIN]; } }
        public FormulaProperty FormulaMiterLimit { get { return Formula[PROP_MITERLIMIT]; } }
        public FormulaProperty FormulaFillRule { get { return Formula[PROP_FILLRULE]; } }
        public FormulaProperty FormulaCratedDate { get { return Formula[PROP_CREATEDDATE]; } }
        /// <summary>
        /// 直線もしくは線分の組み合わせの場合true
        /// trueの場合はローカル座標系を生成しない
        /// (プロパティ Pin, LocPin, Angle, Width, Height は意味を持たなくなる)
        /// </summary>
        [FormulaIndex(PROP_ISLINE)]
        public bool IsLine {
            get { return Formula[PROP_ISLINE].BooleanValue; }
            set { Formula[PROP_ISLINE].SetValue(value, EditingLevel.EditByValue); }
        }

        public Point2D Pin {
            get { return new Point2D(PinX, PinY); }
            set {
                PinX = value.X;
                PinY = value.Y;
            }
        }

        [FormulaIndex(PROP_PINX)]
        public Distance PinX {
            get { return Formula[PROP_PINX].DistanceValue; }
            set { Formula[PROP_PINX].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_PINY)]
        public Distance PinY {
            get { return Formula[PROP_PINY].DistanceValue; }
            set { Formula[PROP_PINY].SetValue(value, EditingLevel.EditByValue); }
        }

        public Size2D Size { get { return new Size2D(Width, Height); } }

        [FormulaIndex(PROP_WIDTH)]
        public Distance Width {
            get { return Formula[PROP_WIDTH].DistanceValue; }
            set {
                if (LockAspect) {
                    if (Formula[PROP_WIDTH].CanModifyValue() && Formula[PROP_HEIGHT].CanModifyValue()) {
                        Distance oldW = Width;
                        Formula[PROP_WIDTH].SetValue(value, EditingLevel.EditByValue);
                        Distance newW = Width;
                        if (oldW != Distance.Zero && oldW != newW) {
                            Distance h = Height * (newW / oldW);
                            Formula[PROP_HEIGHT].SetValue(h, EditingLevel.EditByFormula);
                        }
                    }
                } else {
                    Formula[PROP_WIDTH].SetValue(value, EditingLevel.EditByValue);
                }
            }
        }

        [FormulaIndex(PROP_HEIGHT)]
        public Distance Height {
            get { return Formula[PROP_HEIGHT].DistanceValue; }
            set {
                if (LockAspect) {
                    if (Formula[PROP_WIDTH].CanModifyValue() && Formula[PROP_HEIGHT].CanModifyValue()) {
                        Distance oldH = Height;
                        Formula[PROP_HEIGHT].SetValue(value, EditingLevel.EditByValue);
                        Distance newH = Height;
                        if (oldH != Distance.Zero && oldH != newH) {
                            Distance w = Width * (newH / oldH);
                            Formula[PROP_WIDTH].SetValue(w, EditingLevel.EditByFormula);
                        }
                    }
                } else {
                    Formula[PROP_HEIGHT].SetValue(value, EditingLevel.EditByValue);
                }
            }
        }

        public Point2D LocPin { get { return new Point2D(LocPinX, LocPinY); } }

        [FormulaIndex(PROP_LOCPINX)]
        public Distance LocPinX {
            get { return Formula[PROP_LOCPINX].DistanceValue; }
            set { Formula[PROP_LOCPINX].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LOCPINY)]
        public Distance LocPinY {
            get { return Formula[PROP_LOCPINY].DistanceValue; }
            set { Formula[PROP_LOCPINY].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_ANGLE)]
        public Angle Angle {
            get { return Formula[PROP_ANGLE].AngleValue; }
            set { Formula[PROP_ANGLE].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_FLIPPED)]
        public bool Flipped {
            get { return Formula[PROP_FLIPPED].BooleanValue; }
            set { Formula[PROP_FLIPPED].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_IGNORESCALING)]
        public bool IgnoreScaling {
            get { return Formula[PROP_IGNORESCALING].BooleanValue; }
            set { Formula[PROP_IGNORESCALING].SetValue(value, EditingLevel.EditByValue); }
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

        [FormulaIndex(PROP_LOCKPINX)]
        public bool LockPinX {
            get { return Formula[PROP_LOCKPINX].BooleanValue; }
            set { Formula[PROP_LOCKPINX].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LOCKPINY)]
        public bool LockPinY {
            get { return Formula[PROP_LOCKPINY].BooleanValue; }
            set { Formula[PROP_LOCKPINY].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LOCKWIDTH)]
        public bool LockWidth {
            get { return Formula[PROP_LOCKWIDTH].BooleanValue; }
            set { Formula[PROP_LOCKWIDTH].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LOCKHEIGHT)]
        public bool LockHeight {
            get { return Formula[PROP_LOCKHEIGHT].BooleanValue; }
            set { Formula[PROP_LOCKHEIGHT].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LOCKASPECT)]
        public bool LockAspect {
            get { return Formula[PROP_LOCKASPECT].BooleanValue; }
            set { Formula[PROP_LOCKASPECT].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LOCKLOCPINX)]
        public bool LockLocPinX {
            get { return Formula[PROP_LOCKLOCPINX].BooleanValue; }
            set { Formula[PROP_LOCKLOCPINX].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LOCKLOCPINY)]
        public bool LockLocPinY {
            get { return Formula[PROP_LOCKLOCPINY].BooleanValue; }
            set { Formula[PROP_LOCKLOCPINY].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LOCKANGLE)]
        public bool LockAngle {
            get { return Formula[PROP_LOCKANGLE].BooleanValue; }
            set { Formula[PROP_LOCKANGLE].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LOCKFLIPPED)]
        public bool LockFlipped {
            get { return Formula[PROP_LOCKFLIPPED].BooleanValue; }
            set { Formula[PROP_LOCKFLIPPED].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LOCKTEXT)]
        public bool LockText {
            get { return Formula[PROP_LOCKTEXT].BooleanValue; }
            set { Formula[PROP_LOCKTEXT].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_FONTNAME)]
        public string FontName {
            get { return Formula[PROP_FONTNAME].StringValue; }
            set { Formula[PROP_FONTNAME].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_FONTSIZE)]
        public double FontSize {
            get { return Formula[PROP_FONTSIZE].DoubleValue; }
            set { Formula[PROP_FONTSIZE].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_BOLD)]
        public bool Bold {
            get { return Formula[PROP_BOLD].BooleanValue; }
            set { Formula[PROP_BOLD].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_ITALIC)]
        public bool Italic {
            get { return Formula[PROP_ITALIC].BooleanValue; }
            set { Formula[PROP_ITALIC].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_TEXT)]
        public string Text {
            get { return Formula[PROP_TEXT].StringValue; }
            set { Formula[PROP_TEXT].SetValue(value, EditingLevel.EditByValue); }
        }

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

        [FormulaIndex(PROP_ROTATINGANGLE)]
        public Angle RotatingAngle {
            get { return Formula[PROP_ROTATINGANGLE].AngleValue; }
            set { Formula[PROP_ROTATINGANGLE].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LINECAP)]
        public PenLineCap LineCap {
            get { return (PenLineCap)Formula[PROP_LINECAP].Int16Value; }
            set { Formula[PROP_LINECAP].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_LINEJOIN)]
        public PenLineJoin LineJoin {
            get { return (PenLineJoin)Formula[PROP_LINEJOIN].Int16Value; }
            set { Formula[PROP_LINEJOIN].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_MITERLIMIT)]
        public double MiterLimit {
            get { return Formula[PROP_MITERLIMIT].DoubleValue; }
            set { Formula[PROP_MITERLIMIT].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_FILLRULE)]
        public FillRule FillRule {
            get { return (FillRule)Formula[PROP_FILLRULE].Int16Value; }
            set { Formula[PROP_FILLRULE].SetValue(value, EditingLevel.EditByValue); }
        }

        public double Scaling() {
            if (Sheet != null) {
                return Sheet.Scaling;
            }
            return 1.0;
        }

        public Point DisplayPin {
            get {
                double sc = Scaling();
                return new Point(PinX.Px * sc, PinY.Px * sc);
            }
        }

        public Point DisplayLocPin {
            get {
                double sc = Scaling();
                return new Point(LocPinX.Px * sc, LocPinY.Px * sc);
            }
        }

        public Size DisplaySize {
            get {
                double sc = Scaling();
                return new Size(Width.Px * sc, Height.Px * sc);
            }
        }

        public Rect DisplayBounds {
            get {
                double sc = Scaling();
                return new Rect(0, 0, Width.Px * sc, Height.Px * sc);
            }
        }

        public Distance Left {
            get { return PinX + LocPinX - Width / 2; }
            set {
                Distance dx = value - Left;
                PinX += dx / 2;
                Width -= dx;
            }
        }

        public Distance Top {
            get { return PinY + LocPinY - Height / 2; }
            set {
                Distance dy = value - Top;
                PinY += dy / 2;
                Width -= dy;
            }
        }

        public Distance Right {
            get { return PinX + LocPinX + Width / 2; }
            set {
                Distance dx = value - Right;
                PinX += dx / 2;
                Width += dx;
            }
        }

        public Distance Bottom {
            get { return PinY + LocPinY + Height / 2; }
            set {
                Distance dy = value - Bottom;
                PinY += dy / 2;
                Width += dy;
            }
        }
        #endregion

        #region interface implementation
        void IExprComponentOwnable.Add(object item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            DrawingPath p = item as DrawingPath;
            if (p != null) {
                _drawingPaths.Add(p);
                return;
            }
        }

        void IExprComponentOwnable.Remove(object item) {
            _drawingPaths.Remove(item as DrawingPath);
        }

        string IExprComponentOwnable.GetNewName(object target) {
            return null;
        }
        #endregion

        #region 描画API

        /// <summary>
        /// 面積を持たないシェイプ(直線、コネクタ線等)向けに初期化されたシェイプを作成
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Shape CreateLineShape(IShapeParentable parent) {
            IShapeParentable p = parent;
            //if (p is Sheet) {
            //    Sheet sh = p as Sheet;
            //    if (sh.CurrentLayer != null) {
            //        p = sh.CurrentLayer;
            //    }
            //}
            Shape ret = new Shape(p, Point2D.Zero);
            ret.IsLine = true;
            ret.LockAspect = false;
            //ret.FormulaPinX.SetFormula("DrawingPaths.BoundsRect.CenterX");
            //ret.FormulaPinY.SetFormula("DrawingPaths.BoundsRect.CenterY");
            //ret.FormulaWidth.SetFormula("DrawingPaths.BoundsRect.Width");
            //ret.FormulaHeight.SetFormula("DrawingPaths.BoundsRect.Height");
            //ret.FormulaLocPinX.SetFormula("Width/2");
            //ret.FormulaLocPinY.SetFormula("Height/2");
            ret.PinX = Distance.Zero;
            ret.PinY = Distance.Zero;
            ret.Width = Distance.Zero;
            ret.Height = Distance.Zero;
            ret.LocPinX = Distance.Zero;
            ret.LocPinY = Distance.Zero;
            ret.Angle = Angle.Zero;

            ret.FormulaPinX.LockLevel = LockLevel.Disabled;
            ret.FormulaPinY.LockLevel = LockLevel.Disabled;
            ret.FormulaWidth.LockLevel = LockLevel.Disabled;
            ret.FormulaHeight.LockLevel = LockLevel.Disabled;
            ret.FormulaLocPinX.LockLevel = LockLevel.Disabled;
            ret.FormulaLocPinY.LockLevel = LockLevel.Disabled;
            ret.FormulaAngle.LockLevel = LockLevel.Disabled;
            ret.FormulaLockAspect.LockLevel = LockLevel.Disabled;
            return ret;
        }

        /// <summary>
        /// 面積を持つシェイプ向けに初期化されたシェイプを作成
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public static Shape CreateBoundaryShape(IShapeParentable parent, Rectangle2D rectangle) {
            IShapeParentable p = parent;
            //if (p is Sheet) {
            //    Sheet sh = p as Sheet;
            //    if (sh.CurrentLayer != null) {
            //        p = sh.CurrentLayer;
            //    }
            //}
            Shape ret = new Shape(p, rectangle.Center);
            ret.Width = rectangle.Width;
            ret.Height = rectangle.Height;
            //ret.FormulaLocPinX.SetFormula("Width/2");
            //ret.FormulaLocPinY.SetFormula("Height/2");
            return ret;
        }

        public DrawingPath StartPath(Point2D point) {
            StartPath path = new StartPath(this, point);
            return path;
        }
        public DrawingPath StartPath(Distance x, Distance y) {
            StartPath path = new StartPath(this, x, y);
            return path;
        }
        public DrawingPath StartPath(string formulaX, string formulaY) {
            StartPath path = new StartPath(this, formulaX, formulaY);
            return path;
        }
        public DrawingPath StartPath(Point2D point, bool stroke, Color lineColor, Distance lineWidth) {
            StartPath path = new StartPath(this, point);
            path.Stroke = stroke;
            path.LineColor = lineColor;
            path.LineWidth = lineWidth;
            return path;
        }
        public DrawingPath StartPath(Distance x, Distance y, bool stroke, Color lineColor, Distance lineWidth) {
            StartPath path = new StartPath(this, x, y);
            path.Stroke = stroke;
            path.LineColor = lineColor;
            path.LineWidth = lineWidth;
            return path;
        }
        public DrawingPath StartPath(string formulaX, string formulaY, bool stroke, Color lineColor, Distance lineWidth) {
            StartPath path = new StartPath(this, formulaX, formulaY);
            path.Stroke = stroke;
            path.LineColor = lineColor;
            path.LineWidth = lineWidth;
            return path;
        }

        public DrawingPath ClosePath() {
            ClosePath path = new ClosePath(this);
            return path;
        }
        public DrawingPath ClosePath(bool fill, Color fillColor) {
            ClosePath path = new ClosePath(this);
            path.Fill = fill;
            path.FillColor = fillColor;
            return path;
        }

        public DrawingPath StrokePath() {
            StrokePath path = new StrokePath(this);
            return path;
        }

        public DrawingPath LineTo(Point2D point) {
            LineToPath path = new LineToPath(this, point);
            return path;
        }
        public DrawingPath LineTo(Distance x, Distance y) {
            LineToPath path = new LineToPath(this, x, y);
            return path;
        }
        public DrawingPath LineTo(string formulaX, string formulaY) {
            LineToPath path = new LineToPath(this, formulaX, formulaY);
            return path;
        }

        public DrawingPath BezierTo(Point2D point1, Point2D point2, Point2D point3) {
            BezierPath path = new BezierPath(this, point1, point2, point3);
            return path;
        }

        public DrawingPath ArcTo(Point2D endPoint, Size2D radius, Angle rotatingAngle, bool isLargeArc, bool isClockwise) {
            ArcToPath path = new ArcToPath(this, endPoint, radius, rotatingAngle, isLargeArc, isClockwise);
            return path;
        }
        public DrawingPath ArcTo(string formulaEndPointX, string formulaEndPointY, string formulaRadiusX, string formulaRadiusY, string formulaRotatingAngle, string formulaIsLargeArc, string formulaIsClockwise) {
            ArcToPath path = new ArcToPath(this, formulaEndPointX, formulaEndPointY, formulaRadiusX, formulaRadiusY, formulaRotatingAngle, formulaIsLargeArc, formulaIsClockwise);
            return path;
        }

        public DrawingPath DrawEllipsePath(Point2D center, Size2D radius, Angle rotatingAngle) {
            EllipsePath path = new EllipsePath(this, center, radius, rotatingAngle);
            return path;
        }
        public DrawingPath DrawEllipsePath(Distance centerX, Distance centerY, Distance radiusX, Distance radiusY, Angle rotatingAngle) {
            EllipsePath path = new EllipsePath(this, centerX, centerY, radiusX, radiusY, rotatingAngle);
            return path;
        }
        public DrawingPath DrawEllipsePath(string formulaCenterX, string formulaCenterY, string formulaRadiusX, string formulaRadiusY, string formulaRotatingAngle) {
            EllipsePath path = new EllipsePath(this, formulaCenterX, formulaCenterY, formulaRadiusX, formulaRadiusY, formulaRotatingAngle);
            return path;
        }
        public DrawingPath DrawArrowPath(Point2D endPoint, Point2D startPoint, Distance arrowSize, ArrowStyle style) {
            ArrowPath path = new ArrowPath(this, startPoint, endPoint, arrowSize, style);
            return path;
        }
        #endregion

        #region 基本シェイプ作成用API
        public static Shape DrawLine(IShapeParentable parent, Point2D startPoint, Point2D endPoint) {
            Shape ret = Shape.CreateLineShape(parent);
            ret.StartPath(startPoint);
            ret.LineTo(endPoint);
            return ret;
        }

        public static Shape DrawLine(IShapeParentable parent, Distance startX, Distance startY, Distance endX, Distance endY) {
            Shape ret = Shape.CreateLineShape(parent);
            ret.StartPath(startX, startY);
            ret.LineTo(endX, endY);
            return ret;
        }

        public static Shape DrawLine(IShapeParentable parent, string startX, string startY, string endX, string endY) {
            Shape ret = Shape.CreateLineShape(parent);
            ret.StartPath(startX, startY);
            ret.LineTo(endX, endY);
            return ret;
        }

        public static Shape DrawPolyLine(IShapeParentable parent, Point2D[] points) {
            if (points == null) {
                throw new ArgumentNullException("points");
            }
            if (points.Length < 2) {
                throw new ArgumentException("points");
            }
            Rectangle2D rect = Rectangle2D.GetContainingRect(points);
            Shape ret = Shape.CreateLineShape(parent);
            ret.PinX = rect.Center.X;
            ret.PinY = rect.Center.Y;
            ret.StartPath(points[0]);
            for (int i = 1; i < points.Length; i++) {
                ret.LineTo(points[i]);
            }
            ret.StrokePath();
            return ret;
        }

        public static Shape DrawPolygon(IShapeParentable parent, Point2D[] points) {
            if (points == null) {
                throw new ArgumentNullException("points");
            }
            if (points.Length < 3) {
                throw new ArgumentException("points");
            }
            Rectangle2D rect = Rectangle2D.GetContainingRect(points);
            Shape ret = Shape.CreateBoundaryShape(parent, rect);
            {
                Point2D p = points[0];
                double rX = (p.X - rect.Left) / rect.Width;
                double rY = (p.Y - rect.Top) / rect.Height;
                string fX = rX.ToString("0.0#####").TrimEnd() + "*Shape.Width";
                string fY = rY.ToString("0.0#####").TrimEnd() + "*Shape.Height";
                ret.StartPath(fX, fY);
            }
            for (int i = 1; i < points.Length; i++) {
                Point2D p = points[i];
                double rX = (p.X - rect.Left) / rect.Width;
                double rY = (p.Y - rect.Top) / rect.Height;
                string fX = rX.ToString("0.0#####").TrimEnd() + "*Shape.Width";
                string fY = rY.ToString("0.0#####").TrimEnd() + "*Shape.Height";
                ret.LineTo(fX, fY);
            }
            ret.ClosePath();
            return ret;
        }

        public static Shape DrawEllipse(IShapeParentable parent, Point2D center, Distance radiusX, Distance radiusY) {
            return DrawEllipse(parent, center, radiusX, radiusY, Angle.Zero);
        }

        public static Shape DrawEllipse(IShapeParentable parent, Point2D center, Distance radiusX, Distance radiusY, Angle rotatingAngle) {
            Rectangle2D r = EllipsePath.GetContainingRect(center.X, center.Y, radiusX, radiusY, rotatingAngle);
            Shape ret = Shape.CreateBoundaryShape(parent, r);
            ret.Width = radiusX * 2;
            ret.Height = radiusY * 2;
            ret.FormulaRadiusX.SetFormula("0.5*Width");
            ret.FormulaRadiusY.SetFormula("0.5*Height");
            //ret.RadiusX = radiusX;
            //ret.RadiusY = radiusY;
            ret.RotatingAngle = rotatingAngle;
            ret.DrawEllipsePath("0.5*Shape.Width", "0.5*Shape.Height", "Shape.RadiusX", "Shape.RadiusY", "Shape.RotatingAngle");
            return ret;
        }

        public static Shape DrawArc(IShapeParentable parent, Point2D startPoint, Point2D endPoint, Distance radiusX, Distance radiusY, Angle rotatingAngle, bool isLargeArc, bool isClockwise, PathTermination termination) {
            Point2D center = ArcToPath.GetEllipseCenter(startPoint, endPoint, radiusX, radiusY, rotatingAngle, isLargeArc, isClockwise);
            Rectangle2D r = EllipsePath.GetContainingRect(center.X, center.Y, radiusX, radiusY, rotatingAngle);
            //Rectangle2D r = ArcToPath.GetContainingRect(startPoint, endPoint, radiusX, radiusY, rotatingAngle, isLargeArc, isClockwise);
            Shape ret = Shape.CreateBoundaryShape(parent, r);
            //Shape ret = Shape.CreateLineShape(parent);
            ret.StartPath(startPoint.X - r.Left, startPoint.Y - r.Top);
            ret.ArcTo(endPoint - r.TopLeft, new Size2D(radiusX, radiusY), rotatingAngle, isLargeArc, isClockwise);
            switch (termination) {
                case PathTermination.Stroke:
                    ret.StrokePath();
                    break;
                case PathTermination.Close:
                    ret.ClosePath();
                    break;
            }
            return ret;
        }

        //public static Shape DrawImage(IShapeParentable parent, ImageSource imageSource, Rect rectangle);
        public static Shape DrawRectangle(IShapeParentable parent, Rectangle2D rectangle) {
            Shape ret = Shape.CreateBoundaryShape(parent, rectangle);
            ret.StartPath("0.0*Shape.Width", "0.0*Shape.Height");
            ret.LineTo("1.0*Shape.Width", "0.0*Shape.Height");
            ret.LineTo("1.0*Shape.Width", "1.0*Shape.Height");
            ret.LineTo("0.0*Shape.Width", "1.0*Shape.Height");
            ret.ClosePath();
            return ret;
        }

        public static Shape DrawRoundedRectangle(IShapeParentable parent, Rectangle2D rectangle, Distance radiusX, Distance radiusY) {
            Shape ret = Shape.CreateBoundaryShape(parent, rectangle);
            ret.RadiusX = radiusX;
            ret.RadiusY = radiusY;
            ret.StartPath("0.0*Shape.Width", "0.0*Shape.Height+Shape.RadiusY");
            ret.ArcTo("0.0*Shape.Width+Shape.RadiusX", "0.0*Shape.Height", "Shape.RadiusX", "Shape.RadiusY", "0deg", "false", "true");
            ret.LineTo("1.0*Shape.Width-Shape.RadiusX", "0.0*Shape.Height");
            ret.ArcTo("1.0*Shape.Width", "0.0*Shape.Height+Shape.RadiusY", "Shape.RadiusX", "Shape.RadiusY", "0deg", "false", "true");
            ret.LineTo("1.0*Shape.Width", "1.0*Shape.Height-Shape.RadiusY");
            ret.ArcTo("1.0*Shape.Width-Shape.RadiusX", "1.0*Shape.Height", "Shape.RadiusX", "Shape.RadiusY", "0deg", "false", "true");
            ret.LineTo("0.0*Shape.Width+Shape.RadiusX", "1.0*Shape.Height");
            ret.ArcTo("0.0*Shape.Width", "1.0*Shape.Height-Shape.RadiusY", "Shape.RadiusX", "Shape.RadiusY", "0deg", "false", "true");
            ret.ClosePath();
            return ret;
        }

        public static Shape DrawText(IShapeParentable parent, string formattedText, Point2D origin) {
            throw new NotImplementedException();
        }
        #endregion

        #region 子シェイプ作成API
        public Shape DrawLine(Point2D startPoint, Point2D endPoint) {
            return Shape.DrawLine(this, startPoint, endPoint);
        }

        public Shape DrawLine(Distance startX, Distance startY, Distance endX, Distance endY) {
            return Shape.DrawLine(this, startX, startY, endX, endY);
        }

        public Shape DrawLine(string startX, string startY, string endX, string endY) {
            return Shape.DrawLine(this, startX, startY, endX, endY);
        }

        public Shape DrawPolyLine(Point2D[] points) {
            return Shape.DrawPolyLine(this, points);
        }

        public Shape DrawPolygon(Point2D[] points) {
            return Shape.DrawPolygon(this, points);
        }

        public Shape DrawEllipse(Point2D center, Distance radiusX, Distance radiusY) {
            return Shape.DrawEllipse(this, center, radiusX, radiusY);
        }

        public Shape DrawEllipse(Point2D center, Distance radiusX, Distance radiusY, Angle rotatingAngle) {
            return Shape.DrawEllipse(this, center, radiusX, radiusY, rotatingAngle);
        }

        public Shape DrawArc(Point2D startPoint, Point2D endPoint, Distance radiusX, Distance radiusY, Angle rotatingAngle, bool isLargeArc, bool isClockwise, PathTermination termination) {
            return Shape.DrawArc(this, startPoint, endPoint, radiusX, radiusY, rotatingAngle, isLargeArc, isClockwise, termination);
        }

        //Shape DrawImage(ImageSource imageSource, Rect rectangle);
        public Shape DrawRectangle(Rectangle2D rectangle) {
            return Shape.DrawRectangle(this, rectangle);
        }

        public Shape DrawRoundedRectangle(Rectangle2D rectangle, Distance radiusX, Distance radiusY) {
            return Shape.DrawRoundedRectangle(this, rectangle, radiusX, radiusY);
        }

        public Shape DrawText(string formattedText, Point2D origin) {
            return Shape.DrawText(this, formattedText, origin);
        }
        #endregion

        #region 座標計算API
        /// <summary>
        /// 親シェイプの座標系上での座標を返す
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Point2D PointToParent(Point2D value) {
            if (IsLine) {
                return value;
            }
            return Angle.Rotate(value, LocPin) + Pin - LocPin;
        }

        public Point2D PointToClient(Point2D value) {
            if (IsLine) {
                return value;
            }
            Angle a = -Angle;
            return a.Rotate(value - Pin) - LocPin;
        }

        public Point2D PointToSheet(Point2D value) {
            Point2D ret = PointToParent(value);
            if (Parent != null) {
                ret = Parent.PointToSheet(ret);
            } else {
                ret += Sheet.Offset;
            }
            return ret;
        }

        public Point2D ParentPointToSheet(Point2D value) {
            if (Parent == null) {
                Point2D p = value + Sheet.Offset;
                return p;
            }
            return Parent.PointToSheet(value);
        }

        public Point2D PointFromSheet(Point2D value) {
            Point2D ret = value;
            if (Parent != null) {
                ret = Parent.PointFromSheet(ret);
            } else {
                ret -= Sheet.Offset;
            }
            return ret;
        }

        public Point2D ParentPointFromSheet(Point2D value) {
            if (Parent == null) {
                Point2D p = value - Sheet.Offset;
                return p;
            }
            return Parent.PointFromSheet(value);
        }

        public Point2D PointToCanvas(Point2D value) {
            Point2D ret = PointToSheet(value);
            //ret += Sheet.Offset;
            return ret;
        }
        
        public Point2D PointFromCanvas(Point2D value) {
            Point2D ret = PointFromSheet(value);
            //ret -= Sheet.Offset;
            return ret;
        }

        #endregion

        // WPF側で実装すべきか
        public bool HitTest(Point2D point) {
            // 高速化のためまず外接矩形で判定
            //if (!Bounds.Contains(point)) {
            //    return false;
            //}
            return HitTestCore(point);
        }

        private Hashtable _selectedCanvas = new Hashtable();

        public void Select(Canvas canvas) {
            if (canvas != null && canvas.Selection != null) {
                canvas.Selection.Add(this);
            }
        }

        public void Deselect(Canvas canvas) {
            if (canvas != null && canvas.Selection != null) {
                canvas.Selection.Remove(this);
            }
        }

        public bool IsSelected(Canvas canvas) {
            if (canvas != null && canvas.Selection != null) {
                return canvas.Selection.Contains(this);
            }
            return false;
        }

        public KnobControl[] GetKnobControls(Canvas canvas) {
            List<KnobControl> l = new List<KnobControl>();
            foreach (IKnobInfo knob in Knobs) {
                if (knob.Visible) {
                    l.Add(knob.RequireKnobControl(canvas));
                }
            }
            return l.ToArray();
        }

        public void BringToTop(Shape shape) {
            Shapes.BringToTop(shape);
        }

        public void BringToFront(Shape shape) {
            Shapes.BringToFront(shape);
        }

        public void BringToFront(Shape shape, Shape target) {
            Shapes.BringToFront(shape, target);
        }

        public void SendToBottom(Shape shape) {
            Shapes.SendToBottom(shape);
        }

        public void SendToBack(Shape shape) {
            Shapes.SendToBack(shape);
        }

        public void SendToBack(Shape shape, Shape target) {
            Shapes.SendToBack(shape, target);
        }

        /// <summary>
        /// 指定座標を含む図形のリストを返す
        /// リストは上に表示されているものから順に格納される
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Shape[] GetShapesAt(Point2D point) {
            List<Shape> l = new List<Shape>();
            for (int i = Shapes.Count - 1; 0 <= i; i--) {
                Shape sh = Shapes[i];
                if (sh.HitTest(point)) {
                    l.Add(sh);
                }
            }
            return l.ToArray();
        }

        /// <summary>
        /// 指定座標を含む図形のうち一番上に表示されているものを返す
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Shape GetTopShapeAt(Point2D point) {
            for (int i = Shapes.Count - 1; 0 <= i; i--) {
                Shape sh = Shapes[i];
                if (sh.HitTest(point)) {
                    return sh;
                }
            }
            return null;
        }
    }
}