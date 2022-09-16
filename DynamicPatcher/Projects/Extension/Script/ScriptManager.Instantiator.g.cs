
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@0");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 0);
                var ps = constructor.GetParameters();

                var argTypes = new Type[1] { typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@1");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 1);
                var ps = constructor.GetParameters();

                var argTypes = new Type[2] { ps[0].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@2");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 2);
                var ps = constructor.GetParameters();

                var argTypes = new Type[3] { ps[0].ParameterType, ps[1].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@3");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 3);
                var ps = constructor.GetParameters();

                var argTypes = new Type[4] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@4");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 4);
                var ps = constructor.GetParameters();

                var argTypes = new Type[5] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@5");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 5);
                var ps = constructor.GetParameters();

                var argTypes = new Type[6] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@6");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 6);
                var ps = constructor.GetParameters();

                var argTypes = new Type[7] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@7");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 7);
                var ps = constructor.GetParameters();

                var argTypes = new Type[8] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType, ps[6].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@8");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 8);
                var ps = constructor.GetParameters();

                var argTypes = new Type[9] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType, ps[6].ParameterType, ps[7].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@9");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 9);
                var ps = constructor.GetParameters();

                var argTypes = new Type[10] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType, ps[6].ParameterType, ps[7].ParameterType, ps[8].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@10");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 10);
                var ps = constructor.GetParameters();

                var argTypes = new Type[11] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType, ps[6].ParameterType, ps[7].ParameterType, ps[8].ParameterType, ps[9].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@11");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 11);
                var ps = constructor.GetParameters();

                var argTypes = new Type[12] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType, ps[6].ParameterType, ps[7].ParameterType, ps[8].ParameterType, ps[9].ParameterType, ps[10].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@12");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 12);
                var ps = constructor.GetParameters();

                var argTypes = new Type[13] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType, ps[6].ParameterType, ps[7].ParameterType, ps[8].ParameterType, ps[9].ParameterType, ps[10].ParameterType, ps[11].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@13");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 13);
                var ps = constructor.GetParameters();

                var argTypes = new Type[14] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType, ps[6].ParameterType, ps[7].ParameterType, ps[8].ParameterType, ps[9].ParameterType, ps[10].ParameterType, ps[11].ParameterType, ps[12].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@14");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 14);
                var ps = constructor.GetParameters();

                var argTypes = new Type[15] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType, ps[6].ParameterType, ps[7].ParameterType, ps[8].ParameterType, ps[9].ParameterType, ps[10].ParameterType, ps[11].ParameterType, ps[12].ParameterType, ps[13].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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
                
            string uniqueCtorName = String.Concat(script.ScriptableType.FullName, "@15");
            if (!ScriptCtors.TryGetValue(uniqueCtorName, out var func))
            {
                var constructor = script.ScriptableType.GetConstructors().First(c => c.GetParameters().Length == 15);
                var ps = constructor.GetParameters();

                var argTypes = new Type[16] { ps[0].ParameterType, ps[1].ParameterType, ps[2].ParameterType, ps[3].ParameterType, ps[4].ParameterType, ps[5].ParameterType, ps[6].ParameterType, ps[7].ParameterType, ps[8].ParameterType, ps[9].ParameterType, ps[10].ParameterType, ps[11].ParameterType, ps[12].ParameterType, ps[13].ParameterType, ps[14].ParameterType, typeof(ScriptComponent) };

                List<ParameterExpression> parameterExpressions = ps.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();

                NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
                var expression = Expression.Lambda(Expression.GetFuncType(argTypes), ctorExpression, parameterExpressions);
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