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
        public bool ForceTransform = false;

        public GiftBoxEntity Data;
        public GiftBoxEntity EliteData;
        public bool Remove;
        public bool Destroy;
        public bool RealCoords;
        public int RandomRange;
        public bool EmptyCell;
        public bool OpenWhenDestroyed;
        public bool OpenWhenHealthPercent;
        public double OpenHealthPercent;

        public string[] OnlyOpenWhenMarks;

        public bool IsTransform;
        public bool InheritHealth;
        public double HealthPercent;
        public int HealthNumber;
        public bool InheritHealthNumber;
        public bool InheritTarget;
        public bool InheritExperience;
        public bool InheritPassenger;
        public bool InheritROF;
        public bool InheritAmmo;
        public bool InheritAE;
        public Mission ForceMission = Mission.None;

        public string[] RemoveEffects;
        public string[] AttachEffects;
        public double[] AttachChances;

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
            this.RealCoords = false;
            this.RandomRange = 0;
            this.EmptyCell = false;
            this.OpenWhenDestroyed = false;
            this.OpenWhenHealthPercent = false;
            this.OpenHealthPercent = 0;

            this.OnlyOpenWhenMarks = null;

            this.IsTransform = false;
            this.InheritHealth = false;
            this.HealthPercent = 1;
            this.HealthNumber = 0;
            this.InheritTarget = true;
            this.InheritExperience = true;
            this.InheritPassenger = false;
            this.InheritROF = false;
            this.InheritAmmo = false;
            this.InheritAE = false;
            this.ForceMission = Mission.None;

            this.RemoveEffects = null;
            this.AttachEffects = null;
            this.AttachChances = null;
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

        public override void Read(ISectionReader reader, string title)
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
            this.RealCoords = reader.Get(title + "RealCoords", this.RealCoords);
            this.RandomRange = reader.Get(title + "RandomRange", this.RandomRange);
            this.EmptyCell = reader.Get(title + "RandomToEmptyCell", this.EmptyCell);

            this.OpenWhenDestroyed = reader.Get(title + "OpenWhenDestroyed", this.OpenWhenDestroyed);
            this.OpenHealthPercent = reader.GetPercent(title + "OpenWhenHealthPercent", this.OpenHealthPercent);
            this.OpenWhenHealthPercent = OpenHealthPercent > 0 && OpenHealthPercent < 1;

            this.OnlyOpenWhenMarks = reader.GetList(title + "OnlyOpenWhenMarks", this.OnlyOpenWhenMarks);

            this.IsTransform = reader.Get(title + "IsTransform", this.IsTransform);
            if (IsTransform || ForceTransform)
            {
                ForTransform();
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

            this.HealthNumber = reader.Get(title + "HealthNumber", this.HealthNumber);
            {
                if (HealthNumber > 0)
                {
                    // 设置血量数字
                    this.InheritHealth = true;
                }
            }

            this.InheritHealthNumber = reader.Get(title + "InheritHealthNumber", this.InheritHealthNumber);
            if (InheritHealthNumber)
            {
                // 继承血量数字
                this.InheritHealth = true;
            }

            this.InheritTarget = reader.Get(title + "InheritTarget", this.InheritTarget);
            this.InheritExperience = reader.Get(title + "InheritExp", this.InheritExperience);
            this.InheritPassenger = reader.Get(title + "InheritPassenger", this.InheritPassenger);
            this.InheritROF = reader.Get(title + "InheritROF", this.InheritROF);
            this.InheritAmmo = reader.Get(title + "InheritAmmo", this.InheritAmmo);
            this.InheritAE = reader.Get(title + "InheritAE", this.InheritAE);
            this.ForceMission = reader.Get(title + "ForceMission", Mission.None);

            this.RemoveEffects = reader.GetList(title + "RemoveEffects", this.RemoveEffects);
            this.AttachEffects = reader.GetList(title + "AttachEffects", this.AttachEffects);
            this.AttachChances = reader.GetChanceList(title + "AttachChances", this.AttachChances);
        }

        public void ForTransform()
        {
            this.Remove = true; // 释放后移除
            this.Destroy = false; // 静默删除
            this.OpenWhenDestroyed = false;
            this.OpenWhenHealthPercent = false;
            this.IsTransform = true;
            this.InheritHealth = true; // 继承血量
            this.HealthPercent = 0;
            this.InheritTarget = true; // 继承目标
            this.InheritExperience = true; // 继承经验
            this.InheritPassenger = true; // 继承乘客
            this.InheritROF = true; // 继承ROF计时器
            this.InheritAmmo = true; // 继承弹药
            this.InheritAE = true; // 继承AE
        }

    }


}
