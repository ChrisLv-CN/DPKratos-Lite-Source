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

        public State<FreezeData> FreezeState = new State<FreezeData>();

        public bool Freezing;

        private bool cantMoveFlag;

        public void InitState_Freeze()
        {
            // 初始化状态机
            FreezeData data = Ini.GetConfig<FreezeData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                FreezeState.Enable(data);
            }
        }

        public void OnUpdate_Freeze()
        {
            this.Freezing = FreezeState.IsActive();
            if (Freezing)
            {
                if (!cantMoveFlag)
                {
                    // 清除所有目标
                    pTechno.ClearAllTarget();
                    // 马上停止活动
                    if (pTechno.CastToFoot(out Pointer<FootClass> pFoot))
                    {
                        pFoot.Ref.Locomotor.ForceStopMoving();
                    }
                }
            }
            else
            {
                cantMoveFlag = false;
            }
        }

    }
}
