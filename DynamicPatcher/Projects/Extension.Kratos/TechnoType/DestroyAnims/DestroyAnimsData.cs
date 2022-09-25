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

    public class DestroyAnimsData : INIAutoConfig
    {
        [INIField(Key = "DestroyAnims")]
        public string[] DestroyAnims = null;

        [INIField(Key = "DestroyAnims.Random")]
        public bool DestroyAnimsRandom = true;

        [INIField(Key = "DestoryAnims.WreckType")]
        public string WreckType = null;

        [INIField(Key = "DestroyAnims.WreckOwner")]
        public WreckOwner WreckOwner = WreckOwner.INVOKER;

        [INIField(Key = "DestroyAnims.WreckMission")]
        public Mission WreckMission = Mission.Sleep;

        [INIField(Key = "Wreck")]
        public bool Wreck = false;

        static DestroyAnimsData()
        {
            new WreckOwnerParser().Register();
            new MissionParser().Register();
        }

    }


}
