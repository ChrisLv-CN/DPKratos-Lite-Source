using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class WeaponTypeData
    {
        public bool UseROF = true;
        public bool CheckRange = false;
        public bool CheckAA = false;
        public bool CheckAG = false;
        public bool CheckVersus = true;

        public bool RadialFire = false;
        public int RadialAngle = 180;

        public bool RadialZ = true;

        public bool SimulateBurst = false;
        public int SimulateBurstDelay = 7;
        public int SimulateBurstMode = 0;

        public bool OnlyFireInTransport = false;
        public bool UseAlternateFLH = false;

        public bool Feedback = false;

        public bool AffectsOwner = true;
        public bool AffectsAllies = true;
        public bool AffectsEnemies = true;
        public bool AffectsCivilian = true;

        public void ReadAttachFireData(ISectionReader reader)
        {
            string title = "AttachFire.";

            this.UseROF = reader.Get(title + "UseROF", this.UseROF);
            this.CheckRange = reader.Get(title + "CheckRange", this.CheckRange);
            this.CheckAA = reader.Get(title + "CheckAA", this.CheckAA);
            this.CheckAG = reader.Get(title + "CheckAG", this.CheckAG);
            this.CheckVersus = reader.Get(title + "CheckVersus", this.CheckVersus);

            this.RadialFire = reader.Get(title + "RadialFire", this.RadialFire);
            this.RadialAngle = reader.Get(title + "RadialAngle", this.RadialAngle);

            this.RadialZ = reader.Get(title + "RadialZ", this.RadialZ);

            this.SimulateBurst = reader.Get(title + "SimulateBurst", this.SimulateBurst);
            this.SimulateBurstDelay = reader.Get(title + "SimulateBurstDelay", this.SimulateBurstDelay);
            this.SimulateBurstMode = reader.Get(title + "SimulateBurstMode", this.SimulateBurstMode);

            this.OnlyFireInTransport = reader.Get(title + "OnlyFireInTransport", this.OnlyFireInTransport);
            this.UseAlternateFLH = reader.Get(title + "UseAlternateFLH", this.UseAlternateFLH);

            this.Feedback = reader.Get(title + "Feedback", this.Feedback);

            this.AffectsAllies = reader.Get(title + "AffectsAllies", this.AffectsAllies);
            this.AffectsOwner = reader.Get(title + "AffectsOwner", this.AffectsAllies);
            this.AffectsEnemies = reader.Get(title + "AffectsEnemies", this.AffectsEnemies);
            this.AffectsCivilian = reader.Get(title + "AffectsCivilian", this.AffectsCivilian);
        }

    }


}
