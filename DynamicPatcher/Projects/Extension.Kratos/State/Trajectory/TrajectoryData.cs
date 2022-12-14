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
    public enum SubjectToGroundType
    {
        AUTO = 0, YES = 1, NO = 2
    }

    public class SubjectToGroundTypeParser : KEnumParser<SubjectToGroundType>
    {
        public override bool ParseInitials(string t, ref SubjectToGroundType buffer)
        {
            switch (t)
            {
                case "Y":
                case "T":
                case "1":
                    buffer = SubjectToGroundType.YES;
                    return true;
                case "N":
                case "F":
                case "0":
                    buffer = SubjectToGroundType.NO;
                    return true;
            }
            return false;
        }
    }


    public class TrajectoryData : INIAutoConfig
    {

        static TrajectoryData()
        {
            new SubjectToGroundTypeParser().Register();
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
        public int Gravity = RulesClass.Global().Gravity;

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
        public SubjectToGroundType SubjectToGround = SubjectToGroundType.AUTO;

        public bool IsStraight()
        {
            return Straight || AbsolutelyStraight;
        }

    }


}
