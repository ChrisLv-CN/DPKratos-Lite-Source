using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class TechnoAttachEffectScript : TechnoScriptable
    {
        public TechnoAttachEffectScript(TechnoExt owner) : base(owner) { }

        private TechnoStatusScript technoStatus => GameObject.GetComponent<TechnoStatusScript>();

    }
}
