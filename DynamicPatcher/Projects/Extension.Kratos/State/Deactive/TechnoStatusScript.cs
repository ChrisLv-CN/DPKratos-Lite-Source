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

        public State<DeactiveData> DeactiveState = new State<DeactiveData>();

        public bool CantMove;

        public void InitState_Deactive()
        {
            // 初始化状态机
            DeactiveData data = Ini.GetConfig<DeactiveData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                DeactiveState.Enable(data);
            }
        }

        public void OnUpdate_Deactive()
        {
            this.CantMove = DeactiveState.IsActive();
        }

    }
}
