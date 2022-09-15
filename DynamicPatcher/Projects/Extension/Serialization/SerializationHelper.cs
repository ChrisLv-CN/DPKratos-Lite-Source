using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamicPatcher;

namespace Extension.Serialization
{
    public enum Serializability
    {
        NotSerializable,

        Unsure,

        Serializable
    }

    public class SerializationHelper
    {
        public static string GetUniqueMemberName(FieldInfo field)
        {
            return $"{field.FieldType.FullName}+{field.Name}";
        }

        public static void Check(object obj)
        {
            if (obj == null)
                return;

            Type type = obj.GetType();
            Serializability serializability = SerializationChecker.StaticCheck(type);

            if (serializability == Serializability.NotSerializable)
            {
                Logger.LogError("can not serialize object {0} with type {1}", obj, type.FullName);
                return;
            }

            if (serializability == Serializability.Unsure)
            {
                _usingChecker = true;
                var checker = GetChecker();

                if (checker.RuntimeCheck(obj) != Serializability.Serializable)
                {
                    Logger.LogError("can not serialize object {0} with type {1}", obj, type.FullName);
                }

                _usingChecker = false;
            }
        }

        private static SerializationChecker _checker;
        private static object _locker = new object();
        private static bool _usingChecker = false;
        private static SerializationChecker GetChecker()
        {
            if (_checker == null)
            {
                lock (_locker)
                {
                    _checker ??= new SerializationChecker();
                    // release after 20 seconds
                    Task.Run(() =>
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(20));

                        while (_usingChecker)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                        }

                        _checker.Clear();
                        _checker = null;
                    });
                }
            }

            return _checker;
        }

        private static ConcurrentDictionary<Type, MemberInfo[]> m_MemberInfoTable = new();
        // reference to System.Runtime.Serialization.FormatterServices.GetSerializableMembers(Type type, StreamingContext context)
        public static MemberInfo[] GetSerializableMembers(Type type, StreamingContext context)
        {
            if ((object)type == null)
            {
                throw new ArgumentNullException("type");
            }
            
            return m_MemberInfoTable.GetOrAdd(type, InternalGetSerializableMembers);
		}

        // reference to System.Runtime.Serialization.FormatterServices.GetSerializableMembers(Type type)
        private static MemberInfo[] GetSerializableMembers(Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fields.Where(f => !f.IsNotSerialized).ToArray();
        }

        // reference to System.Runtime.Serialization.FormatterServices.InternalGetSerializableMembers(Type type)
        private static MemberInfo[] InternalGetSerializableMembers(Type type)
        {
            if (type.IsInterface)
            {
                return Array.Empty<MemberInfo>();
            }
            MemberInfo[] array = GetSerializableMembers(type);
            Type baseType = type.BaseType;
            if (baseType != null && baseType != typeof(object))
            {
                bool takeShortName = GetParentTypes(baseType, out Type[] parentTypes, out int parentTypeCount);
                if (parentTypeCount > 0)
                {
                    List<FieldInfo> parentFields = new List<FieldInfo>();
                    for (int i = 0; i < parentTypeCount; i++)
                    {
                        baseType = parentTypes[i];

                        if (!baseType.IsSerializable)
                        {
                            SerializationChecker.NotifyNotSerializable(baseType);
                        }

                        // should we really consider namePrefix?
                        string namePrefix = (takeShortName ? baseType.Name : baseType.FullName);
                        // get fields that not exposed to derived type
                        FieldInfo[] fields = baseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                        foreach (FieldInfo fieldInfo in fields)
                        {
                            if (!fieldInfo.IsNotSerialized)
                            {
                                parentFields.Add(fieldInfo);
                            }
                        }
                    }

                    if (parentFields.Count > 0)
                    {
                        array = array.Concat(parentFields).ToArray();
                    }
                }
            }
            return array;
        }

        // reference to System.Runtime.Serialization.FormatterServices.GetParentTypes(Type parentType, out Type[] parentTypes, out int parentTypeCount)
        private static bool GetParentTypes(Type parentType, out Type[] parentTypes, out int parentTypeCount)
        {
            List<Type> parentList = new List<Type>(12);
            bool takeShortName = true;

            Type baseType = parentType;
            while (baseType != typeof(object))
            {
                if (!baseType.IsInterface)
                {
                    string name = baseType.Name;
                    // does it possible mean single inherited?
                    // or mean no same name parent class in different namespace?
                    takeShortName = parentList.All(p => p.Name != name);

                    parentList.Add(baseType);
                }
                baseType = baseType.BaseType;
            }

            parentTypes = parentList.ToArray();
            parentTypeCount = parentList.Count;

            return takeShortName;
        }


    }
}
