using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Utilities;
using PatcherYRpp;

namespace Extension.INI
{
    /// <summary>
    /// parser to find AbstractTypeClass
    /// </summary>
    /// <typeparam name="T">type can be converted to AbstractTypeClass</typeparam>
    public class FindTypeParser<T> : IParserRegister, IParser<Pointer<T>>, IParser<SwizzleablePointer<T>>
    {
        public FindTypeParser(IEnumerable<Pointer<T>> enumerable)
        {
            _enumerable = enumerable;
        }

        public void Register()
        {
            Parsers.AddParser<Pointer<T>>(this);
            Parsers.AddParser<SwizzleablePointer<T>>(this);
        }

        public void Unregister()
        {
            Parsers.RemoveParser<Pointer<T>>();
            Parsers.RemoveParser<SwizzleablePointer<T>>();
        }

        public bool Parse(string val, ref Pointer<T> buffer)
        {
            Pointer<T> parsed = _enumerable.First(p => p.Convert<AbstractTypeClass>().Ref.ID == val);
            if (parsed.IsNotNull)
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        public bool Parse(string val, ref SwizzleablePointer<T> buffer)
        {
            return Parse(val, ref buffer.Pointer);
        }

        private IEnumerable<Pointer<T>> _enumerable;
    }
}
