using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;

namespace Sketch {
    public enum EvalSpec {
        /// <summary>
        /// 定数を返す。常にこの値は変わらない
        /// 例: 数値を直接指定した場合
        /// </summary>
        Constant = 0,
        /// <summary>
        /// 他オブジェクトのプロパティを参照している
        /// Invalidate()が呼ばれない限りは値が不変であることが保証されている
        /// </summary>
        PropertyDependent = 1,
        /// <summary>
        /// 評価するたびに可変である。
        /// PropertyDependentと違い、評価するたびに値が変わる可能性がある
        /// 例: 日付・時刻を返す関数
        /// </summary>
        Variable = 2,
        /// <summary>
        /// 関数従属性(引数が同じなら必ず同じ値を返す)がある。
        /// </summary>
        FunctionalDependent = 3,
    }
    public class EvalSpecAttribute: Attribute {
        private EvalSpec _spec;
        public EvalSpecAttribute(EvalSpec spec) {
            _spec = spec;
        }
        public EvalSpec Spec { get { return _spec; } }
    }

    public class EmbedFunc {
        private static EmbedFunc _value = new EmbedFunc();
        public static EmbedFunc Value { get { return _value; } }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static sbyte Abs(sbyte value) {
            return Math.Abs(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static byte Abs(byte value) {
            return value;
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static short Abs(short value) {
            return Math.Abs(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static ushort Abs(ushort value) {
            return value;
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Abs(int value) {
            return Math.Abs(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static uint Abs(uint value) {
            return value;
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static long Abs(long value) {
            return Math.Abs(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static ulong Abs(ulong value) {
            return value;
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static decimal Abs(decimal value) {
            return Math.Abs(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static float Abs(float value) {
            return Math.Abs(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Abs(double value) {
            return Math.Abs(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static decimal Ceiling(decimal value) {
            return Math.Ceiling(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static float Ceiling(float value) {
            return (float)Math.Ceiling(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Ceiling(double value) {
            return Math.Ceiling(value);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static float Exp(float value) {
            return (float)Math.Exp(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Exp(double value) {
            return Math.Exp(value);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static decimal Floor(decimal value) {
            return Math.Floor(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static float Floor(float value) {
            return (float)Math.Floor(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Floor(double value) {
            return Math.Floor(value);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static sbyte Max(sbyte val1, sbyte val2) {
            return Math.Max(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static byte Max(byte val1, byte val2) {
            return Math.Max(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static short Max(short val1, short val2) {
            return Math.Max(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static ushort Max(ushort val1, ushort val2) {
            return Math.Max(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Max(int val1, int val2) {
            return Math.Max(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static uint Max(uint val1, uint val2) {
            return Math.Max(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static long Max(long val1, long val2) {
            return Math.Max(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static ulong Max(ulong val1, ulong val2) {
            return Math.Max(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static decimal Max(decimal val1, decimal val2) {
            return Math.Max(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static float Max(float val1, float val2) {
            return Math.Max(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Max(double val1, double val2) {
            return Math.Max(val1, val2);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static sbyte Min(sbyte val1, sbyte val2) {
            return Math.Min(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static byte Min(byte val1, byte val2) {
            return Math.Min(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static short Min(short val1, short val2) {
            return Math.Min(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static ushort Min(ushort val1, ushort val2) {
            return Math.Min(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Min(int val1, int val2) {
            return Math.Min(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static uint Min(uint val1, uint val2) {
            return Math.Min(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static long Min(long val1, long val2) {
            return Math.Min(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static ulong Min(ulong val1, ulong val2) {
            return Math.Min(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static decimal Min(decimal val1, decimal val2) {
            return Math.Min(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static float Min(float val1, float val2) {
            return Math.Min(val1, val2);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Min(double val1, double val2) {
            return Math.Min(val1, val2);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Pow(double x, double y) {
            return Math.Pow(x, y);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static decimal Round(decimal value) {
            return Math.Round(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static float Round(float value) {
            return (float)Math.Round(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Round(double value) {
            return Math.Round(value);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Sign(sbyte value) {
            return Math.Sign(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Sign(byte value) {
            return Math.Sign(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Sign(short value) {
            return Math.Sign(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Sign(int value) {
            return Math.Sign(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Sign(long value) {
            return Math.Sign(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Sign(decimal value) {
            return Math.Sign(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Sign(float value) {
            return Math.Sign(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static int Sign(double value) {
            return Math.Sign(value);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Sqrt(double value) {
            return Math.Sqrt(value);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static decimal Trunc(decimal value) {
            return Math.Truncate(value);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Trunc(double value) {
            return Math.Truncate(value);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Log10(double value) {
            return Math.Log10(value);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Ln(double value) {
            return Math.Log(value);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static double Log(double d, double newBase) {
            return Math.Log(d, newBase);
        }

        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static DateTime Date(int year, int month, int day) {
            return new DateTime(year, month, day);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static DateTime Date(int year, int month, int day, int hour, int minute, int second) {
            return new DateTime(year, month, day, hour, minute, second);
        }
        [EvalSpec(EvalSpec.FunctionalDependent)]
        public static TimeSpan Time(int days, int hour, int minute, int second) {
            return new TimeSpan(days, hour, minute, second);
        }

        [EvalSpec(EvalSpec.Variable)]
        public static DateTime Now { get { return DateTime.Now; } }
        [EvalSpec(EvalSpec.Variable)]
        public static DateTime UtcNow { get { return DateTime.UtcNow; } }
        [EvalSpec(EvalSpec.Variable)]
        public static DateTime Today { get { return DateTime.Today; } }

        [EvalSpec(EvalSpec.Constant)]
        public static double E { get { return Math.E; } }
        [EvalSpec(EvalSpec.Constant)]
        public static double PI { get { return Math.PI; } }
    }
}
