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

    public enum BulletType
    {
        UNKNOWN = 0,
        INVISO = 1,
        ARCING = 2,
        MISSILE = 3,
        ROCKET = 4,
        NOROT = 5,
        BOMB = 6
    }

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
                status.pSourceHouse = pHouse;
            }
        }

        // Inviso???????????????
        // Arcing ??? ROT>0 ????????????????????????
        // Arcing ??? ROT=0 ????????????????????????
        // Arcing ??? Vertical ????????????????????????
        // ROT>0 ??? Vertical ?????????????????????
        // ROT=0 ??? Vertical ????????????????????????SHP?????????????????????VXL???????????????
        public static BulletType WhatTypeAmI(this Pointer<BulletClass> pBullet)
        {
            Pointer<BulletTypeClass> pType;
            if (!pBullet.IsNull && !(pType = pBullet.Ref.Type).IsNull)
            {
                if (pType.Ref.Inviso)
                {
                    // Inviso???????????????
                    return BulletType.INVISO;
                }
                else if (pType.Ref.ROT > 0)
                {
                    // ??????????????????
                    if (pType.Ref.ROT == 1)
                    {
                        return BulletType.ROCKET;
                    }
                    return BulletType.MISSILE;
                }
                else if (pType.Ref.Vertical)
                {
                    // ????????????
                    return BulletType.BOMB;
                }
                else if (pType.Ref.Arcing)
                {
                    // ?????????Arcing
                    return BulletType.ARCING;
                }
                else if (pType.Ref.ROT == 0)
                {
                    // ?????????????????????ROT=0????????????????????????Arcing
                    return BulletType.NOROT;
                }
            }
            return default;
        }

        // public static unsafe bool AmIInviso(this Pointer<BulletClass> pBullet)
        // {
        //     return pBullet.Ref.Type.Ref.Inviso;
        // }

        public static unsafe bool AmIArcing(this Pointer<BulletClass> pBullet)
        {
            return pBullet.WhatTypeAmI() == BulletType.ARCING;
        }

        // public static unsafe bool AmIMissile(this Pointer<BulletClass> pBullet)
        // {
        //     return !pBullet.AmIArcing() && !pBullet.Ref.Type.Ref.Inviso && pBullet.Ref.Type.Ref.ROT > 1;
        // }

        // public static unsafe bool AmIRocket(this Pointer<BulletClass> pBullet)
        // {
        //     return !pBullet.AmIArcing() && !pBullet.Ref.Type.Ref.Inviso && pBullet.Ref.Type.Ref.ROT == 1;
        // }

        // public static unsafe bool AmIBomb(this Pointer<BulletClass> pBullet)
        // {
        //     Pointer<BulletTypeClass> pType = pBullet.Ref.Type;
        //     return !pBullet.AmIInviso() && !pBullet.AmIArcing() && pBullet.Ref.Type.Ref.ROT <= 0 && pBullet.Ref.Type.Ref.Vertical;
        // }

        public static bool CanAttack(this Pointer<BulletClass> pBullet, Pointer<TechnoClass> pTarget, bool isPassiveAcquire = false)
        {
            bool canAttack = false;
            Pointer<WarheadTypeClass> pWH = pBullet.Ref.WH;
            if (!pWH.IsNull)
            {
                // ????????????
                double versus = pWH.GetData().GetVersus(pTarget.Ref.Type.Ref.Base.Armor, out bool forceFire, out bool retaliate, out bool passiveAcquire);
                if (isPassiveAcquire)
                {
                    // ????????????????????????
                    canAttack = versus > 0.2 || passiveAcquire;
                }
                else
                {
                    canAttack = versus != 0.0;
                }
                // ???????????????
                if (canAttack && pTarget.InAir())
                {
                    canAttack = pBullet.Ref.Type.Ref.AA;
                }
            }
            return canAttack;
        }

    }
}
