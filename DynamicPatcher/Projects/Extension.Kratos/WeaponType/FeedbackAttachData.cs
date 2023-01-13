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

    [Serializable]
    public class FeedbackAttachData : FilterEffectData
    {
        public string[] AttachEffects;
        public double[] AttachChances;
        public bool AttachToTransporter;

        public FeedbackAttachData()
        {
            this.AttachEffects = null;
            this.AttachChances = null;
            this.AttachToTransporter = false;
        }

        public override void Read(ISectionReader reader, string title)
        {
            base.Read(reader, title);
            this.AttachEffects = reader.GetList(title + "AttachEffects", this.AttachEffects);
            this.AttachChances = reader.GetChanceList(title + "AttachChances", this.AttachChances);

            this.Enable = null != AttachEffects && AttachEffects.Any();
        }

    }


}
