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
    public class TechnoTypeData : INIConfig
    {
        // Ares
        public bool AllowCloakable;

        // Phobos
        public CoordStruct TurretOffset;
        public string WarpIn;
        public string WarpOut;
        public string WarpAway;
        public int ChronoDelay;
        public int ChronoDistanceFactor;
        public Bool ChronoTrigger;
        public int ChronoMinimumDelay;
        public int ChronoRangeMinimum;

        public TechnoTypeData()
        {
            this.AllowCloakable = true;

            this.TurretOffset = default;
            this.WarpIn = null;
            this.WarpOut = null;
            this.WarpAway = null;

            this.ChronoDelay = 60;
            this.ChronoDistanceFactor = 32;
            this.ChronoTrigger = true;
            this.ChronoMinimumDelay = 0;
            this.ChronoRangeMinimum = 0;
        }

        public override void Read(IConfigReader reader)
        {
            this.AllowCloakable = reader.Get("Cloakable.Allowed", this.AllowCloakable);

            string turretOffset = reader.Get<string>("TurretOffset", null);
            if (!turretOffset.IsNullOrEmptyOrNone())
            {
                if (ExHelper.Number.IsMatch(turretOffset))
                {
                    // 只有一个数字
                    this.TurretOffset.X = Convert.ToInt32(turretOffset);
                }
                else
                {
                    // 有几个数字写几个
                    string[] pos = turretOffset.Split(',');
                    if (null != pos && pos.Length > 0)
                    {
                        for (int i = 0; i < pos.Length; i++)
                        {
                            int value = Convert.ToInt32(pos[i].Trim());
                            switch (i)
                            {
                                case 0:
                                    TurretOffset.X = value;
                                    break;
                                case 1:
                                    TurretOffset.Y = value;
                                    break;
                                case 2:
                                    TurretOffset.Z = value;
                                    break;
                            }
                        }
                    }
                }
            }
            this.WarpIn = reader.Get("WarpIn", WarpIn);
            this.WarpOut = reader.Get("WarpOut", WarpOut);
            this.WarpAway = reader.Get("WarpAway", WarpAway);
            // 在游戏开始后再访问，读取全局没问题
            RulesClass rules = RulesClass.Global();
            this.ChronoDelay = reader.Get("ChronoDelay", rules.ChronoDelay);
            this.ChronoDistanceFactor = reader.Get("ChronoDistanceFactor", rules.ChronoDistanceFactor);
            this.ChronoTrigger = reader.Get("ChronoTrigger", rules.ChronoTrigger);
            this.ChronoMinimumDelay = reader.Get("ChronoMinimumDelay", rules.ChronoMinimumDelay);
            this.ChronoRangeMinimum = reader.Get("ChronoRangeMinimum", rules.ChronoRangeMinimum);
        }

    }

}
