using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Sketch {
    [Serializable]
    public sealed class Document: ExprComponent<DocumentRoot>, IExprComponentOwnable, INamedCollectionOwnable<Sheet>, INamedCollectionOwnable<Layer> {
        /// <summary>
        /// シートが1つ存在する新規ドキュメントを作成します
        /// </summary>
        /// <returns></returns>
        public static Document NewDocument() {
            Document ret = new Document(DocumentRoot.Root);
            Sheet sh = new Sheet(ret);
            //sh.Layers.Add(new Layer(sh));
            new Layer(sh);
            return ret;
        }
        public class SheetCollection: NamedCollection<Sheet, Document> {
            internal SheetCollection(Document owner) : base(owner) { }

            public Sheet NewItem() {
                return new Sheet(Owner);
            }

        }

        private string _fullPath;
        private SheetCollection _sheets;

        public string FullPath {
            get {
                return _fullPath;
            }
            // ファイル名はSaveAsで変更する
        }

        public SheetCollection Sheets {
            get {
                return _sheets;
            }
        }

        //public Sheet ActiveSheet {
        //    get {
        //        return _activeSheet;
        //    }
        //    set {
        //        _activeSheet = value;
        //    }
        //}

        //private Layer _activeLayer;
        //public Layer ActiveLayer {
        //    get {
        //        return _activeLayer;
        //    }
        //    set {
        //        _activeLayer = value;
        //    }
        //}

        //private UndoManager _undoManager = new UndoManager();

        //public UndoManager UndoManager { get { return _undoManager; } }

        private void InvalidateScaling() {
        }

        private static PropertyDefCollection _builtinPropertyDefs = new PropertyDefCollection(
            new PropertyDef[] {
                new PropertyDef("DefaultScaling", typeof(double), "1.0"),
                new PropertyDef("DefaultSheetWidth", typeof(Distance), "Settings.DefaultSheetWidth"),
                new PropertyDef("DefaultSheetHeight", typeof(Distance), "Settings.DefaultSheetHeight"),
                new PropertyDef("DefaultPaperSizeName", typeof(string), "Settings.DefaultPaperSizeName"),
                new PropertyDef("DefaultPaperWidth", typeof(Distance), "Settings.DefaultPaperWidth"),
                new PropertyDef("DefaultPaperHeight", typeof(Distance), "Settings.DefaultPaperHeight"),
                new PropertyDef("DefaultFontName", typeof(string), "Settings.DefaultFontName"),
                new PropertyDef("DefaultFontSize", typeof(double), "Settings.DefaultFontSize"),
                new PropertyDef("DefaultIsFontBold", typeof(bool), "Settings.DefaultIsFontBold"),
                new PropertyDef("DefaultIsFontItalic", typeof(bool), "Settings.DefaultIsFontItalic"),
            });

        public const int PROP_SCALING = 0;
        public const int PROP_DEFAULTSHEETWIDTH = 1;
        public const int PROP_DEFAULTSHEETHEIGHT = 2;
        public const int PROP_DEFAULTPAPERSIZENAME = 3;
        public const int PROP_DEFAULTPAPERWIDTH = 4;
        public const int PROP_DEFAULTPAPERHEIGHT = 5;
        public const int PROP_DEFAULTFONTNAME = 6;
        public const int PROP_DEFAULTFONTSIZE = 7;
        public const int PROP_DEFAULTISFONTBOLD = 8;
        public const int PROP_DEFAULTISFONTITALIC = 9;

        public override PropertyDefCollection GetBuiltinPropertyDefs() {
            return _builtinPropertyDefs;
        }

        public FormulaProperty FormulaScaling { get { return Formula[PROP_SCALING]; } }
        public FormulaProperty FormulaDefaultSheetWidth { get { return Formula[PROP_DEFAULTSHEETWIDTH]; } }
        public FormulaProperty FormulaDefaultSheetHeight { get { return Formula[PROP_DEFAULTSHEETHEIGHT]; } }
        public FormulaProperty FormulaDefaultPaperSizeName { get { return Formula[PROP_DEFAULTPAPERSIZENAME]; } }
        public FormulaProperty FormulaDefaultPaperWidth { get { return Formula[PROP_DEFAULTPAPERWIDTH]; } }
        public FormulaProperty FormulaDefaultPaperHeight { get { return Formula[PROP_DEFAULTPAPERHEIGHT]; } }
        public FormulaProperty FormulaDefaultFontName { get { return Formula[PROP_DEFAULTFONTNAME]; } }
        public FormulaProperty FormulaDefaultFontSize { get { return Formula[PROP_DEFAULTFONTSIZE]; } }
        public FormulaProperty FormulaDefaultIsFontBold { get { return Formula[PROP_DEFAULTISFONTBOLD]; } }
        public FormulaProperty FormulaDefaultIsFontItalic { get { return Formula[PROP_DEFAULTISFONTITALIC]; } }

        [FormulaIndex(PROP_SCALING)]
        public double DefaultScaling {
            get { return Formula[PROP_SCALING].DoubleValue; }
            set { Formula[PROP_SCALING].SetValue(value, EditingLevel.EditByValue); }
        }

        public Size2D DefaultSheetSize { get { return new Size2D(DefaultSheetWidth, DefaultSheetHeight); } }
        [FormulaIndex(PROP_DEFAULTSHEETWIDTH)]
        public Distance DefaultSheetWidth {
            get { return Formula[PROP_DEFAULTSHEETWIDTH].DistanceValue; }
            set { Formula[PROP_DEFAULTSHEETWIDTH].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_DEFAULTSHEETHEIGHT)]
        public Distance DefaultSheetHeight {
            get { return Formula[PROP_DEFAULTSHEETHEIGHT].DistanceValue; }
            set { Formula[PROP_DEFAULTSHEETHEIGHT].SetValue(value, EditingLevel.EditByValue); }
        }


        [FormulaIndex(PROP_DEFAULTPAPERSIZENAME)]
        public string DefaultPaperSizeName {
            get { return Formula[PROP_DEFAULTPAPERSIZENAME].StringValue; }
            set { Formula[PROP_DEFAULTPAPERSIZENAME].SetValue(value, EditingLevel.EditByValue); }
        }
        public Size2D DefaultPaperSize { get { return new Size2D(DefaultPaperWidth, DefaultPaperHeight); } }
        [FormulaIndex(PROP_DEFAULTPAPERWIDTH)]
        public Distance DefaultPaperWidth {
            get { return Formula[PROP_DEFAULTPAPERWIDTH].DistanceValue; }
            set { Formula[PROP_DEFAULTPAPERWIDTH].SetValue(value, EditingLevel.EditByValue); }
        }
        [FormulaIndex(PROP_DEFAULTPAPERHEIGHT)]
        public Distance DefaultPaperHeight {
            get { return Formula[PROP_DEFAULTPAPERHEIGHT].DistanceValue; }
            set { Formula[PROP_DEFAULTPAPERHEIGHT].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_DEFAULTFONTNAME)]
        public string DefaultFontName {
            get { return Formula[PROP_DEFAULTFONTNAME].StringValue; }
            set { Formula[PROP_DEFAULTFONTNAME].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_DEFAULTFONTSIZE)]
        public double DefaultFontSize {
            get { return Formula[PROP_DEFAULTFONTSIZE].DoubleValue; }
            set { Formula[PROP_DEFAULTFONTSIZE].SetValue(value, EditingLevel.EditByValue); }
        }

        [FormulaIndex(PROP_DEFAULTISFONTBOLD)]
        public bool DefaultIsFontBold {
            get { return Formula[PROP_DEFAULTISFONTBOLD].BooleanValue; }
            set { Formula[PROP_DEFAULTISFONTBOLD].SetValue(value, EditingLevel.EditByValue); }
        }
        
        [FormulaIndex(PROP_DEFAULTISFONTITALIC)]
        public bool DefaultIsFontItalic {
            get { return Formula[PROP_DEFAULTISFONTITALIC].BooleanValue; }
            set { Formula[PROP_DEFAULTISFONTITALIC].SetValue(value, EditingLevel.EditByValue); }
        }

        public Document(DocumentRoot owner)
            : base(owner) {
            _sheets = new SheetCollection(this);
            _fullPath = string.Empty;
        }

        public void OnCollectionItemAdded(object collection, Sheet item) {
            //
        }

        public void OnCollectionItemRemoved(object collection, Sheet item) {
            //
        }

        public void OnCollectionItemAdded(object collection, Layer item) {
            //
        }

        public void OnCollectionItemRemoved(object collection, Layer item) {
            //
        }

        void IExprComponentOwnable.Add(object item) {
            if (item == null) {
                throw new ArgumentNullException();
            }
            Sheet p = item as Sheet;
            if (p != null) {
                Sheets.Add(p);
            }
        }

        void IExprComponentOwnable.Remove(object item) {
            Sheets.Remove(item as Sheet);
        }

        string IExprComponentOwnable.GetNewName(object target) {
            if (target == null) {
                throw new ArgumentNullException();
            }
            Sheet p = target as Sheet;
            if (p != null) {
                return Sheets.GetNewName(p.Name);
            }
            throw new ArgumentException();
        }
    }
}
