using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;


namespace Sketch {
    partial class Selection : Thumb {
        //private Dictionary<Shape, Point2D> _startPoints = null;
        public Dictionary<Shape, Point2D> GetShapePoints()
        {
            Dictionary<Shape, Point2D> ret = new Dictionary<Shape, Point2D>();
            if (_items.Count == 0) {
                return ret;
            }
            foreach (Shape sh in _items) {
                ret.Add(sh, sh.Pin);
            }
            return ret;
        }

        public void OnMouseDown(MouseButtonEventArgs e, bool isFirstResponder) { }

        public void OnPreviewMouseMove(MouseEventArgs e, bool isFirstResponder) { }

        public void OnMouseMove(MouseEventArgs e, bool isFirstResponder) {
            InvalidateBounds();
        }

        public void OnMouseUp(MouseButtonEventArgs e, bool isFirstResponder) { }

        public void OnMouseLeave(MouseEventArgs e, bool isFirstResponder) { }

        public void OnMouseEnter(MouseEventArgs e, bool isFirstResponder) { }

        private static readonly Brush TRANSPARENT_BRUSH = new SolidColorBrush(Colors.Transparent);
        public void PreRender(DrawingContext drawingContext) {
            //foreach (Shape sh in _items) {
            //    sh.Render(drawingContext);
            //}
            if (1 < Count) {
                Canvas c = Owner;
                if (c != null) {
                    Rectangle2D r = Bounds;
                    Rect rect = new Rect(c.Sheet.ToDisplayPoint(r.TopLeft), c.Sheet.ToDisplayPoint(r.BottomRight));
                    drawingContext.DrawRectangle(TRANSPARENT_BRUSH, RangeSelectingMouseBehavior.RangePen, rect);
                }
            }
        }
        public void PostRender(DrawingContext drawingContext) { }
    }
}
