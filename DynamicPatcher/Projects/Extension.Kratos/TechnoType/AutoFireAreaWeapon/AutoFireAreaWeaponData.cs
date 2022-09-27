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
    public class AutoFireAreaWeaponData : INIAutoConfig
    {
        [INIField(Key = "AutoFireAreaWeapon")]
        public int WeaponIndex;

        [INIField(Key = "AutoFireAreaWeapon.InitialDelay")]
        public int InitialDelay;

        [INIField(Key = "AutoFireAreaWeapon.CheckAmmo")]
        public bool CheckAmmo;

        [INIField(Key = "AutoFireAreaWeapon.UseAmmo")]
        public bool UseAmmo;

        [INIField(Key = "AutoFireAreaWeapon.TargetToGround")]
        public bool TargetToGround;

        public AutoFireAreaWeaponData()
        {
            this.WeaponIndex = -1;
            this.InitialDelay = 0;
            this.CheckAmmo = false;
            this.UseAmmo = false;
            this.TargetToGround = false;
        }

    }


}
