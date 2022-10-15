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
    public class AircraftAreaGuardData : INIConfig
    {
        public const string TITLE = "Fighter.";

        public bool AreaGuard;
        public int GuardRange;
        public bool AutoFire;
        public int MaxAmmo;
        public int GuardRadius;
        public bool FindRangeAroundSelf;
        public int ChaseRange;
        public bool Clockwise;
        public bool Randomwise;

        public AircraftAreaGuardData()
        {
            this.AreaGuard = false;
            this.GuardRange = 5;
            this.AutoFire = false;
            this.MaxAmmo = 1;
            this.GuardRadius = 5;
            this.FindRangeAroundSelf = false;
            this.ChaseRange = 30;
            this.Clockwise = false;
            this.Randomwise = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.AreaGuard = reader.Get(TITLE + "AreaGuard", this.AreaGuard);
            this.GuardRange = reader.Get(TITLE + "GuardRange", this.GuardRange);
            this.AutoFire = reader.Get(TITLE + "AutoFire", this.AutoFire);
            this.MaxAmmo = reader.Get(TITLE + "Ammo", this.MaxAmmo);
            this.GuardRadius = reader.Get(TITLE + "GuardRadius", this.GuardRadius);
            this.FindRangeAroundSelf = reader.Get(TITLE + "FindRangeAroundSelf", this.FindRangeAroundSelf);
            this.ChaseRange = reader.Get(TITLE + "ChaseRange", this.ChaseRange);
            this.Clockwise = reader.Get(TITLE + "Clockwise", this.Clockwise);
            this.Randomwise = reader.Get(TITLE + "Randomwise", this.Randomwise);
        }
    }

}
