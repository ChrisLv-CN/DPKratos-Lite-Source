using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext
{
    public abstract class ExtensionFactory<TExt, TBase> where TExt : Extension<TBase>
    {
        
        public abstract TExt Create(Pointer<TBase> ownerObject);


    }


    internal class LambdaExtensionFactory<TExt, TBase> : ExtensionFactory<TExt, TBase> where TExt : Extension<TBase>
    {
        private Func<Pointer<TBase>, TExt> m_Ctor;

        public LambdaExtensionFactory()
        {
            List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<TBase>), "ownerObject") };

            var constructor = typeof(TExt).GetConstructors()[0];
            NewExpression ctorExpression = Expression.New(constructor, parameterExpressions);
            var expression = Expression.Lambda<Func<Pointer<TBase>, TExt>>(ctorExpression, parameterExpressions);
            var lambda = expression.Compile();

            m_Ctor = lambda;
        }

        public override TExt Create(Pointer<TBase> ownerObject)
        {
            return m_Ctor(ownerObject);
        }
    }
}
