using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class WeaponTypeData
    {
        [INIField(Key = "AttachFire.UseROF")]
        public bool UseROF = true;

        [INIField(Key = "AttachFire.CheckRange")]
        public bool CheckRange = false;

        [INIField(Key = "AttachFire.RadialFire")]
        public bool RadialFire = false;

        [INIField(Key = "AttachFire.RadialAngle")]
        public int RadialAngle = 180;

        [INIField(Key = "AttachFire.RadialZ")]
        public bool RadialZ = true;

        [INIField(Key = "AttachFire.SimulateBurst")]
        public bool SimulateBurst = false;

        [INIField(Key = "AttachFire.SimulateBurstDelay")]
        public int SimulateBurstDelay = 7;

        [INIField(Key = "AttachFire.SimulateBurstMode")]
        public int SimulateBurstMode = 0;

        [INIField(Key = "AttachFire.OnlyFireInTransport")]
        public bool OnlyFireInTransport = false;

        [INIField(Key = "AttachFire.UseAlternateFLH")]
        public bool UseAlternateFLH = false;

        [INIField(Key = "AttachFire.Feedback")]
        public bool Feedback = false;

    }


}
