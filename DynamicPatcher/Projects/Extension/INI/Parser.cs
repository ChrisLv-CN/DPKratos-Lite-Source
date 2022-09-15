using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    public interface IParser<T>
    {
        /// <summary>
        /// parse value and assign to buffer if success
        /// </summary>
        /// <param name="val">the value to parse</param>
        /// <param name="buffer">the buffer to be assign if success</param>
        /// <returns></returns>
        bool Parse(string val, ref T buffer);
    }

    public interface IParserRegister
    {
        void Register();
        void Unregister();
    }

    public static class ParserExtension
    {
        public static string[] SplitValue(string val, params char[] separator)
        {
            return val.Split(separator).Select(s => s.Trim()).ToArray();
        }
        public static string[] SplitValue(string val)
        {
            return SplitValue(val, ',');
        }

        /// <summary>
        /// parse value into array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <param name="val"></param>
        /// <param name="buffer"></param>
        /// <param name="count">the count of values to read into buffer. -1 mean the length of buffer</param>
        /// <returns></returns>
        public static bool ParseArray<T>(this IParser<T> parser, string val, ref T[] buffer, int count = -1)
        {
            if (count == -1)
            {
                count = buffer.Length;
            }

            string[] valList = SplitValue(val);
            int i;
            for (i = 0; i < Math.Min(valList.Length, count); i++)
            {
                string curVal = valList[i].Trim();
                if (parser.Parse(curVal, ref buffer[i]) == false)
                {
                    break;
                }
            }

            return i == count;
        }

        /// <summary>
        /// parse value into list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <param name="val"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool ParseList<T, TList>(this IParser<T> parser, string val, ref TList buffer) where TList : IList<T>
        {
            return parser.ParseCollection(val, ref buffer);
        }

        /// <summary>
        /// parse value into collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <param name="val"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool ParseCollection<T, TCollection>(this IParser<T> parser, string val, ref TCollection buffer) where TCollection : ICollection<T>
        {
            string[] strs = SplitValue(val);
            T[] aBuffer = new T[strs.Length];
            if (parser.ParseArray(val, ref aBuffer, strs.Length))
            {
                buffer.Clear();
                foreach (T v in aBuffer)
                {
                    buffer.Add(v);
                }
                return true;
            }

            return false;
        }
    }

}
