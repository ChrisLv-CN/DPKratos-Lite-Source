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
    public class UploadAttachData : FilterEffectData
    {
        public string[] AttachEffects;

        public UploadAttachData()
        {
            this.AttachEffects = null;
        }

        public override void Read(ISectionReader reader, string title)
        {
            base.Read(reader, title);
            this.AttachEffects = reader.GetList(title + "AttachEffects", this.AttachEffects);

            this.Enable = null != AttachEffects && AttachEffects.Any();
            if (Enable)
            {
                this.Enable = AffectTechno;
            }
        }

    }


}
