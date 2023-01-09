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
    public class ECMData : FilterEffectData, IStateData
    {
        public const string TITLE = "ECM.";

        public double RangeMin; // 搜索新目标的范围
        public double RangeMax; // 搜索新目标的范围
        public bool FullAirspace; // 圆柱形搜索

        public double Chance; // 发生的概率

        public int LockDuration; // 锁定时间
        public bool Feedback; // 反噬

        public bool AroundSelf; // 围绕自己搜索
        public bool AroundSource; // 围绕来源搜索

        public double ToTechnoChance; // 新目标是单位的概率
        public bool ForceRetarget; // 一定重置目标
        public bool NoOwner; // 清除抛射体的发射者

        public int Rate;
        public int TriggeredTimes;

        public ECMData()
        {
            this.RangeMin = 0;
            this.RangeMax = 0;
            this.FullAirspace = false;

            this.Chance = 1;

            this.LockDuration = 0;
            this.Feedback = false;

            this.AroundSelf = true;
            this.AroundSource = true;

            this.ToTechnoChance = 0;
            this.ForceRetarget = false;
            this.NoOwner = false;

            this.Rate = 15;
            this.TriggeredTimes = 1;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.RangeMin = reader.Get(TITLE + "RangeMin", this.RangeMin);
            this.RangeMax = reader.Get(TITLE + "RangeMax", this.RangeMax);
            if (RangeMin > RangeMax)
            {
                this.RangeMin = 0;
            }
            this.FullAirspace = reader.Get(TITLE + "FullAirspace", this.FullAirspace);

            this.Chance = reader.GetChance(TITLE + "Chance", this.Chance);

            this.LockDuration = reader.Get(TITLE + "LockDuration", this.LockDuration);
            this.Feedback = reader.Get(TITLE + "Feedback", this.Feedback);

            this.AroundSelf = reader.Get(TITLE + "AroundSelf", this.AroundSelf);
            this.AroundSource = reader.Get(TITLE + "AroundSource", this.AroundSource);

            this.ToTechnoChance = reader.GetChance(TITLE + "ToTechnoChance", this.ToTechnoChance);
            this.ForceRetarget = reader.Get(TITLE + "ForceRetarget", this.ForceRetarget);
            this.NoOwner = reader.Get(TITLE + "NoOwner", this.NoOwner);

            this.Rate = reader.Get(TITLE + "Rate", this.Rate);
            this.TriggeredTimes = reader.Get(TITLE + "TriggeredTimes", this.TriggeredTimes);

            this.Enable = RangeMax > 0 && Chance > 0 && TriggeredTimes != 0;
        }

    }


}
