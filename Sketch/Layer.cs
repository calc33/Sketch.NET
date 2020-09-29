using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch {
    /// <summary>
    /// レイヤーの表示
    /// </summary>
    [Flags]
    public enum LayerVisiblity {
        Displayable = 1,
        Printable = 2
    }

    [Serializable]
    public sealed class Layer : ExprComponent<Sheet>, IShapeParentable {
        private static string _defaultLayerName;
        public static string DefaultLayerName { get { return _defaultLayerName; } set { _defaultLayerName = value; } }

        private string _name;
        private ShapeCollection _shapes;

        public Document Document { get { return Container != null ? Container.Container : null; } }
        Sheet IShapeParentable.Sheet { get { return Container; } }
        Layer IShapeParentable.Layer { get { return this; } }
        Shape IShapeParentable.Parent { get { return null; } }
        public ShapeCollection Shapes {
            get {
                return _shapes;
            }
        }

        //public Layer() : base() {
        //    _shapes = new ShapeCollection(this);
        //}

        internal Layer(Sheet owner)
            : base(owner) {
            _name = DefaultLayerName;
            _shapes = new ShapeCollection(this);
        }
        internal Layer(Sheet owner, string name)
            : base(owner) {
            _name = name;
            _shapes = new ShapeCollection(this);
        }

        private static PropertyDefCollection _builtinPropertyDefs = new PropertyDefCollection(
            new PropertyDef[] {
                new PropertyDef("Visible", typeof(bool), "true"),
                new PropertyDef("Printable", typeof(bool), "true"),
            }
        );

        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinPropertyDefs;
        }

        public const int PROP_VISIBLE = 0;
        public const int PROP_PRINTABLE = 1;

        [FormulaIndex(PROP_VISIBLE)]
        public bool Visible {
            get { return Formula[PROP_VISIBLE].BooleanValue; }
            set { Formula[PROP_VISIBLE].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_PRINTABLE)]
        public bool Printable {
            get { return Formula[PROP_PRINTABLE].BooleanValue; }
            set { Formula[PROP_PRINTABLE].SetValue(value, EditingLevel.EditByValue); }
        }

        Document IShapeParentable.Document {
            get { return Container.Document; }
        }

        void IExprComponentOwnable.Add(object item) {
            Shape shp = item as Shape;
            if (shp != null) {
                Shapes.Add(shp);
            }
        }

        void IExprComponentOwnable.Remove(object item) {
            Shape shp = item as Shape;
            if (shp != null) {
                Shapes.Remove(shp);
            }
        }

        string IExprComponentOwnable.GetNewName(object target) {
            return (Container as IExprComponentOwnable).GetNewName(target);
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
