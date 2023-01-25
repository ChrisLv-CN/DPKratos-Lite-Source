using System;
using System.Collections.Generic;
using System.Linq;
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
        public ECMData ECMData;

        private void ReadECMData(IConfigReader reader)
        {
            ECMData data = new ECMData();
            data.Read(reader);
            if (data.Enable)
            {
                this.ECMData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public enum ECMAround
    {
        Source = 0, Target = 1, Self = 2, Shooter = 3
    }

    [Serializable]
    public class ECMData : EffectData, IStateData
    {
        public const string TITLE = "ECM.";


        public double Chance; // 发生的概率
        public int Rate;

        public int LockDuration; // 锁定时间
        public bool Feedback; // 反噬

        public double ToTechnoChance; // 新目标是单位的概率
        public bool ForceRetarget; // 一定重置目标
        public bool NoOwner; // 清除抛射体的发射者

        public ECMAround Around; // 新目标搜索范围圆心

        public double RangeMin;
        public double RangeMax;
        public bool FullAirspace;

        public ECMData()
        {
            this.Chance = 0;
            this.Rate = 15;
            this.TriggeredTimes = 1;

            this.LockDuration = 0;
            this.Feedback = false;

            this.ToTechnoChance = 0;
            this.ForceRetarget = false;
            this.NoOwner = false;

            this.Around = ECMAround.Source;
            this.RangeMin = 0;
            this.RangeMax = 1;
            this.FullAirspace = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Chance = reader.GetChance(TITLE + "Chance", this.Chance);
            this.Rate = reader.Get(TITLE + "Rate", this.Rate);

            this.LockDuration = reader.Get(TITLE + "LockDuration", this.LockDuration);
            this.Feedback = reader.Get(TITLE + "Feedback", this.Feedback);

            this.ToTechnoChance = reader.GetChance(TITLE + "ToTechnoChance", this.ToTechnoChance);
            this.ForceRetarget = reader.Get(TITLE + "ForceRetarget", this.ForceRetarget);
            this.NoOwner = reader.Get(TITLE + "NoOwner", this.NoOwner);

            this.Around = reader.Get(TITLE + "Around", this.Around);
            this.RangeMax = reader.Get(TITLE + "RangeMax", this.RangeMax);
            if (RangeMax < 0)
            {
                RangeMax = 0;
            }
            this.RangeMin = reader.Get(TITLE + "RangeMin", this.RangeMin);
            if (RangeMin < 0)
            {
                RangeMin = 0;
            }
            if (RangeMin > RangeMax)
            {
                RangeMin = RangeMax;
            }
            this.FullAirspace = reader.Get(TITLE + "FullAirspace", this.FullAirspace);

            this.Enable = Chance > 0 && (Feedback || RangeMax > 0);

        }

    }


}
