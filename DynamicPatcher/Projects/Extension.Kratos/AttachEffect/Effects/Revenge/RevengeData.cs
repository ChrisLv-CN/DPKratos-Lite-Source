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
    public class RevengeData : EffectData, IStateData
    {
        public const string TITLE = "Revenge.";

        public double Chance;

        public string[] Types;
        public int WeaponIndex;

        public string[] AttachEffects;
        public double[] AttachChances;
        public CoordStruct FireFLH;
        public bool IsOnTurret;
        public bool IsOnTarget;
        public bool Realtime;
        public bool FromSource;
        public bool ToSource;

        public bool ActiveOnce;

        public string[] OnlyReactionWarheads;

        static RevengeData()
        {
            new WreckOwnerParser().Register();
            new MissionParser().Register();
        }

        public RevengeData()
        {
            this.Chance = 1;

            this.Types = null;
            this.WeaponIndex = -1;

            this.AttachEffects = null;
            this.AttachChances = null;
            this.FireFLH = default;
            this.IsOnTurret = true;
            this.IsOnTarget = false;
            this.Realtime = false;
            this.FromSource = false;
            this.ToSource = false;

            this.ActiveOnce = false;

            this.OnlyReactionWarheads = null;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Chance = reader.GetChance(TITLE + "Chance", this.Chance);

            this.Types = reader.GetList(TITLE + "Types", this.Types);
            this.WeaponIndex = reader.Get(TITLE + "WeaponIndex", this.WeaponIndex);

            this.AttachEffects = reader.GetList<string>(TITLE + "AttachEffects", this.AttachEffects);
            this.AttachChances = reader.GetChanceList(TITLE + "AttachChances", this.AttachChances);
            this.Enable = Chance > 0 && (null != Types && Types.Any() || WeaponIndex > -1 || null != AttachEffects && AttachEffects.Any());
            if (Enable)
            {
                this.Enable = AffectTechno;
            }
            this.FireFLH = reader.Get(TITLE + "FireFLH", this.FireFLH);
            this.IsOnTurret = reader.Get(TITLE + "IsOnTurret", this.IsOnTurret);
            this.IsOnTarget = reader.Get(TITLE + "IsOnTarget", this.IsOnTarget);
            this.Realtime = reader.Get(TITLE + "Realtime", this.Realtime);
            this.FromSource = reader.Get(TITLE + "FromSource", this.FromSource);
            this.ToSource = reader.Get(TITLE + "ToSource", this.ToSource);

            this.ActiveOnce = reader.Get(TITLE + "ActiveOnce", this.ActiveOnce);

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
