using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    /// <summary>
    /// buffer to store parsed and unparsed pairs in ini
    /// </summary>
    internal class INIBuffer
    {
        public INIBuffer(string name, string section)
        {
            Name = name;
            Section = section;
            Unparsed = new Dictionary<string, string>();
            Parsed = new Dictionary<string, object>();
        }

        public string Name;
        public string Section;
        public Dictionary<string, string> Unparsed;
        public Dictionary<string, object> Parsed;

        public bool GetParsed<T>(string key, out T val, IParser<T> parser)
        {
            if (Parsed.TryGetValue(key, out object parsed))
            {
                val = (T)parsed;
                return true;
            }

            T tmp = default;
            if (Unparsed.TryGetValue(key, out string unparsed) && parser.Parse(unparsed, ref tmp))
            {
                Parsed[key] = val = tmp;
                return true;
            }

            val = default;
            return false;
        }
        public bool GetParsed<T>(string key, out T val)
        {
            return GetParsed(key, out val, Parsers.GetParser<T>());
        }

        public bool GetParsedList<T>(string key, out T[] val, IParser<T> parser)
        {
            if (Parsed.TryGetValue(key, out object parsed))
            {
                val = (T[])parsed;
                return true;
            }

            List<T> tmp = new List<T>();
            if (Unparsed.TryGetValue(key, out string unparsed) && parser.ParseList(unparsed, ref tmp))
            {
                Parsed[key] = val = tmp.ToArray();
                return true;
            }

            val = default;
            return false;
        }
        public bool GetParsedList<T>(string key, out T[] val)
        {
            return GetParsedList(key, out val, Parsers.GetParser<T>());
        }
    }

    internal class INILinkedBuffer
    {
        public INILinkedBuffer(INIBuffer buffer, INILinkedBuffer nextBuffer = null)
        {
            m_Buffer = buffer;
            m_LinkedBuffer = nextBuffer;
        }

        public string Name => m_Buffer.Name;
        public string Dependency => m_LinkedBuffer != null ? Name + "->" + m_LinkedBuffer.Dependency : Name;
        public string Section => m_Buffer.Section;

        public bool Expired { get; set; }

        public INIBuffer GetFirstOccurrence(string key)
        {
            if (m_Buffer.Unparsed.TryGetValue(key, out _))
                return m_Buffer;

            return m_LinkedBuffer?.GetFirstOccurrence(key);
        }

        public bool GetUnparsed(string key, out string val)
        {
            if (m_Buffer.Unparsed.TryGetValue(key, out val))
                return true;

            if (m_LinkedBuffer != null)
            {
                return m_LinkedBuffer.GetUnparsed(key, out val);
            }

            return false;
        }

        public bool GetParsed<T>(string key, out T val, IParser<T> parser)
        {
            if (m_Buffer.GetParsed(key, out val, parser))
                return true;

            if (m_LinkedBuffer != null)
            {
                return m_LinkedBuffer.GetParsed(key, out val, parser);
            }

            return false;
        }
        public bool GetParsed<T>(string key, out T val)
        {
            return GetParsed(key, out val, Parsers.GetParser<T>());
        }

        public bool GetParsedList<T>(string key, out T[] val, IParser<T> parser)
        {
            if (m_Buffer.GetParsedList(key, out val, parser))
                return true;

            if (m_LinkedBuffer != null)
            {
                return m_LinkedBuffer.GetParsedList(key, out val, parser);
            }

            return false;
        }
        public bool GetParsedList<T>(string key, out T[] val)
        {
            return GetParsedList(key, out val, Parsers.GetParser<T>());
        }

        private INIBuffer m_Buffer;
        private INILinkedBuffer m_LinkedBuffer;
    }
}
