using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch {
    [Serializable]
    public sealed partial class Sheet : ExprComponent<Document>, IExprComponentOwnable, /*IShapeParentable,*/ INamedCollectionOwnable<Layer>, INamedCollectionOwnable<Shape> {
        public class LayerCollection: NamedCollection<Layer, Sheet> {
            public LayerCollection(Sheet owner)
                : base(owner) {
                NewNameBase = "Layer";
            }
        }

        public class ShapeCollection: NamedCollection<Shape, Sheet> {
            internal ShapeCollection(Sheet owner)
                : base(owner) {
                NewNameBase = "Shape";
            }
        }
        public class SheetShapeCollection: IEnumerable<Shape> {
            internal class ShapeEnumerator: IEnumerator<Shape> {
                private IEnumerator<Layer> _layerEnumerator;
                private IEnumerator<Shape> _shapeEnumerator;
                internal ShapeEnumerator(Sheet owner) {
                    _layerEnumerator = owner.Layers.GetEnumerator();
                    _shapeEnumerator = null;
                }

                public Shape Current {
                    get {
                        return (_shapeEnumerator != null) ? _shapeEnumerator.Current : null;
                    }
                }

                public void Dispose() {
                    if (_shapeEnumerator != null) {
                        _shapeEnumerator.Dispose();
                        _shapeEnumerator = null;
                    }
                    if (_layerEnumerator != null) {
                        _layerEnumerator.Dispose();
                        _layerEnumerator = null;
                    }
                }

                object IEnumerator.Current {
                    get {
                        return (_shapeEnumerator != null) ? _shapeEnumerator.Current : null;
                    }
                }

                public bool MoveNext() {
                    if (_shapeEnumerator == null) {
                        if (!_layerEnumerator.MoveNext()) {
                            return false;
                        }
                        if (_layerEnumerator.Current == null) {
                            _shapeEnumerator = null;
                            return false;
                        }
                        _shapeEnumerator = _layerEnumerator.Current.Shapes.GetEnumerator();
                    }
                    if (_shapeEnumerator.MoveNext()) {
                        return true;
                    }
                    if (!_layerEnumerator.MoveNext()) {
                        return false;
                    }
                    if (_shapeEnumerator != null) {
                        _shapeEnumerator.Dispose();
                    }
                    if (_layerEnumerator.Current == null) {
                        _shapeEnumerator = null;
                        return false;
                    }
                    _shapeEnumerator = _layerEnumerator.Current.Shapes.GetEnumerator();
                    return _shapeEnumerator.MoveNext();
                }

                public void Reset() {
                    _layerEnumerator.Reset();
                    if (_shapeEnumerator != null) {
                        _shapeEnumerator.Dispose();
                        _shapeEnumerator = (_layerEnumerator.Current != null) ? _layerEnumerator.Current.Shapes.GetEnumerator() : null;
                    }
                }
            }
            private Sheet _owner;

            public SheetShapeCollection(Sheet owner) {
                _owner = owner;
            }

            public IEnumerator<Shape> GetEnumerator() {
                return new ShapeEnumerator(_owner);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new ShapeEnumerator(_owner);
            }

            public Shape this[int index] {
                get {
                    if (index < 0) {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    int n = 0;
                    foreach (Layer l in _owner.Layers) {
                        int n0 = n;
                        n += l.Shapes.Count;
                        if (n0 <= index && index < n) {
                            return l.Shapes[index - n0];
                        }
                    }
                    throw new ArgumentOutOfRangeException("index");
                }
            }

            public int Count {
                get {
                    int n = 0;
                    foreach (Layer l in _owner.Layers) {
                        n += l.Shapes.Count;
                    }
                    return n;
                }
            }
        }

        private Sheet _background;
        private ShapeCollection _allShapes;
        private SheetShapeCollection _shapes;
        private LayerCollection _layers;
        private Layer _currentLayer;

        public Sheet Background {
            get {
                return _background;
            }
            set {
                _background = value;
            }
        }

        public Document Document {
            get {
                return Container;
            }
        }

        public SheetShapeCollection Shapes { get { return _shapes; } }

        public LayerCollection Layers { get { return _layers; } }

        public ShapeCollection AllShapes { get { return _allShapes; } }

        //public Layer ActiveLayer {
        //    get { return _activeLayer; }
        //    set {
        //        if (!_layers.Contains(value)) {
        //            throw new ArgumentOutOfRangeException("ActiveLayer");
        //        }
        //        _activeLayer = value;
        //    }
        //}

        public Sheet(Document container)
            : base(container) {
            _shapes = new SheetShapeCollection(this);
            _layers = new LayerCollection(this);
            _allShapes = new ShapeCollection(this);
        }

        private static PropertyDefCollection _builtinPropertyDefs = new PropertyDefCollection(
            new PropertyDef[] {
                new PropertyDef("Flipped", typeof(bool), "false"),
                new PropertyDef("BackgroundSheet", typeof(Sheet), null),
                new PropertyDef("Scaling", typeof(double), "Document.DefaultScaling"),
                new PropertyDef("SheetWidth", typeof(Distance), "Document.DefaultSheetWidth"),
                new PropertyDef("SheetHeight", typeof(Distance), "Document.DefaultSheetHeight"),
                new PropertyDef("PaperSizeName", typeof(string), "Document.DefaultPaperSizeName"),
                new PropertyDef("PaperWidth", typeof(Distance), "Document.DefaultPaperWidth"),
                new PropertyDef("PaperHeight", typeof(Distance), "Document.DefaultPaperHeight"),
                new PropertyDef("Angle", typeof(Angle), "0deg"),
                new PropertyDef("OffsetX", typeof(Distance), "0cm"),
                new PropertyDef("OffsetY", typeof(Distance), "0cm"),
            }
        );

        public const int PROP_FLIPPED = 0;
        public const int PROP_BACKGROUPNDSHEET = 1;
        public const int PROP_SCALING = 2;
        public const int PROP_SHEETWIDTH = 3;
        public const int PROP_SHEETHEIGHT = 4;
        public const int PROP_PAPERSIZENAME = 5;
        public const int PROP_PAPERWIDTH = 6;
        public const int PROP_PAPERHEIGHT = 7;
        public const int PROP_ANGLE = 8;
        public const int PROP_OFFSETX = 9;
        public const int PROP_OFFSETY = 10;

        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinPropertyDefs;
        }

        [FormulaIndex(PROP_FLIPPED)]
        public bool Flipped {
            get { return Formula[PROP_FLIPPED].BooleanValue; }
            set { Formula[PROP_FLIPPED].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_SCALING)]
        public double Scaling {
            get { return Formula[PROP_SCALING].DoubleValue; }
            set { Formula[PROP_SCALING].SetValue(value, EditingLevel.EditByValue); }
        }

        public Size2D SheetSize { get { return new Size2D(SheetWidth, SheetHeight); } }

        [FormulaIndex(PROP_SHEETWIDTH)]
        public Distance SheetWidth {
            get { return Formula[PROP_SHEETWIDTH].DistanceValue; }
            set { Formula[PROP_SHEETWIDTH].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_SHEETHEIGHT)]
        public Distance SheetHeight {
            get { return Formula[PROP_SHEETHEIGHT].DistanceValue; }
            set { Formula[PROP_SHEETHEIGHT].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_PAPERSIZENAME)]
        public string PaperSizeName {
            get { return Formula[PROP_PAPERSIZENAME].StringValue; }
            set { Formula[PROP_PAPERSIZENAME].SetValue(value, EditingLevel.EditByValue); }
        }
        
        public Size2D PaperSize { get { return new Size2D(PaperWidth, PaperHeight); } }

        [FormulaIndex(PROP_PAPERWIDTH)]
        public Distance PaperWidth {
            get { return Formula[PROP_PAPERWIDTH].DistanceValue; }
            set { Formula[PROP_PAPERWIDTH].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_PAPERHEIGHT)]
        public Distance PaperHeight {
            get { return Formula[PROP_PAPERHEIGHT].DistanceValue; }
            set { Formula[PROP_PAPERHEIGHT].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_ANGLE)]
        public Angle Angle {
            get { return Formula[PROP_ANGLE].AngleValue; }
            set { Formula[PROP_ANGLE].SetValue(value, EditingLevel.EditByValue); }
        }

        public Point2D Offset {
            get { return new Point2D(OffsetX, OffsetY); }
        }

        [FormulaIndex(PROP_OFFSETX)]
        public Distance OffsetX {
            get { return Formula[PROP_OFFSETX].DistanceValue; }
            set { Formula[PROP_OFFSETX].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_OFFSETY)]
        public Distance OffsetY {
            get { return Formula[PROP_OFFSETY].DistanceValue; }
            set { Formula[PROP_OFFSETY].SetValue(value, EditingLevel.EditByValue); }
        }

        //Sheet IShapeParentable.Sheet { get { return this; } }

        public Layer CurrentLayer {
            get { return _currentLayer; }
            set {
                if (!_layers.Contains(value)) {
                    throw new ArgumentOutOfRangeException("CurrentLayer");
                }
                _currentLayer = value;
            }
        }

        public Shape Parent { get { return null; } }

        void IExprComponentOwnable.Add(object item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            Layer l = item as Layer;
            if (l != null) {
                Layers.Add(l);
                if (_currentLayer == null) {
                    _currentLayer = l;
                }
            }
            Shape s = item as Shape;
            if (s != null) {
                AllShapes.Add(s);
            }
        }

        void IExprComponentOwnable.Remove(object item) {
            Layers.Remove(item as Layer);
            if (item == _currentLayer) {
                _currentLayer = (0 < Layers.Count) ? Layers.Last<Layer>() : null;
            }
            if (item is Shape) {
                AllShapes.Remove((Shape)item);
            }
        }

        string IExprComponentOwnable.GetNewName(object target) {
            if (target == null) {
                throw new ArgumentNullException();
            }
            Layer l = target as Layer;
            if (l != null) {
                return Layers.GetNewName(l.Name);
            }
            Shape s = target as Shape;
            if (s != null) {
                _allShapes.GetNewName();
            }
            throw new ArgumentException();
        }

        void INamedCollectionOwnable<Layer>.OnCollectionItemAdded(object collection, Layer item) {
            //
        }

        void INamedCollectionOwnable<Layer>.OnCollectionItemRemoved(object collection, Layer item) {
            //
        }

        void INamedCollectionOwnable<Shape>.OnCollectionItemAdded(object collection, Shape item) {
            //
        }

        void INamedCollectionOwnable<Shape>.OnCollectionItemRemoved(object collection, Shape item) {
            //
        }

        //public Shape[] GetSelectableShapesAt(Point2D point) {
        //    List<Shape> l = new List<Shape>();
        //    return l.ToArray();
        //}

        #region 操作用API
        public Shape DrawLine(Point2D startPoint, Point2D endPoint) {
            return Shape.DrawLine(CurrentLayer, startPoint, endPoint);
        }

        public Shape DrawLine(Distance startX, Distance startY, Distance endX, Distance endY) {
            return Shape.DrawLine(CurrentLayer, startX, startY, endX, endY);
        }

        public Shape DrawLine(string startX, string startY, string endX, string endY) {
            return Shape.DrawLine(CurrentLayer, startX, startY, endX, endY);
        }

        public Shape DrawPolyLine(Point2D[] points) {
            return Shape.DrawPolyLine(CurrentLayer, points);
        }

        public Shape DrawPolygon(Point2D[] points) {
            return Shape.DrawPolygon(CurrentLayer, points);
        }

        public Shape DrawEllipse(Point2D center, Distance radiusX, Distance radiusY) {
            return Shape.DrawEllipse(CurrentLayer, center, radiusX, radiusY);
        }

        public Shape DrawEllipse(Point2D center, Distance radiusX, Distance radiusY, Angle rotatingAngle) {
            return Shape.DrawEllipse(CurrentLayer, center, radiusX, radiusY, rotatingAngle);
        }

        public Shape DrawArc(Point2D startPoint, Point2D endPoint, Distance radiusX, Distance radiusY, Angle rotatingAngle, bool isLargeArc, bool isClockwise, PathTermination termination) {
            return Shape.DrawArc(CurrentLayer, startPoint, endPoint, radiusX, radiusY, rotatingAngle, isLargeArc, isClockwise, termination);
        }

        //Shape DrawImage(ImageSource imageSource, Rect rectangle);
        public Shape DrawRectangle(Rectangle2D rectangle) {
            return Shape.DrawRectangle(CurrentLayer, rectangle);
        }

        public Shape DrawRoundedRectangle(Rectangle2D rectangle, Distance radiusX, Distance radiusY) {
            return Shape.DrawRoundedRectangle(CurrentLayer, rectangle, radiusX, radiusY);
        }

        public Shape DrawText(string formattedText, Point2D origin) {
            return Shape.DrawText(CurrentLayer, formattedText, origin);
        }
        #endregion
    }
}
