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
    public class FighterAreaGuardData : INIConfig
    {
        public const string TITLE = "Fighter.";

        public bool AreaGuard;
        public int GuardRange;
        public bool AutoFire;
        public int MaxAmmo;

        public FighterAreaGuardData()
        {
            AreaGuard = false;
            GuardRange = 5;
            AutoFire = false;
            MaxAmmo = 1;
        }

        public override void Read(IConfigReader reader)
        {
            this.AreaGuard = reader.Get(TITLE + "AreaGuard", this.AreaGuard);
            this.GuardRange = reader.Get(TITLE + "GuardRange", this.GuardRange);
            this.AutoFire = reader.Get(TITLE + "AutoFire", this.AutoFire);
            this.MaxAmmo = reader.Get(TITLE + "Ammo", this.MaxAmmo);
        }
    }

}
