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

        public int DefaultAngle;

        public bool AngleLimit;
        public Point2D Angle;
        public DeathZoneAction Action;

        public bool AutoTurn;
        public Point2D SideboardAngleL;
        public Point2D SideboardAngleR;

        public TurretAngleData()
        {
            this.Enable = false;

            this.DefaultAngle = 0;

            this.AngleLimit = false;
            this.Angle = default;
            this.Action = DeathZoneAction.CLEAR;

            this.AutoTurn = false;
            this.SideboardAngleL = default;
            this.SideboardAngleR = default;
        }

        public override void Read(IConfigReader reader)
        {
            this.DefaultAngle = reader.Get(TITLE + "DefaultAngle", this.DefaultAngle);
            if (DefaultAngle < 0)
            {
                DefaultAngle = -DefaultAngle;
            }
            if (DefaultAngle >= 360)
            {
                DefaultAngle = 0;
            }

            this.Angle = reader.Get(TITLE + "Angle", this.Angle);
            // 计算死区
            if (default != Angle)
            {
                FormatAngle360(ref Angle);
            }
            this.AngleLimit = default != Angle && (Angle.Y - Angle.X) > 0;

            this.Action = reader.Get(TITLE + "Action", this.Action);
            this.SideboardAngleL = reader.Get(TITLE + "SideboardAngleL", this.SideboardAngleL);
            if (default != SideboardAngleL)
            {
                FormatAngle180(ref SideboardAngleL);
                // 换算成180-360，顺时针
                int min = SideboardAngleL.X;
                int max = SideboardAngleL.Y;
                min = 360 - min;
                max = 360 - max;
                SideboardAngleL.X = max;
                SideboardAngleL.Y = min;
                if (AngleLimit)
                {
                    // 存在死区，可以区域不能覆盖死区
                    if (SideboardAngleL.X < Angle.Y)
                    {
                        SideboardAngleL.X = Angle.Y;
                    }
                    if (SideboardAngleL.Y < Angle.Y)
                    {
                        SideboardAngleL.Y = Angle.Y;
                    }
                }
            }
            this.SideboardAngleR = reader.Get(TITLE + "SideboardAngleR", this.SideboardAngleR);
            if (default != SideboardAngleR)
            {
                FormatAngle180(ref SideboardAngleR);
                if (AngleLimit)
                {
                    // 存在死区，可以区域不能覆盖死区
                    if (SideboardAngleR.X > Angle.X)
                    {
                        SideboardAngleR.X = Angle.X;
                    }
                    if (SideboardAngleR.Y > Angle.X)
                    {
                        SideboardAngleR.Y = Angle.X;
                    }
                }
            }
            this.AutoTurn = (default != SideboardAngleL && (SideboardAngleL.X > 180 || SideboardAngleL.Y < 360)) || (default != SideboardAngleR && (SideboardAngleR.X > 0 || SideboardAngleR.Y < 180));

            this.Enable = DefaultAngle > 0 || AngleLimit || AutoTurn;
        }

        private void FormatAngle180(ref Point2D angle)
        {
            int min = angle.X;
            int max = angle.Y;
            if (min < 0)
            {
                min = -min;
            }
            if (min > 180)
            {
                min = 0;
            }
            if (max < 0)
            {
                max = -max;
            }
            if (max > 180 || max == 0)
            {
                max = 180;
            }
            if (max < min)
            {
                angle.X = max;
                angle.Y = min;
            }
            else
            {
                angle.X = min;
                angle.Y = max;
            }
        }

        /// <summary>
        /// 将X=[0,180]，Y=[0,180]，换算成[0,360]，重新分配给X=min，Y=max
        /// 顺时针旋转，所以X是右，Y是左，与设定值颠倒
        /// </summary>
        /// <param name="angle"></param>
        private void FormatAngle360(ref Point2D angle)
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

        /// <summary>
        /// 目标在死区范围内更靠近哪一边
        /// </summary>
        /// <param name="targetBodyDelta"></param>
        /// <param name="angleZone"></param>
        /// <returns></returns>
        public static int GetTurnAngle(int targetBodyDelta, Point2D angleZone)
        {
            int min = angleZone.X;
            int max = angleZone.Y;
            return GetTurnAngle(targetBodyDelta, min, max);
        }

        /// <summary>
        /// 目标在死区范围内更靠近哪一边
        /// </summary>
        /// <param name="targetBodyDelta"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetTurnAngle(int targetBodyDelta, int min, int max)
        {
            int turnAngle = 0;
            // 更靠近哪一边
            int length = max - min;
            if ((targetBodyDelta - min) < (length / 2))
            {
                // 靠近小的那一边
                turnAngle = min;
            }
            else
            {
                // 靠近大的那一边
                turnAngle = max;
            }
            return turnAngle;
        }

        /*
        public static void ShowLineForTest(Point2D angle, int bodyDirIndex, CoordStruct location, ColorStruct color, int length = 1024)
        {
            // 测试显示范围
            ShowLineForTest(angle.X, bodyDirIndex, location, color, length);
            ShowLineForTest(angle.Y, bodyDirIndex, location, color, length);
        }

        public static void ShowLineForTest(int angle, int bodyDirIndex, CoordStruct location, ColorStruct color, int length = 1024)
        {
            int index = angle + bodyDirIndex;
            if (index > 360)
            {
                index -= 360;
            }
            DirStruct dir = FLHHelper.DirNormalized(index, 360);
            CoordStruct targetPos = FLHHelper.GetFLHAbsoluteCoords(location, new CoordStruct(length, 0, 0), dir);
            BulletEffectHelper.DrawLine(location, targetPos, color, default, 1, 1);
            Surface.Primary.Ref.DrawText(index.ToString(), targetPos, color);
        }
        */
    }


}
