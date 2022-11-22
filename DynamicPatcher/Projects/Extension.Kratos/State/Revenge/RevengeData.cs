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
        public RevengeData RevengeData;

        private void ReadRevengeData(IConfigReader reader)
        {
            RevengeData data = new RevengeData();
            data.Read(reader);
            if (data.Enable)
            {
                this.RevengeData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class RevengeData : FilterEffectData, IStateData
    {
        public const string TITLE = "Revenge.";

        public string[] Types;
        public string[] AttachEffects;

        static RevengeData()
        {
            new WreckOwnerParser().Register();
            new MissionParser().Register();
        }

        public RevengeData()
        {
            this.Types = null;
            this.AttachEffects = null;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Types = reader.GetList(TITLE + "Types", this.Types);
            this.AttachEffects = reader.GetList<string>(TITLE + "AttachEffects", null);
            this.Enable = null != Types && Types.Any() || null != AttachEffects && AttachEffects.Any();
            if (Enable)
            {
                this.Enable = AffectTechno;
            }
        }

    }


}
