using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    public class NormalParser : IParserRegister,
        IParser<bool>,
        IParser<byte>,
        IParser<sbyte>,
        IParser<short>,
        IParser<ushort>,
        IParser<int>,
        IParser<uint>,
        IParser<long>,
        IParser<ulong>,
        IParser<float>,
        IParser<double>,
        IParser<string>
    {
        public void Register()
        {
            Parsers.AddParser<bool>(this);
            Parsers.AddParser<byte>(this);
            Parsers.AddParser<sbyte>(this);
            Parsers.AddParser<short>(this);
            Parsers.AddParser<ushort>(this);
            Parsers.AddParser<int>(this);
            Parsers.AddParser<uint>(this);
            Parsers.AddParser<long>(this);
            Parsers.AddParser<ulong>(this);
            Parsers.AddParser<float>(this);
            Parsers.AddParser<double>(this);
            Parsers.AddParser<string>(this);
        }

        public void Unregister()
        {
            Parsers.RemoveParser<bool>();
            Parsers.RemoveParser<byte>();
            Parsers.RemoveParser<sbyte>();
            Parsers.RemoveParser<short>();
            Parsers.RemoveParser<ushort>();
            Parsers.RemoveParser<int>();
            Parsers.RemoveParser<uint>();
            Parsers.RemoveParser<long>();
            Parsers.RemoveParser<ulong>();
            Parsers.RemoveParser<float>();
            Parsers.RemoveParser<double>();
            Parsers.RemoveParser<string>();
        }

        public bool Parse(string val, ref bool buffer)
        {
            switch (val.ToUpper()[0])
            {
                case '1':
                case 'T':
                case 'Y':
                    buffer = true;
                    return true;
                case '0':
                case 'F':
                case 'N':
                    buffer = false;
                    return true;
                default:
                    return false;
            }
        }

        public bool Parse(string val, ref byte buffer)
        {
            if (byte.TryParse(val, out var parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref sbyte buffer)
        {
            if (sbyte.TryParse(val, out var parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref short buffer)
        {
            if (short.TryParse(val, out var parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref ushort buffer)
        {
            if (ushort.TryParse(val, out var parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref int buffer)
        {
            if (int.TryParse(val, out var parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref uint buffer)
        {
            if (uint.TryParse(val, out var parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref long buffer)
        {
            if (long.TryParse(val, out var parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref ulong buffer)
        {
            if (ulong.TryParse(val, out var parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref float buffer)
        {
            if (float.TryParse(val, out var parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref double buffer)
        {
            if (double.TryParse(val, out var parsed))
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref string buffer)
        {
            buffer = val;
            return true;
        }

    }
}
