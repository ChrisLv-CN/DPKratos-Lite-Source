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
    public class SupportSpawnsFLHData : INIAutoConfig
    {
        public CoordStruct SupportWeaponFLH = default;
        public CoordStruct EliteSupportWeaponFLH = default;

        public CoordStruct SupportWeaponHitFLH = default;
        public CoordStruct EliteSupportWeaponHitFLH = default;

    }

}
