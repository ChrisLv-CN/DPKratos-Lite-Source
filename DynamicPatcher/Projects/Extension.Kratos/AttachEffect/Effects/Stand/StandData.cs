using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{
    public class LayerParser : KEnumParser<Layer>
    {
        public override bool ParseInitials(string t, ref Layer buffer)
        {
            switch (t)
            {
                case "U":
                    buffer = Layer.Underground;
                    return true;
                case "S":
                    buffer = Layer.Surface;
                    return true;
                case "G":
                    buffer = Layer.Ground;
                    return true;
                case "A":
                    buffer = Layer.Air;
                    return true;
                case "T":
                    buffer = Layer.Top;
                    return true;
            }
            return false;
        }
    }


    public partial class AttachEffectData
    {
        public StandData StandData;

        private void ReadStandData(ISectionReader reader)
        {
            StandData data = new StandData(reader);
            if (data.Enable)
            {
                this.StandData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class StandData : EffectData
    {
        static StandData()
        {
            new LayerParser().Register();
        }

        public const string TITLE = "Stand.";

        public string Type; // 替身类型
        public CoordStruct Offset; // 替身相对位置
        // public Direction Direction; // 相对朝向
        public int Direction; // 相对朝向，16分圆，[0-15]
        public bool LockDirection; // 强制朝向，不论替身在做什么
        public bool FreeDirection; // 完全不控制朝向
        public bool IsOnTurret; // 相对炮塔或者身体
        public bool IsOnWorld; // 相对世界

        public Layer DrawLayer; // 渲染的层
        public int ZOffset; // ZAdjust偏移值
        public bool SameTilter; // 同步倾斜
        public bool SameMoving; // 同步移动动画

        public bool SameHouse; // 与使者同所属
        public bool SameTarget; // 与使者同个目标
        public bool SameLoseTarget; // 使者失去目标时替身也失去
        public bool SameAmmo; // 与使者弹药数相同
        public bool UseMasterAmmo; // 消耗使者的弹药

        public bool ForceAttackMaster; // 强制选择使者为目标
        public bool MobileFire; // 移动攻击
        public bool Powered; // 是否需要电力支持

        public bool Immune; // 无敌
        public double DamageFromMaster; // 分摊JOJO的伤害
        public double DamageToMaster; // 分摊伤害给JOJO
        public bool AllowShareRepair; // 是否允许负伤害分摊

        public bool Explodes; // 死亡会爆炸
        public bool ExplodesWithMaster; // 使者死亡时强制替身爆炸
        public bool RemoveAtSinking; // 沉船时移除

        public bool PromoteFromMaster; // 与使者同等级
        public double ExperienceToMaster; // 经验给使者

        public bool SelectToMaster; // 选中替身时，改为选中使者

        public bool VirtualUnit; // 虚单位

        public bool IsVirtualTurret; // 虚拟炮塔

        public bool IsTrain; // 火车类型
        public bool CabinHead; // 插入车厢前端
        public int CabinGroup; // 车厢分组

        public StandData()
        {
            this.Type = null;
            this.Offset = default;
            this.Direction = 0;
            this.LockDirection = false;
            this.FreeDirection = false;
            this.IsOnTurret = false;
            this.IsOnWorld = false;

            this.DrawLayer = Layer.None;
            this.ZOffset = 14;
            this.SameTilter = true;
            this.SameMoving = false;

            this.SameHouse = true;
            this.SameTarget = true;
            this.SameLoseTarget = false;
            this.SameAmmo = false;
            this.UseMasterAmmo = false;

            this.ForceAttackMaster = false;
            this.MobileFire = true;
            this.Powered = false;

            this.Immune = true;
            this.DamageFromMaster = 0.0;
            this.DamageToMaster = 0.0;
            this.AllowShareRepair = false;

            this.Explodes = false;
            this.ExplodesWithMaster = false;
            this.RemoveAtSinking = false;

            this.PromoteFromMaster = false;
            this.ExperienceToMaster = 0.0;

            this.SelectToMaster = false;

            this.VirtualUnit = true;

            this.IsVirtualTurret = true;

            this.IsTrain = false;
            this.CabinHead = false;
            this.CabinGroup = -1;
        }

        public StandData(ISectionReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            this.Read(reader);
        }

        public void Read(ISectionReader reader)
        {
            this.Type = reader.Get(TITLE + "Type", this.Type);

            if (this.Enable = !Type.IsNullOrEmptyOrNone())
            {
                this.Offset = reader.Get(TITLE + "Offset", this.Offset);
                this.Direction = reader.GetDir16(TITLE + "Direction", this.Direction);
                this.LockDirection = reader.Get(TITLE + "LockDirection", this.LockDirection);
                this.FreeDirection = reader.Get(TITLE + "FreeDirection", this.FreeDirection);
                this.IsOnTurret = reader.Get(TITLE + "IsOnTurret", this.IsOnTurret);
                this.IsOnWorld = reader.Get(TITLE + "IsOnWorld", this.IsOnWorld);

                this.DrawLayer = reader.Get(TITLE + "DrawLayer", this.DrawLayer);
                this.ZOffset = reader.Get(TITLE + "ZOffset", this.ZOffset);
                this.SameTilter = reader.Get(TITLE + "SameTilter", this.SameTilter);
                this.SameMoving = reader.Get(TITLE + "SameMoving", this.SameMoving);

                this.SameHouse = reader.Get(TITLE + "SameHouse", this.SameHouse);
                this.SameTarget = reader.Get(TITLE + "SameTarget", this.SameTarget);
                this.SameLoseTarget = reader.Get(TITLE + "SameLoseTarget", this.SameLoseTarget);
                this.SameAmmo = reader.Get(TITLE + "SameAmmo", this.SameAmmo);
                this.UseMasterAmmo = reader.Get(TITLE + "UseMasterAmmo", this.UseMasterAmmo);

                this.ForceAttackMaster = reader.Get(TITLE + "ForceAttackMaster", this.ForceAttackMaster);
                this.MobileFire = reader.Get(TITLE + "MobileFire", this.MobileFire);
                this.Powered = reader.Get(TITLE + "Powered", this.Powered);

                this.Immune = reader.Get(TITLE + "Immune", this.Immune);
                this.DamageFromMaster = reader.GetPercent(TITLE + "DamageFromMaster", this.DamageFromMaster);
                this.DamageToMaster = reader.GetPercent(TITLE + "DamageToMaster", this.DamageToMaster);
                this.AllowShareRepair = reader.Get(TITLE + "AllowShareRepair", this.AllowShareRepair);

                this.Explodes = reader.Get(TITLE + "Explodes", this.Explodes);
                this.ExplodesWithMaster = reader.Get(TITLE + "ExplodesWithMaster", this.ExplodesWithMaster);
                this.RemoveAtSinking = reader.Get(TITLE + "RemoveAtSinking", this.RemoveAtSinking);

                this.PromoteFromMaster = reader.Get(TITLE + "PromoteFromMaster", this.PromoteFromMaster);
                this.ExperienceToMaster = reader.GetPercent(TITLE + "ExperienceToMaster", this.ExperienceToMaster);

                this.SelectToMaster = reader.Get(TITLE + "SelectToMaster", this.SelectToMaster);

                this.VirtualUnit = reader.Get(TITLE + "VirtualUnit", this.VirtualUnit);

                this.IsVirtualTurret = reader.Get(TITLE + "IsVirtualTurret", this.IsVirtualTurret);

                this.IsTrain = reader.Get(TITLE + "IsTrain", this.IsTrain);
                this.CabinHead = reader.Get(TITLE + "CabinHead", this.CabinHead);
                this.CabinGroup = reader.Get(TITLE + "CabinGroup", this.CabinGroup);
            }
        }

    }


}
