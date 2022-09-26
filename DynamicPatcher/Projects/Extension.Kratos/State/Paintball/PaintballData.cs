using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class PaintballData : EffectData, IStateData
    {
        public const string TITLE = "Paintball.";

        public ColorStruct Color; // 颜色
        public bool IsHouseColor; // 使用所属色
        public float BrightMultiplier; // 亮度系数

        public PaintballData()
        {
            this.Color = default;
            this.IsHouseColor = false;
            this.BrightMultiplier = 1.0f;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Color = reader.Get(TITLE + "Color", this.Color);

            this.IsHouseColor = reader.Get(TITLE + "IsHouseColor", this.IsHouseColor);
            this.BrightMultiplier = reader.Get(TITLE + "BrightMultiplier", this.BrightMultiplier);
            if (BrightMultiplier < 0.0f)
            {
                BrightMultiplier = 0.0f;
            }
            else if (BrightMultiplier > 2.0f)
            {
                BrightMultiplier = 2.0f;
            }
            this.Enable = this.Color != default || this.BrightMultiplier != 1.0f;
        }

    }


}
