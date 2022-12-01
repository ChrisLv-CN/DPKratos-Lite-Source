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
    public class BounceLandData : INIConfig
    {
        public const string TITLE = "Bounce.";

        public double Elasticity; // 弹性削减比例

        public BounceLandData()
        {
            this.Elasticity = 1;
        }

        public override void Read(IConfigReader reader)
        {
            this.Elasticity = reader.GetPercent(TITLE + "Elasticity", this.Elasticity);
        }
    }

}
