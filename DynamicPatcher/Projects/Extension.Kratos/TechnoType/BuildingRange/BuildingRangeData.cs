using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public enum BuildingRangeMode
    {
        NONE = 0, LINE = 1, CELL = 2, SHP = 3
    }

    public class BuildingRangeModeParser : KEnumParser<BuildingRangeMode>
    {
        public override bool ParseInitials(string t, ref BuildingRangeMode buffer)
        {
            switch (t)
            {
                case "L":
                    buffer = BuildingRangeMode.LINE;
                    return true;
                case "C":
                    buffer = BuildingRangeMode.CELL;
                    return true;
                case "S":
                    buffer = BuildingRangeMode.SHP;
                    return true;
                default:
                    buffer = BuildingRangeMode.NONE;
                    return true;
            }
        }
    }

    [Serializable]
    public class BuildingRangeData : INIConfig
    {
        static BuildingRangeData()
        {
            new BuildingRangeModeParser().Register();
        }

        public static string TITLE = "BuildingRange.";

        public BuildingRangeMode Mode; // 用什么方式显示建造范围
        public ColorStruct Color; // 显示线条的颜色
        public bool Dashed; // 虚线
        public string SHPFileName; // shp文件名
        public int ZeroFrameIndex; // 平面的起始帧序号

        public BuildingRangeData()
        {
            this.Mode = BuildingRangeMode.NONE; // 不显示
            this.Color = ColorStruct.White;
            this.Dashed = true;
            this.SHPFileName = "placerange.shp";
            this.ZeroFrameIndex = 0;
        }

        public override void Read(IConfigReader reader)
        {
            // 读全局
            ISectionReader sectionReader = Ini.GetSection(Ini.RulesDependency, RulesClass.SectionAudioVisual);
            this.Mode = sectionReader.Get(TITLE + "Mode", this.Mode);
            this.Color = sectionReader.Get(TITLE + "Color", this.Color);
            this.Dashed = sectionReader.Get(TITLE + "Dashed", this.Dashed);
            this.SHPFileName = sectionReader.Get(TITLE + "SHP", this.SHPFileName);
            this.ZeroFrameIndex = sectionReader.Get(TITLE + "ZeroFrameIndex", this.ZeroFrameIndex);

            // 读个体
            this.Mode = reader.Get(TITLE + "Mode", this.Mode);
            this.Color = reader.Get(TITLE + "Color", this.Color);
            this.Dashed = reader.Get(TITLE + "Dashed", this.Dashed);
            this.SHPFileName = reader.Get(TITLE + "SHP", this.SHPFileName);
            this.ZeroFrameIndex = reader.Get(TITLE + "ZeroFrameIndex", this.ZeroFrameIndex);
        }

    }

}
