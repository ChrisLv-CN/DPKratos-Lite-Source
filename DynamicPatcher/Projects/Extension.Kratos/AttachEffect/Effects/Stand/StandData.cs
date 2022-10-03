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

    /// <summary>
    /// AE动画
    /// </summary>
    [Serializable]
    public class StandData : EffectData
    {
        public string Type; // 替身类型
        public CoordStruct Offset; // 替身相对位置
        // public Direction Direction; // 相对朝向
        public int Direction; // 相对朝向，16分圆，[0-15]
        public bool LockDirection; // 强制朝向，不论替身在做什么
        public bool IsOnTurret; // 相对炮塔或者身体
        public bool IsOnWorld; // 相对世界
        public Layer DrawLayer; // 渲染的层
        public int ZOffset; // ZAdjust偏移值
        public bool Powered; // 是否需要电力支持
        public bool SameHouse; // 与使者同所属
        public bool SameTarget; // 与使者同个目标
        public bool SameLoseTarget; // 使者失去目标时替身也失去
        public bool SameAmmo; // 与使者弹药数相同
        public bool UseMasterAmmo; // 消耗使者的弹药
        public bool ForceAttackMaster; // 强制选择使者为目标
        public bool MobileFire; // 移动攻击
        public bool Immune; // 无敌
        public double DamageFromMaster; // 分摊JOJO的伤害
        public double DamageToMaster; // 分摊伤害给JOJO
        public bool AllowShareRepair; // 是否允许负伤害分摊
        public bool Explodes; // 死亡会爆炸
        public bool ExplodesWithMaster; // 使者死亡时强制替身爆炸
        public bool RemoveAtSinking; // 沉船时移除
        public bool PromoteFromMaster; // 与使者同等级
        public double ExperienceToMaster; // 经验给使者
        public bool VirtualUnit; // 虚单位
        public bool SameTilter; // 同步倾斜
        public bool IsTrain; // 火车类型
        public bool CabinHead; // 插入车厢前端
        public int CabinGroup; // 车厢分组

        public StandData()
        {
            this.Type = null;
            this.Offset = default;
            this.Direction = 0;
            this.LockDirection = false;
            this.IsOnTurret = false;
            this.IsOnWorld = false;
            this.DrawLayer = Layer.None;
            this.ZOffset = 14;
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
            this.VirtualUnit = true;
            this.SameTilter = true;
            this.IsTrain = false;
            this.CabinHead = false;
            this.CabinGroup = -1;
        }

        public StandData(ISectionReader reader) : this()
        {
            Read(reader);
        }

        public void Read(ISectionReader reader)
        {
        }

    }


}
