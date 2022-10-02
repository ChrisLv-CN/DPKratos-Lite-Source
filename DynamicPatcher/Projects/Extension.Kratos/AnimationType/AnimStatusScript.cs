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
    [GlobalScriptable(typeof(AnimExt))]
    public partial class AnimStatusScript : AnimScriptable
    {
        public AnimStatusScript(AnimExt owner) : base(owner) { }


        public override void Awake()
        {
            
        }

        public override void OnUpdate()
        {
            OnUpdate_Visibility();
        }

    }
}
