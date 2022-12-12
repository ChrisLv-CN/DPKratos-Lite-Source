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

        public static List<string> BlackList = new List<string>();
        public static List<string> WhiteList = new List<string>();

        public bool BaseNormal; // 做基地建造节点
        public bool EligibileForAllyBuilding; // 做友军的基地建造节点

        public BaseNormalData()
        {
            this.BaseNormal = false;
            this.EligibileForAllyBuilding = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.BaseNormal = reader.Get("BaseNormal", this.EligibileForAllyBuilding);
            this.EligibileForAllyBuilding = reader.Get("EligibileForAllyBuilding", this.EligibileForAllyBuilding);
            if (EligibileForAllyBuilding)
            {
                this.BaseNormal = true;
            }
        }

        public static void Clear(object sender, EventArgs args)
        {
            WhiteList.Clear();
            BlackList.Clear();
        }

        public static bool CanBeBase(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            if (WhiteList.Contains(id))
            {
                return true;
            }
            else if (BlackList.Contains(id))
            {
                return false;
            }
            else
            {
                BaseNormalData data = Ini.GetConfig<BaseNormalData>(Ini.RulesDependency, id).Data;
                if (data.BaseNormal || data.EligibileForAllyBuilding)
                {
                    WhiteList.Add(id);
                    return true;
                }
                else
                {
                    BlackList.Add(id);
                    return false;
                }
            }
        }

    }

}
