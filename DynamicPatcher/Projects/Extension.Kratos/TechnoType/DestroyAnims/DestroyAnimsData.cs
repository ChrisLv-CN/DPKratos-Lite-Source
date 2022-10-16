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
    public class DestroyAnimsData : INIConfig
    {
        public const string TITLE = "DestroyAnims.";

        public string[] Anims;
        public bool Random;
        public bool PlayInAir;

        public string WreckType;
        public WreckOwner WreckOwner;
        public Mission WreckMission;

        public bool Wreck = false;

        static DestroyAnimsData()
        {
            new WreckOwnerParser().Register();
            new MissionParser().Register();
        }

        public DestroyAnimsData()
        {
            this.Anims = null;
            this.Random = true;
            this.PlayInAir = false;

            this.WreckType = null;
            this.WreckOwner = WreckOwner.INVOKER;
            this.WreckMission = Mission.Sleep;

            this.Wreck = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.Anims = reader.GetList("DestroyAnims", this.Anims);
            this.Random = reader.Get(TITLE + "Random", this.Random);
            this.PlayInAir = reader.Get(TITLE + "PlayInAir", this.PlayInAir);

            this.WreckType = reader.Get(TITLE + "WreckType", this.WreckType);
            this.WreckOwner = reader.Get(TITLE + "WreckOwner", this.WreckOwner);
            this.WreckMission = reader.Get(TITLE + "WreckMission", this.WreckMission);

            this.Wreck = reader.Get(TITLE + "Wreck", this.Wreck);
        }

    }


}
