using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public delegate bool FireBulletToTarget(int index, int burst, Pointer<BulletClass> pBullet, Pointer<AbstractClass> pTarget);

    /*
     * 改用 PatcherYRpp.Utilities.MathEx
    public static class EXMath
    {
        public const double HalfPI = Math.PI / 2;
        public const double DEG_TO_RAD = Math.PI / 180;
        public const double BINARY_ANGLE_MAGIC = -(360.0 / (65535 - 1)) * DEG_TO_RAD;

        public static double Deg2Rad(double degrees)
        {
            return degrees * DEG_TO_RAD;
        }

        public static double Rad2Deg(double radians)
        {
            return radians / DEG_TO_RAD;
        }
    }
    */

    public static partial class ExHelper
    {
        // public static Random Random = new Random(114514);
        public const double BINARY_ANGLE_MAGIC = -(360.0 / (65535 - 1)) * (Math.PI / 180);


        public static Dictionary<Point2D, int> MakeTargetPad(this int[] weights, int count, out int maxValue)
        {
            int weightCount = null != weights ? weights.Length : 0;
            Dictionary<Point2D, int> targetPad = new Dictionary<Point2D, int>();
            maxValue = 0;
            // 将所有的概率加起来，获得上游指标
            for (int index = 0; index < count; index++)
            {
                Point2D target = new Point2D();
                target.X = maxValue;
                int weight = 1;
                if (weightCount > 0 && index < weightCount)
                {
                    int w = weights[index];
                    if (w > 0)
                    {
                        weight = w;
                    }
                }
                maxValue += weight;
                target.Y = maxValue;
                targetPad.Add(target, index);
            }
            return targetPad;
        }

        public static int Hit(this Dictionary<Point2D, int> targetPad, int maxValue)
        {
            int index = 0;
            int p = MathEx.Random.Next(0, maxValue);
            foreach (var target in targetPad)
            {
                Point2D tKey = target.Key;
                if (p >= tKey.X && p < tKey.Y)
                {
                    index = target.Value;
                    break;
                }
            }
            return index;
        }

        public static bool Bingo(this double[] chances, int index)
        {
            if (null == chances || chances.Length < index + 1)
            {
                return true;
            }
            double chance = chances[index];
            return chance.Bingo();
        }

        public static bool Bingo(this double chance)
        {
            if (chance <= 0)
            {
                return false;
            }
            return chance >= 1 || chance >= MathEx.Random.NextDouble();
        }

        public static int Category(this LandType landType)
        {
            switch (landType)
            {
                case LandType.Rock:
                case LandType.Wall:
                    // 不可用
                    return 0;
                case LandType.Water:
                    // 水
                    return 2;
                case LandType.Beach:
                    // 两栖
                    return 3;
                default:
                    // 陆地
                    return 1;
            }
        }

        public static int CountAircraft(Pointer<HouseClass> pHouse, List<String> padList)
        {
            int count = 0;
            AircraftClass.Array.FindObject((pTarget) =>
            {
                if (padList.Contains(pTarget.Ref.Type.Ref.Base.Base.Base.ID)
                    && pTarget.Ref.Type.Ref.AirportBound)
                {
                    count++;
                }
                return false;
            }, default, 0, pHouse);
            return count;
        }

        public static CoordStruct OneCellOffsetToTarget(CoordStruct sourcePos, CoordStruct targetPos)
        {
            double angle = Math.Atan2((targetPos.Y - sourcePos.Y), (targetPos.X - sourcePos.X));
            int y = (int)(256 * Math.Tan(angle));
            int x = (int)(256 / Math.Tan(angle));
            CoordStruct offset = new CoordStruct();
            if (y == 0)
            {
                offset.Y = 0;
                if (angle < Math.PI)
                {
                    offset.X = 256;
                }
                else
                {
                    offset.X = -256;
                }
            }
            else if (x == 0)
            {
                offset.X = 0;
                if (angle < 0)
                {
                    offset.Y = -256;
                }
                else
                {
                    offset.Y = 256;
                }
            }
            else
            {
                if (Math.Abs(x) <= 256)
                {
                    offset.X = x;
                    if (angle > 0)
                    {
                        offset.Y = 256;
                    }
                    else
                    {
                        offset.X = -offset.X;
                        offset.Y = -256;
                    }
                }
                else
                {
                    offset.Y = y;
                    if (Math.Abs(angle) < 0.5 * Math.PI)
                    {
                        offset.X = 256;
                    }
                    else
                    {
                        offset.X = -256;
                        offset.Y = -offset.Y;
                    }
                }
            }
            return offset;
        }

        public static DirStruct DirNormalized(int index, int facing)
        {
            double radians = MathEx.Deg2Rad((-360 / facing * index));
            DirStruct dir = new DirStruct();
            dir.SetValue((short)(radians / BINARY_ANGLE_MAGIC));
            return dir;
        }

        public static int Dir2FacingIndex(DirStruct dir, int facing)
        {
            uint bits = (uint)Math.Round(Math.Sqrt(facing), MidpointRounding.AwayFromZero);
            double face = dir.GetValue(bits);
            double x = (face / (1 << (int)bits)) * facing;
            int index = (int)Math.Round(x, MidpointRounding.AwayFromZero);
            return index;
        }

        public static DirStruct Point2Dir(CoordStruct sourcePos, CoordStruct targetPos)
        {
            // get angle
            double radians = Math.Atan2(sourcePos.Y - targetPos.Y, targetPos.X - sourcePos.X);
            // Magic form tomsons26
            radians -= MathEx.Deg2Rad(90);
            return Radians2Dir(radians);
        }

        public static DirStruct Radians2Dir(double radians)
        {
            short d = (short)(radians / BINARY_ANGLE_MAGIC);
            return new DirStruct(d);
        }
    }
}
