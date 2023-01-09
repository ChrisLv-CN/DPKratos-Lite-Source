using System.Data;
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

        public State<ECMData> ECMState = new State<ECMData>();

        public void InitState_ECM()
        {
            // 初始化状态机
            if (isMissile)
            {
                // 初始化状态机
                ECMData data = Ini.GetConfig<ECMData>(Ini.RulesDependency, section).Data;
                if (data.Enable)
                {
                    ECMState.Enable(data);
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} ECM {data.Enable} {data.Chance} {data.Elasticity}");
                }
            }
        }

        public void OnUpdate_ECM()
        {
            if (isMissile && ECMState.IsActive())
            {

            }
        }
    }
}
