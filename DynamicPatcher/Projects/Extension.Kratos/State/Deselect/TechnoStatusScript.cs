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

        public State<DeselectData> DeselectState = new State<DeselectData>();

        public bool Disappear;
        private bool disableSelectable;

        public void InitState_Deselect()
        {
            // 初始化状态机
            DeselectData data = Ini.GetConfig<DeselectData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                DeselectState.Enable(data);
            }
        }

        public void OnUpdate_Deselect()
        {
            if (this.disableSelectable = DeselectState.IsActive())
            {
                Disappear = DeselectState.Data.Disappear;
            }
            else
            {
                Disappear = false;
            }
            if (pTechno.Ref.Base.IsSelected && disableSelectable)
            {
                pTechno.Ref.Base.Deselect();
            }
        }

        public bool OnSelect_Deselect()
        {
            return !disableSelectable;
        }

    }
}
