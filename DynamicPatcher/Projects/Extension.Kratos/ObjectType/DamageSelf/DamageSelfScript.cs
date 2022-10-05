using System.ComponentModel;
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
    [GlobalScriptable(typeof(TechnoExt), typeof(BulletExt))]
    [UpdateAfter(typeof(AttachEffectScript))]
    public class DamageSelfScript : ObjectScriptable
    {
        public DamageSelfScript(IExtension owner) : base(owner) { }

        private IConfigWrapper<DamageSelfData> damageSelfData;

        public override void Awake()
        {
            damageSelfData = Ini.GetConfig<DamageSelfData>(Ini.RulesDependency, section);
            if (!damageSelfData.Data.Enable)
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void OnPut(Pointer<CoordStruct> coord, DirType dirType)
        {
            if (!pObject.IsDead() && null != damageSelfData)
            {
                // 新建一个AE
                AttachEffectData aeData = new AttachEffectData();
                aeData.Enable = true;
                aeData.DamageSelfData = damageSelfData.Data;
                pObject.GetAEManegr().Attach(aeData);
            }
        }

    }
}
