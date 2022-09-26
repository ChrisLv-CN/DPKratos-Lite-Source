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

        private bool disableSelectable;

        public void Awake_Deselect()
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
            this.disableSelectable = DeselectState.IsActive();
            if (pTechno.Ref.Base.IsSelected && disableSelectable)
            {
                pTechno.Ref.Base.Deselect();
            }
        }

        public override void OnSelect(ref bool selectable)
        {
            selectable = !disableSelectable;
        }

    }
}
