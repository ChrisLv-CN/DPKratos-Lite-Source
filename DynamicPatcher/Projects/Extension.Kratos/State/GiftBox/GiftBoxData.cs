using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class AttachEffectData
    {
        public GiftBoxData GiftBoxData;

        private void ReadGiftBoxData(IConfigReader reader)
        {
            GiftBoxData data = new GiftBoxData();
            data.Read(reader);
            if (data.Enable)
            {
                this.GiftBoxData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class GiftBoxEntity
    {
        public string[] Gifts;
        public int[] Nums;
        public double[] Chances;
        public bool RandomType;
        public int[] RandomWeights;

        public int Delay;
        public Point2D RandomDelay;


        public GiftBoxEntity()
        {
            this.Gifts = null;
            this.Nums = null;
            this.Chances = null;
            this.RandomType = false;
            this.RandomWeights = null;

            this.Delay = 0;
            this.RandomDelay = default;
        }

        public GiftBoxEntity(string[] gifts) : this()
        {
            this.Gifts = gifts;
        }

        public GiftBoxEntity Clone()
        {
            GiftBoxEntity data = new GiftBoxEntity();
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

    [Serializable]
    public class GiftBoxData : EffectData, IStateData
    {

        public const string TITLE = "GiftBox.";

        public GiftBoxEntity Data;
        public GiftBoxEntity EliteData;
        public bool Remove;
        public bool Destroy;
        public int RandomRange;
        public bool EmptyCell;
        public bool OpenWhenDestroyed;
        public bool OpenWhenHealthPercent;
        public double OpenHealthPercent;

        public bool IsTransform;
        public bool InheritHealth;
        public double HealthPercent;
        public bool InheritTarget;
        public bool InheritExperience;
        public bool InheritAmmo;
        public bool InheritAE;
        public Mission ForceMission = Mission.None;

        public string[] AttachEffects;

        static GiftBoxData()
        {
            new MissionParser().Register();
        }

        public GiftBoxData()
        {
            this.Data = null;
            this.EliteData = null;

            this.Remove = true;
            this.Destroy = false;
            this.RandomRange = 0;
            this.EmptyCell = false;
            this.OpenWhenDestroyed = false;
            this.OpenWhenHealthPercent = false;
            this.OpenHealthPercent = 0;

            this.IsTransform = false;
            this.InheritHealth = false;
            this.HealthPercent = 1;
            this.InheritTarget = true;
            this.InheritExperience = true;
            this.InheritAmmo = false;
            this.InheritAE = false;
            this.ForceMission = Mission.None;

            this.AttachEffects = null;
        }

        public GiftBoxData(string[] gifts) : this()
        {
            this.Data = new GiftBoxEntity(gifts);
            this.EliteData = this.Data;
        }

        public override void Read(IConfigReader reader)
        {
            Read(reader, TITLE);
        }

        public override void Read(IConfigReader reader, string title)
        {
            base.Read(reader, title);

            GiftBoxEntity data = new GiftBoxEntity();
            data.Read(reader, title);
            if (null != data.Gifts && data.Gifts.Length > 0)
            {
                this.Data = data;
            }

            GiftBoxEntity elite = null != this.Data ? Data.Clone() : new GiftBoxEntity();
            elite.Read(reader, title + "Elite");
            if (null != elite.Gifts && elite.Gifts.Length > 0)
            {
                this.EliteData = elite;
            }

            this.Enable = null != this.Data || null != this.EliteData;

            // 通用设置
            this.Remove = reader.Get(title + "Remove", this.Remove);
            this.Destroy = reader.Get(title + "Explodes", this.Destroy);
            this.RandomRange = reader.Get(title + "RandomRange", this.RandomRange);
            this.EmptyCell = reader.Get(title + "RandomToEmptyCell", this.EmptyCell);

            this.OpenWhenDestroyed = reader.Get(title + "OpenWhenDestroyed", this.OpenWhenDestroyed);
            this.OpenHealthPercent = reader.GetPercent(title + "OpenWhenHealthPercent", this.OpenHealthPercent);
            this.OpenWhenHealthPercent = OpenHealthPercent > 0 && OpenHealthPercent < 1;

            this.IsTransform = reader.Get(title + "IsTransform", this.IsTransform);
            if (IsTransform)
            {
                this.Remove = true; // 释放后移除
                this.Destroy = false; // 静默删除
                this.InheritHealth = true; // 继承血量
            }

            this.HealthPercent = reader.GetPercent(title + "HealthPercent", this.HealthPercent);
            if (HealthPercent <= 0)
            {
                // 设置了0，自动，当IsTransform时，按照礼盒的比例
                this.HealthPercent = 1;
                this.InheritHealth = true;
            }
            else
            {
                // 固定比例
                this.HealthPercent = HealthPercent > 1 ? 1 : HealthPercent;
                this.InheritHealth = false;
            }

            this.InheritTarget = reader.Get(title + "InheritTarget", this.InheritTarget);
            this.InheritExperience = reader.Get(title + "InheritExp", this.InheritExperience);
            this.InheritAmmo = reader.Get(title + "InheritAmmo", this.InheritAmmo);
            this.InheritAE = reader.Get(title + "InheritAE", this.InheritAE);
            this.ForceMission = reader.Get(title + "ForceMission", Mission.None);

            this.AttachEffects = reader.GetList<string>(title + "AttachEffects", null);
        }

        public void ForTransform()
        {
            this.Remove = true;
            this.Destroy = false;
            this.OpenWhenDestroyed = false;
            this.OpenWhenHealthPercent = false;
            this.IsTransform = true;
            this.InheritHealth = true;
            this.HealthPercent = 0;
        }

    }


}
