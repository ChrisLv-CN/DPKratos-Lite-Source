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

        public Point2D SideboardAngle;

        public TurretAngleData()
        {
            this.Enable = false;

            this.Angle = default;
            this.Action = DeathZoneAction.CLEAR;

            this.SideboardAngle = default;
        }

        public override void Read(IConfigReader reader)
        {
            this.Angle = reader.Get(TITLE + "Angle", this.Angle);
            // 计算死区
            if (default != Angle)
            {
                FormatAngle(ref Angle);
            }
            this.Enable = default != Angle && (Angle.Y - Angle.X) > 0;

            this.Action = reader.Get(TITLE + "Action", this.Action);
            this.SideboardAngle = reader.Get(TITLE + "SideboardAngle", this.SideboardAngle);
            if (default != SideboardAngle)
            {
                FormatAngle(ref SideboardAngle);
                // 侧舷角度在死区内，以死区为界
                if (SideboardAngle.X > Angle.X)
                {
                    SideboardAngle.X = Angle.X;
                }
                if (SideboardAngle.Y < Angle.Y)
                {
                    SideboardAngle.Y = Angle.Y;
                }
            }
        }

        /// <summary>
        /// 顺时针旋转，所以X是右，Y是左，与设定值颠倒
        /// </summary>
        /// <param name="angle"></param>
        private void FormatAngle(ref Point2D angle)
        {
            // 最大值
            int max = angle.X;
            if (max < 0)
            {
                max = -max;
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
            int min = angle.Y;
            if (min < 0)
            {
                min = -min;
            }
            if (min > 180)
            {
                min = 180;
            }
            angle.X = min;
            angle.Y = max;
        }
    }


}
