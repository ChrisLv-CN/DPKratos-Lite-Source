using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public enum SubjectToGround
    {
        AUTO = 0, YES = 1, NO = 2
    }

    public class SubjectToGroundParser : KEnumParser<SubjectToGround>
    {
        public override bool ParseInitials(string t, ref SubjectToGround buffer)
        {
            switch (t)
            {
                case "Y":
                case "T":
                case "1":
                    buffer = SubjectToGround.YES;
                    return true;
                case "N":
                case "F":
                case "0":
                    buffer = SubjectToGround.NO;
                    return true;
            }
            return false;
        }
    }


    public class TrajectoryData : INIAutoConfig
    {

        static TrajectoryData()
        {
            new SubjectToGroundParser().Register();
        }

        public bool AdvancedBallistics = true;

        // Arcing
        [INIField(Key = "Arcing.FixedSpeed")]
        public int ArcingFixedSpeed = 0;
        public bool Inaccurate = false;
        [INIField(Key = "BallisticScatter.Min")]
        public float BallisticScatterMin = 0;
        [INIField(Key = "BallisticScatter.Max")]
        public float BallisticScatterMax = 0;

        // Straight
        public bool Straight = false;
        public bool AbsolutelyStraight = false;

        // Missile
        [INIField(Key = "ROT.Reverse")]
        public bool ReverseVelocity = false;
        [INIField(Key = "ROT.ReverseZ")]
        public bool ReverseVelocityZ = false;
        [INIField(Key = "ROT.ShakeMultiplier")]
        public float ShakeVelocity = 0;

        // Status
        public SubjectToGround SubjectToGround = SubjectToGround.AUTO;

        public bool IsStraight()
        {
            return Straight || AbsolutelyStraight;
        }

    }


}
