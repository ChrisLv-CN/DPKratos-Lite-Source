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
    public class AircraftAttitudeData : INIConfig
    {
        [INIField(Key = "DisableAircraftAutoPitch")]
        public bool Disable;

        [INIField(Key = "SpawnTakingOffDir")]
        public int SpawnTakeoffDir;
        [INIField(Key = "SpawnLandingDir")]
        public int SpawnLandDir;

        public AircraftAttitudeData()
        {
            this.Disable = false;

            this.SpawnTakeoffDir = 0;
            this.SpawnLandDir = SpawnTakeoffDir;
        }

        public override void Read(IConfigReader reader)
        {
            // 全局设置
            ISectionReader avReader = Ini.GetSection(Ini.RulesDependency, RulesExt.SectionAudioVisual);
            this.Disable = avReader.Get("DisableAircraftAutoPitch", this.Disable);

            ReadDir("SpawnTakingOffDir", avReader, ref this.SpawnTakeoffDir);
            this.SpawnLandDir = SpawnTakeoffDir;
            ReadDir("SpawnLandingDir", avReader, ref this.SpawnLandDir);

            // 单位设置
            this.Disable = reader.Get("DisableAircraftAutoPitch", this.Disable);

            ReadDir("SpawnTakingOffDir", reader, ref this.SpawnTakeoffDir);
            ReadDir("SpawnLandingDir", reader, ref this.SpawnLandDir);
        }

        private int ReadDir(string key, ISectionReader reader, ref int field)
        {
            string dir = reader.Get(key, "N");
            if (Enum.IsDefined(typeof(Direction), dir))
            {
                Direction direction = (Direction)Enum.Parse(typeof(Direction), dir, true);
                field = (int)direction * 2;
            }
            else if (ExHelper.Number.IsMatch(dir))
            {
                field = Convert.ToInt32(dir);
            }
            return field;
        }

    }


}
