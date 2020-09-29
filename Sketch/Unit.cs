using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sketch {
    public class EnumArray<TIndex, T>: IEnumerable where TIndex: IConvertible {
        private TIndex[] _indexArray;
        private T[] _valueArray;
        private int _offset;

        public EnumArray() {
            int i0 = int.MaxValue;
            int i1 = int.MinValue;
            _indexArray = (TIndex[])Enum.GetValues(typeof(TIndex));
            foreach (TIndex v in _indexArray) {
                int i = Convert.ToInt32(v);
                if (i < i0) {
                    i0 = i;
                }
                if (i1 < i) {
                    i1 = i;
                }
            }
            _offset = -i0;
            _valueArray = new T[i1 - i0 + 1];
        }
        public T this[TIndex index] {
            get {
                int i = Convert.ToInt32(index) + _offset;
                if (i < 0 || _valueArray.Length <= i) {
                    throw new ArgumentOutOfRangeException();
                }
                return _valueArray[i];
            }
            set {
                int i = Convert.ToInt32(index) + _offset;
                if (i < 0 || _valueArray.Length <= i) {
                    throw new ArgumentOutOfRangeException();
                }
                _valueArray[i] = value;
            }
        }

        #region IEnumrableの実装
        #region GetEnumrator()用内部クラス
        internal class EnumArrayEnumerator: IEnumerator<T> {
            private EnumArray<TIndex, T> _owner;

            internal EnumArrayEnumerator(EnumArray<TIndex, T> owner) {
                _owner = owner;
            }

            #region IEnumratorの実装
            private int _currentIndex = -1;
            public T Current {
                get {
                    if (_owner == null || _owner._indexArray == null) {
                        throw new InvalidOperationException();
                    }
                    if (_currentIndex < 0 || _owner._indexArray.Length <= _currentIndex) {
                        throw new InvalidOperationException();
                    }
                    return _owner[_owner._indexArray[_currentIndex]];
                }
            }

            public void Dispose() {
                _owner = null;
                _currentIndex = 0;
            }

            object IEnumerator.Current {
                get {
                    if (_owner == null || _owner._indexArray == null) {
                        throw new InvalidOperationException();
                    }
                    if (_currentIndex < 0 || _owner._indexArray.Length <= _currentIndex) {
                        throw new InvalidOperationException();
                    }
                    return _owner[_owner._indexArray[_currentIndex]];
                }
            }

            public bool MoveNext() {
                _currentIndex++;
                return _currentIndex < _owner._indexArray.Length;
            }

            public void Reset() {
                _currentIndex = -1;
            }
            #endregion
        }
        #endregion

        public IEnumerator GetEnumerator() {
            EnumArrayEnumerator ret = new EnumArrayEnumerator(this);
            return ret;
        }
        #endregion
    }
    public class EnumArray2D<TIndex, T> where TIndex: IConvertible {
        private T[,] _valueArray;
        private int _offset;
        private int _length;

        public EnumArray2D() {
            int i0 = int.MaxValue;
            int i1 = int.MinValue;
            foreach (TIndex v in Enum.GetValues(typeof(TIndex))) {
                int i = Convert.ToInt32(v);
                if (i < i0) {
                    i0 = i;
                }
                if (i1 < i) {
                    i1 = i;
                }
            }
            _offset = -i0;
            _length = i1 - i0 + 1;
            _valueArray = new T[_length,_length];
        }
        public T this[TIndex index1, TIndex index2] {
            get {
                int i1 = Convert.ToInt32(index1) + _offset;
                if (i1 < 0 || _valueArray.Length <= i1) {
                    throw new ArgumentOutOfRangeException();
                }
                int i2 = Convert.ToInt32(index2) + _offset;
                if (i2 < 0 || _valueArray.Length <= i2) {
                    throw new ArgumentOutOfRangeException();
                }
                return _valueArray[i1, i2];
            }
            set {
                int i1 = Convert.ToInt32(index1) + _offset;
                if (i1 < 0 || _valueArray.Length <= i1) {
                    throw new ArgumentOutOfRangeException();
                }
                int i2 = Convert.ToInt32(index2) + _offset;
                if (i2 < 0 || _valueArray.Length <= i2) {
                    throw new ArgumentOutOfRangeException();
                }
                _valueArray[i1, i2] = value;
            }
        }
    }

    //public enum Unit {
    //    Centimeters,
    //    Ciceros,
    //    Date,
    //    Degree,
    //    Didots,
    //    ElapsedWeek,
    //    ElapsedDay,
    //    ElapsedHour,
    //    ElapsedMin,
    //    ElapsedSec,
    //    Feet,
    //    Inches,
    //    Kilometers,
    //    Meters,
    //    Miles,
    //    Millimeters,
    //    Minutes,
    //    NautMiles,
    //    Percent,
    //    Picas,
    //    Points,
    //    Radians,
    //    Seconds,
    //    Yards,
    //}

    /// <summary>
    /// 長さの単位
    /// </summary>
    public enum LengthUnit {
        /// <summary>
        /// ミリメートル
        /// </summary>
        Millimeters,
        /// <summary>
        /// センチメートル
        /// </summary>
        Centimeters,
        /// <summary>
        /// メートル
        /// </summary>
        Meters,
        /// <summary>
        /// キロメートル
        /// </summary>
        Kilometers,

        /// <summary>
        /// インチ
        /// </summary>
        Inches,
        /// <summary>
        /// フィート
        /// </summary>
        Feet,
        /// <summary>
        /// ヤード
        /// </summary>
        Yards,
        /// <summary>
        /// マイル
        /// </summary>
        Miles,
        /// <summary>
        /// 海里
        /// </summary>
        NauticalMiles,

        /// <summary>
        /// シセロ(活字の大きさの単位)
        /// </summary>
        Ciceros,
        /// <summary>
        /// ディドット(活字の大きさの単位)
        /// </summary>
        Didots,
        
        /// <summary>
        /// パイカ(活字の大きさの単位)
        /// </summary>
        Picas,
        /// <summary>
        /// ポイント(活字の大きさの単位)
        /// </summary>
        Points,

        /// <summary>
        /// ピクセル(ディスプレイの表示単位)
        /// Distance.PixelsPerInchで換算値を定義する
        /// </summary>
        Pixels,
    }

    /// <summary>
    /// 単位付距離を保持する構造体
    /// </summary>
    public struct Distance: IComparable, IUnitConvertible {
        #region メンバー宣言
        private double _value;
        private LengthUnit _unit;

        /// <summary>
        /// 長さの内部値
        /// </summary>
        public double Value {
            get { return _value; }
            //set { _value = value; }
        }

        /// <summary>
        /// Valueがあらわしている長さの単位
        /// </summary>
        public LengthUnit Unit {
            get { return _unit; }
            //set { _unit = value; }
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 数値と単位からDistance構造体を生成する
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        public Distance(double value, LengthUnit unit) {
            _value = value;
            _unit = unit;
        }

        /// <summary>
        /// 数値からDistance構造体を生成する
        /// 単位はDistance.DefaultUnitを使用する
        /// </summary>
        /// <param name="value"></param>
        //public Distance(double value) {
        //    _value = value;
        //    _unit = DefaultUnit;
        //}

        /// <summary>
        /// 単位付の数値文字列からDistance構造体を生成する
        /// </summary>
        /// <param name="value"></param>
        public Distance(string value) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException();
            }
            string sVal;
            if (SplitUnitPart(value.Trim(), out sVal, out _unit)) {
                double.TryParse(sVal.Trim(), out _value);
                return;
            }
            throw new FormatException();
        }
        #endregion

        public bool IsZero() {
            return _value == 0.0;
        }

        #region 単位の変換処理
        /// <summary>
        /// 長さをindexで指定した単位に変換して返す
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double this[LengthUnit index] {
            get {
                return Value * Table.UnitConvertTable[Unit, index];
            }
            //set {
            //    _value = value;
            //    _unit = index;
            //}
        }

        /// <summary>
        /// ミリメートル換算値
        /// </summary>
        public double Mm { get { return this[LengthUnit.Millimeters]; } }
        /// <summary>
        /// センチメートル換算値
        /// </summary>
        public double Cm { get { return this[LengthUnit.Centimeters]; } }
        /// <summary>
        /// メートル換算値
        /// </summary>
        public double M { get { return this[LengthUnit.Meters]; } }
        /// <summary>
        /// キロメートル換算値
        /// </summary>
        public double Km { get { return this[LengthUnit.Kilometers]; } }

        /// <summary>
        /// インチ換算値
        /// </summary>
        public double In { get { return this[LengthUnit.Inches]; } }

        /// <summary>
        /// フィート換算値
        /// </summary>
        public double Ft { get { return this[LengthUnit.Feet]; } }

        /// <summary>
        /// ヤード換算値
        /// </summary>
        public double Yd { get { return this[LengthUnit.Yards]; } }

        /// <summary>
        /// マイル換算値
        /// </summary>
        public double Mi { get { return this[LengthUnit.Miles]; } }

        /// <summary>
        /// 海里換算値
        /// </summary>
        public double Nm { get { return this[LengthUnit.NauticalMiles]; } }

        /// <summary>
        /// シセロ換算値
        /// </summary>
        public double C { get { return this[LengthUnit.Ciceros]; } }

        /// <summary>
        /// ディドー換算値
        /// </summary>
        public double D { get { return this[LengthUnit.Didots]; } }

        /// <summary>
        /// パイカ換算値
        /// </summary>
        public double P { get { return this[LengthUnit.Picas]; } }

        /// <summary>
        /// ポイント換算値
        /// </summary>
        public double Pt { get { return this[LengthUnit.Points]; } }

        /// <summary>
        /// 画面表示用ピクセル(PixelsPerInchに基づいて換算)
        /// </summary>
        public double Px { get { return this[LengthUnit.Pixels]; } }

        /// <summary>
        /// 単位付数値の文字列を表示
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return _value.ToString() + Table.UnitInfos[_unit].name;
        }
        #endregion

        #region IComparableの実装
        public int CompareTo(object obj) {
            if (obj == null) {
                throw new ArgumentNullException();
            }
            if (!(obj is Distance)) {
                throw new ArgumentException();
            }
            return Value.CompareTo(((Distance)obj)[Unit]);
        }

        public override bool Equals(object obj) {
            if (!(obj is Distance)) {
                return base.Equals(obj);
            }
            return Value == ((Distance)obj)[Unit];
        }

        public override int GetHashCode() {
            return this[LengthUnit.Meters].GetHashCode();
        }
        #endregion

        #region 演算子の定義

        public static Distance operator +(Distance a, Distance b) {
            return new Distance(a.Value + b[a.Unit], a.Unit);
        }

        public static Distance operator -(Distance a, Distance b) {
            return new Distance(a.Value - b[a.Unit], a.Unit);
        }

        public static Distance operator -(Distance a) {
            return new Distance(-a.Value, a.Unit);
        }

        public static Distance operator *(Distance a, short b) {
            return new Distance(a.Value * b, a.Unit);
        }
        public static Distance operator *(Distance a, int b) {
            return new Distance(a.Value * b, a.Unit);
        }
        public static Distance operator *(Distance a, long b) {
            return new Distance(a.Value * b, a.Unit);
        }
        public static Distance operator *(Distance a, float b) {
            return new Distance(a.Value * b, a.Unit);
        }
        public static Distance operator *(Distance a, double b) {
            return new Distance(a.Value * b, a.Unit);
        }
        public static Distance operator *(Distance a, decimal b) {
            return new Distance(a.Value * (double)b, a.Unit);
        }
        public static Distance operator *(short a, Distance b) {
            return new Distance(a * b.Value, b.Unit);
        }
        public static Distance operator *(int a, Distance b) {
            return new Distance(a * b.Value, b.Unit);
        }
        public static Distance operator *(long a, Distance b) {
            return new Distance(a * b.Value, b.Unit);
        }
        public static Distance operator *(float a, Distance b) {
            return new Distance(a * b.Value, b.Unit);
        }
        public static Distance operator *(double a, Distance b) {
            return new Distance(a * b.Value, b.Unit);
        }
        public static Distance operator *(decimal a, Distance b) {
            return new Distance((double)a * b.Value, b.Unit);
        }

        public static Distance operator /(Distance a, short b) {
            return new Distance(a.Value / b, a.Unit);
        }
        public static Distance operator /(Distance a, int b) {
            return new Distance(a.Value / b, a.Unit);
        }
        public static Distance operator /(Distance a, long b) {
            return new Distance(a.Value / b, a.Unit);
        }
        public static Distance operator /(Distance a, float b) {
            return new Distance(a.Value / b, a.Unit);
        }
        public static Distance operator /(Distance a, double b) {
            return new Distance(a.Value / b, a.Unit);
        }
        public static Distance operator /(Distance a, decimal b) {
            return new Distance(a.Value / (double)b, a.Unit);
        }
        public static double operator /(Distance a, Distance b) {
            return a.Value / b[a.Unit];
        }

        public static bool operator <(Distance a, Distance b) {
            return a.Value < b[a.Unit];
        }
        public static bool operator <=(Distance a, Distance b) {
            return a.Value <= b[a.Unit];
        }
        public static bool operator >(Distance a, Distance b) {
            return a.Value > b[a.Unit];
        }
        public static bool operator >=(Distance a, Distance b) {
            return a.Value >= b[a.Unit];
        }
        public static bool operator ==(Distance a, Distance b) {
            return a.Value == b[a.Unit];
        }
        public static bool operator !=(Distance a, Distance b) {
            return a.Value != b[a.Unit];
        }
        #endregion

        #region その他演算用関数
        public static Distance Abs(Distance a) {
            if (a.Value < 0) {
                return new Distance(-a.Value, a.Unit);
            }
            return a;
        }

        public static Distance Ceiling(Distance a) {
            return new Distance(Math.Ceiling(a.Value), a.Unit);
        }

        public static Distance Floor(Distance a) {
            return new Distance(Math.Floor(a.Value), a.Unit);
        }

        public static Distance Round(Distance a) {
            return new Distance(Math.Round(a.Value), a.Unit);
        }
        public static Distance Round(Distance a, int digits) {
            return new Distance(Math.Round(a.Value, digits), a.Unit);
        }
        public static int Sign(Distance a) {
            return Math.Sign(a.Value);
        }
        public static Distance Truncate(Distance a) {
            return new Distance(Math.Truncate(a.Value), a.Unit);
        }

        //public static Distance Min(Distance a, Distance b) {
        //    return (a <= b) ? a : b;
        //}
        public static Distance Min(params Distance[] values) {
            if (values == null) {
                throw new ArgumentNullException("values");
            }
            if (values.Length == 0) {
                throw new ArgumentException("values");
            }
            Distance ret = values[0];
            for (int i = 1; i < values.Length; i++) {
                if (values[i] < ret) {
                    ret = values[i];
                }
            }
            return ret;
        }
        //public static Distance Max(Distance a, Distance b) {
        //    return (a < b) ? b : a;
        //}
        public static Distance Max(params Distance[] values) {
            if (values == null) {
                throw new ArgumentNullException("values");
            }
            if (values.Length == 0) {
                throw new ArgumentException("values");
            }
            Distance ret = values[0];
            for (int i = 1; i < values.Length; i++) {
                if (ret < values[i]) {
                    ret = values[i];
                }
            }
            return ret;
        }

        private static Distance _zero = new Distance(0, LengthUnit.Meters);
        public static Distance Zero { get { return _zero; } }

        private static LengthUnit _defaultUnit = LengthUnit.Meters;
        public static LengthUnit DefaultUnit {
            get {
                return _defaultUnit;
            }
            set {
                _defaultUnit = value;
                _zero = new Distance(0, _defaultUnit);
            }
        }

        #endregion

        #region 単位名表示・文字列変換・単位換算に必要な変換テーブルの処理
        internal struct UnitInfo: IComparable {
            internal LengthUnit unit;
            internal string name;
            internal double valueOfMeters;
            internal UnitInfo(LengthUnit u, string n, double v) {
                unit = u;
                name = n;
                valueOfMeters = v;
            }

            public int CompareTo(object obj) {
                if (obj == null) {
                    throw new ArgumentNullException();
                }
                if (!(obj is UnitInfo)) {
                    throw new ArgumentException();
                }
                return ((UnitInfo)obj).name.Length - name.Length;
            }
        }

        private static ConvertTable _table = new ConvertTable();
        internal static ConvertTable Table {
            get {
                if (_table == null) {
                    _table = new ConvertTable();
                }
                return _table;
            }
        }

        private static void RegisterUnitInfo(UnitInfo info) {
            Table.RegisterUnitInfo(info);
        }

        public static double PixelsPerInch {
            get { return Table.PixelsPerInch; }
            set { Table.PixelsPerInch = value; }
        }

        public static bool TryParseUnit(string s, out LengthUnit unit) {
            UnitInfo ui;
            if (!Table.StrToUnitInfo.TryGetValue(s.ToUpper(), out ui)) {
                unit = DefaultUnit;
                return false;
            }
            unit = ui.unit;
            return true;
        }

        public static LengthUnit ParseUnit(string s) {
            LengthUnit u;
            if (!TryParseUnit(s, out u)) {
                throw new ArgumentException();
            }
            return u;
        }

        public static string UnitText(LengthUnit unit) {
            return Table.UnitInfos[unit].name;
        }

        private static bool SplitUnitPart(string s, out string value, out LengthUnit unit) {
            value = string.Empty;
            unit = LengthUnit.Meters;
            if (string.IsNullOrEmpty(s)) {
                return false;
            }
            char c = char.ToUpper(s.Last<char>());
            if (Table.LastCharToUnitInfo.ContainsKey(c)) {
                foreach (UnitInfo ui in Table.LastCharToUnitInfo[c]) {
                    if (s.EndsWith(ui.name, StringComparison.CurrentCultureIgnoreCase)) {
                        unit = ui.unit;
                        value = s.Substring(0, s.Length - ui.name.Length);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TryParse(string s, out Distance result) {
            if (string.IsNullOrEmpty(s)) {
                result = new Distance();
                return false;
            }
            string sVal;
            if (SplitUnitPart(s.Trim(), out sVal, out result._unit)) {
                return double.TryParse(sVal.Trim(), out result._value);
            }
            result = new Distance();
            return false;
        }
        public static Distance Parse(string s) {
            return new Distance(s);
        }
        #endregion

        #region IUnitConvertibleの実装
        public Distance ToDistance(LengthUnit unit) {
            return this;
        }

        public Angle ToAngle(AngleUnit unit) {
            return new Angle(Value, unit);
        }
        #endregion

        internal class ConvertTable {
            private EnumArray<LengthUnit, UnitInfo> _unitInfos;

            private EnumArray2D<LengthUnit, double> _unitConvertTable;

            private Dictionary<char, List<UnitInfo>> _lastCharToUnitInfo;

            private Dictionary<string, UnitInfo> _strToUnitInfo;

            internal EnumArray<LengthUnit, UnitInfo> UnitInfos { get { return _unitInfos; } }

            internal EnumArray2D<LengthUnit, double> UnitConvertTable { get { return _unitConvertTable; } }

            internal Dictionary<char, List<UnitInfo>> LastCharToUnitInfo { get { return _lastCharToUnitInfo; } }

            internal Dictionary<string, UnitInfo> StrToUnitInfo { get { return _strToUnitInfo; } }

            private double _pixelsPerInch = 96;
            internal double PixelsPerInch {
                get { return _pixelsPerInch; }
                set {
                    if (_pixelsPerInch != value) {
                        _pixelsPerInch = value;
                        UpdateUnitConvertTable();
                    }
                }
            }

            internal ConvertTable() {
                InitUnitInfos();
                InitUnitConvertTable();
            }

            internal void RegisterUnitInfo(UnitInfo info) {
                _unitInfos[info.unit] = info;
            }

            private void InitUnitInfos() {
                _unitInfos = new EnumArray<LengthUnit, UnitInfo>();
                RegisterUnitInfo(new UnitInfo(LengthUnit.Millimeters, "mm", 0.001));
                RegisterUnitInfo(new UnitInfo(LengthUnit.Centimeters, "cm", 0.01));
                RegisterUnitInfo(new UnitInfo(LengthUnit.Meters, "m", 1));
                RegisterUnitInfo(new UnitInfo(LengthUnit.Kilometers, "km", 1000));

                RegisterUnitInfo(new UnitInfo(LengthUnit.Inches, "in", 0.0254));
                RegisterUnitInfo(new UnitInfo(LengthUnit.Feet, "ft", 0.3048));
                RegisterUnitInfo(new UnitInfo(LengthUnit.Yards, "yd", 0.9144));
                RegisterUnitInfo(new UnitInfo(LengthUnit.Miles, "mi", 1609.344));
                RegisterUnitInfo(new UnitInfo(LengthUnit.NauticalMiles, "nm", 1852));

                RegisterUnitInfo(new UnitInfo(LengthUnit.Ciceros, "c", 3.0 / 665));
                RegisterUnitInfo(new UnitInfo(LengthUnit.Didots, "d", 3.0 / 12 / 665));

                RegisterUnitInfo(new UnitInfo(LengthUnit.Picas, "p", 0.35 / 83));
                RegisterUnitInfo(new UnitInfo(LengthUnit.Points, "pt", 0.0254 / 72));
                RegisterUnitInfo(new UnitInfo(LengthUnit.Pixels, "px", 0.0254 / PixelsPerInch));
            }

            private void UpdateUnitConvertTable() {
                RegisterUnitInfo(new UnitInfo(LengthUnit.Pixels, "px", 0.0254 / PixelsPerInch));
                foreach (LengthUnit u1 in Enum.GetValues(typeof(LengthUnit))) {
                    foreach (LengthUnit u2 in Enum.GetValues(typeof(LengthUnit))) {
                        _unitConvertTable[u1, u2] = _unitInfos[u1].valueOfMeters / _unitInfos[u2].valueOfMeters;
                    }
                }
            }

            private void InitUnitConvertTable() {
                _unitConvertTable = new EnumArray2D<LengthUnit, double>();
                _lastCharToUnitInfo = new Dictionary<char, List<UnitInfo>>();
                _strToUnitInfo = new Dictionary<string, UnitInfo>();

                foreach (UnitInfo ui in _unitInfos) {
                    char c = char.ToUpper(ui.name.Last<char>());
                    if (!_lastCharToUnitInfo.ContainsKey(c)) {
                        _lastCharToUnitInfo[c] = new List<UnitInfo>();
                    }
                    _lastCharToUnitInfo[c].Add(ui);
                    _lastCharToUnitInfo[c].Sort();
                    if (string.IsNullOrEmpty(ui.name)) {
                        _strToUnitInfo.Add(ui.name.ToUpper(), ui);
                    }
                }
                UpdateUnitConvertTable();
            }
        }
    }

    /// <summary>
    /// 角度の単位
    /// </summary>
    public enum AngleUnit {
        Radian,
        Degree,
        Grade,
    }

    /// <summary>
    /// 単位付角度を保持する構造体
    /// </summary>
    public struct Angle: IComparable, IUnitConvertible {
        #region メンバー宣言
        private double _value;
        private AngleUnit _unit;
        
        /// <summary>
        /// 数値と単位からAngle構造体を生成する
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        public Angle(double value, AngleUnit unit) {
            _value = value;
            _unit = unit;
        }

        /// <summary>
        /// 単位付の数値文字列からAngle構造体を生成する
        /// </summary>
        /// <param name="value"></param>
        public Angle(string value) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException();
            }
            string sVal;
            if (SplitUnitPart(value.Trim(), out sVal, out _unit)) {
                double.TryParse(sVal.Trim(), out _value);
                return;
            }
            throw new FormatException();
        }

        /// <summary>
        /// 角度の値
        /// </summary>
        public double Value {
            get { return _value; }
            //set { _value = value; }
        }

        /// <summary>
        /// 角度の単位
        /// </summary>
        public AngleUnit Unit {
            get { return _unit; }
            //set { _unit = value; }
        }
        #endregion

        #region 演算処理
        public static Angle Asin(double a) {
            return new Angle(Math.Asin(a), AngleUnit.Radian);
        }
        public static Angle Acos(double a) {
            return new Angle(Math.Acos(a), AngleUnit.Radian);
        }
        public static Angle Atan(double a) {
            return new Angle(Math.Atan(a), AngleUnit.Radian);
        }
        public static Angle Atan2(Distance y, Distance x) {
            return new Angle(Math.Atan2(y.Value, x[y.Unit]), AngleUnit.Radian);
        }

        public static double Sin(Angle a) {
            return Math.Sin(a.Rad);
        }
        public static double Cos(Angle a) {
            return Math.Cos(a.Rad);
        }

        public static double Tan(Angle a) {
            return Math.Tan(a.Rad);
        }

        public static double Sinh(Angle a) {
            return Math.Sinh(a.Rad);
        }

        public static double Cosh(Angle a) {
            return Math.Cosh(a.Rad);
        }

        public static double Tanh(Angle a) {
            return Math.Tanh(a.Rad);
        }

        public static Angle Abs(Angle a) {
            if (a.Value < 0) {
                return new Angle(-a.Value, a.Unit);
            }
            return a;
        }

        public static Angle Ceiling(Angle a) {
            return new Angle(Math.Ceiling(a.Value), a.Unit);
        }

        public static Angle Floor(Angle a) {
            return new Angle(Math.Floor(a.Value), a.Unit);
        }

        public static Angle Max(Angle a, Angle b) {
            return (a < b) ? b : a;
        }

        public static Angle Min(Angle a, Angle b) {
            return (a <= b) ? a : b;
        }

        public static Angle Round(Angle a) {
            return new Angle(Math.Round(a.Value), a.Unit);
        }
        public static Angle Round(Angle a, int digits) {
            return new Angle(Math.Round(a.Value, digits), a.Unit);
        }
        public static int Sign(Angle a) {
            return Math.Sign(a.Value);
        }
        public static Angle Truncate(Angle a) {
            return new Angle(Math.Truncate(a.Value), a.Unit);
        }

        public double Sin() {
            return Math.Sin(Rad);
        }

        public double Cos() {
            return Math.Cos(Rad);
        }

        public double Tan() {
            return Math.Tan(Rad);
        }

        public double Sinh() {
            return Math.Sinh(Rad);
        }

        public double Cosh() {
            return Math.Cosh(Rad);
        }

        public double Tanh() {
            return Math.Tanh(Rad);
        }

        /// <summary>
        /// targetPointを原点を中心にAngleの値だけ回転した座標を返す
        /// </summary>
        /// <param name="targetPoint"></param>
        /// <returns></returns>
        public Point2D Rotate(Point2D targetPoint) {
            return Angle.Rotate(this, targetPoint);
        }

        /// <summary>
        /// targetPointをbasePointを中心にAngleの値だけ回転した座標を返す
        /// </summary>
        /// <param name="basePoint"></param>
        /// <returns></returns>
        public Point2D Rotate(Point2D targetPoint, Point2D basePoint) {
            return Angle.Rotate(this, targetPoint, basePoint);
        }

        /// <summary>
        /// targetPointを原点を中心にangleの値だけ回転した座標を返す
        /// </summary>
        /// <param name="targetPoint"></param>
        /// <returns></returns>
        public static Point2D Rotate(Angle angle, Point2D targetPoint) {
            double sin = angle.Sin();
            double cos = angle.Cos();
            return new Point2D(targetPoint.X * cos - targetPoint.Y * sin, targetPoint.X * sin + targetPoint.Y * cos);
        }

        /// <summary>
        /// targetPointをbasePointを中心にangleの値だけ回転した座標を返す
        /// </summary>
        /// <param name="basePoint"></param>
        /// <returns></returns>
        public static Point2D Rotate(Angle angle, Point2D targetPoint, Point2D basePoint) {
            return Rotate(angle, targetPoint - basePoint) + basePoint;
        }

        /// <summary>
        /// 角度を0°～360°(もしくは-180°～180°)の同じ角度に変換する
        /// 例)390deg をNormalizeすると 30deg
        /// </summary>
        /// <param name="signed">
        /// false: 0°～359.99...°の範囲で正規化
        /// true: -179.99...°～180°の範囲で正規化
        /// </param>
        /// <returns></returns>
        public Angle Normalize(bool signed) {
            double vLoop = Table.UnitInfos[Unit].valueOfLoop;
            int n = (int)(Value / vLoop);
            // -360°～360°の範囲に正規化
            double v = Value - vLoop * n;
            if (signed) {
                // -180°～180°の範囲に正規化
                if (vLoop / 2 < v) {
                    v -= vLoop;
                }
                if (v <= -vLoop / 2) {
                    v += vLoop;
                }
            } else {
                // 0°～360°の範囲に正規化
                if (v < 0) {
                    v += vLoop;
                }
            }
            return new Angle(v, Unit);
        }

        private static Angle _zero = new Angle(0, AngleUnit.Degree);
        public static Angle Zero { get { return _zero; } }
        private static AngleUnit _defaultUnit = AngleUnit.Degree;
        public static AngleUnit DefaultUnit {
            get {
                return _defaultUnit;
            }
            set {
                _defaultUnit = value;
                _zero = new Angle(0, _defaultUnit);
            }
        }

        #endregion

        #region 単位の変換処理
        /// <summary>
        /// 長さをindexで指定した単位に変換して返す
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double this[AngleUnit index] {
            get {
                return Value * Table.UnitConvertTable[index, Unit];
            }
            //set {
            //    _value = value;
            //    _unit = index;
            //}
        }

        /// <summary>
        /// ラジアン換算値
        /// </summary>
        public double Rad { get { return this[AngleUnit.Radian]; } }

        /// <summary>
        /// 度(°)換算値
        /// </summary>
        public double Deg { get { return this[AngleUnit.Degree]; } }

        /// <summary>
        /// Grade換算値
        /// </summary>
        public double Grad { get { return this[AngleUnit.Grade]; } }

        /// <summary>
        /// 単位付数値の文字列を表示
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return _value.ToString() + Table.UnitInfos[_unit].name;
        }
        #endregion

        #region IComparableの実装
        public int CompareTo(object obj) {
            if (obj == null) {
                throw new ArgumentNullException();
            }
            if (!(obj is Angle)) {
                throw new ArgumentException();
            }
            return Value.CompareTo(((Angle)obj)[Unit]);
        }

        public override bool Equals(object obj) {
            if (!(obj is Angle)) {
                return base.Equals(obj);
            }
            return Value == ((Angle)obj)[Unit];
        }

        public override int GetHashCode() {
            return this[AngleUnit.Degree].GetHashCode();
        }
        #endregion

        #region 演算子の定義
        public static Angle operator +(Angle a, Angle b) {
            return new Angle(a.Value + b[a.Unit], a.Unit);
        }

        public static Angle operator -(Angle a, Angle b) {
            return new Angle(a.Value - b[a.Unit], a.Unit);
        }

        public static Angle operator -(Angle a) {
            return new Angle(-a.Value, a.Unit);
        }

        public static Angle operator *(Angle a, short b) {
            return new Angle(a.Value * b, a.Unit);
        }
        public static Angle operator *(Angle a, int b) {
            return new Angle(a.Value * b, a.Unit);
        }
        public static Angle operator *(Angle a, long b) {
            return new Angle(a.Value * b, a.Unit);
        }
        public static Angle operator *(Angle a, float b) {
            return new Angle(a.Value * b, a.Unit);
        }
        public static Angle operator *(Angle a, double b) {
            return new Angle(a.Value * b, a.Unit);
        }
        public static Angle operator *(Angle a, decimal b) {
            return new Angle(a.Value * (double)b, a.Unit);
        }
        public static Angle operator *(short a, Angle b) {
            return new Angle(a * b.Value, b.Unit);
        }
        public static Angle operator *(int a, Angle b) {
            return new Angle(a * b.Value, b.Unit);
        }
        public static Angle operator *(long a, Angle b) {
            return new Angle(a * b.Value, b.Unit);
        }
        public static Angle operator *(float a, Angle b) {
            return new Angle(a * b.Value, b.Unit);
        }
        public static Angle operator *(double a, Angle b) {
            return new Angle(a * b.Value, b.Unit);
        }
        public static Angle operator *(decimal a, Angle b) {
            return new Angle((double)a * b.Value, b.Unit);
        }

        public static Angle operator /(Angle a, short b) {
            return new Angle(a.Value / b, a.Unit);
        }
        public static Angle operator /(Angle a, int b) {
            return new Angle(a.Value / b, a.Unit);
        }
        public static Angle operator /(Angle a, long b) {
            return new Angle(a.Value / b, a.Unit);
        }
        public static Angle operator /(Angle a, float b) {
            return new Angle(a.Value / b, a.Unit);
        }
        public static Angle operator /(Angle a, double b) {
            return new Angle(a.Value / b, a.Unit);
        }
        public static Angle operator /(Angle a, decimal b) {
            return new Angle(a.Value / (double)b, a.Unit);
        }
        public static double operator /(Angle a, Angle b) {
            return a.Value / b[a.Unit];
        }

        public static bool operator <(Angle a, Angle b) {
            return a.Value < b[a.Unit];
        }
        public static bool operator <=(Angle a, Angle b) {
            return a.Value <= b[a.Unit];
        }
        public static bool operator >(Angle a, Angle b) {
            return a.Value > b[a.Unit];
        }
        public static bool operator >=(Angle a, Angle b) {
            return a.Value >= b[a.Unit];
        }
        public static bool operator ==(Angle a, Angle b) {
            return a.Value == b[a.Unit];
        }
        public static bool operator !=(Angle a, Angle b) {
            return a.Value != b[a.Unit];
        }
        #endregion

        #region 単位名表示・文字列変換・単位換算に必要な変換テーブルの処理
        internal struct UnitInfo: IComparable {
            internal AngleUnit unit;
            internal string name;
            internal double valueOfLoop;
            internal UnitInfo(AngleUnit u, string n, double v) {
                unit = u;
                name = n;
                valueOfLoop = v;
            }

            public int CompareTo(object obj) {
                if (obj == null) {
                    throw new ArgumentNullException();
                }
                if (!(obj is UnitInfo)) {
                    throw new ArgumentException();
                }
                return ((UnitInfo)obj).name.Length - name.Length;
            }
        }

        //private static UnitInfo[] _unitInfos = new UnitInfo[] {
        //    new UnitInfo(AngleUnit.Radian, "rad", 2 * Math.PI),
        //    new UnitInfo(AngleUnit.Degree, "deg", 360),
        //    new UnitInfo(AngleUnit.Grade, "grad", 400),
        //};
        private static ConvertTable _table;

        internal static ConvertTable Table {
            get {
                if (_table == null) {
                    _table = new ConvertTable();
                }
                return _table;
            }
        }

        //private static void RegisterUnitInfo(UnitInfo info) {
        //    Table.UnitInfos[info.unit] = info;
        //}
        private static void RegisterUnitInfo(UnitInfo info) {
            Table.RegisterUnitInfo(info);
        }

        public static bool TryParseUnit(string s, out AngleUnit unit) {
            UnitInfo ui;
            if (!Table.StrToUnitInfo.TryGetValue(s.ToUpper(), out ui)) {
                unit = DefaultUnit;
                return false;
            }
            unit = ui.unit;
            return true;
        }

        public static AngleUnit ParseUnit(string s) {
            AngleUnit u;
            if (!TryParseUnit(s, out u)) {
                throw new ArgumentException();
            }
            return u;
        }

        public static string UnitText(AngleUnit unit) {
            return Table.UnitInfos[unit].name;
        }

        private static bool SplitUnitPart(string s, out string value, out AngleUnit unit) {
            value = string.Empty;
            unit = AngleUnit.Radian;
            if (string.IsNullOrEmpty(s)) {
                return false;
            }
            char c = char.ToUpper(s.Last<char>());
            if (Table.LastCharToUnitInfo.ContainsKey(c)) {
                foreach (UnitInfo ui in Table.LastCharToUnitInfo[c]) {
                    if (s.EndsWith(ui.name, StringComparison.CurrentCultureIgnoreCase)) {
                        unit = ui.unit;
                        value = s.Substring(0, s.Length - ui.name.Length);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TryParse(string s, out Angle result) {
            if (string.IsNullOrEmpty(s)) {
                result = new Angle();
                return false;
            }
            string sVal;
            if (SplitUnitPart(s.Trim(), out sVal, out result._unit)) {
                return double.TryParse(sVal.Trim(), out result._value);
            }
            result = new Angle();
            return false;
        }
        public static Angle Parse(string s) {
            return new Angle(s);
        }
        #endregion

        #region IUnitConvertibleの実装
        public Distance ToDistance(LengthUnit unit) {
            return new Distance(Value, unit);
        }

        public Angle ToAngle(AngleUnit unit) {
            return this;
        }
        #endregion

        internal class ConvertTable {
            private EnumArray<AngleUnit, UnitInfo> _unitInfos;
            private EnumArray2D<AngleUnit, double> _unitConvertTable;
            private Dictionary<char, List<UnitInfo>> _lastCharToUnitInfo;
            private Dictionary<string, UnitInfo> _strToUnitInfo;

            internal EnumArray<AngleUnit, UnitInfo> UnitInfos { get { return _unitInfos; } }
            internal EnumArray2D<AngleUnit, double> UnitConvertTable { get { return _unitConvertTable; } }
            internal Dictionary<char, List<UnitInfo>> LastCharToUnitInfo { get { return _lastCharToUnitInfo; } }
            internal Dictionary<string, UnitInfo> StrToUnitInfo { get { return _strToUnitInfo; } }

            internal ConvertTable() {
                InitUnitInfos();
                InitUnitConvertTable();
            }

            internal void RegisterUnitInfo(UnitInfo info) {
                _unitInfos[info.unit] = info;
            }

            private void InitUnitInfos() {
                _unitInfos = new EnumArray<AngleUnit, UnitInfo>();
                RegisterUnitInfo(new UnitInfo(AngleUnit.Radian, "rad", 2 * Math.PI));
                RegisterUnitInfo(new UnitInfo(AngleUnit.Degree, "deg", 360));
                RegisterUnitInfo(new UnitInfo(AngleUnit.Grade, "grad", 400));
            }

            private void UpdateUnitConvertTable() {
                foreach (AngleUnit u1 in Enum.GetValues(typeof(AngleUnit))) {
                    foreach (AngleUnit u2 in Enum.GetValues(typeof(AngleUnit))) {
                        _unitConvertTable[u1, u2] = _unitInfos[u1].valueOfLoop / _unitInfos[u2].valueOfLoop;
                    }
                }
            }

            private void InitUnitConvertTable() {
                _unitConvertTable = new EnumArray2D<AngleUnit, double>();
                _lastCharToUnitInfo = new Dictionary<char, List<UnitInfo>>();
                _strToUnitInfo = new Dictionary<string, UnitInfo>();

                foreach (UnitInfo ui in _unitInfos) {
                    char c = char.ToUpper(ui.name.Last<char>());
                    if (!_lastCharToUnitInfo.ContainsKey(c)) {
                        _lastCharToUnitInfo[c] = new List<UnitInfo>();
                    }
                    _lastCharToUnitInfo[c].Add(ui);
                    _lastCharToUnitInfo[c].Sort();
                    if (string.IsNullOrEmpty(ui.name)) {
                        _strToUnitInfo.Add(ui.name.ToUpper(), ui);
                    }
                }

                UpdateUnitConvertTable();

            }
        }
    }

    public interface IUnitConvertible {
        Distance ToDistance(LengthUnit unit);
        Angle ToAngle(AngleUnit unit);
    }

    public class UnitConvert {
        #region Distance型への変換処理
        public static Distance ToDistance(byte value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(byte value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(sbyte value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(sbyte value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(short value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(short value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(ushort value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(ushort value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(int value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(int value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(uint value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(uint value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(long value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(long value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(ulong value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(ulong value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(float value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(float value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(double value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(double value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(decimal value, LengthUnit unit) {
            return new Distance((double)value, unit);
        }
        public static Distance ToDistance(decimal value) {
            return ToDistance(value, Distance.DefaultUnit);
        }

        public static Distance ToDistance(Distance value, LengthUnit unit) {
            return value;
        }
        public static Distance ToDistance(Distance value) {
            return value;
        }

        public static Distance ToDistance(string value) {
            return Distance.Parse(value);
        }

        public static Distance ToDistance(object value, LengthUnit unit) {
            if (value is string) {
                return Distance.Parse((string)value);
            }
            IUnitConvertible uc = value as IUnitConvertible;
            if (uc != null) {
                return uc.ToDistance(unit);
            }

            if (value is IConvertible) {
                return new Distance(Convert.ToDouble(value), unit);
            }
            throw new InvalidCastException();
        }

        public static Distance ToDistance(object value) {
            return ToDistance(value, Distance.DefaultUnit);
        }
        #endregion

        #region Angle型への変換処理
        public static Angle ToAngle(byte value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(byte value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(sbyte value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(sbyte value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(short value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(short value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(ushort value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(ushort value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(int value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(int value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(uint value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(uint value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(long value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(long value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(ulong value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(ulong value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(float value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(float value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(double value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(double value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(decimal value, AngleUnit unit) {
            return new Angle((double)value, unit);
        }
        public static Angle ToAngle(decimal value) {
            return ToAngle(value, Angle.DefaultUnit);
        }

        public static Angle ToAngle(Angle value, AngleUnit unit) {
            return value;
        }
        public static Angle ToAngle(Angle value) {
            return value;
        }

        public static Angle ToAngle(string value) {
            return Angle.Parse(value);
        }

        public static Angle ToAngle(object value, AngleUnit unit) {
            if (value is string) {
                return Angle.Parse((string)value);
            }
            IUnitConvertible uc = value as IUnitConvertible;
            if (uc != null) {
                return uc.ToAngle(unit);
            }

            if (value is IConvertible) {
                return new Angle(Convert.ToDouble(value), unit);
            }
            throw new InvalidCastException();
        }

        public static Angle ToAngle(object value) {
            return ToAngle(value, Angle.DefaultUnit);
        }
        #endregion
    }
}
