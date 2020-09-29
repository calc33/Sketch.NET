using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Sketch {
    public enum DependencyOperation {
        Add,
        Remove,
    };

    public class DependencyAddedEevntArgs: EventArgs {
        private object _target;
        private MemberInfo _member;
        public DependencyAddedEevntArgs(object target, MemberInfo member) {
            _target = target;
            _member = member;
        }
        public object Target { get { return _target; } }
        public MemberInfo Member { get { return _member; } }
    }

    partial class Expression {
        private static bool HasFormulaIndexAttribute(MemberInfo member) {
            foreach (FormulaIndexAttribute a in member.GetCustomAttributes(typeof(FormulaIndexAttribute), true)) {
                return true;
            }
            return false;
        }

        public static EvalSpec GetEvalSpecFromAttribute(MemberInfo member) {
            foreach (EvalSpecAttribute a in member.GetCustomAttributes(typeof(EvalSpecAttribute), false)) {
                return a.Spec;
            }
            foreach (EvalSpecAttribute a in member.GetCustomAttributes(typeof(EvalSpecAttribute), true)) {
                return a.Spec;
            }
            if (member is PropertyInfo) {
                if (HasFormulaIndexAttribute(member)) {
                    return EvalSpec.PropertyDependent;
                } else {
                    return EvalSpec.Variable;
                }
            } else if (member is MethodInfo) {
                return EvalSpec.FunctionalDependent;
            }
            return EvalSpec.Variable;
        }

        public Context GetCurrentContext() {
            IExpressionOwnable o = Owner;
            return new Context((o != null) ? o.Owner : null);
        }

        /// <summary>
        /// 数式のコンテキストを保持する
        /// コンテキストとは数式内のメソッド、プロパティを保持する参照先の一覧であり
        ///  1. 明示されたターゲット
        ///  2. Ownerのメソッド、プロパティ
        ///  3. 組み込みメソッド、定数
        /// の順番に参照する
        /// </summary>
        public class Context {
            private static Type[] _embedMethods = new Type[] {
                    typeof(EmbedFunc),
                    typeof(Distance),
                    typeof(Angle),
                    typeof(Color),
                    typeof(SketchSettings),
            };
            internal struct ConstInfo {
                internal string Name;
                internal object Value;
                internal ConstInfo(string name, object value) {
                    Name = name;
                    Value = value;
                }
            }
            private static Dictionary<string, ConstInfo> _constants = new Dictionary<string, ConstInfo>();

            private static void RegisterConstInfo(ConstInfo value) {
                _constants.Add(value.Name.ToUpper(), value);
            }
            public static void RegisterConstant(object value) {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                RegisterConstInfo(new ConstInfo(value.ToString(), value));
            }

            public static void RegisterConstant(string name, object value) {
                if (string.IsNullOrEmpty(name)) {
                    throw new ArgumentNullException("name");
                }
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                RegisterConstInfo(new ConstInfo(name, value));
            }

            public static void RegisterEnum(Type enumType) {
                if (enumType == null) {
                    throw new ArgumentNullException("enumType");
                }
                if (!enumType.IsEnum) {
                    throw new ArgumentException("enumTypeが列挙型ではありません");
                }
                foreach (object v in Enum.GetValues(enumType)) {
                    RegisterConstInfo(new ConstInfo(v.ToString(), v));
                }
            }

            private static void RegisterColor() {
                foreach (PropertyInfo p in typeof(Colors).GetProperties()) {
                    if (p.PropertyType == typeof(Color)) {
                        RegisterConstInfo(new ConstInfo(p.Name, p.GetValue(null, null)));
                    }
                }
            }

            private static bool TryGetConstant(string name, out object value) {
                ConstInfo ret;
                if (_constants.TryGetValue(name.ToUpper(), out ret)) {
                    value = ret.Value;
                    return true;
                }
                value = null;
                return false;
            }

            static Context() {
                RegisterConstant(false);
                RegisterConstant(true);
                RegisterEnum(typeof(LineCap));
                RegisterEnum(typeof(LineJoin));
                RegisterEnum(typeof(FillRule));
                RegisterEnum(typeof(ArrowStyle));
                RegisterEnum(typeof(LockLevel));
                RegisterColor();
            }

            private object _owner;

            internal Context(object owner) {
                _owner = owner;
            }

            private Context() {
                _owner = null;
            }

            public object Owner { get { return _owner; } }

            public object EvalMethod(object target, string methodName, Node[] arguments, out EvalSpec style, AddDependencyCallback addDependency) {
                object[] args = EvalNodes(arguments, true, addDependency);
                Type[] argTypes = GetTypes(args, false);
                if (target != null) {
                    MethodInfo m = target.GetType().GetMethod(methodName, argTypes);
                    if (m != null) {
                        style = GetEvalSpecFromAttribute(m);
                        return m.Invoke(target, args);
                    }
                } else {
                    // targetが省略された場合はOwnerのメソッド呼出しの場合と
                    // 組込関数の呼出しの場合がある
                    if (Owner != null) {
                        MethodInfo m = Owner.GetType().GetMethod(methodName, argTypes);
                        if (m != null) {
                            style = GetEvalSpecFromAttribute(m);
                            return m.Invoke(Owner, args);
                        }
                    }
                    foreach (Type t in _embedMethods) {
                        MethodInfo m = t.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, argTypes, null);
                        if (m != null) {
                            style = GetEvalSpecFromAttribute(m);
                            return m.Invoke(null, args);
                        }
                    }
                }
                throw new InvalidOperationException();
            }

            public object[] EvalNodes(Node[] nodes, bool returnNullOnEmpty, AddDependencyCallback addDependency) {
                if (nodes == null || nodes.Length == 0) {
                    return returnNullOnEmpty ? null : new Type[0];
                }
                object[] ret = new object[nodes.Length];
                for (int i = 0; i < nodes.Length; i++) {
                    ret[i] = nodes[i].Eval(this, addDependency);
                }
                return ret;
            }

            public static Type[] GetTypes(object[] objects, bool returnNullOnEmpty) {
                if (objects == null || objects.Length == 0) {
                    return returnNullOnEmpty ? null : new Type[0];
                }
                Type[] ret = new Type[objects.Length];
                for (int i = 0; i < objects.Length; i++) {
                    ret[i] = objects[i].GetType();
                }
                return ret;
            }

            /// <summary>
            /// プロパティ値を取得する
            /// </summary>
            /// <param name="target">プロパティの取得対象。nullの場合はOwnerがプロパティの取得対象</param>
            /// <param name="propertyName">取得したいプロパティの名前</param>
            /// <returns></returns>
            public object EvalProperty(object target, string propertyName, Node[] indexNodes, out EvalSpec style, AddDependencyCallback addDependency) {
                object[] prms = EvalNodes(indexNodes, true, addDependency);
                Type[] prmTypes = GetTypes(prms, false);
                if (target != null) {
                    PropertyInfo p = target.GetType().GetProperty(propertyName, prmTypes);
                    if (p != null) {
                        style = GetEvalSpecFromAttribute(p);
                        MethodInfo m = p.GetGetMethod();
                        if (m != null) {
                            if (addDependency != null) {
                                addDependency(new DependencyAddedEevntArgs(target, p));
                            }
                            return m.Invoke(target, prms);
                        }
                    }
                } else {
                    // targetが省略された場合はOwnerのプロパティ取得の場合と
                    // 定数取得の場合がある
                    if (Owner != null) {
                        PropertyInfo p = Owner.GetType().GetProperty(propertyName);
                        if (p != null) {
                            style = GetEvalSpecFromAttribute(p);
                            MethodInfo m = p.GetGetMethod();
                            if (m != null) {
                                if (addDependency != null) {
                                    addDependency(new DependencyAddedEevntArgs(Owner, p));
                                }
                                return m.Invoke(Owner, prms);
                            }
                        }
                    }
                    foreach (Type t in _embedMethods) {
                        PropertyInfo p = t.GetProperty(propertyName);
                        if (p != null) {
                            style = GetEvalSpecFromAttribute(p);
                            MethodInfo m = p.GetGetMethod();
                            if (m != null) {
                                if (addDependency != null) {
                                    addDependency(new DependencyAddedEevntArgs(null, p));
                                }
                                return m.Invoke(null, prms);
                            }
                        }
                    }
                    if (prms == null) {
                        object ret;
                        if (TryGetConstant(propertyName, out ret)) {
                            style = EvalSpec.Constant;
                            return ret;
                        }
                    }
                }
                throw new InvalidOperationException();
            }

            public void SetProperyValue(object target, string propertyName, object value) {
                if (target != null) {
                    PropertyInfo p = target.GetType().GetProperty(propertyName);
                    if (p != null) {
                        MethodInfo m = p.GetSetMethod();
                        if (m != null) {
                            m.Invoke(target, new object[] { value });
                            return;
                        }
                    }
                } else {
                    // targetが省略された場合はOwnerのプロパティ取得の場合と
                    // 定数取得の場合がある
                    if (Owner != null) {
                        PropertyInfo p = Owner.GetType().GetProperty(propertyName);
                        if (p != null) {
                            MethodInfo m = p.GetSetMethod();
                            if (m != null) {
                                m.Invoke(Owner, new object[] { value });
                                return;
                            }
                        }
                    }
                    foreach (Type t in _embedMethods) {
                        PropertyInfo p = t.GetProperty(propertyName, BindingFlags.Static);
                        if (p != null) {
                            MethodInfo m = p.GetSetMethod();
                            if (m != null) {
                                m.Invoke(null, new object[] { value });
                            }
                        }
                    }
                }
                throw new InvalidOperationException();
            }

            private static Context _global = new Context();
            //private static Dictionary<Thread, Context> _current = new Dictionary<Thread,Context>();
            public static Context Global { get { return _global; } }
        }
    }
}
