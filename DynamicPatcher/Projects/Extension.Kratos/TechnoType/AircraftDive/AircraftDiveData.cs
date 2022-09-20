using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class AircraftDiveData : INIAutoConfig
    {
        [INIField(Key = "Dive")]
        public bool Enable;

        [INIField(Key = "Dive.Distance")]
        public float Distance;

        [INIField(Key = "Dive.FlightLevel")]
        public int FlightLevel;

        [INIField(Key = "Dive.PullUpAfterFire")]
        public bool PullUpAfterFire;

        public AircraftDiveData()
        {
            this.Enable = false;
            this.Distance = 10;
            this.FlightLevel = 500;
            this.PullUpAfterFire = false;
        }

        public override string ToString()
        {
            return string.Format("{{\"Enable\":{0}, \"Distance\":{1}, \"FlightLevel\":{4}}}", Enable, Distance, FlightLevel);
        }

    }


}
