using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{
    public class MissionParser : KEnumParser<Mission>
    {
        public override bool ParseInitials(string t, ref Mission buffer)
        {
            switch (t)
            {
                case "G":
                    buffer = Mission.Guard;
                    break;
                case "A":
                    buffer = Mission.Area_Guard;
                    break;
                case "M":
                    buffer = Mission.Move;
                    break;
                case "H":
                    buffer = Mission.Hunt;
                    break;
                case "S":
                    buffer = Mission.Sleep;
                    break;
                case "D": // Deploy
                case "U":
                    buffer = Mission.Unload;
                    break;
                default:
                    buffer = Mission.None;
                    break;
            }
            return true;
        }
    }

}
