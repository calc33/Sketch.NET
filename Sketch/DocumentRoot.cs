using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sketch {
    public sealed class DocumentRoot : /*ExprComponent,*/ IExprComponentOwnable, INamedCollectionOwnable<Document> {
        public class DocumentCollection: NamedCollection<Document, DocumentRoot> {
            internal DocumentCollection(DocumentRoot owner)
                : base(owner) {
                NewNameBase = "新規ドキュメント";
            }

            public override string ValidNameDescription {
                get {
                    return "\\ / : * ? \" < > | は使えません";
                }
            }

            public override bool IsValidName(string name) {
                if (string.IsNullOrEmpty(name)) {
                    return true;
                }
                foreach (char c in name) {
                    if (_invalidChars.ContainsKey(c)) {
                        return false;
                    }
                }
                return true;
            }
            public override string GetNewName(string baseName) {
                if (this[baseName] == null) {
                    return baseName;
                }
                string s = Path.GetFileNameWithoutExtension(baseName);
                int i = s.Length - 1;
                for (; 0 <= i && char.IsDigit(s[i]); i--) ;
                string basePart = s.Substring(0, i + 1);
                string numPart = s.Substring(i + 1);
                int n = 0;
                int.TryParse(numPart, out n);
                n++;
                string newName = basePart + n.ToString();
                while (this[newName] != null) {
                    n++;
                    newName = basePart + n.ToString();
                }
                return newName;
            }
        }

        private static readonly Dictionary<char, char> _invalidChars = GetInvalidChars();
        private static Dictionary<char, char> GetInvalidChars() {
            const string INVALID_CHARS = "\\/:*?\"!<>| ";
            Dictionary<char, char> ret = new Dictionary<char, char>();
            foreach (char c in INVALID_CHARS) {
                ret.Add(c, c);
            }
            return ret;
        }

        private DocumentCollection _documents;
        public DocumentCollection Documents { get { return _documents; } }

        public DocumentRoot() {
            _documents = new DocumentCollection(this);
        }

        private static DocumentRoot _root = new DocumentRoot();
        public static DocumentRoot Root { get { return _root; } }

        //protected override SectionDef[] GetBuiltinSectionDefs() {
        //    return null;
        //}

        public void OnCollectionItemAdded(object collection, Document item) {
            //
        }

        public void OnCollectionItemRemoved(object collection, Document item) {
            //
        }

        void IExprComponentOwnable.Add(object item) {
            Document doc = item as Document;
            if (doc != null) {
                Documents.Add(doc);
            }
        }

        void IExprComponentOwnable.Remove(object item) {
            Documents.Remove(item as Document);
        }


        string IExprComponentOwnable.GetNewName(object target) {
            if (target == null) {
                throw new ArgumentNullException();
            }
            Document d = target as Document;
            if (d != null) {
                return Documents.GetNewName(d.Name);
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// シートが1つ存在する新規ドキュメントを作成します
        /// </summary>
        /// <returns></returns>
        public static Document NewDocument() {
            return Document.NewDocument();
        }
    }
}
