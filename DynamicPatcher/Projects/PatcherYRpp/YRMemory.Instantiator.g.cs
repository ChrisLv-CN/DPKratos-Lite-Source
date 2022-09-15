
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace PatcherYRpp
{

	public static partial class YRMemory
	{
	
        private static MemoryCache ctorCache = new MemoryCache("CTOR cache");
        
        public static Pointer<T> Construct<T>(this Pointer<T> @this)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@0";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"),  };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>),  });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this);

            return @this;
        }

        public static Pointer<T> Construct<T, T1>(this Pointer<T> @this, T1 p1)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@1";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2>(this Pointer<T> @this, T1 p1, T2 p2)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@2";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@3";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@4";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@5";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5, T6>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@6";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5, T6>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5, T6>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5, p6);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5, T6, T7>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@7";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5, p6, p7);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5, T6, T7, T8>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@8";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5, p6, p7, p8);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@9";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5, p6, p7, p8, p9);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@10";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@11";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10"), Expression.Parameter(typeof(T11), "t11") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@12";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10"), Expression.Parameter(typeof(T11), "t11"), Expression.Parameter(typeof(T12), "t12") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@13";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10"), Expression.Parameter(typeof(T11), "t11"), Expression.Parameter(typeof(T12), "t12"), Expression.Parameter(typeof(T13), "t13") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@14";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10"), Expression.Parameter(typeof(T11), "t11"), Expression.Parameter(typeof(T12), "t12"), Expression.Parameter(typeof(T13), "t13"), Expression.Parameter(typeof(T14), "t14") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);

            return @this;
        }

        public static Pointer<T> Construct<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Pointer<T> @this, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15)
        {
            if (@this.IsNull)
            {
                throw new NullReferenceException("'this' is nullptr");
            }

            string uniqueCtorName = typeof(T).FullName + "@15";
            var ctor = ctorCache[uniqueCtorName] as Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>;

            if (ctor == null)
            {
                Type type = typeof(T);
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<T>), "TPtr"), Expression.Parameter(typeof(T1), "t1"), Expression.Parameter(typeof(T2), "t2"), Expression.Parameter(typeof(T3), "t3"), Expression.Parameter(typeof(T4), "t4"), Expression.Parameter(typeof(T5), "t5"), Expression.Parameter(typeof(T6), "t6"), Expression.Parameter(typeof(T7), "t7"), Expression.Parameter(typeof(T8), "t8"), Expression.Parameter(typeof(T9), "t9"), Expression.Parameter(typeof(T10), "t10"), Expression.Parameter(typeof(T11), "t11"), Expression.Parameter(typeof(T12), "t12"), Expression.Parameter(typeof(T13), "t13"), Expression.Parameter(typeof(T14), "t14"), Expression.Parameter(typeof(T15), "t15") };
                    
                MethodInfo constructor = type.GetMethod("Constructor", new[] { typeof(Pointer<T>), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15) });
                MethodCallExpression ctorExpression = Expression.Call(constructor, parameterExpressions);
                var expression = Expression.Lambda<Action<Pointer<T>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>(ctorExpression, parameterExpressions);
                var lambda = expression.Compile();
                
                var policy = new CacheItemPolicy
                {
                    // TOCHECK: if there is no memory leak, expire after 60s
                    SlidingExpiration = ObjectCache.NoSlidingExpiration
                };
                ctorCache.Set(uniqueCtorName, lambda, policy);

                ctor = lambda;
            }

            ctor(@this, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);

            return @this;
        }

	}
}