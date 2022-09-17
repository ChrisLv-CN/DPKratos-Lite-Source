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
    public class TechnoStatusScript : TechnoScriptable
    {
        public TechnoStatusScript(TechnoExt owner) : base(owner) { }

        public override void Awake()
        {
            // Logger.Log($"{Game.CurrentFrame} + Techno 全局主程");
        }
    }
}