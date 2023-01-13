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

    public partial class AttachEffectData
    {
        public StackData StackData;

        private void ReadStackData(IConfigReader reader)
        {
            StackData data = new StackData(reader);
            if (data.Enable)
            {
                this.StackData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class StackData : EffectData
    {

        public const string TITLE = "Stack.";

        public string Watch;
        public int Level;
        public string[] AttachEffects;
        public double[] AttachChances;
        public int TriggeredTimes;
        public bool RemoveAll;


        public StackData()
        {
            this.Watch = null;
            this.Level = 0;
            this.AttachEffects = null;
            this.AttachChances = null;
            this.TriggeredTimes = -1;
            this.RemoveAll = true;
        }

        public StackData(IConfigReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Watch = reader.Get(TITLE + "Watch", this.Watch);
            this.Level = reader.Get(TITLE + "Level", this.Level);
            this.AttachEffects = reader.GetList(TITLE + "AttachEffects", this.AttachEffects);
            this.AttachChances = reader.GetChanceList(TITLE + "AttachChances", this.AttachChances);
            this.TriggeredTimes = reader.Get(TITLE + "TriggeredTimes", this.TriggeredTimes);
            this.RemoveAll = reader.Get(TITLE + "RemoveAll", this.RemoveAll);

            this.Enable = !Watch.IsNullOrEmptyOrNone() && null != AttachEffects && AttachEffects.Any();
        }

    }


}
