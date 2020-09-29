using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sketch {
    partial class Expression {
        /// <summary>
        /// 数式を字句解析した際の最小単位を保持する
        /// </summary>
        public abstract class Node {
            private Type _returnType;
            protected EvalSpec _style;
            //private Node[] _subNodes;
            protected abstract Node[] GetSubNodes();
            protected abstract void ReplaceSubNodeAt(int index, Node newNode);
            protected void SetStyle(EvalSpec style) {
                _style = style;
            }

            protected void SetReturnType(Type value) {
                _returnType = value;
            }

            public EvalSpec Style { get { return _style; } }
            public Type ReturnType { get { return _returnType; } }
            public abstract object Eval(Expression.Context context, AddDependencyCallback addDependency);
            public bool IsConstant() {
                switch (_style) {
                    case EvalSpec.Constant:
                        return true;
                    case EvalSpec.FunctionalDependent:
                        Node[] nodes = GetSubNodes();
                        if (nodes != null) {
                            foreach (Node node in nodes) {
                                if (node != null) {
                                    if (!node.IsConstant()) {
                                        return false;
                                    }
                                }
                            }
                        }
                        return true;
                    case EvalSpec.PropertyDependent:
                    case EvalSpec.Variable:
                        return false;
                    default:
                        // EvalSpecに定義が追加されたが対応する処理を追加し忘れていたらここを通る
                        throw new NotImplementedException();
                }
            }
            public bool CanReduce() {
                if (_style == EvalSpec.FunctionalDependent) {
                    Node[] nodes = GetSubNodes();
                    if (nodes != null) {
                        foreach (Node node in nodes) {
                            if (node != null) {
                                if (!node.IsConstant()) {
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }
                return false;
            }
            protected Node DoReduce(Expression.Context context) {
                if (CanReduce()) {
                    return new ImmediateNode(this, context, null);
                }
                Node[] nodes = GetSubNodes();
                if (nodes != null) {
                    for (int i = 0; i < nodes.Length; i++) {
                        Node node = nodes[i];
                        if (node != null) {
                            node = node.DoReduce(context);
                        }
                        if (node != null) {
                            ReplaceSubNodeAt(i, node);
                        }
                    }
                }
                return null;
            }

            public Node Reduce(Expression.Context context) {
                Node node = DoReduce(context);
                return node != null ? node : this;
            }

            public virtual string ToDebugString(Expression.Context context) {
                return string.Format("[{0}: \"{1}\"]", Eval(context, null), ToString());
            }

            public Node() { }

        }

        public class ParenthesisNode: Node {
            private Node _node;
            internal ParenthesisNode(Node node) {
                SetStyle(EvalSpec.FunctionalDependent);
                _node = node;
            }
            protected override Node[] GetSubNodes() {
                return new Node[] { _node };
            }
            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                if (index != 0) {
                    throw new ArgumentOutOfRangeException();
                }
                _node = newNode;
            }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                return (_node != null) ? _node.Eval(context, addDependency) : null;
            }
            public override string ToString() {
                if (_node != null) {
                    string s = _node.ToString();
                    StringBuilder buf = new StringBuilder(s.Length + 2);
                    buf.Append('(');
                    buf.Append(s);
                    buf.Append(')');
                    return buf.ToString();
                }
                return "()";
            }
        }

        public abstract class BinaryOperatorNode: Node {
            internal Node _left;
            internal Node _right;
            protected abstract string OperatorText();
            public BinaryOperatorNode(Node left, Node right) {
                if (left == null) {
                    throw new ArgumentNullException();
                }
                if (right == null) {
                    throw new ArgumentNullException();
                }
                SetStyle(EvalSpec.FunctionalDependent);
                _left = left;
                _right = right;
            }
            protected override Node[] GetSubNodes() {
                return new Node[] { _left, _right };
            }
            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                switch (index) {
                    case 0:
                        _left = newNode;
                        break;
                    case 1:
                        _right = newNode;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            public override string ToString() {
                return _left.ToString() + OperatorText() + _right.ToString();
            }
        }
        public class AddNode: BinaryOperatorNode {
            public AddNode(Node left, Node right) : base(left, right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                dynamic lval = _left.Eval(context, addDependency);
                dynamic rval = _right.Eval(context, addDependency);
                return lval + rval;
            }
            protected override string OperatorText() {
                return "+";
            }
        }
        public class SubtractNode: BinaryOperatorNode {
            public SubtractNode(Node left, Node right) : base(left, right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                dynamic lval = _left.Eval(context, addDependency);
                dynamic rval = _right.Eval(context, addDependency);
                return lval - rval;
            }
            protected override string OperatorText() {
                return "-";
            }
        }
        public class MulNode: BinaryOperatorNode {
            public MulNode(Node left, Node right) : base(left, right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                dynamic lval = _left.Eval(context, addDependency);
                dynamic rval = _right.Eval(context, addDependency);
                return lval * rval;
            }
            protected override string OperatorText() {
                return "*";
            }
        }
        private static Dictionary<Type, bool> _intTypes = GetIntTypes();
        private static Dictionary<Type, bool> GetIntTypes() {
            Dictionary<Type, bool> ret = new Dictionary<Type, bool>();
            ret.Add(typeof(sbyte), true);
            ret.Add(typeof(byte), true);
            ret.Add(typeof(Int16), true);
            ret.Add(typeof(UInt16), true);
            ret.Add(typeof(Int32), true);
            ret.Add(typeof(UInt32), true);
            ret.Add(typeof(Int64), true);
            ret.Add(typeof(UInt64), true);
            return ret;
        }
        public class DivNode: BinaryOperatorNode {
            public DivNode(Node left, Node right) : base(left, right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                object lval = _left.Eval(context, addDependency);
                object rval = _right.Eval(context, addDependency);
                if (lval.GetType().IsPrimitive && rval.GetType().IsPrimitive) {
                    return Convert.ToDouble(lval) / Convert.ToDouble(rval);
                } else {
                    dynamic lval2 = _left.Eval(context, addDependency);
                    dynamic rval2 = _right.Eval(context, addDependency);
                    return lval2 / rval2;
                }
            }
            protected override string OperatorText() {
                return "/";
            }
        }

        public class EqualNode: BinaryOperatorNode {
            public EqualNode(Node left, Node right) : base(left, right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                object lval = _left.Eval(context, addDependency);
                object rval = _right.Eval(context, addDependency);
                if (lval == null) {
                    return rval == null;
                }
                return lval.Equals(_right.Eval(context, addDependency));
            }
            protected override string OperatorText() {
                return "=";
            }
        }

        public class NotEqualNode: BinaryOperatorNode {
            public NotEqualNode(Node left, Node right) : base(left, right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                object lval = _left.Eval(context, addDependency);
                object rval = _right.Eval(context, addDependency);
                if (lval == null) {
                    return rval != null;
                }
                return !lval.Equals(_right.Eval(context, addDependency));
            }
            protected override string OperatorText() {
                return "<>";
            }
        }

        public class LessThanNode: BinaryOperatorNode {
            public LessThanNode(Node left, Node right) : base(left, right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                dynamic lval = _left.Eval(context, addDependency);
                dynamic rval = _right.Eval(context, addDependency);
                return lval < rval;
            }
            protected override string OperatorText() {
                return "<";
            }
        }

        public class LessEqualNode: BinaryOperatorNode {
            public LessEqualNode(Node left, Node right) : base(left, right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                dynamic lval = _left.Eval(context, addDependency);
                dynamic rval = _right.Eval(context, addDependency);
                return lval <= rval;
            }
            protected override string OperatorText() {
                return "<=";
            }
        }

        public class GreaterThanNode: BinaryOperatorNode {
            public GreaterThanNode(Node left, Node right) : base(left, right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                dynamic lval = _left.Eval(context, addDependency);
                dynamic rval = _right.Eval(context, addDependency);
                return lval > rval;
            }
            protected override string OperatorText() {
                return ">";
            }
        }

        public class GreaterEqualNode: BinaryOperatorNode {
            public GreaterEqualNode(Node left, Node right) : base(left, right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                dynamic lval = _left.Eval(context, addDependency);
                dynamic rval = _right.Eval(context, addDependency);
                return lval >= rval;
            }
            protected override string OperatorText() {
                return ">=";
            }
        }

        public abstract class UnaryOperatorNode: Node {
            internal Node _right;
            public UnaryOperatorNode(Node right) {
                if (right == null) {
                    throw new ArgumentNullException();
                }
                _right = right;
            }
            
            protected override Node[] GetSubNodes() {
                return new Node[] { _right };
            }

            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                if (index != 0) {
                    throw new ArgumentOutOfRangeException();
                }
                _right = newNode;
            }

            protected abstract string OperatorText();
            public override string ToString() {
                return OperatorText() + _right.ToString();
            }
        }
        public class PlusNode: UnaryOperatorNode {
            public PlusNode(Node right) : base(right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                return _right.Eval(context, addDependency);
            }
            protected override string OperatorText() {
                return "+";
            }
        }
        public class MinusNode: UnaryOperatorNode {
            public MinusNode(Node right) : base(right) { }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                dynamic rval = _right.Eval(context, addDependency);
                return -rval;
            }
            protected override string OperatorText() {
                return "-";
            }
        }

        public class ImmediateNode: Node {
            private string _formula;
            private object _value;
            public ImmediateNode(Node node, Expression.Context context, AddDependencyCallback addDependency) {
                SetStyle(EvalSpec.Variable);
                _formula = node.ToString();
                _value = node.Eval(context, addDependency);
            }
            internal ImmediateNode(string formula, object value) {
                SetStyle(EvalSpec.Constant);
                _formula = formula;
                _value = value;
            }

            protected override Node[] GetSubNodes() {
                return null;
            }

            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                throw new ArgumentOutOfRangeException();
            }
            
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                return _value;
            }
            public override string ToString() {
                return _formula;
            }
        }

        public class ImmediateEvalNode : Node {
            private bool _evaluated = false;
            private Node _node;
            internal ImmediateEvalNode(Node node) {
                SetStyle(EvalSpec.FunctionalDependent);
                _node = node;
            }
            protected override Node[] GetSubNodes() {
                return new Node[] { _node };
            }
            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                if (index != 0) {
                    throw new ArgumentOutOfRangeException();
                }
                _node = newNode;
            }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                if (_node == null) {
                    return null;
                }
                if (!_evaluated) {
                    _node = new ImmediateNode(_node, context, null);
                    _evaluated = true;
                }
                return _node.Eval(context, null);
            }
            public override string ToString() {
                if (_node != null) {
                    string s = _node.ToString();
                    if (_evaluated) {
                        return s;
                    }
                    StringBuilder buf = new StringBuilder(s.Length + 2);
                    buf.Append('`');
                    buf.Append(s);
                    buf.Append('`');
                    return buf.ToString();
                }
                return "``";
            }
        }

        public class PropertyNode: Node {
            private Node _target;
            private string _propertyName;
            private Node[] _indexNodes;


            public PropertyNode(Node target, string propertyName)
                : base() {
                SetStyle(EvalSpec.PropertyDependent);
                _target = target;
                _propertyName = propertyName;
            }

            public PropertyNode(Node target, string propertyName, Node arguments)
                : base() {
                //SetStyle(EvalSpec.PropertyDependent);
                SetStyle(EvalSpec.Variable);
                _target = target;
                _propertyName = propertyName;
                ArgumentsNode args = arguments as ArgumentsNode;
                if (args != null) {
                    _indexNodes = args.ToArray();
                }
            }

            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                object target = null;
                if (_target != null) {
                    target = _target.Eval(context, addDependency);
                }
                return context.EvalProperty(target, _propertyName, _indexNodes, out _style, addDependency);
            }

            public void SetValue(object value, Expression.Context context, AddDependencyCallback addDependency) {
                object target = null;
                if (_target != null) {
                    target = _target.Eval(context, addDependency);
                }
                context.SetProperyValue(target, _propertyName, value);
            }

            protected override Node[] GetSubNodes() {
                return new Node[] { _target };
                //return (_target != null) ? new Node[] { _target } : null;
            }
            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                if (index == 0) {
                    if (newNode is PropertyNode) {
                        throw new ArgumentException();
                    }
                    _target = newNode as PropertyNode;
                    return;
                }
                throw new ArgumentOutOfRangeException();
            }

            public override string ToString() {
                return ((_target != null) ? _target.ToString() + "."  : string.Empty) + _propertyName;
            }
        }

        public class MethodNode: Node {
            private Node _target;
            private string _methodName;
            private Node[] _arguments;
            private Node[] _subNodes;

            public MethodNode(Node target, string method, Node[] arguments)
                : base() {
                //if (target == null) {
                //    throw new ArgumentNullException("target");
                //}
                if (string.IsNullOrEmpty(method)) {
                    throw new ArgumentNullException("method");
                }
                SetStyle(EvalSpec.Variable);
                _target = target;
                _methodName = method;
                _arguments = arguments;
            }

            protected override Node[] GetSubNodes() {
                if (_subNodes == null) {
                    List<Node> l = new List<Node>(_arguments.Length + 1);
                    l.Add(_target);
                    l.AddRange(_arguments);
                    _subNodes = l.ToArray();
                }
                return _subNodes;
            }

            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                if (index < 0 || _subNodes.Length <= index) {
                    throw new ArgumentOutOfRangeException();
                }
                if (index == 0) {
                    _target = newNode;
                    _subNodes = null;
                } else {
                    _arguments[index - 1] = newNode;
                    _subNodes = null;
                }
            }

            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                object target = null;
                if (_target != null) {
                    target = _target.Eval(context, addDependency);
                    if (target == null) {
                        throw new NullReferenceException();
                    }
                }
                return context.EvalMethod(target, _methodName, _arguments, out _style, addDependency);
            }
            public override string ToString() {
                StringBuilder buf = new StringBuilder();
                if (_target != null) {
                    buf.Append(_target.ToString());
                    buf.Append('.');
                }
                buf.Append(_methodName);
                buf.Append('(');
                if (_arguments != null && 0 < _arguments.Length) {
                    buf.Append(_arguments[0]);
                    for (int i = 1; i < _arguments.Length; i++) {
                        buf.Append(", ");
                        buf.Append(_arguments[i]);
                    }
                }
                buf.Append(')');
                return buf.ToString();
            }
        }

        public class ArgumentsNode: Node {
            private List<Node> _args = new List<Node>();
            public ArgumentsNode() : base() { }
            internal void AddArgument(Node node) {
                _args.Add(node);
            }
            protected override Node[] GetSubNodes() {
                return ToArray();
            }
            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                _args[index] = newNode;
            }
            public override object Eval(Expression.Context context, AddDependencyCallback addDependency) {
                // ArgumentsNode.Evalが呼ばれるケースは存在しない
                throw new NotImplementedException();
            }
            internal Node[] ToArray() {
                return _args.ToArray();
            }
        }

        public abstract class UnitNode: Node {
            public static UnitNode NewUnitNode(string text) {
                if (string.IsNullOrEmpty(text)) {
                    throw new ArgumentNullException();
                }
                LengthUnit lu;
                AngleUnit au;
                if (Distance.TryParseUnit(text, out lu)) {
                    return new LengthUnitNode(lu);
                } else if (Angle.TryParseUnit(text, out au)) {
                    return new AngleUnitNode(au);
                }
                throw new FormatException();
            }
        }

        public class LengthUnitNode: UnitNode {
            private LengthUnit _unit;

            public LengthUnitNode(LengthUnit unit) {
                _unit = unit;
            }

            public LengthUnit Unit { get { return _unit; } }

            public Distance ToDistance(object value) {
                return UnitConvert.ToDistance(value, _unit);
            }

            public double ToValue(Distance value) {
                return value[_unit];
            }

            protected override Node[] GetSubNodes() {
                return null;
            }

            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                return;
            }

            public override object Eval(Context context, AddDependencyCallback addDependency) {
                return _unit;
            }

            public override string ToString() {
                return _unit.ToString();
            }
        }

        public class AngleUnitNode: UnitNode {
            private AngleUnit _unit;

            public AngleUnitNode(AngleUnit unit) {
                _unit = unit;
            }

            public AngleUnit Unit { get { return _unit; } }

            public Angle ToAngle(object value) {
                return UnitConvert.ToAngle(value, _unit);
            }

            public double ToValue(Angle value) {
                return value[_unit];
            }

            protected override Node[] GetSubNodes() {
                return null;
            }

            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                return;
            }

            public override object Eval(Context context, AddDependencyCallback addDependency) {
                return _unit;
            }

            public override string ToString() {
                return _unit.ToString();
            }
        }

        public class DataSourceNode: Node {
            private PropertyNode _property;
            private UnitNode _unit;

            public DataSourceNode(PropertyNode property, UnitNode unit) {
                _property = property;
                _unit = unit;
                SetStyle(EvalSpec.Variable);
            }

            protected override Node[] GetSubNodes() {
                if (_unit == null) {
                    return new Node[1] { _property };
                } else {
                    return new Node[2] { _property, _unit };
                }
            }

            protected override void ReplaceSubNodeAt(int index, Node newNode) {
                switch (index) {
                    case 0:
                        if (newNode == null) {
                            throw new ArgumentNullException();
                        }
                        if (!(newNode is PropertyNode)) {
                            throw new ArgumentException();
                        }
                        _property = newNode as PropertyNode;
                        break;
                    case 1:
                        if (newNode == null) {
                            throw new ArgumentNullException();
                        }
                        if (!(newNode is UnitNode)) {
                            throw new ArgumentException();
                        }
                        _unit = newNode as UnitNode;
                        break;
                }
            }

            public override object Eval(Context context, AddDependencyCallback addDependency) {
                //double val = double.NaN;
                object v = null;
                if (_property != null) {
                    v = _property.Eval(context, addDependency);
                    if (v == null) {
                        return null;
                    }
                }
                if (_unit == null) {
                    return v;
                }
                double val = double.NaN;
                if (v != null) {
                    val = Convert.ToDouble(v);
                }
                string us = _unit.ToString();
                if (string.IsNullOrEmpty(us)) {
                    throw new ArgumentNullException();
                }
                LengthUnit lu;
                AngleUnit au;
                if (Distance.TryParseUnit(_unit.ToString(), out lu)) {
                    return new Distance(val, lu);
                } else if (Angle.TryParseUnit(_unit.ToString(), out au)) {
                    return new Angle(val, au);
                }
                throw new FormatException();
            }

            public void SetValue(object value, Context context) {
                _property.SetValue(value, context, null);
            }

            public override string ToString() {
                if (_unit == null) {
                    return string.Format("@{0}", _property);
                } else {
                    return string.Format("@{0},{1}" + _property, _unit);
                }
            }
        }

        public class EmbedNodeTemplate {
            internal static Dictionary<string, TemplateGroup> _nameToTemplate;
            internal static Dictionary<string, Node> _nameToConstNode;
            private static void AddNoteTemplate(EmbedNodeTemplate template) {
                TemplateGroup gr;
                if (!_nameToTemplate.ContainsKey(template.Name)) {
                    gr = new TemplateGroup(template.Name);
                    _nameToTemplate.Add(gr.Name, gr);
                } else {
                    gr = _nameToTemplate[template.Name];
                }
                gr.Add(template);
            }
            private static void AddMethod(Type owner, bool functionalDependent, string[] names) {
                foreach (MethodInfo info in owner.GetMethods(BindingFlags.Static)) {
                    foreach (string s in names) {
                        if (info.Name == s) {
                            new EmbedNodeTemplate(s, functionalDependent, info);
                        }
                    }
                }
            }
            internal static void InitNameToTemplate() {
                _nameToTemplate = new Dictionary<string, TemplateGroup>();
                //AddMethod(typeof(Math), true, new string[] {
                //    "Abs", "Ceiling",
                //    "Exp", "Floor", "Max", "Min", "Pow", "Round", "Sign",
                //    "Sqrt", "Truncate"
                //});
                //new EmbedNodeTemplate("Log", true, typeof(Math).GetMethod("Log", BindingFlags.Static, null, new Type[] { typeof(double), typeof(double) }, null));
                //new EmbedNodeTemplate("Ln", true, typeof(Math).GetMethod("Log", BindingFlags.Static, null, new Type[] { typeof(double) }, null));

                AddMethod(typeof(Angle), true, new string[] {
                    "Acos", "Asin", "Atan", "Atan2", "Cos", "Cosh", "Sin", "Sinh", "Tan", "Tanh",
                    "Abs", "Ceiling", "Floor", "Max", "Min", "Round", "Sign", "Truncate"
                });
                AddMethod(typeof(Distance), true, new string[] {
                    "Abs", "Ceiling", "Floor", "Max", "Min", "Round", "Sign", "Truncate"
                });
                AddMethod(typeof(EmbedFunc), false, new string[] {
                    "Abs", "Ceiling",
                    "Exp", "Floor", "Max", "Min", "Pow", "Round", "Sign",
                    "Sqrt", "Truncate", "Log", "Ln",
                     "Now", "UtcNow", "Today" });
            }

            private static void AddConstNode(string name, object value) {
                ImmediateNode node = new ImmediateNode(name, value);
                _nameToConstNode.Add(name, node);
            }
            internal static void InitNameToConstNode() {
                _nameToConstNode = new Dictionary<string, Node>();
                AddConstNode("E", Math.E);
                AddConstNode("PI", Math.PI);
            }
            //internal static Node CreateEmbedFuncNode(string name, Node[] args) {
            //    EmbedNodeTemplate.TemplateGroup gr = EmbedNodeTemplate._nameToTemplate[name];
            //    if (gr == null) {
            //        return null;
            //    }
            //    List<Type> l = new List<Type>();
            //    foreach (Node a in args) {
            //        l.Add(a.ReturnType);
            //    }
            //    Type[] types = l.ToArray();
            //    EmbedNodeTemplate tmpl = gr.FindStrictly(types);
            //    if (tmpl != null) {
            //        return new EmbedFuncNode(tmpl, args);
            //    }
            //    EmbedNodeTemplate[] tmpl2 = gr.FindAppliable(types);
            //    if (tmpl2 != null && 1 < tmpl2.Length) {
            //        return new VagueFuncNode(tmpl2, args);
            //    }
            //    return null;
            //}

            internal static Node GetConstNode(string name) {
                Node ret;
                if (_nameToConstNode.TryGetValue(name, out ret)) {
                    return ret;
                }
                return null;
            }

            internal class TemplateGroup {
                private string _name;
                private List<EmbedNodeTemplate> _candidates = new List<EmbedNodeTemplate>();
                internal TemplateGroup(string name) {
                    _name = name;
                }
                internal string Name { get { return _name; } }
                internal void Add(EmbedNodeTemplate value) {
                    _candidates.Add(value);
                }

                /// <summary>
                /// 引数の数と型が一致するメソッドを返す
                /// </summary>
                /// <param name="argumentTypes"></param>
                /// <returns></returns>
                internal EmbedNodeTemplate FindStrictly(Type[] argumentTypes) {
                    int nArg = argumentTypes != null ? argumentTypes.Length : 0;
                    foreach (EmbedNodeTemplate t in _candidates) {
                        if (nArg == t._argumentTypes.Length) {
                            bool matched = true;
                            for (int i = 0; i < argumentTypes.Length; i++) {
                                if (!argumentTypes[i].IsSubclassOf(t._argumentTypes[i])) {
                                    matched = false;
                                    break;
                                }
                                if (matched) {
                                    return t;
                                }
                            }
                        }
                    }
                    return null;
                }
                /// <summary>
                /// 曖昧な引数型の指定に対して、一致する可能性のある候補を返す
                /// </summary>
                /// <param name="argumentTypes"></param>
                /// <returns></returns>
                internal EmbedNodeTemplate[] FindAppliable(Type[] argumentTypes) {
                    List<EmbedNodeTemplate> l = new List<EmbedNodeTemplate>();
                    int nArg = argumentTypes != null ? argumentTypes.Length : 0;
                    foreach (EmbedNodeTemplate t in _candidates) {
                        if (nArg == t._argumentTypes.Length) {
                            bool matched = true;
                            for (int i = 0; i < argumentTypes.Length; i++) {
                                if (!t._argumentTypes[i].IsSubclassOf(argumentTypes[i])) {
                                    matched = false;
                                    break;
                                }
                                if (matched) {
                                    l.Add(t);
                                }
                            }
                        }
                    }
                    return (l.Count != 0) ? l.ToArray() : null;
                }
            }
            private EvalSpec _style;
            private string _name;
            private MethodInfo _method;
            private Type[] _argumentTypes;

            internal string Name { get { return _name; } }
            internal MethodInfo Method { get { return _method; } }
            internal Type[] ArgumentTypes { get { return _argumentTypes; } }

            internal EmbedNodeTemplate(string name, bool functionalDependent, MethodInfo method) {
                _style = functionalDependent ? EvalSpec.FunctionalDependent : EvalSpec.Variable;
                _name = name;
                _method = method;
                ParameterInfo[] prms = _method.GetParameters();
                _argumentTypes = new Type[prms.Length];
                for (int i = 0; i < prms.Length; i++) {
                    _argumentTypes[i] = prms[i].ParameterType;
                }
                AddNoteTemplate(this);
            }
        }
    }
}
