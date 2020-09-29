using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Not implemented
namespace Sketch {
    public interface IUndoRecorder {
        string Description { get; }
        void Undo();
        void Redo();
    }

    public interface IUndoManager {
        void Add(IUndoRecorder item);
    }

    public class ShapePropertyUndoRecorder : IUndoRecorder {
        private Document _document;
        private Sheet _sheet;
        private string _description;
        private string _expr;
        private string _oldFormula;
        private string _newFormula;

        private void SetDocumentAndPath(Shape shape) {
            _document = shape.Document;
            _expr = string.Format("Shapes(\"{0}\")", shape.Name);
        }

        public ShapePropertyUndoRecorder() { }
        public ShapePropertyUndoRecorder(string description, Shape shape, string formulaName, string oldFormula, string newFormula) {
            if (shape == null) {
                throw new ArgumentNullException("component");
            }
            SetDocumentAndPath(shape);
            _description = description;
            _oldFormula = oldFormula;
            _newFormula = newFormula;
            _sheet.AllShapes["shape.1"].Formula["Angle"].SetFormula("0deg");
        }

        public string Description { get { return _description; } }

        public void Undo() {
            TemporaryFormula expr = new TemporaryFormula(_sheet, _expr);
            FormulaProperty prop = expr.Value as FormulaProperty;
            if (prop != null) {
                prop.SetFormula(_oldFormula);
            }
            //UndoManager.
        }
        public void Redo() {

        }
    }

    public class GroupUndoRecorder: IUndoRecorder, IUndoManager {
        private string _description;
        private List<IUndoRecorder> _list = new List<IUndoRecorder>();

        public GroupUndoRecorder(string description) {
            _description = description;
        }

        public string Description { get { return _description; } }

        public void Undo() {
            for (int i = 0, n = _list.Count; i < n; i++) {
                _list[i].Undo();
            }
        }
        public void Redo() {
            for (int i = _list.Count - 1; 0 <= i; i--) {
                _list[i].Redo();
            }
        }

        public void Add(IUndoRecorder item) {
            _list.Add(item);
        }
    }

    public class UndoManager {
        private List<IUndoRecorder> _undoList = new List<IUndoRecorder>();
        private List<IUndoRecorder> _redoList = new List<IUndoRecorder>();
        public void Add(IUndoRecorder item) {
            _undoList.Add(item);
            _redoList.Clear();
        }
        public void Undo() {
            if (0 < _undoList.Count) {
                IUndoRecorder obj = _undoList[0];
                try {
                    obj.Undo();
                } finally {
                    _redoList.Add(obj);
                    _undoList.RemoveAt(0);
                }
            }
        }
    }
}
