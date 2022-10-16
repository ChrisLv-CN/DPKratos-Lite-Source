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

        public ExpandAnimsData()
        {
            this.Anims = null;
            this.Nums = null;
            this.Chances = null;
            this.RandomType = false;
            this.RandomWeights = null;
        }

        public override void Read(IConfigReader reader) { }

        public void Read(IConfigReader reader, string title)
        {
            this.Anims = reader.GetList(title + "Anims", Anims);
            this.Nums = reader.GetList(title + "Nums", Nums);
            this.Chances = reader.GetChanceList(title + "Chances", this.Chances);

            this.RandomType = reader.Get(title + "RandomType", RandomType);
            this.RandomWeights = reader.GetList(title + "RandomWeights", RandomWeights);
        }

    }


}
