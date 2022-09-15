using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;

namespace Extension.Serialization
{
    class SerializationChecker
    {
        static ConcurrentDictionary<Type, Serializability> _typeCheckTable = new();

        /// <summary>
        /// this is an incomplete check, which will ignore derive classes
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Serializability StaticCheck(Type type)
        {
            if (_typeCheckTable.TryGetValue(type, out Serializability serializable))
            {
                return serializable;
            }

            _typeCheckTable.AddOrUpdate(type, (_) => serializable = CheckSerializable(type), (_, v) => v);

            if (ShouldCheckMembers(type))
            {
                FieldInfo[] fields = SerializationHelper.GetSerializableMembers(type, new StreamingContext(StreamingContextStates.All)).Cast<FieldInfo>().ToArray();

                List<Type> typesToCheck = fields.Select(f => f.FieldType).ToList();

                if (type.HasElementType)
                {
                    typesToCheck.Add(type.GetElementType());
                }

                foreach (Type t in typesToCheck)
                {
                    Serializability childSerializability = StaticCheck(t);
                    if (childSerializability < serializable)
                    {
                        if (_typeCheckTable.TryUpdate(type, childSerializability, serializable))
                        {
                            string message = string.Format("[Serialization Static Check] {0} become {2} because {1} is {2}", type.FullName, t.FullName, childSerializability);
                            if (childSerializability == Serializability.NotSerializable)
                                Logger.LogError(message);
                            else
                                Logger.LogWarning(message);
                        }
                    }
                }
            }


            serializable = StaticCheck(type);
            if (serializable == Serializability.NotSerializable)
            {
                NotifyNotSerializable(type);
            }
            return serializable;
        }


        ConcurrentDictionary<object, Serializability> _objectCheckTable = new();
        public Serializability RuntimeCheck(object obj)
        {
            if (obj == null)
                return Serializability.Serializable;

            if (_objectCheckTable.TryGetValue(obj, out Serializability serializable))
            {
                return serializable;
            }

            Type type = obj.GetType();

            _objectCheckTable.AddOrUpdate(obj, (_) => serializable = CheckSerializable(type), (_, v) => v);

            if (ShouldCheckMembers(type))
            {
                FieldInfo[] fields = SerializationHelper.GetSerializableMembers(type, new StreamingContext(StreamingContextStates.All)).Cast<FieldInfo>().ToArray();

                List<object> objectsToCheck = fields.Select(f => f.GetValue(obj)).ToList();

                if (obj is Array array)
                {
                    foreach (object element in array)
                    {
                        objectsToCheck.Add(element);
                    }
                }

                for (int i = 0; i < objectsToCheck.Count; i++)
                {
                    object o = objectsToCheck[i];
                    Serializability childSerializability = RuntimeCheck(o);
                    if (childSerializability < serializable)
                    {
                        if (_objectCheckTable.TryUpdate(type, childSerializability, serializable))
                        {
                            Type t = o.GetType();
                            string message = string.Format("[Serialization Runtime Check] {0} become {2} because {1} is {2}", type.FullName, t.FullName, childSerializability);
                            if (childSerializability == Serializability.NotSerializable)
                                Logger.LogError(message);
                            else
                                Logger.LogWarning(message);
                        }
                    }
                }
            }


            serializable = RuntimeCheck(obj);
            if (serializable != Serializability.Serializable)
            {
                NotifyNotSerializable(type);
            }
            return serializable;
        }

        public void Clear()
        {
            _objectCheckTable.Clear();
        }

        private static bool ShouldCheckMembers(Type type)
        {
            if (typeof(ISerializable).IsAssignableFrom(type))
            {
                return false;
            }

            return true;
        }

        private static Serializability CheckSerializable(Type type)
        {
            if (type.IsSerializable)
                return Serializability.Serializable;
            if (type.IsSubclassOf(typeof(Delegate)))
                return Serializability.Serializable;
            if (HasSurrogate(type))
                return Serializability.Serializable;

            if (type.IsInterface)
                return Serializability.Unsure;
            if (type == typeof(object))
                return Serializability.Unsure;

            return Serializability.NotSerializable;
        }

        private static EnhancedFormatter formatter = new EnhancedFormatter();
        private static bool HasSurrogate(Type t)
        {
            return formatter.SurrogateSelector.GetSurrogate(t, formatter.Context, out ISurrogateSelector selector) != null;
        }

        internal static void NotifyNotSerializable(Type type)
        {
            Logger.LogError("{0} in {1} is not marked as Serializable or can not be serialized", type.FullName, type.Module.Assembly.FullName);
        }
    }
}
