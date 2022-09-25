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
    public enum TrailMode
    {
        NONE = 0, LASER = 1, ELECTIRIC = 2, BEAM = 3, PARTICLE = 4, ANIM = 5
    }
    public class TrailModeParser : KEnumParser<TrailMode>
    {
        public override bool ParseInitials(string t, ref TrailMode buffer)
        {
            switch (t)
            {
                case "L":
                    buffer = TrailMode.LASER;
                    return true;
                case "E":
                    buffer = TrailMode.ELECTIRIC;
                    return true;
                case "B":
                    buffer = TrailMode.BEAM;
                    return true;
                case "P":
                    buffer = TrailMode.PARTICLE;
                    return true;
                case "A":
                    buffer = TrailMode.ANIM;
                    return true;
            }
            return false;
        }

    }

    public partial class TrailType : INIConfig
    {

        public TrailMode Mode;
        public int Distance;
        public bool IgnoreVertical;
        public int InitialDelay;

        static TrailType()
        {
            new TrailModeParser().Register();
        }

        public TrailType()
        {
            this.Mode = TrailMode.LASER;
            this.Distance = 64;
            this.IgnoreVertical = false;
            this.InitialDelay = 0;

            // 初始化具体效果
            InitAnimType();
            InitBeamType();
            InitElectricType();
            InitLaserType();
            InitParticleType();
        }

        public override void Read(IConfigReader reader)
        {
            this.Mode = reader.Get("Mode", Mode);
            this.Distance = reader.Get("Distance", Distance);
            this.IgnoreVertical = reader.Get("IgnoreVertical", IgnoreVertical);
            this.InitialDelay = reader.Get("InitialDelay", InitialDelay);

            ReadAnimType(reader);
            ReadBeamType(reader);
            ReadElectricType(reader);
            ReadLaserType(reader);
            ReadParticleType(reader);
        }

        public override string ToString()
        {
            return $"{{\"Mode\":{Mode}, \"Distance\":{Distance}, \"InitialDelay\":{InitialDelay}, \"IgnoreVertical\":{IgnoreVertical}}}";
        }

    }
}

