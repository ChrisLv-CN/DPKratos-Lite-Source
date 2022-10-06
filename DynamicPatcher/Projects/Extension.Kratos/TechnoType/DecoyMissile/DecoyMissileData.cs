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
    public class DecoyMissileData : INIConfig
    {
        public const string TITLE = "DecoyMissile.";

        public bool Enable;
        public string Weapon;
        public string EliteWeapon;
        public CoordStruct FLH;
        public CoordStruct Velocity;
        public int Delay;
        public int Life;
        public bool AlwaysFire;

        public DecoyMissileData()
        {
            this.Enable = false;
            this.Weapon = null;
            this.EliteWeapon = null;
            this.FLH = default;
            this.Velocity = default;
            this.Delay = 4;
            this.Life = 99999;
            this.AlwaysFire = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.Weapon = reader.Get(TITLE + "Weapon", this.Weapon);
            this.EliteWeapon = reader.Get(TITLE + "EliteWeapon", this.Weapon);
            this.Enable = !Weapon.IsNullOrEmptyOrNone() || !EliteWeapon.IsNullOrEmptyOrNone();

            this.FLH = reader.Get(TITLE + "FLH", this.FLH);
            this.Velocity = reader.Get(TITLE + "Velocity", this.Velocity);
            this.Delay = reader.Get(TITLE + "Delay", this.Delay);
            this.Life = reader.Get(TITLE + "Life", this.Life);
            this.AlwaysFire = reader.Get(TITLE + "AlwaysFire", this.AlwaysFire);
        }
    }

}
