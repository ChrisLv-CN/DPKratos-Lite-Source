using Extension.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    [Serializable]
    public abstract class ScriptComponent : Component
    {
        protected ScriptComponent()
        {

        }

        protected ScriptComponent(Script script)
        {
            Script = script;
        }

        public Script Script { get; internal set; }
    }
}
