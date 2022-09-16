using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using System.Threading.Tasks;
using Extension.INI;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    public class TechnoScript : TechnoScriptable
    {
        public TechnoScript(TechnoExt owner) : base(owner) { }

        public override void Awake()
        {
            Logger.Log($"{Game.CurrentFrame} + Techno 全局主程");
        }
    }
}