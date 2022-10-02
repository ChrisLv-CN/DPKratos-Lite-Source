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

    public partial class AnimStatusScript
    {
        public AnimDamageData AnimDamageData => Ini.GetConfig<AnimDamageData>(Ini.ArtDependency, section).Data;

        private SwizzleablePointer<TechnoClass> creater = new SwizzleablePointer<TechnoClass>(IntPtr.Zero);
        private bool createrIsDeadth = false;

        public void OnUpdate_Damage()
        {
            // 断言第一次执行update时，所属已被设置
            if (!initInvisibleFlag)
            {
                // 初始化
                AnimVisibilityData data = Ini.GetConfig<AnimVisibilityData>(Ini.ArtDependency, section).Data;
                UpdateVisibility(data.Visibility);
            }
        }

        public void SetCreater(Pointer<TechnoClass> pTechno)
        {
            if (AnimDamageData.KillByCreater)
            {
                creater.Pointer = pTechno;
            }
        }

    }
}
