using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch {
    public class Page: ExprComponent<Document> {
        private Page _background;
        //private Document _document;
        //private Dictionary<Layer, LayerVisiblity> _layerVisiblity;
        //private Layer _activeLayer;
        //private string _paperSizeName;
        //private Size2D _paperSize;

        public Page Background {
            get {
                return _background;
            }
            set {
                _background = value;
            }
        }

        public Document Document {
            get {
                return Container;
            }
        }

        //public Layer ActiveLayer {
        //    get {
        //        return _activeLayer;
        //    }
        //}

        public event EventHandler<PropertyChangingEventArgs<string>> PaperSizeNameChanging;
        protected void OnPaperSizeNameChanging(PropertyChangingEventArgs<string> e) {
            if (PaperSizeNameChanging != null) {
                PaperSizeNameChanging(this, e);
            }
        }

        public event EventHandler<PropertyChangedEventArgs<string>> PaperSizeNameChanged;
        protected void OnPaperSizeNameChanged(PropertyChangedEventArgs<string> e) {
            if (PaperSizeNameChanged != null) {
                PaperSizeNameChanged(this, e);
            }
        }

        public event EventHandler<PropertyChangingEventArgs<Size2D>> PaperSizeChanging;
        protected void OnPaperSizeChanging(PropertyChangingEventArgs<Size2D> e) {
            if (PaperSizeChanging != null) {
                PaperSizeChanging(this, e);
            }
        }

        public event EventHandler<PropertyChangedEventArgs<Size2D>> PaperSizeChanged;
        protected void OnPaperSizeChanged(PropertyChangedEventArgs<Size2D> e) {
            if (PaperSizeChanged != null) {
                PaperSizeChanged(this, e);
            }
        }

        public Page(Document container)
            : base(container) {
            //Formula[SECTION_PRINTING, PROP_PAPERSIZENAME].Formula = "=Document.DefaultPaperSizeName";
            //Formula[SECTION_PRINTING, PROP_PAPERWIDTH].Formula = "=Document.DefaultPaperWidth";
            //Formula[SECTION_PRINTING, PROP_PAPERHEIGHT].Formula = "=Document.DefaultPaperHeight";
        }

        public Shape AddLine(Point2D lineFrom, Point2D lineTo) {
            return new Shape(Document, Document.ActiveLayer);
        }
        public Shape AddLine(Layer layer, Point2D lineFrom, Point2D lineTo) {
            return new Shape(Document, layer);
        }

        internal static readonly SectionDef[] _builtinSectionDefs = new SectionDef[] {
            new SectionDef("Geometry", 1, 1, false, new PropertyDef[] {
                new PropertyDef("Flipped", typeof(bool), "=FALSE"),
                new PropertyDef("BackgroundPage", typeof(Page), null),
                new PropertyDef("Scaling", typeof(double), "=Document.Scaling"),
                new PropertyDef("PageWidth", typeof(Distance), "=PaperWidth/Scaling"),
                new PropertyDef("PageHeight", typeof(Distance), "=PaperHeight/Scaling"),
            }),
            new SectionDef("Printing", 1, 1, false, new PropertyDef[] {
                new PropertyDef("PaperSizeName", typeof(string), "=Document.DefaultPaperSizeName"),
                new PropertyDef("PaperWidth", typeof(Distance), "=Document.DefaultPaperWidth"),
                new PropertyDef("PaperHeight", typeof(Distance), "=Document.DefaultPaperHeight")
            }),
            //new SectionDef("Layer", null),
        };

        public const int SECTION_GEOMETRY = 0;
        public const int INDEX_GEOMETRY = 0;
        public const int PROP_FLIPPED = 0;
        public const int PROP_BACKGROUPNDPAGE = 1;
        public const int PROP_SCALING = 2;
        public const int PROP_PAGEWIDTH = 3;
        public const int PROP_PAGEHEIGHT = 4;

        public const int SECTION_PRINTING = 1;
        public const int INDEX_PRINTING = 0;
        public const int PROP_PAPERSIZENAME = 0;
        public const int PROP_PAPERWIDTH = 1;
        public const int PROP_PAPERHEIGHT = 2;

        protected override SectionDef[] GetSectionDefs() {
            return _builtinSectionDefs;
        }

        [FormulaIndex(SECTION_GEOMETRY, INDEX_GEOMETRY, PROP_FLIPPED)]
        public bool Flipped {
            get { return GetBooleanFormulaValue(SECTION_GEOMETRY, INDEX_GEOMETRY, PROP_FLIPPED); }
            set { SetFormulaValue(SECTION_GEOMETRY, INDEX_GEOMETRY, PROP_FLIPPED, value); }
        }

        [FormulaIndex(SECTION_GEOMETRY, INDEX_GEOMETRY, PROP_SCALING)]
        public double Scaling {
            get { return GetDoubleFormulaValue(SECTION_GEOMETRY, INDEX_GEOMETRY, PROP_SCALING); }
            set { SetFormulaValue(SECTION_GEOMETRY, INDEX_GEOMETRY, PROP_SCALING, value); }
        }

        public Size2D PageSize { get { return new Size2D(PageWidth, PageHeight); } }
        [FormulaIndex(SECTION_PRINTING, INDEX_PRINTING, PROP_PAGEWIDTH)]
        public Distance PageWidth {
            get { return GetDistanceFormulaValue(SECTION_PRINTING, INDEX_GEOMETRY, PROP_PAGEWIDTH); }
            set { SetFormulaValue(SECTION_PRINTING, INDEX_PRINTING, PROP_PAGEWIDTH, value); }
        }
        [FormulaIndex(SECTION_PRINTING, INDEX_PRINTING, PROP_PAGEHEIGHT)]
        public Distance PageHeight {
            get { return GetDistanceFormulaValue(SECTION_PRINTING, INDEX_GEOMETRY, PROP_PAGEHEIGHT); }
            set { SetFormulaValue(SECTION_PRINTING, INDEX_PRINTING, PROP_PAGEHEIGHT, value); }
        }


        [FormulaIndex(SECTION_PRINTING, INDEX_PRINTING, PROP_PAPERSIZENAME)]
        public string PaperSizeName {
            get { return GetStringFormulaValue(SECTION_PRINTING, INDEX_PRINTING, PROP_PAPERSIZENAME); }
            set { SetFormulaValue(SECTION_PRINTING, INDEX_PRINTING, PROP_PAPERSIZENAME, value); }
        }
        public Size2D PaperSize { get { return new Size2D(PaperWidth, PaperHeight); } }
        [FormulaIndex(SECTION_PRINTING, INDEX_PRINTING, PROP_PAPERWIDTH)]
        public Distance PaperWidth {
            get { return GetDistanceFormulaValue(SECTION_PRINTING, INDEX_PRINTING, PROP_PAPERWIDTH); }
            set { SetFormulaValue(SECTION_PRINTING, INDEX_PRINTING, PROP_PAPERWIDTH, value); }
        }
        [FormulaIndex(SECTION_PRINTING, INDEX_PRINTING, PROP_PAPERHEIGHT)]
        public Distance PaperHeight {
            get { return GetDistanceFormulaValue(SECTION_PRINTING, INDEX_PRINTING, PROP_PAPERHEIGHT); }
            set { SetFormulaValue(SECTION_PRINTING, INDEX_PRINTING, PROP_PAPERHEIGHT, value); }
        }
    }
}
