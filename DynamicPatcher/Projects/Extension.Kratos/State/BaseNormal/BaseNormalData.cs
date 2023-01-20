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
    public class BaseNormalData : INIConfig
    {

        public bool BaseNormal; // 做基地建造节点
        public bool EligibileForAllyBuilding; // 做友军的基地建造节点

        public BaseNormalData()
        {
            this.BaseNormal = false;
            this.EligibileForAllyBuilding = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.EligibileForAllyBuilding = reader.Get("EligibileForAllyBuilding", this.EligibileForAllyBuilding);
            this.BaseNormal = reader.Get("BaseNormal", this.EligibileForAllyBuilding);
        }

    }

}
