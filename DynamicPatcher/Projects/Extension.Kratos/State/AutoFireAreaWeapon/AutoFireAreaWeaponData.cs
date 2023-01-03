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
    public class AutoFireAreaWeaponData : INIConfig
    {
        private const string TITLE = "AutoFireAreaWeapon.";

        public bool Enable;

        public int WeaponIndex;
        public int InitialDelay;
        public bool CheckAmmo;
        public bool UseAmmo;
        public bool TargetToGround;

        public AutoFireAreaWeaponData()
        {
            this.Enable = false;

            this.WeaponIndex = -1;
            this.InitialDelay = 0;
            this.CheckAmmo = false;
            this.UseAmmo = false;
            this.TargetToGround = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.WeaponIndex = reader.Get("AutoFireAreaWeapon", this.WeaponIndex);
            this.Enable = WeaponIndex > -1 && WeaponIndex < 2;

            this.InitialDelay = reader.Get(TITLE + "InitialDelay", this.InitialDelay);
            this.CheckAmmo = reader.Get(TITLE + "CheckAmmo", this.CheckAmmo);
            this.UseAmmo = reader.Get(TITLE + "UseAmmo", this.UseAmmo);
            this.TargetToGround = reader.Get(TITLE + "TargetToGround", this.TargetToGround);
        }
    }


}
