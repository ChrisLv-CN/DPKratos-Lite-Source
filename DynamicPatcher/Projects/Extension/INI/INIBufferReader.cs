using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Extension.INI
{
    /// <summary>
    /// An ini reader using parsed buffer
    /// </summary>
    public sealed class INIBufferReader : INIReader, IConfigReader
    {
        /// <summary>
        /// instantiate an ini reader using parsed buffer
        /// </summary>
        /// <param name="dependency">the ini filename or dependency chain</param>
        public INIBufferReader(string dependency, string section)
        {
            m_Buffer = INIComponentManager.FindLinkedBuffer(dependency, section);
        }

        internal INIBufferReader(INILinkedBuffer linkedBuffer)
        {
            m_Buffer = linkedBuffer;
        }

        public string Dependency => m_Buffer.Dependency;
        public string Section => m_Buffer.Section;

        /// <summary>
        /// get key value from ini
        /// </summary>
        /// <remarks>you can only get basic type value</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public T Get<T>(string key, T def = default, IParser<T> parser = null)
        {
            if (GetBuffer().GetParsed(key, out T val, parser ?? Parsers.GetParser<T>()))
            {
                return val;
            }

            return def;
        }

        /// <summary>
        /// get key values from ini
        /// </summary>
        /// <remarks>you can only get basic type value</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public T[] GetList<T>(string key, T[] def = default, IParser<T> parser = null)
        {
            if (GetBuffer().GetParsedList(key, out T[] val, parser ?? Parsers.GetParser<T>()))
            {
                return val;
            }

            return def;
        }

        protected override bool ReadString(string section, string key, out string str)
        {
            // find buffer
            INILinkedBuffer linkedBuffer = INIComponentManager.FindLinkedBuffer(Dependency, section);

            return linkedBuffer.GetUnparsed(key, out str);
        }

        private INILinkedBuffer GetBuffer()
        {
            if (m_Buffer.Expired)
            {
                m_Buffer = INIComponentManager.FindLinkedBuffer(Dependency, Section);
            }

            return m_Buffer;
        }

        private INILinkedBuffer m_Buffer;
    }
}
