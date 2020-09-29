using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch {
    public enum SelectOperation {
        Select = 0,
        Deselect = 1,
        DeselectAll = 2
    }

    public class SelectionChangingEventArgs: EventArgs {
        private Shape _shape;
        private SelectOperation _operation;
        private bool _permitted;
        public Shape Shape { get { return _shape; } }
        public SelectOperation Operation { get { return _operation; } }
        public bool Permitted { get { return _permitted; } set { _permitted = value; } }

        public SelectionChangingEventArgs(Shape shape, SelectOperation operation) {
            _shape = shape;
            _operation = operation;
            _permitted = true;
        }

        public SelectionChangedEventArgs ToSelectionChangedEventArgs() {
            return Permitted ? new SelectionChangedEventArgs(Shape, Operation) : null;
        }
    }

    public class SelectionChangedEventArgs: EventArgs {
        private Shape _shape;
        private SelectOperation _operation;
        public Shape Shape { get { return _shape; } }
        public SelectOperation Operation { get { return _operation; } }

        public SelectionChangedEventArgs(Shape shape, SelectOperation operation) {
            _shape = shape;
            _operation = operation;
        }
    }

    public partial class Selection: ICollection<Shape> {
        private WeakReference _owner;
        private List<Shape> _items = new List<Shape>();
        private Dictionary<Shape, bool> _shapeToSelected = null;
        private List<KnobInfo> _knobs = new List<KnobInfo>();
        private Rectangle2D _bounds;
        private bool _isBoundsValid = false;

        public Rectangle2D Bounds {
            get {
                UpdateBounds();
                return _bounds;
            }
        }


        //private List<KnobControl> _knobControls = new List<KnobControl>();
        
        //private Sheet _sheet;
        //private bool _isVisibleRectValid = false;
        //private Rectangle2D _visibleRect;

        internal Selection(Canvas owner) {
            _owner = new WeakReference(owner);
            if (owner != null) {
                SelectionChanging += owner.OnSelectionChanging;
                SelectionChanged += owner.OnSelectionChanged;
            }
        }

        private Canvas Owner { get { return (_owner != null) & _owner.IsAlive ? _owner.Target as Canvas : null; } }
        public Shape this[int index] { get { return _items[index]; } }

        public event EventHandler<SelectionChangingEventArgs> SelectionChanging;
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        //public void UpdateKnob() {
        //    foreach (KnobControl c in _knobControls) {
        //        c.DisposeControl();
        //    }
        //    if (Count == 1) {
        //        KnobControl[] ctls = this[0].GetKnobControls(Owner);
        //    }
        //}

        //public void UpdateKnobPosition() {
        //    foreach (KnobControl c in _knobControls) {
        //        c.UpdatePosition();
        //    }
        //}

        private void InvalidateShapeToSelected() {
            _shapeToSelected = null;
        }

        private void UpdateShapeToSelected() {
            if (_shapeToSelected != null) {
                return;
            }
            _shapeToSelected = new Dictionary<Shape, bool>(_items.Count);
            foreach (Shape sh in _items) {
                _shapeToSelected.Add(sh, true);
            }
        }

        private void InvalidateBounds() {
            _isBoundsValid = false;
        }

        private void UpdateBounds() {
            if (_isBoundsValid) {
                return;
            }
            _bounds = Rectangle2D.Empty;
            if (0 < _items.Count) {
                Distance x0 = new Distance(double.MaxValue, Distance.DefaultUnit);
                Distance x1 = new Distance(double.MinValue, Distance.DefaultUnit);
                Distance y0 = new Distance(double.MaxValue, Distance.DefaultUnit);
                Distance y1 = new Distance(double.MinValue, Distance.DefaultUnit);
                foreach (Shape sh in _items) {
                    Rectangle2D r = sh.GetBounds(Angle.Zero);
                    x0 = Distance.Min(x0, r.Left);
                    x1 = Distance.Max(x1, r.Right);
                    y0 = Distance.Min(y0, r.Top);
                    y1 = Distance.Max(y1, r.Bottom);
                }
                _bounds = new Rectangle2D(x0, y0, x1, y1);
            }
            _isBoundsValid = true;
        }

        protected virtual void OnSelectionChanging(SelectionChangingEventArgs e) {
            if (SelectionChanging != null) {
                SelectionChanging(this, e);
            }
        }
        
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e) {
            InvalidateShapeToSelected();
            InvalidateBounds();
            Owner.UpdateKnob();
            if (SelectionChanged != null) {
                SelectionChanged(this, e);
            }
        }

        #region ICollectionの実装
        public void Add(Shape item) {
            if (item == null) {
                return;
            }
            if (!_items.Contains(item)) {
                SelectionChangingEventArgs e = new SelectionChangingEventArgs(item, SelectOperation.Select);
                OnSelectionChanging(e);
                if (e.Permitted) {
                    try {
                        _items.Add(item);
                    } finally {
                        OnSelectionChanged(e.ToSelectionChangedEventArgs());
                    }
                }
            }
        }

        public void Clear() {
            for (int i = _items.Count - 1; 0 <= i; i--) {
                Shape sh = _items[i];
                SelectionChangingEventArgs e = new SelectionChangingEventArgs(sh, SelectOperation.DeselectAll);
                OnSelectionChanging(e);
                if (e.Permitted) {
                    try {
                        _items.RemoveAt(i);
                    } finally {
                        OnSelectionChanged(e.ToSelectionChangedEventArgs());
                    }
                }
            }
        }

        public bool Contains(Shape item) {
            //return _items.Contains(item);
            UpdateShapeToSelected();
            bool ret = false;
            if (_shapeToSelected.TryGetValue(item, out ret)) {
                return ret;
            }
            return false;
        }

        public void CopyTo(Shape[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
            InvalidateShapeToSelected();
        }

        public int Count {
            get { return _items.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(Shape item) {
            if (!_items.Contains(item)) {
                return false;
            }
            SelectionChangingEventArgs e = new SelectionChangingEventArgs(item, SelectOperation.Deselect);
            OnSelectionChanging(e);
            if (!e.Permitted) {
                return false;
            }
            try {
                return _items.Remove(item);
            } finally {
                OnSelectionChanged(e.ToSelectionChangedEventArgs());
            }
        }

        public IEnumerator<Shape> GetEnumerator() {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
        #endregion
    }
}
