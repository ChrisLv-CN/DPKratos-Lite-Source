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
        public CoordStruct FireFLH;
        public bool Realtime;
        public bool FromSource;
        public bool ToSource;

        public bool ActiveOnce;
        public int TriggeredTimes;

        public string[] OnlyReactionWarheads;

        static RevengeData()
        {
            new WreckOwnerParser().Register();
            new MissionParser().Register();
        }

        public RevengeData()
        {
            this.Types = null;
            this.AttachEffects = null;
            this.FireFLH = default;
            this.Realtime = false;
            this.FromSource = false;
            this.ToSource = false;

            this.ActiveOnce = false;
            this.TriggeredTimes = -1;

            this.OnlyReactionWarheads = null;
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
            this.FireFLH = reader.Get(TITLE + "FireFLH", this.FireFLH);
            this.Realtime = reader.Get(TITLE + "Realtime", this.Realtime);
            this.FromSource = reader.Get(TITLE + "FromSource", this.FromSource);
            this.ToSource = reader.Get(TITLE + "ToSource", this.ToSource);

            this.ActiveOnce = reader.Get(TITLE + "ActiveOnce", this.ActiveOnce);
            this.TriggeredTimes = reader.Get(TITLE + "TriggeredTimes", this.TriggeredTimes);

            this.OnlyReactionWarheads = reader.GetList(TITLE + "OnlyReactionWarheads", this.OnlyReactionWarheads);
            if (null != OnlyReactionWarheads && OnlyReactionWarheads.Count() == 1 && OnlyReactionWarheads[0].IsNullOrEmptyOrNone())
            {
                OnlyReactionWarheads = null;
            }
        }

        public bool IsOnMark(Pointer<WarheadTypeClass> pWH)
        {
            return IsOnMark(pWH.Ref.Base.ID);
        }

        public bool IsOnMark(string warheadId)
        {
            return null == OnlyReactionWarheads || OnlyReactionWarheads.Contains(warheadId);
        }

    }


}
