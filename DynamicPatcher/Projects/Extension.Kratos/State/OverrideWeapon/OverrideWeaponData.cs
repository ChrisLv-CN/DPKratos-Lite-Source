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
    public class OverrideWeapon
    {

        public string[] Types; // 替换武器序号
        public bool RandomType;
        public int[] Weights;
        public int Index; // 替换武器序号
        public double Chance; // 概率

        public OverrideWeapon()
        {
            this.Types = null;
            this.RandomType = false;
            this.Weights = null;
            this.Index = -1;
            this.Chance = 1;
        }

        public OverrideWeapon Clone()
        {
            OverrideWeapon data = new OverrideWeapon();
            data.Types = null != this.Types ? (string[])this.Types.Clone() : null;
            data.RandomType = this.RandomType;
            data.Weights = null != this.Weights ? (int[])this.Weights.Clone() : null;
            data.Index = this.Index;
            data.Chance = this.Chance;
            return data;
        }

        public void Read(ISectionReader reader, string title)
        {
            this.Types = reader.GetList(title + "Types", this.Types);
            this.RandomType = null != Types && Types.Length > 0;
            this.Weights = reader.GetList(title + "Weights", this.Weights);
            this.Index = reader.Get(title + "Index", this.Index);
            this.Chance = reader.GetChance(title + "Chance", this.Chance);
        }

    }

    [Serializable]
    public class OverrideWeaponData : EffectData, IStateData
    {
        public const string TITLE = "OverrideWeapon.";

        public OverrideWeapon Data;
        public OverrideWeapon EliteData;

        public OverrideWeaponData()
        {
            this.Data = null;
            this.EliteData = null;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            OverrideWeapon data = new OverrideWeapon();
            data.Read(reader, TITLE);
            if (null != data.Types && data.Types.Length > 0)
            {
                this.Data = data;
            }

            OverrideWeapon elite = null != this.Data ? Data.Clone() : new OverrideWeapon();
            elite.Read(reader, TITLE + "Elite");
            if (null != elite.Types && elite.Types.Length > 0)
            {
                this.EliteData = elite;
            }

            this.Enable = null != this.Data || null != this.EliteData;

        }

    }

}
