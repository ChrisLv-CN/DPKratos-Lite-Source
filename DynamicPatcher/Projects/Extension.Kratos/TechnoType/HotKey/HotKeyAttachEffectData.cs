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

        public string[] AttachEffectTypes;
        public int[] Keys;

        public HotKeyAttachEffectData()
        {
            this.AttachEffectTypes = null;
            this.Keys = null;

            this.AffectsOwner = true;
            this.AffectsAllies = false;
            this.AffectsEnemies = false;
            this.AffectsCivilian = false;
        }

        public override void Read(ISectionReader reader, string title)
        {
            base.Read(reader, title);
            this.AttachEffectTypes = reader.GetList<string>(title + "AttachEffectTypes", AttachEffectTypes);
            this.Keys = reader.GetList<int>(title + "Keys", this.Keys);

            this.Enable = null != AttachEffectTypes && AttachEffectTypes.Any();
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
