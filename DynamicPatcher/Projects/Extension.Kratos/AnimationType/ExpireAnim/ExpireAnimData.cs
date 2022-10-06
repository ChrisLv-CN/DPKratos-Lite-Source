using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class ExpireAnimData : INIConfig
    {
        public string ExpireAnimOnWater;

        public ExpireAnimData()
        {
            this.ExpireAnimOnWater = null;
        }

        public override void Read(IConfigReader reader)
        {
            this.ExpireAnimOnWater = reader.Get("ExpireAnimOnWater", this.ExpireAnimOnWater);
        }

    }


}
