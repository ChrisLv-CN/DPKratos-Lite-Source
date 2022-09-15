
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Extension.Components;


namespace Extension.Script
{
    public partial class ScriptManager
    {

        private static Dictionary<string, object> ScriptCtors = new Dictionary<string, object>();



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo(Component root, Script script)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1>(Component root, Script script, T1 p1)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2>(Component root, Script script, T1 p1, T2 p2)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3>(Component root, Script script, T1 p1, T2 p2, T3 p3)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5, T6>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5, p6);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5, T6, T7>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5, p6, p7);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5, T6, T7, T8>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5, p6, p7, p8);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5, p6, p7, p8, p9);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScriptComponent CreateScriptableTo<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Component root, Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15)
        {
            if (script == null)
                return null;

            var scriptComponent = CreateScriptable(script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
            scriptComponent.AttachToComponent(root);
            return scriptComponent;
        }

        public static ScriptComponent CreateScriptable(Script script)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    {  };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<ScriptComponent>;

            var scriptable = ctor();
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1>(Script script, T1 p1)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, ScriptComponent>;

            var scriptable = ctor(p1);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2>(Script script, T1 p1, T2 p2)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, ScriptComponent>;

            var scriptable = ctor(p1, p2);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3>(Script script, T1 p1, T2 p2, T3 p3)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4>(Script script, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5, T6>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, T6, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5, p6);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5, T6, T7>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, T6, T7, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5, p6, p7);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, T6, T7, T8, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5, p6, p7, p8);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5, p6, p7, p8, p9);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10"), Expression.Parameter(typeof(T11), "t11") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10"), Expression.Parameter(typeof(T11), "t11"), Expression.Parameter(typeof(T12), "t12") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10"), Expression.Parameter(typeof(T11), "t11"), Expression.Parameter(typeof(T12), "t12"), Expression.Parameter(typeof(T13), "t13") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10"), Expression.Parameter(typeof(T11), "t11"), Expression.Parameter(typeof(T12), "t12"), Expression.Parameter(typeof(T13), "t13"), Expression.Parameter(typeof(T14), "t14") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
            scriptable.Script = script;
            return scriptable;
        }

        public static ScriptComponent CreateScriptable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Script script, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15)
        {
            if (script == null)
                return null;
                
            string uniqueCtorName = script.ScriptableType.FullName;
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10"), Expression.Parameter(typeof(T11), "t11"), Expression.Parameter(typeof(T12), "t12"), Expression.Parameter(typeof(T13), "t13"), Expression.Parameter(typeof(T14), "t14"), Expression.Parameter(typeof(T15), "t15") };

                var constructor = script.ScriptableType.GetConstructors()[0];
                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, ScriptComponent>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                ScriptCtors.Add(uniqueCtorName, lambda);

                func = lambda;
            }

            var ctor = func as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, ScriptComponent>;

            var scriptable = ctor(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
            scriptable.Script = script;
            return scriptable;
        }

    }
}