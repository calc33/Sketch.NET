using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sketch;

namespace AppStub {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow: Window {
        private Sketch.Document _document;
        private Sketch.Shape _aShape;
        public MainWindow() {
            InitializeComponent();
            Size2D s = PaperSize.A4;
            SketchSettings.Settings.DefaultSheetWidth = s.Width;
            SketchSettings.Settings.DefaultSheetHeight = s.Height;
            _document = Document.NewDocument();
            Sheet sh = _document.Sheets[0];
            sh.Scaling = 1;
            sh.OffsetX = sh.SheetWidth / 2;
            sh.OffsetY = sh.SheetHeight / 2;
            canvas1.Sheet = sh;
            canvas1.Scaling = 1;
            canvas1.Width = sh.SheetWidth.Px * canvas1.Scaling;
            canvas1.Height = sh.SheetHeight.Px * canvas1.Scaling;
            canvas1.VisibleCenter = new Point2D(sh.SheetWidth / 2, sh.SheetHeight / 2);
            canvas1.RotatingAngle = new Angle(0, AngleUnit.Degree);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Sheet sh = _document.Sheets[0];
            //sh.OffsetX = sh.SheetWidth / 2;
            //sh.OffsetY = sh.SheetHeight / 2;
            //canvas1.Sheet = sh;
            //canvas1.Scaling = 1;
            //canvas1.Width = sh.SheetWidth.Px * canvas1.Scaling;
            //canvas1.Height = sh.SheetHeight.Px * canvas1.Scaling;
            //canvas1.VisibleCenter = new Point2D(sh.SheetWidth / 2, sh.SheetHeight / 2);
            //canvas1.RotateAngle = new Angle(0, AngleUnit.Degree);
            //Sketch.Shape shp = sh.DrawLine(Point2D.Zero, new Point2D(10, 10, LengthUnit.Centimeters));
            //shp.LineColor = Colors.Red;
            //sh.DrawLine(Point2D.Zero, new Point2D(-10, 10, LengthUnit.Centimeters));
            ////sh.DrawLine(new Point2D(10, 10, LengthUnit.Centimeters), new Point2D(0, 20, LengthUnit.Centimeters));
            //sh.DrawLine(new Point2D(0, 1, LengthUnit.Centimeters), new Point2D(1, 0, LengthUnit.Centimeters));
            //sh.DrawLine(new Point2D(1, 0, LengthUnit.Centimeters), new Point2D(0, -1, LengthUnit.Centimeters));
            //sh.DrawLine(new Point2D(0, -1, LengthUnit.Centimeters), new Point2D(-1, 0, LengthUnit.Centimeters));
            //sh.DrawLine(new Point2D(-1, 0, LengthUnit.Centimeters), new Point2D(0, 1, LengthUnit.Centimeters));
            ////shp = sh.DrawEllipse(Point2D.Zero, new Distance(3, LengthUnit.Centimeters), new Distance(2, LengthUnit.Centimeters), new Angle(30, AngleUnit.Degree));
            ////shp = sh.DrawEllipse(Point2D.Zero, new Distance(4, LengthUnit.Centimeters), new Distance(2, LengthUnit.Centimeters), new Angle(15, AngleUnit.Degree));

            //sh.DrawLine(new Point2D(-sh.OffsetX, Distance.Zero), new Point2D(sh.SheetWidth - sh.OffsetX, Distance.Zero));
            //sh.DrawLine(new Point2D(Distance.Zero, -sh.OffsetY), new Point2D(Distance.Zero, sh.SheetHeight - sh.OffsetY));
            Sketch.Shape shp = sh.DrawArc(new Point2D(0, 0, LengthUnit.Centimeters), new Point2D(2, 0, LengthUnit.Centimeters), new Distance(2, LengthUnit.Centimeters), new Distance(1, LengthUnit.Centimeters),
                new Angle(0, AngleUnit.Degree), true, true, PathTermination.Close);
            shp.FillRule = Sketch.FillRule.EvenOdd;
            shp.FillColor = Colors.Aqua;
            shp.Fill = true;
            shp.DrawEllipse(new Point2D(2, 1, LengthUnit.Centimeters), new Distance(0.25, LengthUnit.Centimeters), new Distance(0.25, LengthUnit.Centimeters));
            shp.LineColor = Colors.Black;
            shp.LineWidth = new Distance(1, LengthUnit.Pixels);
            _aShape = shp;

            Sketch.Shape shp2 = sh.DrawArc(new Point2D(0, 3, LengthUnit.Centimeters), new Point2D(2, 3, LengthUnit.Centimeters), new Distance(2, LengthUnit.Centimeters), new Distance(1, LengthUnit.Centimeters),
                new Angle(120, AngleUnit.Degree), true, true, PathTermination.Stroke);
            shp2.FillColor = Colors.White;
            shp2.Fill = true;
            shp2.LineColor = Colors.Black;
            shp2.LineWidth = new Distance(1, LengthUnit.Pixels);
            //_aShape = shp2;

            Sketch.Shape shp3 = sh.DrawArc(new Point2D(0, 6, LengthUnit.Centimeters), new Point2D(2, 6, LengthUnit.Centimeters), new Distance(2, LengthUnit.Centimeters), new Distance(1, LengthUnit.Centimeters),
                new Angle(90, AngleUnit.Degree), true, true, PathTermination.Stroke);
            shp3.FillColor = Colors.White;
            shp3.Fill = true;
            shp3.LineColor = Colors.Black;
            shp3.LineWidth = new Distance(1, LengthUnit.Pixels);

            _aShape.Select(canvas1);
            Rect r = _aShape.DisplayBounds;
            string s2 = r.ToString() + Environment.NewLine;
            Point p1 = canvas1.PointToScreen(new Point());
            p1 = canvas1.PointToScreen(r.Location);
            s2 += p1.ToString() + Environment.NewLine;
            Point p2 = canvas1.PointToScreen(new Point());
            p2.X = p1.X - p2.X;
            p2.Y = p1.Y - p2.Y;
            s2 += p2.ToString() + Environment.NewLine;
            textBox1.Text = s2;
            //shp.LockAspect = true;
            //CollectionViewSource formulaPropertyCollectionViewSource = ((CollectionViewSource)(this.FindResource("formulaPropertyCollectionViewSource")));
            //formulaPropertyCollectionViewSource.Source = _aShape.Formula;
        }

        private void formulaDataGrid_GotFocus(object sender, RoutedEventArgs e) {
            CollectionViewSource formulaPropertyCollectionViewSource = ((CollectionViewSource)(this.FindResource("formulaPropertyCollectionViewSource")));
            formulaPropertyCollectionViewSource.Source = null;
            formulaPropertyCollectionViewSource.Source = _aShape.Formula;
        }

        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e) {
            Label1.Content = "MouseDown";
        }

        private void canvas1_MouseUp(object sender, MouseButtonEventArgs e) {
            Label1.Content = "MouseUp";
        }

        private void canvas1_MouseEnter(object sender, MouseEventArgs e) {
            Label1.Content = "MouseEnter";
        }

        private void canvas1_MouseLeave(object sender, MouseEventArgs e) {
            Label1.Content = "MouseLeave";
        }

        private void canvas1_MouseMove(object sender, MouseEventArgs e) {
            Point p = e.GetPosition(canvas1);
            Label2.Content = string.Format("MouseMove({0}, {1})", p.X, p.Y);
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e) {
            scrollViewer1.ScrollToVerticalOffset(100);
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {

        }
    }
}
