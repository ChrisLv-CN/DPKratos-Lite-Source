using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Serialization
{
    public class DefaultSurrogateSelector : ISurrogateSelector
    {
        ISurrogateSelector _next;

        public virtual void ChainSelector(ISurrogateSelector selector)
        {
            _next = selector;
        }

        public virtual ISurrogateSelector GetNextSelector()
        {
            return _next;
        }

        public virtual ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (_next == null)
            {
                selector = null;
                return null;
            }

            return _next.GetSurrogate(type, context, out selector);
        }
    }

    public class DefaultSerializationSurrogate : ISerializationSurrogate
    {
        public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Type type = obj.GetType();
            info.SetType(type);
            MemberInfo[] members = SerializationHelper.GetSerializableMembers(type, context);
            object[] memberData = FormatterServices.GetObjectData(obj, members);
            for (int i = 0; i < members.Length; i++)
            {
                MemberInfo member = members[i];
                Type memberType = ((FieldInfo)member).FieldType;
                object val = memberData[i];
                info.AddValue(SerializationHelper.GetUniqueMemberName((FieldInfo)member), val, memberType);
            }
        }

        public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Type type = obj.GetType();
            MemberInfo[] members = SerializationHelper.GetSerializableMembers(type, context);
            object[] memberData = new object[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                MemberInfo member = members[i];
                Type memberType = ((FieldInfo)member).FieldType;
                memberData[i] = info.GetValue(SerializationHelper.GetUniqueMemberName((FieldInfo)member), memberType);
            }
            obj = FormatterServices.PopulateObjectMembers(obj, members, memberData);
            return obj;
        }
    }
}
