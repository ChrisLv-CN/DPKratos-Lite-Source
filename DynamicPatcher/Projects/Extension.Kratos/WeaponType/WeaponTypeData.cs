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
    public partial class WeaponTypeData : INIConfig
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

        public override void Read(IConfigReader reader)
        {
            ReadAttachFireData(reader);

            this.Ammo = reader.Get("Ammo", this.Ammo);

            this.LaserThickness = reader.Get("LaserThickness", this.LaserThickness);
            this.LaserFade = reader.Get("LaserFade", this.LaserFade);
            this.IsSupported = reader.Get("IsSupported", this.IsSupported);

            this.RockerPitch = reader.Get("RockerPitch", this.RockerPitch);
            this.SelfLaunch = reader.Get("SelfLaunch", this.SelfLaunch);
        }

    }


}
