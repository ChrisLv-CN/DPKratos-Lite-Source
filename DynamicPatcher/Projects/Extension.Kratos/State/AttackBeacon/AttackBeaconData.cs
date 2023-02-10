using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    // AE
    public partial class AttachEffectData
    {
        public AttackBeaconData AttackBeaconData;

        private void ReadAttackBeaconData(IConfigReader reader)
        {
            AttackBeaconData data = new AttackBeaconData();
            data.Read(reader);
            if (data.Enable)
            {
                this.AttackBeaconData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class AttackBeaconData : EffectData, IStateData
    {
        public const string TITLE = "AttackBeacon.";

        public string[] Types;
        public int[] Nums;
        public int Rate;
        public int InitialDelay;
        public int RangeMin;
        public int RangeMax;
        public bool Close;
        public bool Force;
        public int Count;
        public bool TargetToCell;
        public AttackBeaconData()
        {
            this.Types = null;
            this.Nums = null;
            this.Rate = 0;
            this.InitialDelay = 0;
            this.RangeMin = 0;
            this.RangeMax = -1;
            this.Close = true;
            this.Force = false;
            this.Count = 1;
            this.TargetToCell = false;
            this.AffectsOwner = true;
            this.AffectsAllies = false;
            this.AffectsEnemies = false;
            this.AffectsCivilian = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Types = reader.GetList<string>(TITLE + "Types", null);
            this.Nums = reader.GetList<int>(TITLE + "Nums", null);

            this.Rate = reader.Get(TITLE + "Rate", this.Rate);
            this.InitialDelay = reader.Get(TITLE + "InitialDelay", this.InitialDelay);
            float min = reader.Get(TITLE + "RangeMin", 0.0f);
            if (min > 0)
            {
                this.RangeMin = (int)(min * 256);
            }
            float max = reader.Get(TITLE + "RangeMax", -1f);
            if (max > 0)
            {
                this.RangeMax = (int)(max * 256);
            }
            this.Close = reader.Get(TITLE + "Close", this.Close);
            this.Force = reader.Get(TITLE + "Force", this.Force);
            this.Count = reader.Get(TITLE + "Count", this.Count);
            this.TargetToCell = reader.Get(TITLE + "TargetToCell", this.TargetToCell);
        }

    }

}
