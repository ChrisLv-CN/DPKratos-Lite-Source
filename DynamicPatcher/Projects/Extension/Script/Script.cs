using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ScriptAliasAttribute : Attribute
    {
        public ScriptAliasAttribute(params string[] names)
        {
            Names = names;
        }

        public string[] Names { get; }
    }

    public interface IHaveScript
    {
        List<Script> Scripts { get; }
    }

    [Serializable]
    public class Script
    {
        internal Script(string name)
        {
            Name = name;
        }

        public string Name { get; protected set; }
        public Type ScriptableType { get; internal set; }
    }
}
