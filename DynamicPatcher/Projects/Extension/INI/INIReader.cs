using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    public interface INonaggressiveReader
    {
        bool Read<T>(string section, string key, ref T buffer, IParser<T> parser = null);
        bool ReadArray<T>(string section, string key, ref T[] buffer, int count = -1, IParser<T> parser = null);
        bool ReadCollection<T, TCollection>(string section, string key, ref TCollection buffer, IParser<T> parser = null) where TCollection : ICollection<T>;
    }

    public abstract class INIReader : INonaggressiveReader
    {

        public Encoding Encoding { get; set; } = Encoding.UTF8;


        /// <summary>
        /// read data into buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="buffer"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool Read<T>(string section, string key, ref T buffer, IParser<T> parser = null)
        {
            parser ??= Parsers.GetParser<T>();

            if (ReadString(section, key, out string val))
            {
                return parser.Parse(val, ref buffer);
            }

            return false;
        }

        /// <summary>
        /// read data into array array.Length times
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="buffer"></param>
        /// <param name="count">the count of values to read into buffer. -1 mean the length of buffer</param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool ReadArray<T>(string section, string key, ref T[] buffer, int count = -1, IParser<T> parser = null)
        {
            if (count == -1)
            {
                count = buffer.Length;
            }
            parser ??= Parsers.GetParser<T>();

            if (ReadString(section, key, out string val))
            {
                return parser.ParseArray(val, ref buffer, count);
            }
            return false;
        }

        /// <summary>
        /// read data into list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool ReadList<T, TList>(string section, string key, ref TList buffer, IParser<T> parser = null) where TList : IList<T>
        {
            return ReadCollection(section, key, ref buffer, parser);
        }

        /// <summary>
        /// read data into collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public bool ReadCollection<T, TCollection>(string section, string key, ref TCollection buffer, IParser<T> parser = null) where TCollection : ICollection<T>
        {
            parser ??= Parsers.GetParser<T>();

            if (ReadString(section, key, out string val))
            {
                return parser.ParseCollection(val, ref buffer);
            }

            return false;
        }

        protected abstract bool ReadString(string section, string key, out string val);
    }
}
