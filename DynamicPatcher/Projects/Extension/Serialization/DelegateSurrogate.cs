using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Serialization
{
    class DelegateSurrogateSelector : DefaultSurrogateSelector
    {
        public override ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (type.IsSerializable == false
                && type.Name.StartsWith("<>c__DisplayClass")
                && type.IsDefined(typeof(CompilerGeneratedAttribute)))
            {
                selector = this;
                return new DelegateSerializationSurrogate();
            }

            return base.GetSurrogate(type, context, out selector);
        }
    }
    class DelegateSerializationSurrogate : DefaultSerializationSurrogate
    {

    }
}
