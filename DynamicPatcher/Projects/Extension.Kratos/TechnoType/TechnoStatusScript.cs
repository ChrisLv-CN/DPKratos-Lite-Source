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

        public SwizzleablePointer<TechnoClass> MyMaster = new SwizzleablePointer<TechnoClass>(IntPtr.Zero);

        public bool DisableVoxelCache;
        public float VoxelShadowScaleInAir;

        public bool DisableSelectable;

        public override void Awake()
        {
            this.VoxelShadowScaleInAir = Ini.GetSection(Ini.RulesDependency, RulesExt.SectionAudioVisual).Get("VoxelShadowScaleInAir", 2f);
        }

        public override void OnSelect(ref bool selectable)
        {
            selectable = !DisableSelectable;
        }
    }
}