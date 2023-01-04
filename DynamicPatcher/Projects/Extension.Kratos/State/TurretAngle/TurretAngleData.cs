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
    public enum DeathZoneAction
    {
        CLEAR, BLOCK, TURN
    }

    public class DeathZoneActionParser : KEnumParser<DeathZoneAction>
    {
        public override bool ParseInitials(string t, ref DeathZoneAction buffer)
        {
            switch (t)
            {
                case "B":
                    buffer = DeathZoneAction.BLOCK;
                    return true;
                case "T":
                    buffer = DeathZoneAction.TURN;
                    return true;
                default:
                    buffer = DeathZoneAction.CLEAR;
                    return true;
            }
        }
    }

    [Serializable]
    public class TurretAngleData : INIConfig
    {

        static TurretAngleData()
        {
            new DeathZoneActionParser().Register();
        }

        private const string TITLE = "Turret.";

        public bool Enable;

        public Point2D Angle;
        public DeathZoneAction Action;

        public TurretAngleData()
        {
            this.Enable = false;

            this.Angle = default;
            this.Action = DeathZoneAction.CLEAR;
        }

        public override void Read(IConfigReader reader)
        {
            this.Angle = reader.Get(TITLE + "Angle", this.Angle);
            // 计算死区
            if (default != Angle)
            {
                // 最大值
                int max = Angle.X;
                if (max < 0)
                {
                    max = Math.Abs(max);
                }
                if (max > 180)
                {
                    max = 180;
                }
                else if (max == 0)
                {
                    max = 360;
                }
                else
                {
                    max = 360 - max;
                }
                // 最小值
                int min = Angle.Y;
                if (min < 0)
                {
                    min = Math.Abs(min);
                }
                if (min > 180)
                {
                    min = 180;
                }
                // 死区
                this.Angle.X = min;
                this.Angle.Y = max;
            }
            this.Enable = default != Angle && (Angle.Y - Angle.X) > 0;

            this.Action = reader.Get(TITLE + "Action", this.Action);
        }
    }


}
