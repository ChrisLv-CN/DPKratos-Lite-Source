using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Components
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    sealed class RequireComponent : Attribute
    {
        public RequireComponent(params Type[] componentTypes)
        {
            ComponentTypes = componentTypes;
        }

        public Type[] ComponentTypes { get; }
    }
}
