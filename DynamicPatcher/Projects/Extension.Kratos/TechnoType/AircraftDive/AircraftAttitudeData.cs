using System.Runtime.CompilerServices;
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
        public bool Disable;

        public int SpawnTakeoffDir;
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
            ISectionReader avReader = Ini.GetSection(Ini.RulesDependency, RulesClass.SectionAudioVisual);
            this.Disable = avReader.Get("DisableAircraftAutoPitch", this.Disable);

            this.SpawnTakeoffDir = avReader.GetDir16("SpawnTakingOffDir", this.SpawnTakeoffDir);
            this.SpawnLandDir = SpawnTakeoffDir;
            this.SpawnLandDir = avReader.GetDir16("SpawnLandingDir", this.SpawnLandDir);

            // 单位设置
            this.Disable = reader.Get("DisableAircraftAutoPitch", this.Disable);

            this.SpawnTakeoffDir = reader.GetDir16("SpawnTakingOffDir", this.SpawnTakeoffDir);
            this.SpawnLandDir = reader.GetDir16("SpawnLandingDir", this.SpawnLandDir);
        }

    }


}
