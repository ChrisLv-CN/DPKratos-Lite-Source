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
    public class SupportSpawnsData : INIAutoConfig
    {
        [INIField(Key = "SupportSpawns")]
        public bool Enable = false;

        [INIField(Key = "SupportSpawns.Weapon")]
        public string SupportWeapon = null;

        [INIField(Key = "SupportSpawns.EliteWeapon")]
        public string EliteSupportWeapon = null;

        [INIField(Key = "SupportSpawns.SwitchFLH")]
        public bool SwitchFLH = false;

        [INIField(Key = "SupportSpawns.AlwaysFire")]
        public bool Always = false;

    }

}
