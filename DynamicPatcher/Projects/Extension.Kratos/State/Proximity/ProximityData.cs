using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class ProximityData : INIAutoConfig
    {
        [INIField(Key = "Proximity.Force")]
        public bool Force;
        [INIField(Key = "Proximity.Blade")]
        public bool Blade;
        [INIField(Key = "Proximity.Arm")]
        public int Arm;
        [INIField(Key = "Proximity.ZOffset")]
        public int ZOffset;
        [INIField(Key = "Proximity.AffectsOwner")]
        public bool AffectsOwner;
        [INIField(Key = "Proximity.AffectsAllies")]
        public bool AffectsAllies;
        [INIField(Key = "Proximity.AffectsEnemies")]
        public bool AffectsEnemies;
        [INIField(Key = "Proximity.AffectsClocked")]
        public bool AffectsClocked;

        [INIField(Key = "Proximity.Penetration")]
        public bool Penetration;
        [INIField(Key = "Proximity.PenetrationWarhead")]
        public string PenetrationWarhead;
        [INIField(Key = "Proximity.PenetrationWeapon")]
        public string PenetrationWeapon;
        [INIField(Key = "Proximity.PenetrationTimes")]
        public int PenetrationTimes;
        [INIField(Key = "Proximity.PenetrationBuildingOnce")]
        public bool PenetrationBuildingOnce;

        public ProximityData()
        {
            this.Force = false;
            this.Blade = false;
            this.Arm = 128;
            this.ZOffset = Game.LevelHeight;
            this.AffectsOwner = false;
            this.AffectsAllies = false;
            this.AffectsEnemies = true;
            this.AffectsClocked = true;

            this.Penetration = false;
            this.PenetrationWarhead = null;
            this.PenetrationWeapon = null;
            this.PenetrationTimes = -1;
            this.PenetrationBuildingOnce = false;
        }
    }

}
