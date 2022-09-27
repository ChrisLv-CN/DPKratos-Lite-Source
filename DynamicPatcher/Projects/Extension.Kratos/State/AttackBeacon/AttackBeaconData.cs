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
        public bool AffectsOwner;
        public bool AffectsAllies;
        public bool AffectsEnemies;
        public bool AffectsCivilian;

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
            this.InitialDelay =  reader.Get(TITLE + "InitialDelay", this.InitialDelay);
            this.RangeMin =  reader.Get(TITLE + "RangeMin", this.RangeMin);
            this.RangeMax =  reader.Get(TITLE + "RangeMax", this.RangeMax);
            this.Close =  reader.Get(TITLE + "Close", this.Close);
            this.Force =  reader.Get(TITLE + "Force", this.Force);
            this.Count =  reader.Get(TITLE + "Count", this.Count);
            this.TargetToCell =  reader.Get(TITLE + "TargetToCell", this.TargetToCell);
            this.AffectsOwner =  reader.Get(TITLE + "AffectsOwner", this.AffectsOwner);
            this.AffectsAllies =  reader.Get(TITLE + "AffectsAllies", this.AffectsAllies);
            this.AffectsEnemies =  reader.Get(TITLE + "AffectsEnemies", this.AffectsEnemies);
            this.AffectsCivilian =  reader.Get(TITLE + "AffectsCivilian", this.AffectsCivilian);
        }
    }


}
