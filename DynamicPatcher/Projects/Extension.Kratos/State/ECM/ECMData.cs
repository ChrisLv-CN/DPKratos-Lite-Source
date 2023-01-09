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

        public double Range; // 搜索新目标的范围
        public double Chance; // 发生的概率

        public bool AroundSelf; // 围绕自己搜索

        public double ToTechnoChance; // 新目标是单位的概率
        public bool ForceRetarget; // 一定重置目标

        public int Rate;
        public int TriggeredTimes;

        public ECMData()
        {
            this.Range = 0;
            this.Chance = 1;

            this.AroundSelf = false;

            this.ToTechnoChance = 0;
            this.ForceRetarget = false;

            this.Rate = 15;
            this.TriggeredTimes = 1;

            this.AffectTechno = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Range = reader.Get(TITLE + "Range", this.Range);
            this.Chance = reader.GetChance(TITLE + "Chance", this.Chance);

            this.AroundSelf = reader.Get(TITLE + "AroundSelf", this.AroundSelf);

            this.ToTechnoChance = reader.GetChance(TITLE + "ToTechnoChance", this.ToTechnoChance);
            this.ForceRetarget = reader.Get(TITLE + "ForceRetarget", this.ForceRetarget);

            this.Rate = reader.Get(TITLE + "Rate", this.Rate);
            this.TriggeredTimes = reader.Get(TITLE + "TriggeredTimes", this.TriggeredTimes);

            this.Enable = Range > 0 && Chance > 0 && TriggeredTimes != 0;
        }

    }


}
