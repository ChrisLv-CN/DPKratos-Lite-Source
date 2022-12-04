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

    public static class BulletTypeHelper
    {
        public static unsafe BulletVelocity GetVelocity(this Pointer<BulletClass> pBullet)
        {
            return GetVelocity(pBullet.Ref.SourceCoords, pBullet.Ref.TargetCoords, pBullet.Ref.Speed);
        }

        public static unsafe BulletVelocity GetVelocity(CoordStruct sourcePos, CoordStruct targetPos, int speed)
        {
            BulletVelocity velocity = new BulletVelocity(targetPos.X - sourcePos.X, targetPos.Y - sourcePos.Y, targetPos.Z - sourcePos.Z);
            velocity *= speed / targetPos.DistanceFrom(sourcePos);
            return velocity;
        }

        public static unsafe BulletVelocity RecalculateBulletVelocity(this Pointer<BulletClass> pBullet)
        {
            CoordStruct targetPos = pBullet.Ref.TargetCoords;
            Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
            if (!pTarget.IsNull)
            {
                targetPos = pTarget.Ref.GetCoords();
            }
            return pBullet.RecalculateBulletVelocity(targetPos);
        }

        public static unsafe BulletVelocity RecalculateBulletVelocity(this Pointer<BulletClass> pBullet, CoordStruct targetPos)
        {
            return pBullet.RecalculateBulletVelocity(pBullet.Ref.Base.Base.GetCoords(), targetPos);
        }

        public static unsafe BulletVelocity RecalculateBulletVelocity(this Pointer<BulletClass> pBullet, CoordStruct sourcePos, CoordStruct targetPos)
        {
            BulletVelocity velocity = new BulletVelocity(targetPos.X - sourcePos.X, targetPos.Y - sourcePos.Y, targetPos.Z - sourcePos.Z);
            velocity *= pBullet.Ref.Speed / targetPos.DistanceFrom(sourcePos);
            pBullet.Ref.Velocity = velocity;
            pBullet.Ref.SourceCoords = sourcePos;
            pBullet.Ref.TargetCoords = targetPos;
            return velocity;
        }


        public static unsafe Pointer<HouseClass> GetHouse(this Pointer<BulletClass> pBullet)
        {
            Pointer<TechnoClass> pOwner = pBullet.Ref.Owner;
            if (!pOwner.IsNull)
            {
                return pOwner.Ref.Owner;
            }
            return pBullet.GetSourceHouse();
        }

        public static unsafe Pointer<HouseClass> GetSourceHouse(this Pointer<BulletClass> pBullet)
        {
            if (pBullet.TryGetStatus(out BulletStatusScript status))
            {
                return status.pSourceHouse;
            }
            Pointer<TechnoClass> pOwner = pBullet.Ref.Owner;
            if (!pOwner.IsNull)
            {
                return pOwner.Ref.Owner;
            }
            return IntPtr.Zero;
        }

        public static unsafe void SetSourceHouse(this Pointer<BulletClass> pBullet, Pointer<HouseClass> pHouse)
        {
            if (!pHouse.IsNull && pBullet.TryGetStatus(out BulletStatusScript status))
            {
                status.pSourceHouse.Pointer = pHouse;
            }
        }

        public static unsafe bool AmIArcing(this Pointer<BulletClass> pBullet)
        {
            return pBullet.Ref.Type.Ref.Arcing || (pBullet.Ref.Type.Ref.ROT <= 0 && !pBullet.Ref.Type.Ref.Inviso);
        }

        public static unsafe bool AmIMissile(this Pointer<BulletClass> pBullet)
        {
            return !pBullet.AmIArcing() && !pBullet.Ref.Type.Ref.Inviso && pBullet.Ref.Type.Ref.ROT > 1;
        }

        public static unsafe bool AmIRocket(this Pointer<BulletClass> pBullet)
        {
            return !pBullet.AmIArcing() && !pBullet.Ref.Type.Ref.Inviso && pBullet.Ref.Type.Ref.ROT == 1;
        }

    }
}
