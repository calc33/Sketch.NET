using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Linq;
using System.Text;

namespace Sketch {
    partial class Sheet {
        public double ToDisplayOffset(Distance value) {
            return value.Px / Scaling;
        }
        public Point ToDisplayPoint(Point2D value) {
            return new Point(ToDisplayOffset(value.X), ToDisplayOffset(value.Y));
        }

        public Size ToDisplaySize(Size2D value) {
            return new Size(value.Width.Px / Scaling, value.Height.Px / Scaling);
        }

        public Distance ToDistance(double value) {
            return new Distance(value * Scaling, LengthUnit.Pixels);
        }

        public Point2D ToPoint2D(Point value) {
            return new Point2D(value.X * Scaling, value.Y * Scaling, LengthUnit.Pixels);
        }

        //public void Render(DrawingContext drawingContext, Selection movingShapes, Point diff) {
        public void RenderBackground(DrawingContext drawingContext) {
            if (Background != null) {
                Background.Render(drawingContext);
            }
        }
        public void RenderShapes(DrawingContext drawingContext) {
            try {
                foreach (Shape sh in Shapes) {
                    sh.Render(drawingContext);
                }
            } finally {
            }
        }
        public void Render(DrawingContext drawingContext) {
            RenderBackground(drawingContext);
            RenderShapes(drawingContext);
        }
    }
}
