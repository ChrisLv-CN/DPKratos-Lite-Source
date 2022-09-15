using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.INI
{
    public static class Parsers
    {
        private static Dictionary<Type, object> _parsers = new Dictionary<Type, object>();

        static Parsers()
        {
            new NormalParser().Register();
            new ScriptParser().Register();

            YRParsers.Register();
        }

        public static EnumParser<T> GetEnumParser<T>() where T : struct, Enum => EnumParser<T>.Parser;
        public static IParser<T> GetEnumParserUnsafe<T>() => EnumParsers.GetParser<T>();

        public static void AddParser<T>(IParser<T> parser)
        {
            _parsers[typeof(T)] = parser;
        }

        public static void RemoveParser<T>()
        {
            _parsers.Remove(typeof(T));
        }

        public static IParser<T> GetParser<T>()
        {
            if (TryGetParser(out IParser<T> parser))
            {
                return parser;
            }

            if (typeof(T).IsEnum)
            {
                return Parsers.GetEnumParserUnsafe<T>();
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

            parser = null;
            return false;
        }
    }
}
