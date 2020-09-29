using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Sketch {

    /// <summary>
    /// 二次元座標を表現するためのクラス
    /// </summary>
    public struct Point2D {
        #region メンバーの宣言
        private Distance _x;
        private Distance _y;
        public Distance X {
            get { return _x; }
            set { _x = value; }
        }
        public Distance Y {
            get { return _y; }
            set { _y = value; }
        }
        #endregion

        #region 文字列化処理
        public override string ToString() {
            return string.Format("[{0}, {1}]", X, Y);
        }

        public static bool TryParse(string input, out Point2D result) {
            string s = input.Trim();
            if (!s.StartsWith("[") || !s.EndsWith("]")) {
                result = Zero;
                return false;
            }
            List<Distance> vals = new List<Distance>();
            foreach (string v in s.Split('[', ']', ',')) {
                if (!string.IsNullOrWhiteSpace(v)) {
                    Distance d;
                    if (!Distance.TryParse(v, out d)) {
                        result = Zero;
                        return false;
                    }
                    vals.Add(d);
                }
            }
            if (vals.Count != 2){
                result = Zero;
                return false;
            }
            result = new Point2D(vals[0], vals[1]);
            return true;
        }

        public static Point2D Parse(string input) {
            return new Point2D(input);
        }
        #endregion

        #region コンストラクター
        public Point2D(Distance x, Distance y) {
            _x = x;
            _y = y;
        }

        public Point2D(double x, double y, LengthUnit unit) {
            _x = new Distance(x, unit);
            _y = new Distance(y, unit);
        }

        public Point2D(string value) {
            string s = value.Trim();
            if (!s.StartsWith("[") || !s.EndsWith("]")) {
                throw new ArgumentException();
            }
            List<Distance> vals = new List<Distance>();
            foreach (string v in s.Split('[', ']', ',')) {
                if (!string.IsNullOrWhiteSpace(v)) {
                    Distance d;
                    if (!Distance.TryParse(v, out d)) {
                        throw new ArgumentException();
                    }
                    vals.Add(d);
                }
            }
            if (vals.Count != 2){
                throw new ArgumentException();
            }
            _x = vals[0];
            _y = vals[1];
        }
        #endregion

        #region 演算子の定義と演算関連のメソッド
        /// <summary>
        /// X軸との角度を取得
        /// </summary>
        /// <returns></returns>
        public Angle Angle() {
            return new Angle(Math.Atan2(Y.Value, X[Y.Unit]), AngleUnit.Radian);
        }

        /// <summary>
        /// baseLineとの角度を取得。(baseLineから左回り)
        /// </summary>
        /// <param name="baseLine"></param>
        /// <returns></returns>
        public Angle Angle(Point2D baseLine) {
            return (Angle() - baseLine.Angle()).Normalize(false);
        }

        /// <summary>
        /// 原点を中心に角度aだけ回転した座標を取得
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Point2D Rotate(Angle a) {
            return new Point2D(X * a.Cos() - Y * a.Sin(), X * a.Sin() + Y * a.Cos());
        }

        /// <summary>
        /// basePointを中心に角度aだけ回転した座標を取得
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public Point2D Rotate(Point2D basePoint, Angle a) {
            return basePoint + (this - basePoint).Rotate(a);
        }

        /// <summary>
        /// 原点との距離を返す
        /// </summary>
        /// <returns></returns>
        public Distance GetDistance() {
            LengthUnit lu = X.Unit;
            double x = X[lu];
            double y = Y[lu];
            return new Distance(Math.Sqrt(x * x + y * y), lu);
        }

        /// <summary>
        /// basePointとの距離を返す
        /// </summary>
        /// <param name="basePoint"></param>
        /// <returns></returns>
        public Distance GetDistance(Point2D basePoint) {
            LengthUnit lu = X.Unit;
            double x = X[lu] - basePoint.X[lu];
            double y = Y[lu] - basePoint.Y[lu];
            return new Distance(Math.Sqrt(x * x + y * y), lu);
        }

        /// <summary>
        /// ベクトルのスカラー積(内積)を求める
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double ScalarProduct(Point2D a, Point2D b, LengthUnit unit) {
            return a.X[unit] * b.X[unit] + a.Y[unit] * b.Y[unit];
        }

        /// <summary>
        /// ベクトルのクロス積(外積)を求める
        /// (平面上のベクトル同士なのでZ軸値だけを返す)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double CrossProductZ(Point2D a, Point2D b, LengthUnit unit) {
            return a.X[unit] * b.Y[unit] - a.Y[unit] * b.X[unit];
        }

        public Point ToPoint(LengthUnit unit) {
            return new Point(X[unit], Y[unit]);
        }

        public bool IsZero() {
            return X.IsZero() && Y.IsZero();
        }

        private static Point2D _zero = new Point2D(0, 0, LengthUnit.Meters);
        public static Point2D Zero { get { return _zero; } }

        public static Point2D operator +(Point2D a, Point2D b) {
            return new Point2D(a.X + b.X, a.Y + b.Y);
        }
        public static Point2D operator +(Point2D a, Size2D b) {
            return new Point2D(a.X + b.Width, a.Y + b.Height);
        }
        public static Point2D operator +(Size2D a, Point2D b) {
            return new Point2D(a.Width + b.X, a.Height + b.Y);
        }
        public static Point2D operator -(Point2D a, Size2D b) {
            return new Point2D(a.X - b.Width, a.Y - b.Height);
        }
        public static Point2D operator -(Size2D a, Point2D b) {
            return new Point2D(a.Width - b.X, a.Height - b.Y);
        }
        
        public static Point2D operator -(Point2D a, Point2D b) {
            return new Point2D(a.X - b.X, a.Y - b.Y);
        }

        public static Point2D operator -(Point2D a) {
            return new Point2D(-a.X, -a.Y);
        }

        public static Point2D operator *(short a, Point2D b) {
            return new Point2D(a * b.X, a * b.Y);
        }
        public static Point2D operator *(int a, Point2D b) {
            return new Point2D(a * b.X, a * b.Y);
        }
        public static Point2D operator *(long a, Point2D b) {
            return new Point2D(a * b.X, a * b.Y);
        }
        public static Point2D operator *(float a, Point2D b) {
            return new Point2D(a * b.X, a * b.Y);
        }
        public static Point2D operator *(double a, Point2D b) {
            return new Point2D(a * b.X, a * b.Y);
        }

        public static Point2D operator *(Point2D a, short b) {
            return new Point2D(b * a.X, b * a.Y);
        }
        public static Point2D operator *(Point2D a, int b) {
            return new Point2D(b * a.X, b * a.Y);
        }
        public static Point2D operator *(Point2D a, long b) {
            return new Point2D(b * a.X, b * a.Y);
        }
        public static Point2D operator *(Point2D a, float b) {
            return new Point2D(b * a.X, b * a.Y);
        }
        public static Point2D operator *(Point2D a, double b) {
            return new Point2D(b * a.X, b * a.Y);
        }

        public static Point2D operator /(Point2D a, short b) {
            return new Point2D(a.X / b, a.Y / b);
        }
        public static Point2D operator /(Point2D a, int b) {
            return new Point2D(a.X / b, a.Y / b);
        }
        public static Point2D operator /(Point2D a, long b) {
            return new Point2D(b * a.X, a.Y / b);
        }
        public static Point2D operator /(Point2D a, float b) {
            return new Point2D(a.X / b, a.Y / b);
        }
        public static Point2D operator /(Point2D a, double b) {
            return new Point2D(a.X / b, a.Y / b);
        }

        public override bool Equals(object obj) {
            if (!(obj is Point2D)) {
                return base.Equals(obj);
            }
            return X == ((Point2D)obj).X && Y == ((Point2D)obj).Y;
        }

        public override int GetHashCode() {
            return X.GetHashCode() + Y.GetHashCode();
        }

        public static bool operator ==(Point2D a, Point2D b) {
            return (a.X == b.X) && (a.Y == b.Y);
        }
        public static bool operator !=(Point2D a, Point2D b) {
            return (a.X != b.X) || (a.Y != b.Y);
        }
        #endregion
    }

    /// <summary>
    /// 平面上のサイズを表現するクラス
    /// </summary>
    public struct Size2D {
        #region メンバー宣言
        private Distance _width;
        private Distance _height;
        public Distance Width {
            get { return _width; }
            set { _width = value; }
        }
        public Distance Height {
            get { return _height; }
            set { _height = value; }
        }
        #endregion

        #region 文字列化処理
        public override string ToString() {
            return string.Format("[{0}, {1}]", Width, Height);
        }

        public static bool TryParse(string input, out Size2D result) {
            string s = input.Trim();
            if (!s.StartsWith("[") || !s.EndsWith("]")) {
                result = Zero;
                return false;
            }
            List<Distance> vals = new List<Distance>();
            foreach (string v in s.Split('[', ']', ',')) {
                if (!string.IsNullOrWhiteSpace(v)) {
                    Distance d;
                    if (!Distance.TryParse(v, out d)) {
                        result = Zero;
                        return false;
                    }
                    vals.Add(d);
                }
            }
            if (vals.Count != 2) {
                result = Zero;
                return false;
            }
            result = new Size2D(vals[0], vals[1]);
            return true;
        }

        public static Size2D Parse(string input) {
            return new Size2D(input);
        }
        #endregion

        #region コンストラクタ
        public Size2D(Distance w, Distance h) {
            _width = w;
            _height = h;
        }

        //public Size2D(double w, double h) {
        //    _width = new Distance(w);
        //    _height = new Distance(h);
        //}

        public Size2D(double w, double h, LengthUnit unit) {
            _width = new Distance(w, unit);
            _height = new Distance(h, unit);
        }

        public Size2D(Point2D start, Point2D end) {
            _width = end.X - start.X;
            _height = end.Y - start.Y;
        }

        public Size2D(string value) {
            string s = value.Trim();
            if (!s.StartsWith("[") || !s.EndsWith("]")) {
                throw new ArgumentException();
            }
            List<Distance> vals = new List<Distance>();
            foreach (string v in s.Split('[', ']', ',')) {
                if (!string.IsNullOrWhiteSpace(v)) {
                    Distance d;
                    if (!Distance.TryParse(v, out d)) {
                        throw new ArgumentException();
                    }
                    vals.Add(d);
                }
            }
            if (vals.Count != 2){
                throw new ArgumentException();
            }
            _width = vals[0];
            _height = vals[1];
        }
        #endregion

        #region 演算子の定義と演算関連のメソッド
        public static Size2D operator +(Size2D a, Size2D b) {
            return new Size2D(a.Width + b.Width, a.Height + b.Height);
        }
        
        public static Size2D operator -(Size2D a, Size2D b) {
            return new Size2D(a.Width - b.Width, a.Height - b.Height);
        }

        public static Size2D operator -(Size2D a) {
            return new Size2D(-a.Width, -a.Height);
        }

        public static Size2D operator *(short a, Size2D b) {
            return new Size2D(a * b.Width, a * b.Height);
        }
        public static Size2D operator *(int a, Size2D b) {
            return new Size2D(a * b.Width, a * b.Height);
        }
        public static Size2D operator *(long a, Size2D b) {
            return new Size2D(a * b.Width, a * b.Height);
        }
        public static Size2D operator *(float a, Size2D b) {
            return new Size2D(a * b.Width, a * b.Height);
        }
        public static Size2D operator *(double a, Size2D b) {
            return new Size2D(a * b.Width, a * b.Height);
        }

        public static Size2D operator *(Size2D a, short b) {
            return new Size2D(b * a.Width, b * a.Height);
        }
        public static Size2D operator *(Size2D a, int b) {
            return new Size2D(b * a.Width, b * a.Height);
        }
        public static Size2D operator *(Size2D a, long b) {
            return new Size2D(b * a.Width, b * a.Height);
        }
        public static Size2D operator *(Size2D a, float b) {
            return new Size2D(b * a.Width, b * a.Height);
        }
        public static Size2D operator *(Size2D a, double b) {
            return new Size2D(b * a.Width, b * a.Height);
        }

        public static Size2D operator /(Size2D a, short b) {
            return new Size2D(a.Width / b, a.Height / b);
        }
        public static Size2D operator /(Size2D a, int b) {
            return new Size2D(a.Width / b, a.Height / b);
        }
        public static Size2D operator /(Size2D a, long b) {
            return new Size2D(b * a.Width, a.Height / b);
        }
        public static Size2D operator /(Size2D a, float b) {
            return new Size2D(a.Width / b, a.Height / b);
        }
        public static Size2D operator /(Size2D a, double b) {
            return new Size2D(a.Width / b, a.Height / b);
        }

        public override bool Equals(object obj) {
            if (!(obj is Size2D)) {
                return base.Equals(obj);
            }
            return Width == ((Size2D)obj).Width && Height == ((Size2D)obj).Height;
        }
        public override int GetHashCode() {
            return Width.GetHashCode() + Height.GetHashCode();
        }

        public static bool operator ==(Size2D a, Size2D b) {
            return (a.Width == b.Width) && (a.Height == b.Height);
        }
        public static bool operator !=(Size2D a, Size2D b) {
            return (a.Width != b.Width) || (a.Height != b.Height);
        }

        public Size ToSize(LengthUnit unit) {
            return new Size(Width[unit], Height[unit]);
        }

        public bool IsZero() {
            return (_width.IsZero() && _height.IsZero());
        }

        private static Size2D _zero = new Size2D(0.0, 0.0, Distance.DefaultUnit);
        public static Size2D Zero {
           get {
                return _zero;
            }
        }
        #endregion

    }

    public class PaperSize {
        private static Size2D[] _a = new Size2D[] {
            new Size2D(841, 1189, LengthUnit.Millimeters),
            new Size2D(594, 841, LengthUnit.Millimeters),
            new Size2D(420, 594, LengthUnit.Millimeters),
            new Size2D(297, 420, LengthUnit.Millimeters),
            new Size2D(210, 297, LengthUnit.Millimeters),
            new Size2D(148, 210, LengthUnit.Millimeters),
            new Size2D(105, 148, LengthUnit.Millimeters),
            new Size2D(74, 105, LengthUnit.Millimeters),
        };
        private static Size2D[] _b = new Size2D[] {
            new Size2D(1030, 1456, LengthUnit.Millimeters),
            new Size2D(728, 1030, LengthUnit.Millimeters),
            new Size2D(515, 728, LengthUnit.Millimeters),
            new Size2D(364, 515, LengthUnit.Millimeters),
            new Size2D(257, 364, LengthUnit.Millimeters),
            new Size2D(182, 257, LengthUnit.Millimeters),
            new Size2D(128, 182, LengthUnit.Millimeters),
            new Size2D(91, 128, LengthUnit.Millimeters),
        };
        private static Size2D _postcard = new Size2D(100, 148, LengthUnit.Millimeters);

        public static Size2D A0 { get { return _a[0]; } }
        public static Size2D A1 { get { return _a[1]; } }
        public static Size2D A2 { get { return _a[2]; } }
        public static Size2D A3 { get { return _a[3]; } }
        public static Size2D A4 { get { return _a[4]; } }
        public static Size2D A5 { get { return _a[5]; } }
        public static Size2D A6 { get { return _a[6]; } }
        public static Size2D A7 { get { return _a[7]; } }

        public static Size2D B0 { get { return _b[0]; } }
        public static Size2D B1 { get { return _b[1]; } }
        public static Size2D B2 { get { return _b[2]; } }
        public static Size2D B3 { get { return _b[3]; } }
        public static Size2D B4 { get { return _b[4]; } }
        public static Size2D B5 { get { return _b[5]; } }
        public static Size2D B6 { get { return _b[6]; } }
        public static Size2D B7 { get { return _b[7]; } }

        public static Size2D Postcard { get { return _postcard; } }
    }

    /// <summary>
    /// 平面状の矩形を表現するためのクラス
    /// </summary>
    public struct Rectangle2D {
        #region メンバー宣言
        private Point2D _location;
        private Size2D _size;

        public Point2D Location {
            get { return _location; }
            set { _location = value; }
        }
        public Size2D Size {
            get { return _size; }
            set { _size = value; }
        }
        public Distance Top { get { return _location.Y; } }
        public Distance Left { get { return Location.X; } }
        public Distance Bottom { get { return Location.Y + Size.Height; } }
        public Distance Right { get { return Location.X + Size.Width; } }
        public Point2D TopLeft { get { return _location; } }
        public Point2D TopRight { get { return new Point2D(_location.X + _size.Width, _location.Y); } }
        public Point2D BottomLeft { get { return new Point2D(_location.X, _location.Y + _size.Height); } }
        public Point2D BottomRight { get { return new Point2D(_location.X + _size.Width, _location.Y + _size.Height); } }
        public Distance X { get { return _location.X; } }
        public Distance Y { get { return _location.Y; } }
        public Distance Width { get { return _size.Width; } }
        public Distance Height { get { return _size.Height; } }
        public Point2D Center { get { return new Point2D(_location.X + _size.Width / 2, _location.Y + _size.Height / 2); } }
        public Distance CenterX { get { return _location.X + _size.Width / 2; } }
        public Distance CenterY { get { return _location.Y + _size.Height / 2; } }
        #endregion

        #region 文字列化処理
        public override string ToString() {
            return string.Format("[X={0}, Y={1}, Width={2}, Height={3}]", X, Y, Width, Height);
        }

        public static bool TryParse(string input, out Rectangle2D result) {
            string str = input.Trim();
            if (!str.StartsWith("[") || !str.EndsWith("]")) {
                result = Empty;
                return false;
            }

            Dictionary<string, Distance> vals = new Dictionary<string, Distance>();
            foreach (string v in str.Split('[', ']', ',')) {
                string[] v2 = v.Split('=');
                if (v2.Length != 2 || string.IsNullOrEmpty(v2[1])) {
                    result = Empty;
                    return false;
                }
                string k = v2[0].Trim().ToUpper();
                if (vals.ContainsKey(k)) {
                    result = Empty;
                    return false;
                }
                Distance d;
                if (!Distance.TryParse(v2[1], out d)) {
                    result = Empty;
                    return false;
                }
                vals.Add(k, d);
            }
            if (vals.Count != 4) {
                result = Empty;
                return false;
            }
            try {
                Point2D p = new Point2D(vals["X"], vals["Y"]);
                Size2D s = new Size2D(vals["WIDTH"], vals["HEIGHT"]);
                result = new Rectangle2D(p, s);
                return true;
            } catch (KeyNotFoundException) {
                result = Empty;
                return false;
            }
        }

        public static Rectangle2D Parse(string input) {
            if (string.IsNullOrEmpty(input)) {
                throw new ArgumentNullException();
            }
            Rectangle2D result;
            if (!TryParse(input, out result)) {
                throw new FormatException();
            }
            return result;
        }
        #endregion

        #region コンストラクタ
        public Rectangle2D(Point2D point, Size2D size) {
            _location = point;
            _size = size;
        }

        public Rectangle2D(Distance left, Distance top, Distance right, Distance bottom, bool normalize = false) {
            Distance x0;
            Distance y0;
            Distance x1;
            Distance y1;
            if (normalize) {
                x0 = Distance.Min(left, right);
                y0 = Distance.Min(top, bottom);
                x1 = Distance.Max(left, right);
                y1 = Distance.Max(top, bottom);
            } else {
                x0 = left;
                y0 = top;
                x1 = right;
                y1 = bottom;
            }
            _location = new Point2D(x0, y0);
            _size = new Size2D(x1 - x0, y1 - y0);
        }

        public Rectangle2D(double left, double top, double right, double bottom, LengthUnit unit, bool normalize = false) {
            double x0;
            double y0;
            double x1;
            double y1;
            if (normalize) {
                x0 = Math.Min(left, right);
                y0 = Math.Min(top, bottom);
                x1 = Math.Max(left, right);
                y1 = Math.Max(top, bottom);
            } else {
                x0 = left;
                y0 = top;
                x1 = right;
                y1 = bottom;
            }
            _location = new Point2D(new Distance(x0, unit), new Distance(y0, unit));
            _size = new Size2D(new Distance(x1 - x0, unit), new Distance(y1 - y0, unit));
        }

        public Rectangle2D(Point2D topLeft, Point2D bottomRight, bool normalize) {
            if (normalize) {
                _location = GetTopLeft(topLeft, bottomRight);
                _size = new Size2D(_location, GetBottomRight(topLeft, bottomRight));
            } else {
                _location = topLeft;
                _size = new Size2D(topLeft, bottomRight);
            }
            //_point = GetTopLeft(a, b);
            //_size = new Size2D(_point, GetBottomRight(a, b));
        }
        #endregion

        #region 演算子の定義と演算関連のメソッド
        /// <summary>
        /// 幅・高さがマイナスの場合プラスに変換する
        /// </summary>
        /// <returns></returns>
        public Rectangle2D Normalize() {
            Distance x = Left;
            Distance y = Top;
            Distance w = Width;
            Distance h = Height;
            if (w < Distance.Zero) {
                w = -w;
                x -= w;
            }
            if (h < Distance.Zero) {
                h = -h;
                y -= h;
            }
            return new Rectangle2D(new Point2D(x, y), new Size2D(w, h));
        }

        public static Point2D GetTopLeft(Point2D a, Point2D b) {
            return new Point2D(Distance.Min(a.X, b.X), Distance.Min(a.Y, b.Y));
        }
        public static Point2D GetTopRight(Point2D a, Point2D b) {
            return new Point2D(Distance.Max(a.X, b.X), Distance.Min(a.Y, b.Y));
        }
        public static Point2D GetBottomLeft(Point2D a, Point2D b) {
            return new Point2D(Distance.Min(a.X, b.X), Distance.Max(a.Y, b.Y));
        }
        public static Point2D GetBottomRight(Point2D a, Point2D b) {
            return new Point2D(Distance.Max(a.X, b.X), Distance.Max(a.Y, b.Y));
        }
        
        /// <summary>
        /// 引数で指定した点が矩形領域の中に含まれるかどうかを返す
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Contains(Point2D p) {
            return (Left <= p.X) && (p.X <= Right) && (Top <= p.Y) && (p.Y <= Bottom);
        }

        /// <summary>
        /// 引数で指定した矩形領域全体が矩形領域の中に含まれるかどうかを返す
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Contains(Rectangle2D rect) {
            return (Left <= rect.Left) && (rect.Right <= Right) && (Top <= rect.Top) && (rect.Bottom <= Bottom);
        }

        public bool Intersects(Rectangle2D rect) {
            return (Left <= rect.Right) && (rect.Left <= Right) && (Top <= rect.Bottom) && (rect.Top <= Bottom);
        }

        /// <summary>
        /// 矩形のサイズをsizeで指定しただけ広げる
        /// </summary>
        /// <param name="size"></param>
        public void Inflate(Size2D size) {
            Size += size;
        }

        /// <summary>
        /// 二つの矩形を含んだ最小の矩形を返す
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rectangle2D Union(Rectangle2D a, Rectangle2D b) {
            return new Rectangle2D(Distance.Min(a.Left, b.Left), Distance.Min(a.Top, b.Top),
                Distance.Max(a.Right, b.Right), Distance.Max(a.Bottom, b.Bottom), false);
        }

        /// <summary>
        /// 引数で渡された複数の矩形を全て含んだ最小の矩形を返す
        /// </summary>
        /// <param name="rects">矩形の一覧</param>
        /// <returns>rectsが空の場合はRectangle2D.Emptyを返す</returns>
        public static Rectangle2D Union(IEnumerable<Rectangle2D> rects) {
            Point2D topLeft = new Point2D(double.MaxValue, double.MaxValue, Distance.DefaultUnit);
            Point2D bottomRight = new Point2D(double.MinValue, double.MinValue, Distance.DefaultUnit);
            bool isEmpty = true;
            foreach (Rectangle2D r in rects) {
                if (r.Left < topLeft.X) {
                    topLeft.X = r.Left;
                }
                if (bottomRight.X < r.Right) {
                    bottomRight.X = r.Right;
                }
                if (r.Top < topLeft.Y) {
                    topLeft.Y = r.Top;
                }
                if (bottomRight.Y < r.Bottom) {
                    bottomRight.Y = r.Bottom;
                }
                isEmpty = false;
            }
            if (isEmpty) {
                return Rectangle2D.Empty;
            }
            return new Rectangle2D(topLeft, bottomRight, false);
        }
        /// <summary>
        /// 二つの矩形の交差している部分を返す
        /// 交差していない場合はEmptyを返す
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rectangle2D Intersect(Rectangle2D a, Rectangle2D b) {
            Rectangle2D ret = new Rectangle2D(Distance.Max(a.Left, b.Left), Distance.Max(a.Top, b.Top),
                Distance.Min(a.Right, b.Right), Distance.Min(a.Bottom, b.Bottom), false);
            if (ret.Size.IsZero()) {
                return Empty;
            }
            if (ret.Width < Distance.Zero || ret.Height < Distance.Zero) {
                return Empty;
            }
            return ret;
        }

        /// <summary>
        /// 引数で渡された複数の矩形が全て交差している部分を返す
        /// 交差していない場合はEmptyを返す
        /// </summary>
        /// <param name="rects">矩形の一覧</param>
        /// <returns>rectsが空の場合はRectangle2D.Emptyを返す</returns>
        public static Rectangle2D Intersect(IEnumerable<Rectangle2D> rects) {
            Point2D topLeft = new Point2D(double.MinValue, double.MinValue, Distance.DefaultUnit);
            Point2D bottomRight = new Point2D(double.MaxValue, double.MaxValue, Distance.DefaultUnit);
            bool isEmpty = true;
            foreach (Rectangle2D r in rects) {
                if (topLeft.X < r.Left) {
                    topLeft.X = r.Left;
                }
                if (r.Right < bottomRight.X) {
                    bottomRight.X = r.Right;
                }
                if (topLeft.Y < r.Top) {
                    topLeft.Y = r.Top;
                }
                if (r.Bottom < bottomRight.Y) {
                    bottomRight.Y = r.Bottom;
                }
                isEmpty = false;
            }
            if (isEmpty) {
                return Rectangle2D.Empty;
            }
            if (bottomRight.X < topLeft.X || bottomRight.Y < topLeft.Y) {
                return Rectangle2D.Empty;
            }
            return new Rectangle2D(topLeft, bottomRight, false);
        }
        /// <summary>
        /// 矩形aのうち矩形bと交差していない領域を表す矩形を返す
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rectangle2D Subtract(Rectangle2D a, Rectangle2D b) {
            if (b.Left <= a.Left && b.Top <= a.Top && a.Right <= b.Right && a.Bottom <= b.Bottom) {
                return Empty;
            }
            if (a.Left <= b.Left && b.Left < a.Right && a.Right <= b.Right && b.Top <= a.Top && a.Bottom <= b.Bottom) {
                return new Rectangle2D(a.Left, a.Top, b.Left, a.Bottom, false);
            }
            if (b.Left <= a.Left && a.Left < b.Right && b.Right <= a.Right && b.Top <= a.Top && a.Bottom <= b.Bottom) {
                return new Rectangle2D(b.Right, a.Top, a.Right, a.Bottom, false);
            }
            if (a.Top <= b.Top && b.Top < a.Bottom && a.Bottom <= b.Bottom && b.Left <= a.Left && a.Right <= b.Right) {
                return new Rectangle2D(a.Left, a.Top, a.Right, b.Top, false);
            }
            if (b.Top <= a.Top && a.Top < b.Bottom && b.Bottom <= a.Bottom && b.Left <= a.Left && a.Right <= b.Right) {
                return new Rectangle2D(a.Left, b.Bottom, a.Right, a.Bottom, false);
            }
            return a;
        }

        /// <summary>
        /// pointsで指定した全て点を内包する最小の矩形を返す
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Rectangle2D GetContainingRect(IEnumerable<Point2D> points) {
            if (points == null) {
                throw new ArgumentNullException("points");
            }
            //if (points.Length == 0) {
            //    throw new ArgumentException("points");
            //}
            Point2D p1 = new Point2D(double.MaxValue, double.MaxValue, Distance.DefaultUnit);
            Point2D p2 = new Point2D(double.MinValue, double.MinValue, Distance.DefaultUnit);
            foreach (Point2D p in points) {
                p1.X = Distance.Min(p1.X, p.X);
                p1.Y = Distance.Min(p1.Y, p.Y);
                p2.X = Distance.Max(p2.X, p.X);
                p2.Y = Distance.Max(p2.Y, p.Y);
            }
            if (p2.X < p1.X || p2.Y < p1.Y) {
                return Rectangle2D.Empty;
            }
            return new Rectangle2D(p1, p2, false);
        }

        /// <summary>
        /// 矩形を回転した後それを外接する矩形を返す
        /// </summary>
        /// <param name="rotatingAngle"></param>
        /// <returns></returns>
        public Rectangle2D RotateAndCircumscribe(Angle rotatingAngle) {
            throw new NotImplementedException();
        }

        public static Rectangle2D operator +(Rectangle2D a, Rectangle2D b) {
            return Union(a, b);
        }

        public static Rectangle2D operator -(Rectangle2D a, Rectangle2D b) {
            return Subtract(a, b);
        }

        public static Rectangle2D operator *(Rectangle2D a, Rectangle2D b) {
            return Intersect(a, b);
        }

        public override bool Equals(object obj) {
            if (!(obj is Rectangle2D)) {
                return base.Equals(obj);
            }
            return Location == ((Rectangle2D)obj).Location && Size == ((Rectangle2D)obj).Size;
        }

        public override int GetHashCode() {
            return Location.GetHashCode() + Size.GetHashCode();
        }

        public static bool operator ==(Rectangle2D a, Rectangle2D b) {
            return a.Location == b.Location && a.Size == b.Size;
        }

        public static bool operator !=(Rectangle2D a, Rectangle2D b) {
            return a.Location != b.Location || a.Size != b.Size;
        }

        public Rect ToRect(LengthUnit unit) {
            return new Rect(X[unit], Y[unit], Width[unit], Height[unit]);
        }

        public Point2D[] ToCornerPoints() {
            return new Point2D[] { TopLeft, TopRight, BottomRight, BottomLeft };
        }

        public bool IsEmpty() {
            return _size.IsZero();
        }

        private static Rectangle2D _empty = new Rectangle2D(0.0, 0.0, 0.0, 0.0, Distance.DefaultUnit, false);
        public static Rectangle2D Empty { get { return _empty; } }
        #endregion
    }
    public enum LineCap {
        // 概要:
        //     直線の最後の点より先に延長しないキャップ。線キャップがない場合と同じです。
        CapFlat = 0,
        //
        // 概要:
        //     高さが線の太さと等しく、長さが線の太さの半分に等しい四角形。
        CapSquare = 1,
        //
        // 概要:
        //     直径が線の太さと等しい半円。
        CapRound = 2,
        //
        // 概要:
        //     線の太さが底辺の長さに等しい直角二等辺三角形。
        CapTriangle = 3,
    }

    public enum LineJoin {
        // 概要:
        //     通常の角度の頂点。
        JoinMiter = 0,
        //
        // 概要:
        //     傾斜のついた頂点。
        JoinBevel = 1,
        //
        // 概要:
        //     丸い頂点。
        JoinRound = 2,
    }
    public enum FillRule {
        // 概要:
        //     ある点から任意の方向に無限に伸びる射線を描画し、その射線が横断する指定した図形のパス セグメントの数をカウントすることにより、その点が塗りつぶし領域の内側にあるかどうかを判断するルール。この数が奇数の場合、点は内側にあり、偶数の場合は外側にあります。
        EvenOdd = 0,
        //
        // 概要:
        //     ある点から任意の方向に無限に伸びる射線を描画し、図形のセグメントがこの射線と交わる場所を調べることにより、その点がパスの塗りつぶし領域の内側にあるかどうかを判断するルール。0
        //     からカウントを開始し、パス セグメントが左から右に射線と交わるたびに 1 を加算し、パス セグメントが右から左に射線と交わるたびに 1 を減算します。このカウントの結果がゼロの場合、点はパスの外側となります。それ以外の場合は内側です。
        Nonzero = 1,
    }
}
