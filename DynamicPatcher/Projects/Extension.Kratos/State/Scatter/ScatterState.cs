using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class ScatterState : State<ScatterData>
    {
        private bool resetFlag;

        public override void OnEnable()
        {
            resetFlag = true;
        }

        public bool IsReset()
        {
            bool result = false;
            if (resetFlag)
            {
                resetFlag = false;
                result = true;
            }
            return result;
        }
    }

}
