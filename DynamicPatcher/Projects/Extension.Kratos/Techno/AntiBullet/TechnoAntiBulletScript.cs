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
    public class TechnoAntiBulletScript : TechnoScriptable
    {
        public TechnoAntiBulletScript(TechnoExt owner) : base(owner) { }

        public AntiBulletData AntiBulletData;

        public override void Awake()
        {
            Logger.Log($"{Game.CurrentFrame} + AntiBulletScript Awake");
            ISectionReader reader = Ini.GetSection(Ini.RulesDependency, section);
            AntiBulletData data = new AntiBulletData();
            data.Read(reader);
            if (data.Enable)
            {
                this.AntiBulletData = data;
            }
        }
    }
}