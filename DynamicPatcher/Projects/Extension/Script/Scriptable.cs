using Extension.Ext;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class GlobalScriptableAttribute : Attribute
    {
        public GlobalScriptableAttribute(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; }
    }

    public interface IScriptable : IReloadable
    {
    }

    [Serializable]
    public abstract class Scriptable<T> : ScriptComponent, IScriptable
    {
        public T Owner { get; protected set; }

        protected Scriptable(T owner) : base()
        {
            Owner = owner;
        }
    }

}
