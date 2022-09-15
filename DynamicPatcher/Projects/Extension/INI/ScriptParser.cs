using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Script;

namespace Extension.INI
{
    public class ScriptParser : IParserRegister, IParser<List<Script.Script>>, IParser<Script.Script>
    {
        public void Register()
        {
            //Parsers.AddParser<Script.Script>(this);
            Parsers.AddParser<List<Script.Script>>(this);
        }

        public void Unregister()
        {
            //Parsers.RemoveParser<Script.Script>();
            Parsers.RemoveParser<List<Script.Script>>();
        }
        public bool Parse(string val, ref List<Script.Script> buffer)
        {
            var scriptNames = ParserExtension.SplitValue(val);
            var parsed = ScriptManager.GetScripts(scriptNames);
            if (parsed.Count > 0)
            {
                buffer = parsed;
                return true;
            }
            return false;
        }

        [Obsolete("you should parse a Script list instead", false)]
        public bool Parse(string val, ref Script.Script buffer)
        {
            var parsed = ScriptManager.GetScript(val);
            if (parsed != null)
            {
                buffer = parsed;
                return true;
            }
            return false;
        }
    }
}
