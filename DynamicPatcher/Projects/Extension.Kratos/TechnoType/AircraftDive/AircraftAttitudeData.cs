using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class AircraftAttitudeData : INIAutoConfig
    {
        [INIField(Key = "DisableAircraftAutoPitch")]
        public bool Disable;

        public AircraftAttitudeData()
        {
            this.Disable = false;
        }

    }


}
