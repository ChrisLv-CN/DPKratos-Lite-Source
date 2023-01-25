using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class AttachEffectData
    {
        public PumpData PumpData;

        private void ReadPumpData(IConfigReader reader)
        {
            PumpData data = new PumpData();
            data.Read(reader);
            if (data.Enable)
            {
                this.PumpData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class PumpData : EffectData, IStateData
    {
        public const string TITLE = "Pump.";

        public int Range; // 飞多远，距离
        public int Gravity; // 重力
        public bool PowerBySelf; // 由自身提供动力，来源只控制方向

        public bool Lobber; // 高抛
        public bool Inaccurate; // 不精准
        public float ScatterMin; // 散布范围
        public float ScatterMax; // 散布范围


        public PumpData()
        {
            this.Range = 0;
            this.Gravity = 0;
            this.PowerBySelf = false;

            this.Lobber = false;
            this.Inaccurate = false;
            this.ScatterMin = 0f;
            this.ScatterMax = 0f;

            this.AffectBuilding = false;
            this.AffectBullet = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Range = reader.Get(TITLE + "Range", this.Range);
            this.Enable = Range > 0;

            this.Gravity = reader.Get(TITLE + "Gravity", RulesClass.Global().Gravity);
            this.PowerBySelf = reader.Get(TITLE + "PowerBySelf", this.PowerBySelf);

            this.Lobber = reader.Get(TITLE + "Lobber", this.Lobber);
            this.Inaccurate = reader.Get(TITLE + "Inaccurate", this.Inaccurate);
            this.ScatterMin = reader.Get(TITLE + "ScatterMin", this.ScatterMin);
            this.ScatterMax = reader.Get(TITLE + "ScatterMax", this.ScatterMax);
        }

    }


}
