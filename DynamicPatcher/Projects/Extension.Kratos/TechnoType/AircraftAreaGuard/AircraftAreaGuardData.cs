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

        public bool Enable;

        public bool AreaGuard; // 按Ctrl+Alt巡航
        public bool AutoGuard; // 移动巡航
        public bool DefaultToGuard; // 自动起飞
        public int GuardRange;
        public bool AutoFire;
        public int MaxAmmo;
        public int MinAmmo;
        public int GuardRadius;
        public bool FindRangeAroundSelf;
        public int ChaseRange;
        public bool Clockwise;
        public bool Randomwise;

        public AircraftAreaGuardData()
        {
            this.Enable = false;

            this.AreaGuard = false;
            this.AutoGuard = false;
            this.DefaultToGuard = false;
            this.GuardRange = 5;
            this.AutoFire = true;
            this.MaxAmmo = 1;
            this.MinAmmo = 0;
            this.GuardRadius = 5;
            this.FindRangeAroundSelf = false;
            this.ChaseRange = 30;
            this.Clockwise = false;
            this.Randomwise = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.AreaGuard = reader.Get(TITLE + "AreaGuard", this.AreaGuard);
            this.AutoGuard = reader.Get(TITLE + "AutoGuard", this.AutoGuard);
            this.DefaultToGuard = reader.Get(TITLE + "DefaultToGuard", this.DefaultToGuard);

            this.Enable = AreaGuard || AutoGuard || DefaultToGuard;

            this.GuardRange = reader.Get(TITLE + "GuardRange", this.GuardRange);
            this.AutoFire = reader.Get(TITLE + "AutoFire", this.AutoFire);
            this.MaxAmmo = reader.Get(TITLE + "Ammo", this.MaxAmmo);
            this.MaxAmmo = reader.Get(TITLE + "HoldAmmo", this.MaxAmmo);
            this.GuardRadius = reader.Get(TITLE + "GuardRadius", this.GuardRadius);
            this.FindRangeAroundSelf = reader.Get(TITLE + "FindRangeAroundSelf", this.FindRangeAroundSelf);
            this.ChaseRange = reader.Get(TITLE + "ChaseRange", this.ChaseRange);
            this.Clockwise = reader.Get(TITLE + "Clockwise", this.Clockwise);
            this.Randomwise = reader.Get(TITLE + "Randomwise", this.Randomwise);
        }
    }

}
