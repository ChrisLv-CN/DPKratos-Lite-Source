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
    public class HotKeyAttachEffectData : FilterEffectData
    {

        public string[] AttachEffects;
        public int[] Keys;

        public HotKeyAttachEffectData()
        {
            this.AttachEffects = null;
            this.Keys = null;

            this.AffectsOwner = true;
            this.AffectsAllies = false;
            this.AffectsEnemies = false;
            this.AffectsCivilian = false;
        }

        public override void Read(ISectionReader reader, string title)
        {
            base.Read(reader, title);
            this.AttachEffects = reader.GetList<string>(title + "AttachEffects", AttachEffects);
            this.Keys = reader.GetList<int>(title + "Keys", this.Keys);

            this.Enable = null != AttachEffects && AttachEffects.Any();
            if (Enable)
            {
                this.Enable = AffectTechno;
            }
        }

        public bool IsOnKey(int group)
        {
            return null == Keys || group < 1 || !Keys.Any() || Keys.Contains(group);
        }

    }

}
