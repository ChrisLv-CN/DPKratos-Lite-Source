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
    public class AircraftPutData : INIConfig
    {

        public string[] PadAircraftTypes;

        public CoordStruct NoHelipadPutOffset;
        public bool ForcePutOffset;

        public AircraftPutData()
        {
            this.PadAircraftTypes = null;

            this.NoHelipadPutOffset = default;
            this.ForcePutOffset = false;
        }

        public override void Read(IConfigReader reader)
        {
            // 全局设置
            ISectionReader generalReader = Ini.GetSection(Ini.RulesDependency, RulesClass.SectionGeneral);
            this.PadAircraftTypes = generalReader.GetList("PadAircraft", this.PadAircraftTypes);

            this.NoHelipadPutOffset = generalReader.Get("AircraftNoHelipadPutOffset", this.NoHelipadPutOffset);
            if (default != NoHelipadPutOffset)
            {
                this.NoHelipadPutOffset *= 256;
            }
            this.ForcePutOffset = generalReader.Get("AircraftForcePutOffset", this.ForcePutOffset);

            // 单位设置
            CoordStruct offset = reader.Get<CoordStruct>("NoHelipadPutOffset", default);
            if (default != offset)
            {
                this.NoHelipadPutOffset = offset * 256;
            }
            this.ForcePutOffset = reader.Get("ForcePutOffset", this.ForcePutOffset);
        }

    }


}
