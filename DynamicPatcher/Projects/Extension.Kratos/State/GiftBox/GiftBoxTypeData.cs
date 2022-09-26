using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class GiftBoxTypeData : EffectData, IStateData
    {

        public const string TITLE = "GiftBox.";

        public GiftBoxData Data;
        public GiftBoxData EliteData;
        public bool Remove;
        public bool Destroy;
        public int RandomRange;
        public bool EmptyCell;
        public bool OpenWhenDestoryed;
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

        static GiftBoxTypeData()
        {
            new MissionParser().Register();
        }

        public GiftBoxTypeData()
        {
            this.Data = null;
            this.EliteData = null;

            this.Remove = true;
            this.Destroy = false;
            this.RandomRange = 0;
            this.EmptyCell = false;
            this.OpenWhenDestoryed = false;
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

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            GiftBoxData data = new GiftBoxData();
            data.Read(reader, TITLE);
            if (null != data.Gifts && data.Gifts.Length > 0)
            {
                this.Data = data;
            }

            GiftBoxData elite = null != this.Data ? Data.Clone() : new GiftBoxData();
            elite.Read(reader, TITLE + "Elite");
            if (null != elite.Gifts && elite.Gifts.Length > 0)
            {
                this.EliteData = elite;
            }

            this.Enable = null != this.Data || null != this.EliteData;

            if (this.Enable)
            {
                // 通用设置
                this.Remove = reader.Get(TITLE + "Remove", this.Remove);
                this.Destroy = reader.Get(TITLE + "Explodes", this.Destroy);
                this.RandomRange = reader.Get(TITLE + "RandomRange", this.RandomRange);
                this.EmptyCell = reader.Get(TITLE + "RandomToEmptyCell", this.EmptyCell);

                this.OpenWhenDestoryed = reader.Get(TITLE + "OpenWhenDestoryed", this.OpenWhenDestoryed);
                this.OpenHealthPercent = reader.GetPercent(TITLE + "OpenWhenHealthPercent", this.OpenHealthPercent);
                this.OpenWhenHealthPercent = OpenHealthPercent > 0 && OpenHealthPercent < 1;

                this.IsTransform = reader.Get(TITLE + "IsTransform", this.IsTransform);
                if (IsTransform)
                {
                    this.Remove = true; // 释放后移除
                    this.Destroy = false; // 静默删除
                    this.InheritHealth = true; // 继承血量
                }

                this.HealthPercent = reader.GetPercent(TITLE + "HealthPercent", this.HealthPercent);
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

                this.InheritTarget = reader.Get(TITLE + "InheritTarget", this.InheritTarget);
                this.InheritExperience = reader.Get(TITLE + "InheritExp", this.InheritExperience);
                this.InheritAmmo = reader.Get(TITLE + "InheritAmmo", this.InheritAmmo);
                this.InheritAE = reader.Get(TITLE + "InheritAE", this.InheritAE);
                this.ForceMission = reader.Get(TITLE + "ForceMission", Mission.None);

                this.AttachEffects = reader.GetList<string>(TITLE + "AttachEffects", null);
            }
        }

    }


}
