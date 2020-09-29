using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch {
    public enum PaperOrientation {
        /// <summary>
        /// レポートの向きをプリンタのデフォルトの設定から取得します。
        /// </summary>
        DefaultPaperOrientation,
        /// <summary>
        /// レポートの向きを横にします。
        /// </summary>
        Landscape,
        /// <summary>
        /// レポートの向きを縦にします。
        /// </summary>
        Portrait
    };
    public enum RangeSelectionStyle {
        Partial,
        Full
    }
    public class SketchSettings {
        //new PropertyDef("DefaultSheetWidth", typeof(Distance), "Settings.DefaultSheetWidth"),
        //new PropertyDef("DefaultSheetHeight", typeof(Distance), "Settings.DefaultSheetHeight"),
        //new PropertyDef("DefaultPaperSizeName", typeof(string), "Settings.DefaultPaperSizeName"),
        //new PropertyDef("DefaultPaperWidth", typeof(Distance), "Settings.DefaultPaperWidth"),
        //new PropertyDef("DefaultPaperHeight", typeof(Distance), "Settings.DefaultPaperHeight"),
        private Distance _defaultSheetWidth;
        private Distance _defaultSheetHeight;
        private string _defaultPaperSizeName;
        private PaperOrientation _defaultPaperOrientation;
        private Distance _defaultPaperWidth;
        private Distance _defaultPaperHeight;
        private string _defaultFontName;
        private double _defaultFontSize;
        private bool _defaultIsFontBold;
        private bool _defaultIsFontItalic;

        private RangeSelectionStyle _rangeSelectionStyle;

        public Distance DefaultSheetWidth {
            get { return _defaultSheetWidth; }
            set { _defaultSheetWidth = value; }
        }
        public Distance DefaultSheetHeight {
            get { return _defaultSheetHeight; }
            set { _defaultSheetHeight = value; }
        }

        public string DefaultPaperSizeName {
            get { return _defaultPaperSizeName; }
            set {
                if (_defaultPaperSizeName != value) {
                    _defaultPaperSizeName = value;
                    UpdatePaperSize();
                }
            }
        }

        private void UpdatePaperSize() {
            //throw new NotImplementedException();
        }

        public PaperOrientation DefaultPaperOrientation {
            get { return _defaultPaperOrientation; }
            set { _defaultPaperOrientation = value; }
        }

        public Distance DefaultPaperWidth {
            get { return _defaultPaperWidth; }
            set { _defaultPaperWidth = value; }
        }
        public Distance DefaultPaperHeight {
            get { return _defaultPaperHeight; }
            set { _defaultPaperHeight = value; }
        }

        public string DefaultFontName {
            get { return _defaultFontName; }
            set { _defaultFontName = value; }
        }
        public double DefaultFontSize {
            get { return _defaultFontSize; }
            set { _defaultFontSize = value; }
        }
        public bool DefaultIsFontBold {
            get { return _defaultIsFontBold; }
            set { _defaultIsFontBold = value; }
        }
        public bool DefaultIsFontItalic {
            get { return _defaultIsFontItalic; }
            set { _defaultIsFontItalic = value; }
        }

        public RangeSelectionStyle RangeSelectionStyle {
            get { return _rangeSelectionStyle; }
            set { _rangeSelectionStyle = value; }
        }

        public SketchSettings() {
            _defaultPaperSizeName = "A4";
            Size2D s = PaperSize.A4;
            _defaultPaperWidth = s.Width;
            _defaultPaperHeight = s.Height;
            _defaultSheetWidth = s.Width;
            _defaultSheetHeight = s.Height;
            _defaultPaperOrientation = PaperOrientation.DefaultPaperOrientation;
        }

        private static SketchSettings _settings = new SketchSettings();
        public static SketchSettings Settings { get { return _settings; } }
    }
}
