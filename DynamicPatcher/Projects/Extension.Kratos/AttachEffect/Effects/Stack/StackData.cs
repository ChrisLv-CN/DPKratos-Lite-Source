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
    public enum Condition
    {
        EQ = 0,
        NE = 1,
        GT = 2,
        LT = 3,
        GE = 4,
        LE = 5
    }

    [Serializable]
    public class StackData : EffectData
    {

        public const string TITLE = "Stack.";

        public string Watch;

        public int Level;
        public Condition Condition;

        public bool Attach;
        public string[] AttachEffects;
        public double[] AttachChances;

        public bool Remove;
        public string[] RemoveEffects;

        public bool RemoveAll;


        public StackData()
        {
            this.Watch = null;

            this.Level = 0;
            this.Condition = Condition.EQ;

            this.Attach = false;
            this.AttachEffects = null;
            this.AttachChances = null;

            this.Remove = false;
            this.RemoveEffects = null;

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
            this.Condition = reader.Get(TITLE + "Condition", this.Condition);

            this.AttachEffects = reader.GetList(TITLE + "AttachEffects", this.AttachEffects);
            this.AttachChances = reader.GetChanceList(TITLE + "AttachChances", this.AttachChances);
            this.Attach = null != AttachEffects && AttachEffects.Any();

            this.RemoveEffects = reader.GetList(TITLE + "RemoveEffects", this.RemoveEffects);
            this.Remove = null != RemoveEffects && RemoveEffects.Any();

            this.RemoveAll = reader.Get(TITLE + "RemoveAll", this.RemoveAll);

            this.Enable = !Watch.IsNullOrEmptyOrNone() && (Attach || Remove);
        }

    }


}
