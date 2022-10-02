using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    [Flags]
    public enum Relation
    {
        NONE = 0x0, OWNER = 0x1, ALLIES = 0x2, ENEMIES = 0x4,

        Team = OWNER | ALLIES,
        NotAllies = OWNER | ENEMIES,
        NotOwner = ALLIES | ENEMIES,
        All = OWNER | ALLIES | ENEMIES
    }
    public class RelationParser : KEnumParser<Relation>
    {
        public override bool ParseInitials(string t, ref Relation buffer)
        {
            switch(t)
            {
                case "O":
                    buffer = Relation.OWNER;
                    return true;
                case "A":
                    buffer = Relation.Team;
                    return true;
                case "E":
                    buffer = Relation.ENEMIES;
                    return true;
            }
            return false;
        }
    }

    public static partial class ExHelper
    {

        public static bool IsCivilian(this Pointer<HouseClass> pHouse)
        {
            return pHouse.IsNull || pHouse.Ref.Defeated || pHouse.Ref.Type.IsNull
                || HouseClass.CIVILIAN == pHouse.Ref.Type.Ref.Base.ID
                || HouseClass.SPECIAL == pHouse.Ref.Type.Ref.Base.ID; // 被狙掉驾驶员的阵营是Special
        }

        public static Relation GetRelationWithPlayer(this Pointer<HouseClass> pHouse)
        {
            return pHouse.GetRelation(HouseClass.Player);
        }

        public static Relation GetRelation(this Pointer<HouseClass> pHosue, Pointer<HouseClass> pTargetHouse)
        {
            if (pHosue == pTargetHouse)
            {
                return Relation.OWNER;
            }
            if (pHosue.Ref.IsAlliedWith(pTargetHouse))
            {
                return Relation.ALLIES;
            }
            return Relation.ENEMIES;
        }

        public static CoordStruct ToCoordStruct(this BulletVelocity bulletVelocity)
        {
            return new CoordStruct(bulletVelocity.X, bulletVelocity.Y, bulletVelocity.Z);
        }

        public static CoordStruct ToCoordStruct(this SingleVector3D vector3D)
        {
            return new CoordStruct(vector3D.X, vector3D.Y, vector3D.Z);
        }

        public static BulletVelocity ToBulletVelocity(this CoordStruct coord)
        {
            return new BulletVelocity(coord.X, coord.Y, coord.Z);
        }

        public static BulletVelocity ToBulletVelocity(this SingleVector3D vector3D)
        {
            return new BulletVelocity(vector3D.X, vector3D.Y, vector3D.Z);
        }

        public static SingleVector3D ToSingleVector3D(this CoordStruct coord)
        {
            return new SingleVector3D(coord.X, coord.Y, coord.Z);
        }

        public static Point2D ToClientPos(this CoordStruct coord)
        {
            return TacticalClass.Instance.Ref.CoordsToClient(coord);
        }

        public static SingleVector3D ToSingleVector3D(this BulletVelocity bulletVelocity)
        {
            return new SingleVector3D(bulletVelocity.X, bulletVelocity.Y, bulletVelocity.Z);
        }

        public static ColorStruct ToColorAdd(this ColorStruct color)
        {
            int B = color.B >> 3;
            int G = color.G >> 2;
            int R = color.R >> 3;
            return new ColorStruct(R, G, B);
        }

        public static uint Add2RGB565(this ColorStruct colorAdd)
        {
            string R2 = Convert.ToString(colorAdd.R, 2).PadLeft(5, '0');
            string G2 = Convert.ToString(colorAdd.G, 2).PadLeft(6, '0');
            string B2 = Convert.ToString(colorAdd.B, 2).PadLeft(5, '0');
            string c2 = R2 + G2 + B2;
            return Convert.ToUInt32(c2, 2);
        }

        public static int GetRandomValue(this Point2D point, int defVal)
        {
            int min = point.X;
            int max = point.Y;
            if (min > max)
            {
                min = max;
                max = point.X;
            }
            if (max > 0)
            {
                return MathEx.Random.Next(min, max);
            }
            return defVal;
        }

    }

}