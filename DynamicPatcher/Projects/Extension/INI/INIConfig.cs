﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NotINIFieldAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class INIFieldAttribute : Attribute
    {
        public string Key { get; set; }

        //public int MinCount { get; set; }
        //public int MaxCount { get; set; }

        //public object Min { get; set; }
        //public object Max { get; set; }
    }

    public interface ISectionReader
    {
        string Section { get; }
        T Get<T>(string key, T def = default, IParser<T> parser = null);
        bool TryGet<T>(string key, out T value, IParser<T> parser = null);
        T[] GetList<T>(string key, T[] def = default, IParser<T> parser = null);
        bool TryGetList<T>(string key, out T[] value, IParser<T> parser = null);
    }

    public interface IConfigReader : INonaggressiveReader, ISectionReader
    {
    }

    public interface IConfigWrapper<T> where T : INIConfig
    {
        T Data { get; }
    }

    [Serializable]
    public abstract class INIConfig
    {
        /// <summary>
        /// Read data from reader.
        /// </summary>
        /// <param name="ini"></param>
        public abstract void Read(IConfigReader ini);
    }

    [Serializable]
    public abstract class INIAutoConfig : INIConfig
    {
        /// <summary>
        /// Post process after auto reading.
        /// </summary>
        /// <param name="ini"></param>
        public virtual void Post(IConfigReader ini) { }

        public sealed override void Read(IConfigReader ini)
        {
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            MethodInfo getList = ini.GetType().GetMethod("GetList");
            MethodInfo get = ini.GetType().GetMethod("Get");

            foreach (FieldInfo field in fields)
            {
                if (!field.IsDefined(typeof(NotINIFieldAttribute)))
                {
                    var iniField = field.GetCustomAttribute<INIFieldAttribute>();

                    MethodInfo getMethod;
                    if (field.FieldType.IsArray)
                    {
                        getMethod = getList.MakeGenericMethod(field.FieldType.GetElementType());
                    }
                    else
                    {
                        getMethod = get.MakeGenericMethod(field.FieldType);
                    }

                    string key = iniField?.Key ?? field.Name;
                    var val = getMethod.Invoke(ini, new object[] { key, field.GetValue(this), null });
                    field.SetValue(this, val);
                }
            }

            Post(ini);
        }
    }

}
