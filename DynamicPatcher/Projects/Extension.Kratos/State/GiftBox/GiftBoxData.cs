using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class GiftBoxData
    {
        public string[] Gifts;
        public int[] Nums;
        public double[] Chances;
        public bool RandomType;
        public int[] RandomWeights;

        public int Delay;
        public Point2D RandomDelay;


        public GiftBoxData()
        {
            this.Gifts = null;
            this.Nums = null;
            this.Chances = null;
            this.RandomType = false;
            this.RandomWeights = null;

            this.Delay = 0;
            this.RandomDelay = default;
        }

        public GiftBoxData Clone()
        {
            GiftBoxData data = new GiftBoxData();
            data.Gifts = null != this.Gifts ? (string[])this.Gifts.Clone() : null;
            data.Nums = null != this.Nums ? (int[])this.Nums.Clone() : null;
            data.Chances = null != this.Chances ? (double[])this.Chances.Clone() : null;
            data.RandomType = this.RandomType;
            data.RandomWeights = null != this.RandomWeights ? (int[])this.RandomWeights.Clone() : null;

            data.Delay = this.Delay;
            data.RandomDelay = this.RandomDelay;
            return data;
        }

        public void Read(ISectionReader reader, string title)
        {
            this.Gifts = reader.GetList(title + "Types", Gifts);
            this.Nums = reader.GetList(title + "Nums", Nums);
            this.Chances = reader.GetChanceList(title + "Chances", this.Chances);

            this.RandomType = reader.Get(title + "RandomType", RandomType);
            this.RandomWeights = reader.GetList(title + "RandomWeights", RandomWeights);

            this.Delay = reader.Get(title + "Delay", Delay);
            this.RandomDelay = reader.Get(title + "RandomDelay", RandomDelay);
        }

    }


}
