using System;
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
        public TeleportData TeleportData;

        private void ReadTeleportData(IConfigReader reader)
        {
            TeleportData data = new TeleportData();
            data.Read(reader);
            if (data.Enable)
            {
                this.TeleportData = data;
                this.Enable = true;
            }
        }
    }
    [Serializable]
    public enum TeleportMode
    {
        NONE = 0, MOVE = 1, WARHEAD = 2, BOTH = 3
    }

    public class TeleportModeParser : KEnumParser<TeleportMode>
    {
        public override bool ParseInitials(string t, ref TeleportMode buffer)
        {
            switch (t)
            {
                case "M":
                    buffer = TeleportMode.MOVE;
                    return true;
                case "W":
                    buffer = TeleportMode.WARHEAD;
                    return true;
                case "B":
                    buffer = TeleportMode.BOTH;
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public class TeleportData : EffectData, IStateData
    {
        static TeleportData()
        {
            new TeleportModeParser().Register();
        }

        public const string TITLE = "Teleport.";

        public TeleportMode Mode;
        public bool Super;

        public int Delay;
        public int TriggeredTimes;
        public bool ClearTarget;
        public bool MoveForward;

        public double RangeMin;
        public double RangeMax;
        public int Distance;


        public TeleportData()
        {
            this.Mode = TeleportMode.NONE;
            this.Super = false;

            this.Delay = 0;
            this.TriggeredTimes = -1;
            this.ClearTarget = true;
            this.MoveForward = true;

            this.RangeMin = 0;
            this.RangeMax = -1;
            this.Distance = -1;
        }

        public override void Read(IConfigReader reader)
        {

            base.Read(reader, TITLE);

            this.Mode = reader.Get(TITLE + "Mode", this.Mode);
            this.Enable = Mode != TeleportMode.NONE;

            this.Super = reader.Get(TITLE + "Super", this.Super);

            this.Delay = reader.Get(TITLE + "Delay", this.Delay);
            this.TriggeredTimes = reader.Get(TITLE + "TriggeredTimes", this.TriggeredTimes);
            this.ClearTarget = reader.Get(TITLE + "ClearTarget", this.ClearTarget);
            this.MoveForward = reader.Get(TITLE + "MoveForward", this.MoveForward);

            this.RangeMin = reader.Get(TITLE + "RangeMin", this.RangeMin);
            this.RangeMax = reader.Get(TITLE + "RangeMax", this.RangeMax);
            this.Distance = reader.Get(TITLE + "Distance", this.Distance);
            if (Distance > 0)
            {
                Distance *= 256;
            }
        }

    }


}
