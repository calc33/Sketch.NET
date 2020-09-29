using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace Sketch {
    partial class Expression {
        public struct TokenId: IComparable {
            private char _id;
            private const char NAMED_TOKEN_MIN = '\ue000';
            private const char VIRTUAL_TOKEN_MIN = '\uf000';
            private const char VIRTUAL_TOKEN_MAX = '\uffff';
            private static char _namedTokenMax = NAMED_TOKEN_MIN;
            private static char _virtualTokenMax = VIRTUAL_TOKEN_MIN;
            private static string[] _virtualTokenNames = new string[(int)VIRTUAL_TOKEN_MAX - (int)NAMED_TOKEN_MIN + 1];
            private static Dictionary<string, char> _nameToTokenId = new Dictionary<string, char>();

            public TokenId(char id) {
                _id = id;
            }
            public TokenId(string tokenName, bool isVirtual = true) {
                if (!_nameToTokenId.TryGetValue(tokenName, out _id)) {
                    if (isVirtual) {
                        _id = ++_virtualTokenMax;
                    } else {
                        _id = ++_namedTokenMax;
                    }
                    _virtualTokenNames[(int)_id - (int)NAMED_TOKEN_MIN] = tokenName;
                    _nameToTokenId.Add(tokenName, _id);
                } else {
                    if (isVirtual && (NAMED_TOKEN_MIN <= _id) && (_id < VIRTUAL_TOKEN_MIN)) {
                        throw new ArgumentException();
                    }
                    if (!isVirtual && (VIRTUAL_TOKEN_MIN <= _id) && (_id <= VIRTUAL_TOKEN_MAX)) {
                        throw new ArgumentException();
                    }
                }
            }

            public bool IsVirtual { get { return VIRTUAL_TOKEN_MIN < _id; } }
            public int CompareTo(object obj) {
                if (obj is TokenId) {
                    return _id.CompareTo(((TokenId)obj)._id);
                }
                throw new ArgumentException();
            }

            public override bool Equals(object obj) {
                if (obj is TokenId) {
                    return _id == ((TokenId)obj)._id;
                }
                return base.Equals(obj);
            }
            public override int GetHashCode() {
                return _id.GetHashCode();
            }
            public override string ToString() {
                if (NAMED_TOKEN_MIN <= _id) {
                    return _virtualTokenNames[(int)_id - (int)NAMED_TOKEN_MIN];
                } else {
                    return _id.ToString();
                }
            }

            public static bool operator ==(TokenId a, TokenId b) {
                return a._id == b._id;
            }

            public static bool operator !=(TokenId a, TokenId b) {
                return a._id != b._id;
            }

            public static readonly TokenId ANY_ID = new TokenId('\0');
            public static readonly TokenId IDENTIFIER = new TokenId("IDENTIFIER", false);
            public static readonly TokenId NUMERIC = new TokenId("NUMERIC", false);
            public static readonly TokenId LITERAL = new TokenId("LITERAL", false);
            public static readonly TokenId EQUAL2 = new TokenId("==", false);
            public static readonly TokenId NOTEQUAL = new TokenId("<>", false);
            public static readonly TokenId GREATEREQUAL = new TokenId(">=", false);
            public static readonly TokenId LESSEQUAL = new TokenId("<=", false);
            public static readonly TokenId IF = new TokenId("IF", false);
            public static readonly TokenId AND = new TokenId("AND", false);
            public static readonly TokenId OR = new TokenId("OR", false);
            public static readonly TokenId NOT = new TokenId("NOT", false);
            public static readonly TokenId CASE = new TokenId("CASE", false);
            //public static readonly TokenId VIRTUAL_TOKEN_MIN = '\ue000';
            public static readonly TokenId UNITEDVALUE = new TokenId("UNITEDVALUE", true);
            public static readonly TokenId EXPR = new TokenId("EXPR", true);
            public static readonly TokenId COMMAARGS = new TokenId("COMMAARGS", true);
            public static readonly TokenId ARGUMENTS = new TokenId("ARGUMENTS", true);
            public static readonly TokenId PROPERTY = new TokenId("PROPERTY", true);
            public static readonly TokenId UNIT = new TokenId("UNIT", true);
            public static readonly TokenId DATASOURCE = new TokenId("DATASOURCE", true);
            public static readonly TokenId FORMULA = new TokenId("FORMULA", true);
            public static readonly TokenId IMMEDIATEEVAL = new TokenId("IMMEDIATEEVAL", true);
        }
        
        internal static Dictionary<string, TokenId> _reservedWords = GetReservedWords();

        internal static Dictionary<string, TokenId> GetReservedWords() {
            Dictionary<string, TokenId> ret = new Dictionary<string, TokenId>();
            ret.Add("E", new TokenId('E'));
            //ret.Add("e", new TokenId('E'));
            ret.Add("IF", TokenId.IF);
            ret.Add("AND", TokenId.AND);
            ret.Add("OR", TokenId.OR);
            ret.Add("NOT", TokenId.NOT);
            ret.Add("CASE", TokenId.CASE);
            return ret;
        }
        internal static TokenId TextToTokenId(string text) {
            if (string.IsNullOrEmpty(text)) {
                return TokenId.IDENTIFIER;
            }
            TokenId ret;
            if (_reservedWords.TryGetValue(text.ToUpper(), out ret)) {
                return ret;
            }
            return TokenId.IDENTIFIER;
        }

        internal class Token {
            internal TokenId Id;
            internal string Indent;
            internal string Text;
            internal Node Node;
            public override string ToString() {
                return "(" + Id.ToString() + ")" + Text;
            }
        }
        internal class TokenTree {
            internal TokenId TokenId;
            internal ParseRule Rule;
            internal Token Token;
            internal TokenTree Next;
            internal TokenTree Child;
            
            internal TokenTree(Token token) {
                Rule = null;
                Token = token;
                TokenId = Token.Id;
            }

            internal TokenTree(ParseRule rule, TokenTree child) {
                TokenId = rule.RuleId;
                Rule = rule;
                Token = null;
                Next = null;
                Child = child;
            }
            
            internal void Reset() {
                Next = null;
                Child = null;
            }

            internal void Dispose() {
                Reset();
                Rule = null;
                Token = null;
            }
            
            private void InternalToString(StringBuilder buf) {
                if (Token != null) {
                    buf.Append(Token.Text);
                } else {
                    buf.Append("(" + TokenId.ToString() + ")");
                }
                if (Child != null) {
                    buf.Append(" [");
                    Child.InternalToString(buf);
                    for (TokenTree t = Child.Next; t != null; t = t.Next) {
                        buf.Append(' ');
                        t.InternalToString(buf);
                    }
                    buf.Append(']');
                }
            }
            
            public override string ToString() {
                StringBuilder buf = new StringBuilder();
                buf.Append('[');
                InternalToString(buf);
                for (TokenTree t = Next; t != null; t = t.Next) {
                    buf.Append(' ');
                    t.InternalToString(buf);
                }
                buf.Append(']');
                return buf.ToString();
            }

            //public Node ToSyntaxTree() {
            //    Node ret;
            //    List<Node> l = new List<Node>();
            //    if (Token != null && Token.Node != null) {
            //        ret = Token.Node;
            //        for (TokenTree t = Child; t != null; t = t.Next) {
            //            l.Add(ToSyntaxTree());
            //        }
            //        //ret.
            //    }
            //    return ret;
            //}

            public Node GetNode() {
                if (Rule != null) {
                    return Rule.Rule(this);
                }
                return Token.Node;
            }

            static internal TokenTree[] CreateTokenTree(Token[] tokens) {
                if (tokens == null || tokens.Length == 0) {
                    return null;
                }
                int n = tokens.Length;
                TokenTree[] ret = new TokenTree[n];
                for (int i = 0; i < n; i++) {
                    ret[i] = new TokenTree(tokens[i]);
                }
                return ret;
            }
        }
        internal class LexicalToken: Token {
            internal LexicalToken(TokenId id, string indent, string text, Node node) {
                Id = id;
                Indent = indent;
                Text = text;
                Node = node;
            }
            internal LexicalToken(TokenId id, string indent) {
                Id = id;
                Indent = indent;
                Text = id.ToString();
                Node = null;
            }
            internal LexicalToken(char id, string indent, string text) {
                Id = new TokenId(id);
                Indent = indent;
                Text = text;
                Node = null;
            }
            internal LexicalToken(char id, string indent) {
                Id = new TokenId(id);
                Indent = indent;
                Text = id.ToString();
                Node = null;
            }
            public override string ToString() {
                return Text;
            }
        }
        
        internal class CombinedToken: Token {
            Token[] Tokens;
            internal CombinedToken(Token[] tokens, int start, int length) {
                Tokens = new Token[length];
                Array.Copy(tokens, start, Tokens, 0, length);

            }
        }

        private static LexicalToken GetLiteralToken(string expr, int indentIndex, ref int index, int length) {
            int i0 = index;
            while (index < length && expr[index] == '"') {
                index++;
                while (index < length && expr[index] != '"') {
                    if (char.IsSurrogate(expr, index)) {
                        index++;
                    }
                    index++;
                }
                if (length <= index) {
                    throw new ParseException("Unexpected end of line");
                }
                index++;
            }
            string spc = expr.Substring(indentIndex, i0 - indentIndex);
            string v = expr.Substring(i0, index - i0);
            StringBuilder buf = new StringBuilder(v);
            buf.Remove(0, 1);
            buf.Remove(buf.Length - 1, 1);
            for (int i = 0; i < buf.Length; i++) {
                if (buf[i] == '"') {
                    // 2つ並んでいる"を1文字消す処理、1文字Removeしてi++すると処理を1文字スキップすることになる
                    buf.Remove(i, 1);
                }
            }
            return new LexicalToken(TokenId.LITERAL, spc, v, new ImmediateNode(v, buf.ToString()));
        }

        private static LexicalToken GetNumericToken(string expr, int indentIndex, ref int index, int length) {
            bool hasDot = false;
            bool hasExp = false;
            int i0 = index;
            while (index < length && char.IsDigit(expr, index)) {
                index++;
            }
            if ((index + 1 < length) && (expr[index] == '.') && char.IsDigit(expr[index + 1])) {
                index += 2;
                while (index < length && char.IsDigit(expr, index)) {
                    index++;
                }
                hasDot = true;
            }
            if ((index < length) && (expr[index] == 'e' || expr[index] == 'E') && char.IsDigit(expr[index + 1])) {
                index += 2;
                while (index < length && char.IsDigit(expr, index)) {
                    index++;
                }
                hasExp = true;
            } else if (index + 2 < length && (expr[index] == 'e' || expr[index] == 'E') && (expr[index + 1] == '+' || expr[index + 1] == '-' ) && char.IsDigit(expr[index + 2])) {
                index += 3;
                while (index < length && char.IsDigit(expr, index)) {
                    index++;
                }
                hasExp = true;
            }

            string spc = expr.Substring(indentIndex, i0 - indentIndex);
            string v = expr.Substring(i0, index - i0);
            object val;
            if (hasDot || hasExp) {
                val = double.Parse(v);
            } else {
                long vL;
                ulong vUL;
                if (long.TryParse(v, out vL)) {
                    if (int.MinValue <= vL && vL <= int.MaxValue) {
                        int vI = (int)vL;
                        val = vI;
                    } else {
                        val = vL;
                    }
                } else if (UInt64.TryParse(v, out vUL)) {
                    val = vUL;
                } else {
                    decimal vD = decimal.Parse(v);
                    val = vD;
                }
            }
            return new LexicalToken(TokenId.NUMERIC, spc, v, new ImmediateNode(v, val));
        }

        private static bool IsHexDigit(char c) {
            return ('0' <= c && c <= '9') || ('a' <= c && c <= 'f') || ('A' <= c && c <= 'F');
        }

        private static Color StrToColor(string value) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException("value");
            }
            if (value[0] != '#' || value.Length != 9) {
                throw new ParseException(value + " is not color");
            }
            try {
                UInt32 v = UInt32.Parse(value.Substring(1), NumberStyles.AllowHexSpecifier);
                UInt32 a = (v >> 24);
                UInt32 r = (v >> 16) & 0xff;
                UInt32 g = (v >> 8) & 0xff;
                UInt32 b = v & 0xff;
                return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
            } catch {
                throw new ParseException(value + " is not color");
            }
        }

        private static LexicalToken GetColorToken(string expr, int indentIndex, ref int index, int length) {
            int i0 = index;
            index++;
            while (index < length && !char.IsSeparator(expr, index)) {
                index++;
            }

            string spc = expr.Substring(indentIndex, i0 - indentIndex);
            string v = expr.Substring(i0, index - i0);
            Color c = StrToColor(v);
            return new LexicalToken(TokenId.LITERAL, spc, v, new ImmediateNode(v, c));
        }

        private static LexicalToken GetIdentifierToken(string expr, int indentIndex, ref int index, int length) {
            int i0 = index;
            while (index < length && (char.IsLetterOrDigit(expr, index) || expr[index] == '_')) {
                index++;
            }
            string indent = expr.Substring(indentIndex, i0 - indentIndex);
            string text = expr.Substring(i0, index - i0);
            TokenId id = TextToTokenId(text);
            return new LexicalToken(id, indent, text, null);
        }

        internal class ParseException: Exception {
            internal ParseException() : base() { }
            internal ParseException(string message) : base(message) { }
        }

        internal static Token[] LexicalAnalyze(string expr) {
            if (string.IsNullOrWhiteSpace(expr)) {
                return null;
            }
            List<Token> tokens = new List<Token>();
            int i = 0;
            int i0 = 0;
            int n = expr.Length;
            while (i < n) {
                i0 = i;
                while (i < n && char.IsWhiteSpace(expr, i)) {
                    i++;
                }
                char c = expr[i];
                i++;
                switch (c) {
                    case '(':
                    case ')':
                    case ',':
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '[':
                    case ']':
                    case '@':
                    //case '`':
                        tokens.Add(new LexicalToken(c, expr.Substring(i0, i - i0 - 1)));
                        break;
                    case '=':
                        if (i < n && expr[i] == '=') {
                            tokens.Add(new LexicalToken(TokenId.EQUAL2, expr.Substring(i0, i - i0 - 1), "==", null));
                            i++;
                        } else {
                            tokens.Add(new LexicalToken(c, expr.Substring(i0, i - i0 - 1)));
                        }
                        break;
                    case '<':
                        if (i < n && expr[i] == '=') {
                            tokens.Add(new LexicalToken(TokenId.LESSEQUAL, expr.Substring(i0, i - i0 - 1), "<=", null));
                            i++;
                        } else if (i < n && expr[i] == '>') {
                            tokens.Add(new LexicalToken(TokenId.NOTEQUAL, expr.Substring(i0, i - i0 - 1), "<>", null));
                            i++;
                        } else {
                            tokens.Add(new LexicalToken(c, expr.Substring(i0, i - i0 - 1)));
                        }
                        break;
                    case '>':
                        if (i < n && expr[i] == '=') {
                            tokens.Add(new LexicalToken(TokenId.GREATEREQUAL, expr.Substring(i0, i - i0 - 1), ">=", null));
                            i++;
                        } else {
                            tokens.Add(new LexicalToken(c, expr.Substring(i0, i - i0 - 1)));
                        }
                        break;
                    case '.':
                        if (i + 1 < n && char.IsDigit(expr[i + 1])) {
                            --i;
                            tokens.Add(GetNumericToken(expr, i0, ref i, n));
                        } else {
                            tokens.Add(new LexicalToken(c, expr.Substring(i0, i - i0 - 1)));
                        }
                        break;
                    case '"':
                        --i;
                        tokens.Add(GetLiteralToken(expr, i0, ref i, n));
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        --i;
                        tokens.Add(GetNumericToken(expr, i0, ref i, n));
                        break;
                    case '#':
                        --i;
                        tokens.Add(GetColorToken(expr, i0, ref i, n));
                        break;
                    default:
                        if (char.IsLetter(c) || c == '_') {
                            --i;
                            LexicalToken t = GetIdentifierToken(expr, i0, ref i, n);
                            tokens.Add(t);
                            break;
                        }
                        throw new ParseException();
                }
            }
            return tokens.ToArray();
        }

        internal delegate Node TokenToNode(TokenTree current);
        internal class ParseRule {
            internal TokenId RuleId;
            internal int Priority;
            internal bool RightToLeft;
            internal TokenId[] Tokens;
            internal TokenToNode Rule;
            internal ParseRule(TokenId ruleId, int priority, bool rightToLeft, TokenId[] ids, TokenToNode rule) {
                RuleId = ruleId;
                Priority = priority;
                RightToLeft = rightToLeft;
                Tokens = ids;
                Rule = rule;
            }

            public override string ToString() {
                StringBuilder buf = new StringBuilder();
                buf.Append(RuleId.ToString());
                buf.Append(':');
                foreach (TokenId id in Tokens) {
                    buf.Append(' ');
                    buf.Append(id.ToString());
                }
                return buf.ToString();
            }
        }

        private static Dictionary<TokenId, Type> _tokenToNodeClass = GetTokenToNodeClass();
        private static Dictionary<TokenId, Type> GetTokenToNodeClass() {
            Dictionary<TokenId, Type> ret = new Dictionary<TokenId, Type>();
            ret.Add(new TokenId('+'), typeof(AddNode));
            ret.Add(new TokenId('-'), typeof(SubtractNode));
            ret.Add(new TokenId('*'), typeof(MulNode));
            ret.Add(new TokenId('/'), typeof(DivNode));
            ret.Add(new TokenId('='), typeof(EqualNode));
            ret.Add(TokenId.NOTEQUAL, typeof(NotEqualNode));
            ret.Add(new TokenId('<'), typeof(LessThanNode));
            ret.Add(TokenId.LESSEQUAL, typeof(LessEqualNode));
            ret.Add(new TokenId('>'), typeof(GreaterThanNode));
            ret.Add(TokenId.GREATEREQUAL, typeof(GreaterEqualNode));
            return ret;
        }

        private static Node GetBinaryOperatorNode(TokenTree current) {
            TokenTree[] tokens = new TokenTree[3];
            TokenTree t = current.Child;
            for (int i = 0; i < tokens.Length; i++) {
                if (t == null) {
                    throw new ParseException("Internal error");
                }
                tokens[i] = t;
                t = t.Next;
            }
            if (tokens[1].Token == null) {
                throw new ParseException("Internal error");
            }
            Type opType;
            if (!_tokenToNodeClass.TryGetValue(tokens[1].TokenId, out opType)) {
                throw new ParseException("Unknown operator: " + tokens[1].Token.Text);
            }
            ConstructorInfo ctor = opType.GetConstructor(new Type[] { typeof(Node), typeof(Node) });
            Node ret = (Node)ctor.Invoke(new object[] { tokens[0].GetNode(), tokens[2].GetNode() });
            return ret;
        }

        private static Node GetUnitedValueNode(TokenTree current) {
            if (current.Child == null || current.Child.Next == null) {
                throw new ParseException("Internal error");
            }
            TokenTree t = current.Child;
            string s = t.Token.Text + t.Next.Token.Indent + t.Next.Token.Text;
            Distance d;
            if (Distance.TryParse(s, out d)) {
                return new ImmediateNode(s, d);
            }
            Angle a = new Angle(0, AngleUnit.Degree);
            if (Angle.TryParse(s, out a)) {
                return new ImmediateNode(s, a);
            }
            throw new ParseException("Unknown unit: " + t.Next.Token.Text);
        }

        private static Node GetConstantNode(TokenTree current) {
            if (current.Child == null) {
                throw new ParseException("Internal error");
            }
            return new PropertyNode(null, current.Child.Token.Text);
            //Node node = null;
            //string s = current.Token.Text;
            //// プロパティアクセス等の実装があればここに挟む
            //if (node == null) {
            //    context._owner
            //    node = Expression.Context.Global.GetProperyNode(context._owner, s);
            //}
            //if (node == null) {
            //    throw new ParseException("undefined identifier: " + s);
            //}
            //return node;
        }

        private static Node GetPropertyNode0(TokenTree current) {
            if (current.Child == null) {
                throw new ParseException("Internal error");
            }
            return new PropertyNode(null, current.Child.Token.Text);
        }
        private static Node GetPropertyNode(TokenTree current) {
            if (current.Child == null || current.Child.Next == null || current.Child.Next.Next == null) {
                throw new ParseException("Internal error");
            }
            return new PropertyNode(current.Child.GetNode(), current.Child.Next.Next.Token.Text);
        }
        private static Node GetIndexedPropertyNode0(TokenTree current) {
            TokenTree[] tokens = new TokenTree[4];
            TokenTree t = current.Child;
            for (int i = 0; i < tokens.Length; i++) {
                if (t == null) {
                    throw new ParseException("Internal error");
                }
                tokens[i] = t;
                t = t.Next;
            }
            if (tokens[0].Token == null) {
                throw new ParseException("Internal error");
            }
            return new PropertyNode(null, tokens[0].Token.Text, tokens[2].GetNode());
        }
        private static Node GetIndexedPropertyNode(TokenTree current) {
            TokenTree[] tokens = new TokenTree[6];
            TokenTree t = current.Child;
            for (int i = 0; i < tokens.Length; i++) {
                if (t == null) {
                    throw new ParseException("Internal error");
                }
                tokens[i] = t;
                t = t.Next;
            }
            if (tokens[2].Token == null) {
                throw new ParseException("Internal error");
            }
            return new PropertyNode(tokens[0].GetNode(), tokens[2].Token.Text, tokens[4].GetNode());
        }
        private static Node GetParenthesisNode(TokenTree current) {
            if (current.Child == null || current.Child.Next == null || current.Child.Next.Next == null) {
                throw new ParseException("Internal error");
            }
            return new ParenthesisNode(current.Child.Next.GetNode());
        }

        private static Node GetMinusNode(TokenTree current) {
            if (current.Child == null || current.Child.Next == null) {
                throw new ParseException("Internal error");
            }
            return new MinusNode(current.Child.Next.GetNode());
        }
        private static Node GetPlusNode(TokenTree current) {
            if (current.Child == null || current.Child.Next == null) {
                throw new ParseException("Internal error");
            }
            return new PlusNode(current.Child.Next.GetNode());
        }

        private static Node RedirectToken0(TokenTree current) {
            if (current.Child == null) {
                throw new ParseException("Internal error");
            }
            return current.Child.GetNode();
        }

        private static Node RedirectToken1(TokenTree current) {
            if (current.Child == null || current.Child.Next == null) {
                throw new ParseException("Internal error");
            }
            return current.Child.Next.GetNode();
        }

        private static Node GetNoArgFuncNode(TokenTree current) {
            if (current.Child == null) {
                throw new ParseException("Internal error");
            }
            if (current.Child.TokenId == TokenId.IDENTIFIER) {
                string s = current.Child.Token.Text;
                return new MethodNode(null, s, null);
            } else {
                throw new ParseException("Not implement yet");
            }
        }

        private static void AddArguments(ArgumentsNode args, TokenTree current) {
            if (current == null) {
                return;
            }
            if (current.TokenId == TokenId.COMMAARGS) {
                AddArguments(args, current.Child);
            } else {
                args.AddArgument(current.GetNode());
            }
            TokenTree t = current.Next;
            t = (t != null) ? t.Next : null;
            if (t != null) {
                //AddArguments(args, t);
                args.AddArgument(t.GetNode());
            }
        }

        private static Node GetArgumentsNode(TokenTree current) {
            ArgumentsNode args = new ArgumentsNode();
            AddArguments(args, current);
            return args;
        }

        private static Node GetFuncNode(TokenTree current) {
            TokenTree[] tokens = new TokenTree[4];
            TokenTree t = current.Child;
            for (int i = 0; i < tokens.Length; i++) {
                if (t == null) {
                    throw new ParseException("Internal error");
                }
                tokens[i] = t;
                t = t.Next;
            }
            ArgumentsNode args = tokens[2].GetNode() as ArgumentsNode;

            if (current.Child.TokenId == TokenId.IDENTIFIER) {
                string s = current.Child.Token.Text;
                return new MethodNode(null, s, args.ToArray());
            } else if (current.Child.TokenId == TokenId.EXPR && current.Child.Child.TokenId == TokenId.IDENTIFIER) {
                string s = current.Child.Child.Token.Text;
                return new MethodNode(null, s, args.ToArray());
            } else {
                throw new ParseException("Not implement yet");
            }
        }
        private static Node GetNoArgMethodNode(TokenTree current) {
            TokenTree[] tokens = new TokenTree[5];
            TokenTree t = current.Child;
            for (int i = 0; i < tokens.Length; i++) {
                if (t == null) {
                    throw new ParseException("Internal error");
                }
                tokens[i] = t;
                t = t.Next;
            }
            if (tokens[2].TokenId == TokenId.IDENTIFIER) {
                string s = tokens[2].Token.Text;
                return new MethodNode(tokens[0].GetNode(), s, null);
            } else {
                throw new ParseException("Not implement yet");
            }
        }
        private static Node GetMethodNode(TokenTree current) {
            TokenTree[] tokens = new TokenTree[6];
            TokenTree t = current.Child;
            for (int i = 0; i < tokens.Length; i++) {
                if (t == null) {
                    throw new ParseException("Internal error");
                }
                tokens[i] = t;
                t = t.Next;
            }
            ArgumentsNode args = tokens[4].GetNode() as ArgumentsNode;
            if (tokens[2].TokenId == TokenId.IDENTIFIER) {
                string s = tokens[2].Token.Text;
                return new MethodNode(tokens[0].GetNode(), s, args.ToArray());
            } else {
                throw new ParseException("Not implement yet");
            }
        }

        private static Node GetUnitNode(TokenTree current) {
            if (current == null) {
                throw new ParseException("Internal error");
            }
            return UnitNode.NewUnitNode(current.Token.Text);
        }

        private static Node GetUnitedDataSourceNode(TokenTree current) {
            TokenTree[] tokens = new TokenTree[4];
            TokenTree t = current.Child;
            for (int i = 0; i < tokens.Length; i++) {
                if (t == null) {
                    throw new ParseException("Internal error");
                }
                tokens[i] = t;
                t = t.Next;
            }
            Node n1 = tokens[1].GetNode();
            Node n3 = tokens[3].GetNode();
            if (!(n1 is PropertyNode)) {
                throw new ArgumentException();
            }
            if (!(n3 is UnitNode)) {
                throw new ArgumentException();
            }
            return new DataSourceNode(n1 as PropertyNode, n3 as UnitNode);
        }

        private static Node GetIfNode(TokenTree current) {
            throw new NotImplementedException();
        }

        private static Node GetAndNode(TokenTree current) {
            throw new NotImplementedException();
        }

        private static Node GetOrNode(TokenTree current) {
            throw new NotImplementedException();
        }

        private static Node GetNotNode(TokenTree current) {
            throw new NotImplementedException();
        }

        private static Node GetCaseNode(TokenTree current) {
            throw new NotImplementedException();
        }

        private static Node GetImmediateEvalNode(TokenTree current) {
            if (current.Child == null || current.Child.Next == null || current.Child.Next.Next == null) {
                throw new ParseException("Internal error");
            }
            return new ImmediateEvalNode(current.Child.Next.GetNode());
        }

        private static Node GetDataSourceNode(TokenTree current) {
            if (current == null || current.Child == null || current.Child.Next == null) {
                throw new ParseException("Internal error");
            }
            Node node = current.Child.Next.GetNode();
            if (!(node is PropertyNode)) {
                throw new ArgumentException();
            }
            return new DataSourceNode(node as PropertyNode, null);
        }

        private static ParseRule[] _parseRules = new ParseRule[] {
            new ParseRule(TokenId.UNITEDVALUE, 0, false, new TokenId[] { TokenId.NUMERIC, TokenId.IDENTIFIER }, GetUnitedValueNode),

            new ParseRule(TokenId.EXPR, 999, false, new TokenId[] { TokenId.NUMERIC }, RedirectToken0),
            new ParseRule(TokenId.EXPR, 999, false, new TokenId[] { TokenId.UNITEDVALUE }, RedirectToken0),
            new ParseRule(TokenId.EXPR, 999, false, new TokenId[] { TokenId.IDENTIFIER }, GetConstantNode),
            new ParseRule(TokenId.EXPR, 999, false, new TokenId[] { TokenId.LITERAL }, RedirectToken0),
            new ParseRule(TokenId.EXPR, 0, false, new TokenId[] { new TokenId('('), TokenId.EXPR, new TokenId(')') }, GetParenthesisNode),
            new ParseRule(TokenId.EXPR, 1, true, new TokenId[] { TokenId.EXPR, new TokenId('+'), TokenId.EXPR }, GetBinaryOperatorNode),
            new ParseRule(TokenId.EXPR, 1, true, new TokenId[] { TokenId.EXPR, new TokenId('-'), TokenId.EXPR }, GetBinaryOperatorNode),
            new ParseRule(TokenId.EXPR, 2, true, new TokenId[] { TokenId.EXPR, new TokenId('*'), TokenId.EXPR }, GetBinaryOperatorNode),
            new ParseRule(TokenId.EXPR, 2, true, new TokenId[] { TokenId.EXPR, new TokenId('/'), TokenId.EXPR }, GetBinaryOperatorNode),
            new ParseRule(TokenId.EXPR, 0, true, new TokenId[] { TokenId.EXPR, new TokenId('='), TokenId.EXPR }, GetBinaryOperatorNode),
            new ParseRule(TokenId.EXPR, 0, true, new TokenId[] { TokenId.EXPR, new TokenId('<'), TokenId.EXPR }, GetBinaryOperatorNode),
            new ParseRule(TokenId.EXPR, 0, true, new TokenId[] { TokenId.EXPR, new TokenId('>'), TokenId.EXPR }, GetBinaryOperatorNode),
            new ParseRule(TokenId.EXPR, 0, true, new TokenId[] { TokenId.EXPR, TokenId.NOTEQUAL, TokenId.EXPR }, GetBinaryOperatorNode),
            new ParseRule(TokenId.EXPR, 0, true, new TokenId[] { TokenId.EXPR, TokenId.GREATEREQUAL, TokenId.EXPR }, GetBinaryOperatorNode),
            new ParseRule(TokenId.EXPR, 0, true, new TokenId[] { TokenId.EXPR, TokenId.LESSEQUAL, TokenId.EXPR }, GetBinaryOperatorNode),
            new ParseRule(TokenId.EXPR, 3, false, new TokenId[] { new TokenId('-'), TokenId.EXPR }, GetMinusNode),
            new ParseRule(TokenId.EXPR, 3, false, new TokenId[] { new TokenId('+'), TokenId.EXPR }, GetPlusNode),
            new ParseRule(TokenId.COMMAARGS, 0, false, new TokenId[] { TokenId.EXPR, new TokenId(','), TokenId.EXPR }, GetArgumentsNode),
            new ParseRule(TokenId.COMMAARGS, 0, false, new TokenId[] { TokenId.COMMAARGS, new TokenId(','), TokenId.EXPR }, GetArgumentsNode),
            //new ParseRule(TokenId.EXPR, 4, false, new TokenId[] { TokenId.PROPERTY}, RedirectToken0),
            new ParseRule(TokenId.EXPR, 4, false, new TokenId[] { TokenId.EXPR, new TokenId('.'), TokenId.IDENTIFIER }, GetPropertyNode),
            new ParseRule(TokenId.EXPR, 4, false, new TokenId[] { TokenId.EXPR, new TokenId('.'), TokenId.IDENTIFIER, new TokenId('('), new TokenId(')') }, GetNoArgMethodNode),
            new ParseRule(TokenId.EXPR, 4, false, new TokenId[] { TokenId.EXPR, new TokenId('('), new TokenId(')') }, GetNoArgFuncNode),
            new ParseRule(TokenId.EXPR, 4, false, new TokenId[] { TokenId.EXPR, new TokenId('.'), TokenId.IDENTIFIER, new TokenId('('), TokenId.EXPR, new TokenId(')') }, GetMethodNode),
            new ParseRule(TokenId.EXPR, 4, false, new TokenId[] { TokenId.EXPR, new TokenId('.'), TokenId.IDENTIFIER, new TokenId('('), TokenId.COMMAARGS, new TokenId(')') }, GetMethodNode),
            new ParseRule(TokenId.EXPR, 4, false, new TokenId[] { TokenId.EXPR, new TokenId('('), TokenId.EXPR, new TokenId(')') }, GetFuncNode),
            new ParseRule(TokenId.EXPR, 4, false, new TokenId[] { TokenId.EXPR, new TokenId('('), TokenId.COMMAARGS, new TokenId(')') }, GetFuncNode),
            new ParseRule(TokenId.EXPR, 0, false, new TokenId[] { TokenId.IF, new TokenId('('), TokenId.COMMAARGS, new TokenId(')')}, GetIfNode),
            new ParseRule(TokenId.EXPR, 0, false, new TokenId[] { TokenId.AND, new TokenId('('), TokenId.COMMAARGS, new TokenId(')')}, GetAndNode),
            new ParseRule(TokenId.EXPR, 0, false, new TokenId[] { TokenId.OR, new TokenId('('), TokenId.COMMAARGS, new TokenId(')')}, GetOrNode),
            new ParseRule(TokenId.EXPR, 0, false, new TokenId[] { TokenId.NOT, new TokenId('('), TokenId.EXPR, new TokenId(')')}, GetNotNode),
            new ParseRule(TokenId.EXPR, 0, false, new TokenId[] { TokenId.CASE, new TokenId('('), TokenId.COMMAARGS, new TokenId(')')}, GetCaseNode),
            //new ParseRule(TokenId.EXPR, 0, false, new TokenId[] { new TokenId('`'), TokenId.EXPR, new TokenId('`')}, GetImmediateEvalNode),
            new ParseRule(TokenId.PROPERTY, 0, false, new TokenId[] { TokenId.IDENTIFIER }, GetPropertyNode0),
            new ParseRule(TokenId.PROPERTY, 0, false, new TokenId[] { TokenId.EXPR, new TokenId('.'), TokenId.IDENTIFIER }, GetPropertyNode),
            new ParseRule(TokenId.PROPERTY, 0, false, new TokenId[] { TokenId.IDENTIFIER, new TokenId('['), TokenId.COMMAARGS, new TokenId(']') }, GetIndexedPropertyNode0),
            new ParseRule(TokenId.PROPERTY, 0, false, new TokenId[] { TokenId.EXPR, new TokenId('.'), TokenId.IDENTIFIER, new TokenId('['), TokenId.COMMAARGS, new TokenId(']') }, GetIndexedPropertyNode),
            new ParseRule(TokenId.UNIT, 0, false, new TokenId[] { TokenId.IDENTIFIER }, GetUnitNode),
            new ParseRule(TokenId.DATASOURCE, 0, false, new TokenId[] { new TokenId('@'), TokenId.PROPERTY, new TokenId(','), TokenId.UNIT}, GetUnitedDataSourceNode),
            new ParseRule(TokenId.DATASOURCE, 0, false, new TokenId[] { new TokenId('@'), TokenId.PROPERTY}, GetDataSourceNode),
            new ParseRule(TokenId.FORMULA, 0, false, new TokenId[] { TokenId.EXPR }, RedirectToken0),
            new ParseRule(TokenId.FORMULA, 0, false, new TokenId[] { TokenId.DATASOURCE }, RedirectToken0),
            //new ParseRule(TokenId.FORMULA, 0, false, new TokenId[] { new TokenId('='), TokenId.EXPR }, GetFormulaNode),
        };

        private static int CompareParseRule(ParseRule value1, ParseRule value2) {
            return value2.Tokens.Length - value1.Tokens.Length;
        }
        private static Dictionary<TokenId, ParseRule[]> _idToRules = GetIdToRules();
        private static Dictionary<TokenId, ParseRule[]> GetIdToRules() {
            Dictionary<TokenId, List<ParseRule>> buf = new Dictionary<TokenId, List<ParseRule>>();
            foreach (ParseRule r in _parseRules) {
                List<ParseRule> l;
                if (buf.ContainsKey(r.Tokens[0])) {
                    l = buf[r.Tokens[0]];
                } else {
                    l = new List<ParseRule>();
                    buf.Add(r.Tokens[0], l);
                }
                l.Add(r);
            }
            foreach (List<ParseRule> l in buf.Values) {
                l.Sort(CompareParseRule);
            }
            Dictionary<TokenId, ParseRule[]> ret = new Dictionary<TokenId, ParseRule[]>();
            foreach (TokenId c in buf.Keys) {
                ret.Add(c, buf[c].ToArray());
            }
            return ret;
        }
        private static bool DoParse(TokenId requiredId, int priority, ref TokenTree current, TokenTree[] tokens, ref int offset) {
            ParseRule[] rules;
            if (!_idToRules.TryGetValue(current.TokenId, out rules)) {
                return false;
            }
            foreach (ParseRule r in rules) {
                if (requiredId == current.TokenId && (r.Priority < priority || (r.Priority == priority && r.RightToLeft))) {
                    continue;
                }

                TokenTree top = new TokenTree(r, current);
                TokenTree cur = current;
                int tempOff = offset + 1;
                int n = r.Tokens.Length - 1;
                int nToken = tokens.Length;
                bool matched = true;
                int i;
                for (i = 1; i <= n; i++) {
                    if (nToken <= tempOff) {
                        matched = false;
                        break;
                    }
                    TokenTree t = tokens[tempOff];
                    t.Reset();
                    if (r.Tokens[i].IsVirtual) {
                        int p = (i == n && r.Tokens[i] == r.RuleId) ? r.Priority : 0;
                        //int tempOff2 = tempOff - 1;
                        if (!DoParse(r.Tokens[i], p, ref t, tokens, ref tempOff)) {
                            matched = false;
                            break;
                        }
                        //tempOff = tempOff2;
                    } else {
                        if (t.TokenId != r.Tokens[i]) {
                            matched = false;
                            break;
                        }
                        tempOff++;
                    }
                    cur.Next = t;
                    cur = t;
                }
                if (matched) {
                    TokenTree temp = top;
                    int tempOff2 = tempOff - 1;
                    //if (DoParse(r.RuleId, priority, ref temp, tokens, ref tempOff2)) {
                    if (DoParse(requiredId, priority, ref temp, tokens, ref tempOff2)) {
                        if ((requiredId == TokenId.ANY_ID) || (requiredId == temp.TokenId)) {
                        //if (((requiredId == TokenId.ANY_ID) && (tempOff2 == tokens.Length)) || (temp.TokenId == requiredId)) {
                            offset = tempOff2;
                            current = temp;
                            return true;
                        }
                    } else {
                        if ((requiredId == TokenId.ANY_ID) || (requiredId == temp.TokenId)) {
                        //if (((requiredId == TokenId.ANY_ID) && (tempOff == tokens.Length)) || (temp.TokenId == requiredId)) {
                            offset = tempOff;
                            current = top;
                            return true;
                        }
                    }
                    //return false;
                }
            }
            return false;
        }

        //private static TokenTree ReduceVirtualTokenTree(TokenTree tree) {
        //    if (tree == null) {
        //        return tree;
        //    }
        //    tree.Next = ReduceVirtualTokenTree(tree.Next);
        //    tree.Child = ReduceVirtualTokenTree(tree.Child);
        //    if (tree.Next == null && tree.Child != null && tree.Token == null) {
        //        TokenTree ret = tree.Child;
        //        tree.Reset();
        //        return ret;
        //    }
        //    return tree;
        //    //if (tree.Child == null || tree.Child.Child == null) {
        //    //    return;
        //    //}
        //    //if (tree.Child.Token != null) {
        //    //    return;
        //    //}
        //    //if (tree.TokenId == tree.Child.TokenId && tree.Child.Child.Next == null) {
        //    //    TokenTree purge = tree.Child;
        //    //    tree.Child.Child.Next = tree.Child.Next;
        //    //    tree.Child = tree.Child.Child;
        //    //    tree.Child.Next = purge.Next;
        //    //    purge.Reset();
        //    //    purge.Token = null;
        //    //    return;
        //    //}
        //    //return;
        //}

        internal static TokenTree Parse(Token[] tokens) {
            if (tokens == null) {
                return null;
            }
            if (tokens.Length == 0) {
                return null;
            }
            TokenTree[] ret = TokenTree.CreateTokenTree(tokens);
            TokenTree cur = ret[0];
            int i = 0;
            if (!DoParse(TokenId.FORMULA, 0, ref cur, ret, ref i)) {
                throw new ParseException(string.Format("Parse error before \"{0}", tokens[i].Text));
            }
            if (i < ret.Length) {
                throw new ParseException(string.Format("Parse error before \"{0}", tokens[i].Text));
            }
            //return ReduceVirtualTokenTree(cur);
            return cur;
        }
    }
}
