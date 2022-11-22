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
        public const string TITLE = "HotKey.";

        public string[] AttachEffectTypes;

        public HotKeyAttachEffectData()
        {
            this.AttachEffectTypes = null;

            this.AffectsOwner = true;
            this.AffectsAllies = false;
            this.AffectsEnemies = false;
            this.AffectsCivilian = false;
        }

        public override void Read(IConfigReader reader)
        {
            // 读全局设置
            ISectionReader generalReader = Ini.GetSection(Ini.RulesDependency, RulesClass.SectionCombatDamage);
            base.Read(generalReader, TITLE);
            this.AttachEffectTypes = generalReader.GetList<string>(TITLE + "AttachEffectTypes", null);

            // 读私有设置
            base.Read(reader, TITLE);
            this.AttachEffectTypes = reader.GetList<string>(TITLE + "AttachEffectTypes", AttachEffectTypes);

            this.Enable = null != AttachEffectTypes && AttachEffectTypes.Any();
            if (Enable)
            {
                this.Enable = AffectTechno;
            }
        }

    }

}
