using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Serialization
{
    // https://stackoverflow.com/questions/3294224/serialization-and-the-yield-statement

    class CoroutineSurrogateSelector : DefaultSurrogateSelector
    {
        public override ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (type.IsSerializable == false
                && typeof(IEnumerator).IsAssignableFrom(type)
                && type.IsDefined(typeof(CompilerGeneratedAttribute)))
            {
                selector = this;
                return new CoroutineSerializationSurrogate();
            }
            
            return base.GetSurrogate(type, context, out selector);
        }
    }


    class CoroutineSerializationSurrogate : DefaultSerializationSurrogate
    {
        private static void SetCurrent(object obj, object cur)
        {
            FieldInfo f = obj.GetType().GetField("<>2__current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            f?.SetValue(obj, cur);
        }

        public override void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            object currentStored = null;
            if (obj is IEnumerator enumerator)
            {
                if (enumerator.Current is Task)
                {
                    currentStored = enumerator.Current;
                    SetCurrent(obj, null);
                }
            }

            base.GetObjectData(obj, info, context);

            if (currentStored != null)
            {
                SetCurrent(obj, currentStored);
            }
        }
    }
}
