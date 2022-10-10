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
    public partial class AttachEffectScript
    {
        public void InitEffect_DamageSelf()
        {
            DamageSelfData damageSelfData = Ini.GetConfig<DamageSelfData>(Ini.RulesDependency, section).Data;
            if (damageSelfData.Enable)
            {
                // 新建一个AE
                AttachEffectData aeData = new AttachEffectData();
                aeData.Enable = true;
                aeData.DamageSelfData = damageSelfData;
                Attach(aeData);
            }
        }

    }

}
