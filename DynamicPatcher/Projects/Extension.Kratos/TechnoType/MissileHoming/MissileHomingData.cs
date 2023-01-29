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
    public class MissileHomingData : INIAutoConfig
    {
        [INIField(Key = "Missile.Homing")]
        public bool Homing = false;

        [INIField(Key = "Missile.FacingTarget")]
        public bool FacingTarget = false;
    }


}
