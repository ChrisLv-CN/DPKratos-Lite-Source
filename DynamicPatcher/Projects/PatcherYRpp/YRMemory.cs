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
	// provides access to the game's operator new and operator delete.
	public static partial class YRMemory
	{
		// both functions are naked, which means neither prolog nor epilog are
		// generated for them. thus, a simple jump suffices to redirect to the
		// original methods, and no more book keeping or cleanup has to be
		// performed the calling convention has to match for this trick to work.

		// naked does not support inlining. the inline modifier here means that
		// multiple definitions are allowed.

		// the game's operator new
		public static unsafe IntPtr Allocate(uint size)
		{
			var func = (delegate* unmanaged[Cdecl]<uint, IntPtr>)0x7C8E17;
			return func(size);
		}

		// the game's operator delete
		public static unsafe void Deallocate(IntPtr mem)
		{
			var func = (delegate* unmanaged[Cdecl]<IntPtr, void>)0x7C8B3D;
			func(mem);
		}

		public static IntPtr AllocateChecked(uint size)
        {
            var ptr = Allocate(size);
            if (ptr == IntPtr.Zero)
			{
				throw new OutOfMemoryException("YRMemory Alloc fail.");
			}
            return ptr;
        }

        public static Pointer<T> Allocate<T>()
        {
            return AllocateChecked((uint)Marshal.SizeOf<T>());
        }

        class ConstructorCache
        {
            public ConstructorCache(Type[] paramTypes, object[] paramList, Expression expression)
            {
                ParamTypes = paramTypes;
                ParamList = paramList;
                Expression = expression;
            }

            public Type[] ParamTypes { get; set; }
			public object[] ParamList { get; set; }
			public Expression Expression { get; set; }
		}

		static MemoryCache cache = new MemoryCache("constructor parameters");
		static Tuple<Type[], object[]> GetCache(int length)
        {
			string key = length.ToString();
			var ret = cache.Get(key);

			if (ret == null)
            {
				var policy = new CacheItemPolicy
				{
					SlidingExpiration = TimeSpan.FromSeconds(60.0)
				};
				cache.Set(key, new Tuple<Type[], object[]>(new Type[length], new object[length]), policy);
				ret = cache.Get(key);
			}
			return ret as Tuple<Type[], object[]>;
		}

		delegate T ConstructorFunction<T>(ref T @this, params object[] list);

        /// <summary>
        /// reflectly create a yr's class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static Pointer<T> Create<T>(params object[] list)
        {
            Pointer<T> ptr = Allocate<T>();

            try
            {
                Type type = typeof(T);

                int paramCount = list.Length + 1;

                var tuple = GetCache(paramCount);
                Type[] paramTypes = tuple.Item1;
                object[] paramList = tuple.Item2;

                paramTypes[0] = ptr.GetType();
                for (int i = 0; i < list.Length; i++)
                {
                    paramTypes[i + 1] = list[i].GetType();
                }

                paramList[0] = ptr;
                list.CopyTo(paramList, 1);

                MethodInfo constructor = type.GetMethod("Constructor", paramTypes);

                constructor.Invoke(null, paramList);

                return ptr;
            }
            catch (Exception)
            {
                Deallocate(ptr);
                throw;
            }
        }

        static MemoryCache dtorCache = new MemoryCache("DTOR cache");

        public static void Delete<T>(Pointer<T> ptr)
		{
            if (ptr.IsNotNull)
            {
                try
                {
                    Type type = typeof(T);
                    var dtor = dtorCache[type.FullName] as Action<Pointer<T>>;
                    if (dtor == null)
                    {
                        var parameterExpressions = new List<ParameterExpression>() { Expression.Parameter(typeof(Pointer<T>), "t") };

                        MethodInfo destructor = type.GetMethod("Destructor", new Type[] { typeof(Pointer<T>) });
                        MethodCallExpression dtorExpression = Expression.Call(destructor, parameterExpressions);
                        var expression = Expression.Lambda<Action<Pointer<T>>>(dtorExpression, parameterExpressions);
                        var lambda = expression.Compile();

                        var policy = new CacheItemPolicy
                        {
                            // TOCHECK: if there is no memory leak, expire after 60s
                            SlidingExpiration = ObjectCache.NoSlidingExpiration
                        };
                        dtorCache.Set(typeof(T).Name, lambda, policy);

                        dtor = lambda;
                    }

                    dtor(ptr);

                    Deallocate(ptr);
                }
                catch (Exception)
                {
                    Deallocate(ptr);
                    throw;
                }
            }
		}
	}

}
