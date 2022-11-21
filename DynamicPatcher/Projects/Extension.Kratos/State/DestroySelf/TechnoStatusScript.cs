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

    public partial class TechnoStatusScript
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
                // 啊我死了
                if (DestroySelfState.Data.Peaceful)
                {
                    // Logger.Log($"{Game.CurrentFrame} - [{section}]{pTechno} 阿伟死了，直接移除");
                    pTechno.Ref.Base.Remove();
                    pTechno.Ref.Base.Health = 0;
                    pTechno.Ref.Base.UnInit();
                }
                else
                {
                    if (pTechno.TryGetComponent<DamageTextScript>(out DamageTextScript damageText))
                    {
                        damageText.SkipDamageText = true;
                    }
                    // Logger.Log($"{Game.CurrentFrame} - [{section}]{pTechno} 阿伟死了，炸了他");
                    pTechno.Ref.Base.TakeDamage(pTechno.Ref.Base.Health + 1, pTechno.Ref.Type.Ref.Crewed);
                    // pTechno.Ref.Base.Destroy();
                }
            }
        }

        public void OnWarpUpdate_DestroySelf_Stand()
        {
            if (AmIStand())
            {
                // Logger.Log($"{Game.CurrentFrame} - [{section}]{pTechno} 可以去死了吗？{DestroySelfState.AmIDead()}");
                OnUpdate_DestroySelf();
            }
        }

    }
}
