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
    public class PassengersData : INIAutoConfig
    {
        public bool OpenTopped;

        [INIField(Key = "Passengers.PassiveAcquire")]
        public bool PassiveAcquire = true;

        [INIField(Key = "Passengers.ForceFire")]
        public bool ForceFire = false;

        [INIField(Key = "Passengers.MobileFire")]
        public bool MobileFire = true;

        [INIField(Key = "Passengers.SameFire")]
        public bool SameFire = true;

    }


}
