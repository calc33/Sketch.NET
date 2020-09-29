using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch {
    [Serializable]
    public sealed class Master : ExprComponent<Document>, IShapeParentable {
        //private Document _document;
        private Shape _shape;
        private ShapeCollection _singleCollection;
        public new string Name {
            get {
                return _shape.Name;
            }
            set {
                _shape.Name = value;
            }
        }
        public Document Document { get { return Container; } }
        Sheet IShapeParentable.Sheet { get { return null; } }
        Layer IShapeParentable.Layer { get { return null; } }
        Shape IShapeParentable.Parent { get { return null; } }

        void IExprComponentOwnable.Add(object item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            Shape sh = item as Shape;
            if (sh != null) {
                _shape.Shapes.Add(sh);
            }
        }

        void IExprComponentOwnable.Remove(object item) {
            _shape.Shapes.Remove(item as Shape);
        }

        string IExprComponentOwnable.GetNewName(object target) {
            return null;
        }

        public ShapeCollection Shapes {
            get {
                if (_singleCollection == null) {
                    _singleCollection = new ShapeCollection(this);
                    _singleCollection.Add(_shape);
                }
                return _singleCollection;
            }
        }

        public Shape NewShape(Layer layer, Point2D point) {
            Shape sh = new Shape(layer, point);
            throw new NotImplementedException();
        }

        private static PropertyDefCollection _builtinPropertyDefs = new PropertyDefCollection(
            new PropertyDef[] {
                new PropertyDef("Width", typeof(Distance), "0cm"),
                new PropertyDef("Height", typeof(Distance), "0cm"),
                new PropertyDef("LocPinX", typeof(Distance), "0cm"),
                new PropertyDef("LocPinY", typeof(Distance), "0cm"),
                new PropertyDef("Angle", typeof(Angle), "0deg"),
                new PropertyDef("Flipped", typeof(bool), "false"),
                new PropertyDef("IgnoreScaling", typeof(bool), "false"),
            }
        );

        public const int PROP_WIDTH = 0;
        public const int PROP_HEIGHT = 1;
        public const int PROP_LOCPINX = 2;
        public const int PROP_LOCPINY = 3;
        public const int PROP_ANGLE = 4;
        public const int PROP_FLIPPED = 5;
        public const int PROP_IGNORESCALING = 6;

        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinPropertyDefs;
        }

        public FormulaProperty FormulaWidth { get { return Formula[PROP_WIDTH]; } }
        public FormulaProperty FormulaHeight { get { return Formula[PROP_HEIGHT]; } }
        public FormulaProperty FormulaLocPinX { get { return Formula[PROP_LOCPINX]; } }
        public FormulaProperty FormulaLocPinY { get { return Formula[PROP_LOCPINY]; } }
        public FormulaProperty FormulaAngle { get { return Formula[PROP_ANGLE]; } }
        public FormulaProperty FormulaFlipped { get { return Formula[PROP_FLIPPED]; } }
        public FormulaProperty FormulaIgnoreScaling { get { return Formula[PROP_IGNORESCALING]; } }

        public Size2D Size { get { return new Size2D(Width, Height); } }

        [FormulaIndex(PROP_WIDTH)]
        public Distance Width {
            get { return Formula[PROP_WIDTH].DistanceValue; }
            set { Formula[PROP_WIDTH].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_HEIGHT)]
        public Distance Height {
            get { return Formula[PROP_HEIGHT].DistanceValue; }
            set { Formula[PROP_HEIGHT].SetValue(value, EditingLevel.EditByValue); }
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

        [FormulaIndex(false, PROP_FLIPPED)]
        public bool Flipped {
            get { return Formula[PROP_FLIPPED].BooleanValue; }
            set { Formula[PROP_FLIPPED].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_IGNORESCALING)]
        public bool IgnoreScaling {
            get { return Formula[PROP_IGNORESCALING].BooleanValue; }
            set { Formula[PROP_IGNORESCALING].SetValue(value, EditingLevel.EditByValue); }
        }

        public Master(Document owner)
            : base(owner) {
            _shape = new Shape(null, Point2D.Zero);
        }

        #region 操作用API
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
