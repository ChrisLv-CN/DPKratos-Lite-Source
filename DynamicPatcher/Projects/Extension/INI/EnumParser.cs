using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    public class EnumParser<T> : IParserRegister, IParser<T> where T : struct, Enum
    {
        public static readonly EnumParser<T> Parser = new EnumParser<T>();

        public void Register()
        {
            Parsers.AddParser<T>(this);
        }

        public void Unregister()
        {
            Parsers.RemoveParser<T>();
        }

        public bool Parse(string val, ref T buffer)
        {
            if (Enum.TryParse(val, out T parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }
    }

    public static class EnumParsers
    {
        private static Dictionary<Type, object> _parsers = new Dictionary<Type, object>();

        public static IParser<T> GetParser<T>()
        {
            if (TryGetParser(out IParser<T> parser))
            {
                return parser;
            }

            return null;
        }

        public static bool TryGetParser<T>(out IParser<T> parser)
        {
            if (_parsers.TryGetValue(typeof(T), out object val))
            {
                parser = val as IParser<T>;
                return true;
            }

            if (typeof(T).IsEnum)
            {
                _parsers[typeof(T)] = parser = typeof(EnumParser<>).MakeGenericType(typeof(T)).GetField("Parser").GetValue(null) as IParser<T>;
                return true;
            }

            parser = null;
            return false;
        }
    }
}
