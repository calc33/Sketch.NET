using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch {
    /// <summary>
    /// 座標系
    /// </summary>
    public class CoordSystem {
        private Point2D _origin;
        private double _scaling;
        private Angle _rotating;
        private bool _isFlipped;

        /// <summary>
        /// 座標系の原点
        /// </summary>
        public Point2D Origin {
            get {
                return _origin;
            }
            set {
                _origin = value;
            }
        }

        /// <summary>
        /// 縮尺
        /// </summary>
        public double Scaling {
            get {
                return _scaling;
            }
            set {
                _scaling = value;
            }
        }

        /// <summary>
        /// 傾斜
        /// </summary>
        public Angle Rotating {
            get {
                return _rotating;
            }
            set {
                _rotating = value;
            }
        }

        /// <summary>
        /// 上下反転
        /// </summary>
        public bool IsFlipped {
            get {
                return _isFlipped;
            }
            set {
                _isFlipped = value;
            }
        }

        /// <summary>
        /// この座標系上の座標を元座標系の座標へ変換
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Point2D ToGlobal(Point2D point) {
            Point2D p = point.Rotate(Rotating) * Scaling;
            if (IsFlipped) {
                p = new Point2D(p.X, -p.Y);
            }
            return p + Origin;
        }

        /// <summary>
        /// 元座標系上の座標をこの座標系上の座標へ変換
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Point2D ToLocal(Point2D point) {
            Point2D p = (point - Origin).Rotate(-Rotating) / Scaling;
            if (IsFlipped) {
                p = new Point2D(p.X, -p.Y);
            }
            return p;
        }

        public static CoordSystem operator +(CoordSystem a, CoordSystem b) {
            CoordSystem ret = new CoordSystem();
            ret.Origin = a.ToGlobal(b.Origin);
            ret.Scaling = a.Scaling * b.Scaling;
            if (a.IsFlipped) {
                ret.Rotating = a.Rotating - b.Rotating;
            } else {
                ret.Rotating = a.Rotating + b.Rotating;
            }
            ret.IsFlipped = a.IsFlipped ^ b.IsFlipped;
            return ret;
        }
    }
}
