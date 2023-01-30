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
    public class SupportSpawnsFLHData : INIConfig
    {
        public CoordStruct SupportWeaponFLH = default;
        public CoordStruct EliteSupportWeaponFLH = default;

        public CoordStruct SupportWeaponHitFLH = default;
        public CoordStruct EliteSupportWeaponHitFLH = default;

        public SupportSpawnsFLHData()
        {
            this.SupportWeaponFLH = default;
            this.EliteSupportWeaponFLH = default;

            this.SupportWeaponHitFLH = default;
            this.EliteSupportWeaponHitFLH = default;
        }

        public override void Read(IConfigReader reader)
        {
            this.SupportWeaponFLH = reader.Get("SupportWeaponFLH", this.SupportWeaponFLH);
            this.EliteSupportWeaponFLH = reader.Get("EliteSupportWeaponFLH", this.SupportWeaponFLH);

            this.SupportWeaponHitFLH = reader.Get("SupportWeaponHitFLH", this.SupportWeaponHitFLH);
            this.EliteSupportWeaponHitFLH = reader.Get("EliteSupportWeaponHitFLH", this.SupportWeaponHitFLH);
        }

    }

}
