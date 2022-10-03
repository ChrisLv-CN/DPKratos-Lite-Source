using System;
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
    public partial class AttachEffectTypeData : INIAutoConfig
    {
        public string[] AttachEffectTypes;

        public int StandTrainCabinLength; // 持续时间


        public AttachEffectTypeData()
        {
            this.AttachEffectTypes = null;
            this.StandTrainCabinLength = 512;
        }

    }

}
