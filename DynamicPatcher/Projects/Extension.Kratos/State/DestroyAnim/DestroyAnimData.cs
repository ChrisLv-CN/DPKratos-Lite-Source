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
        public DestroyAnimData DestroyAnimData;

        private void ReadDestroyAnimData(IConfigReader reader)
        {
            DestroyAnimData data = new DestroyAnimData();
            data.Read(reader);
            if (data.Enable)
            {
                this.DestroyAnimData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public enum WreckOwner
    {
        INVOKER = 0, KILLER = 1, NEUTRAL = 2
    }

    public class WreckOwnerParser : KEnumParser<WreckOwner>
    {
        public override bool ParseInitials(string t, ref WreckOwner buffer)
        {
            switch (t)
            {
                case "K":
                    buffer = WreckOwner.KILLER;
                    return true;
                case "N":
                    buffer = WreckOwner.NEUTRAL;
                    return true;
                default:
                    buffer = WreckOwner.INVOKER;
                    return true;
            }
        }
    }

    [Serializable]
    public class DestroyAnimData : EffectData, IStateData
    {
        public const string TITLE = "DestroyAnim.";

        public string[] Anims;
        public bool Random;
        public bool PlayInAir;
        public WreckOwner Owner;

        public string WreckType;
        public Mission WreckMission;

        public bool Wreck = false;

        static DestroyAnimData()
        {
            new WreckOwnerParser().Register();
            new MissionParser().Register();
        }

        public DestroyAnimData()
        {
            this.Anims = null;
            this.Random = false;
            this.PlayInAir = false;
            this.Owner = WreckOwner.INVOKER;

            this.WreckType = null;
            this.WreckMission = Mission.Sleep;

            this.Wreck = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Anims = reader.GetList(TITLE + "Types", this.Anims);
            this.Random = reader.Get(TITLE + "Random", this.Random);
            this.PlayInAir = reader.Get(TITLE + "PlayInAir", this.PlayInAir);
            this.Owner = reader.Get(TITLE + "Owner", this.Owner);

            this.WreckType = reader.Get(TITLE + "WreckType", this.WreckType);
            this.WreckMission = reader.Get(TITLE + "WreckMission", this.WreckMission);

            this.Enable = null != Anims || !WreckType.IsNullOrEmptyOrNone();

            this.Wreck = reader.Get(TITLE + "Wreck", this.Wreck);
        }

    }


}
