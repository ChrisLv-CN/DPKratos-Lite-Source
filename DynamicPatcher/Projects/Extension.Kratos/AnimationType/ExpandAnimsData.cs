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
    public class ExpandAnimsData : INIConfig
    {
        public string[] Anims;
        public int[] Nums;
        public double[] Chances;
        public bool RandomType;
        public int[] RandomWeights;

        public CoordStruct Offset;
        public bool UseRandomOffset;
        public Point2D RandomOffsetF;
        public Point2D RandomOffsetL;
        public Point2D RandomOffsetH;

        public ExpandAnimsData()
        {
            this.Anims = null;
            this.Nums = null;
            this.Chances = null;
            this.RandomType = false;
            this.RandomWeights = null;

            this.Offset = default;
            this.UseRandomOffset = false;
            this.RandomOffsetF = default;
            this.RandomOffsetL = default;
            this.RandomOffsetH = default;
        }

        public override void Read(IConfigReader reader) { }

        public void Read(IConfigReader reader, string title)
        {
            this.Anims = reader.GetList(title + "Anims", Anims);
            this.Nums = reader.GetList(title + "Nums", Nums);
            this.Chances = reader.GetChanceList(title + "Chances", this.Chances);

            this.RandomType = reader.Get(title + "RandomType", RandomType);
            this.RandomWeights = reader.GetList(title + "RandomWeights", RandomWeights);

            this.Offset = reader.Get(title + "Offset", this.Offset);
            this.RandomOffsetF = reader.Get(title + "RandomOffsetF", this.RandomOffsetF);
            this.RandomOffsetL = reader.Get(title + "RandomOffsetL", this.RandomOffsetL);
            this.RandomOffsetH = reader.Get(title + "RandomOffsetH", this.RandomOffsetH);

            this.UseRandomOffset = default != RandomOffsetF || default != RandomOffsetL || default != RandomOffsetH;
        }

        public CoordStruct GetOffset()
        {
            if (UseRandomOffset)
            {
                int f = RandomOffsetF.GetRandomValue(0);
                int l = RandomOffsetL.GetRandomValue(0);
                int h = RandomOffsetH.GetRandomValue(0);
                return new CoordStruct(f, l, h);
            }
            return Offset;
        }

    }


}
