using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class BulletStatusScript
    {

        public DestroySelfState DestroySelfState = new DestroySelfState();

        public void InitState_DestroySelf()
        {
            // 初始化状态机
            DestroySelfData destroySelfData = Ini.GetConfig<DestroySelfData>(Ini.RulesDependency, section).Data;
            if (destroySelfData.Enable)
            {
                DestroySelfState.Enable(destroySelfData);
            }
        }

        public void OnUpdate_DestroySelf()
        {
            if (DestroySelfState.AmIDead())
            {
                BulletDamageData bulletDamage = new BulletDamageData(1);
                bulletDamage.Eliminate = true;
                bulletDamage.Harmless = DestroySelfState.Data.Peaceful;
                // Logger.Log("抛射体[{0}]{1}自毁倒计时结束，自毁开始{2}", OwnerObject.Ref.Type.Ref.Base.Base.ID, OwnerObject, bulletDamageStatus);
                TakeDamage(bulletDamage, true);
            }
        }

    }
}
