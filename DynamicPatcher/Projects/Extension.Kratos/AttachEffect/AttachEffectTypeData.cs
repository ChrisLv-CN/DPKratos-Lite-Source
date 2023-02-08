using System;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public partial class AttachEffectTypeData : INIConfig
    {
        public string[] AttachEffectTypes;
        public double[] AttachEffectChances; // 附加成功率，应该只对弹头有用
        public bool AttachFullAirspace; // 搜索圆柱体范围

        // 单条AE
        public int StandTrainCabinLength; // 火车替身间隔
        public int AEMode; // 作为乘客时的激活载具的组序号

        //多组AE
        public bool AttachByPassenger; // 仅由乘客的AEMode赋予
        public int AEModeIndex; // 组序号


        public AttachEffectTypeData()
        {
            this.AttachEffectTypes = null;
            this.AttachEffectChances = null;
            this.AttachFullAirspace = false;
            // 单条
            this.StandTrainCabinLength = 512;
            this.AEMode = -1;
            // 多组
            this.AttachByPassenger = true;
            this.AEModeIndex = -1;
        }

        // 单条AE
        public override void Read(IConfigReader reader)
        {
            this.AttachEffectTypes = reader.GetList("AttachEffectTypes", this.AttachEffectTypes);
            this.AttachEffectChances = reader.GetChanceList("AttachEffectChances", this.AttachEffectChances);
            this.AttachFullAirspace = reader.Get("AttachFullAirspace", this.AttachFullAirspace);

            this.StandTrainCabinLength = reader.Get("StandTrainCabinLength", this.StandTrainCabinLength);
            // 乘客读取
            this.AEMode = reader.Get("AEMode", this.AEMode);
        }

        // 多组AE
        public void Read(ISectionReader reader, int index)
        {
            string title = "AttachEffectTypes" + index;
            this.AttachEffectTypes = reader.GetList(title, this.AttachEffectTypes);
            this.AttachEffectChances = reader.GetChanceList(title + ".Chances", this.AttachEffectChances);

            this.AttachByPassenger = reader.Get(title + ".AttachByPassenger", this.AttachByPassenger); // 默认值为true
            this.AEModeIndex = index;
        }

    }

}
