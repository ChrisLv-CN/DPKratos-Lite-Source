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
    public partial class WeaponTypeData : INIAutoConfig
    {
        // YR
        // Ares
        public int Ammo = 1;
        public int LaserThickness = 0;
        public bool LaserFade = false;
        public bool IsSupported = false;

        // Kratos
        public float RockerPitch = 0;
        public bool SelfLaunch = false;

    }


}
