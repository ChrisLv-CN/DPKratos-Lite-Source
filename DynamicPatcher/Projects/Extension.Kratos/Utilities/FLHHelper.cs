using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public static partial class ExHelper
    {

        public static CoordStruct GetFLH(CoordStruct source, CoordStruct flh, DirStruct dir, bool flip = false)
        {
            CoordStruct res = source;
            if (null != flh && default != flh && null != dir)
            {
                double radians = dir.radians();

                double rF = flh.X;
                double xF = rF * Math.Cos(-radians);
                double yF = rF * Math.Sin(-radians);
                CoordStruct offsetF = new CoordStruct((int)xF, (int)yF, 0);

                double rL = flip ? flh.Y : -flh.Y;
                double xL = rL * Math.Sin(radians);
                double yL = rL * Math.Cos(radians);
                CoordStruct offsetL = new CoordStruct((int)xL, (int)yL, 0);

                res = source + offsetF + offsetL + new CoordStruct(0, 0, flh.Z);
            }
            return res;
        }

        public static unsafe CoordStruct GetFLHAbsoluteCoords(CoordStruct source, CoordStruct flh, DirStruct dir, CoordStruct turretOffset = default)
        {
            CoordStruct res = source;
            if (null != flh && default != flh)
            {
                SingleVector3D offset = GetFLHAbsoluteOffset(flh, dir, turretOffset);
                res += offset.ToCoordStruct();
            }
            return res;
        }

        public static unsafe SingleVector3D GetFLHAbsoluteOffset(CoordStruct flh, DirStruct dir, CoordStruct turretOffset)
        {
            SingleVector3D offset = default;
            if (null != flh && default != flh)
            {
                Matrix3DStruct matrix3D = new Matrix3DStruct(true);
                matrix3D.Translate(turretOffset.X, turretOffset.Y, turretOffset.Z);
                matrix3D.RotateZ((float)dir.radians());
                offset = GetFLHOffset(ref matrix3D, flh);
            }
            return offset;
        }

        public static unsafe CoordStruct GetFLHAbsoluteCoords(this Pointer<TechnoClass> pTechno, CoordStruct flh, bool isOnTurret = true, int flipY = 1)
        {
            CoordStruct turretOffset = default;
            if (isOnTurret)
            {
                if (pTechno.TryGetImageConfig<TechnoTypeData>(out TechnoTypeData typeData))
                {
                    turretOffset = typeData.TurretOffset;
                }
                else
                {
                    turretOffset.X = pTechno.Ref.Type.Ref.TurretOffset;
                }
            }
            return GetFLHAbsoluteCoords(pTechno, flh, isOnTurret, flipY, turretOffset);
        }

        public static unsafe CoordStruct GetFLHAbsoluteCoords(this Pointer<TechnoClass> pTechno, CoordStruct flh, bool isOnTurret, int flipY, CoordStruct turretOffset)
        {
            if (pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Building)
            {
                // 建筑不能使用矩阵方法测算FLH
                return GetFLHAbsoluteCoords(pTechno.Ref.Base.Base.GetCoords(), flh, pTechno.Ref.Facing.current(), turretOffset);
            }
            else
            {
                SingleVector3D res = pTechno.Ref.Base.Base.GetCoords().ToSingleVector3D();

                if (null != flh && default != flh)
                {
                    // Step 1: get body transform matrix
                    Matrix3DStruct matrix3D = GetMatrix3D(pTechno);
                    // Step 2: move to turrretOffset
                    matrix3D.Translate(turretOffset.X, turretOffset.Y, turretOffset.Z);
                    // Step 3: rotation
                    RotateMatrix3D(ref matrix3D, pTechno, isOnTurret);
                    // Step 4: apply FLH offset
                    CoordStruct tempFLH = flh;
                    if (pTechno.Convert<AbstractClass>().Ref.WhatAmI() == AbstractType.Building)
                    {
                        tempFLH.Z += Game.LevelHeight;
                    }
                    tempFLH.Y *= flipY;
                    SingleVector3D offset = GetFLHOffset(ref matrix3D, tempFLH);
                    // Step 5: offset techno location
                    res += offset;
                }
                return res.ToCoordStruct();
            }
        }

        private static unsafe Matrix3DStruct GetMatrix3D(Pointer<TechnoClass> pTechno)
        {
            // Step 1: get body transform matrix
            Matrix3DStruct matrix3D = new Matrix3DStruct(true);
            ILocomotion loco = pTechno.Convert<FootClass>().Ref.Locomotor;
            if (null != loco)
            {
                loco.Draw_Matrix(Pointer<Matrix3DStruct>.AsPointer(ref matrix3D), IntPtr.Zero);
            }
            return matrix3D;
        }

        private static unsafe Matrix3DStruct RotateMatrix3D(ref Matrix3DStruct matrix3D, Pointer<TechnoClass> pTechno, bool isOnTurret)
        {
            // Step 2: rotation
            if (isOnTurret)
            {
                // 旋转到炮塔相同角度
                if (pTechno.Ref.HasTurret())
                {
                    DirStruct turretDir = pTechno.Ref.TurretFacing.current();
                    /*
                    double turretRad = (pTechno.Ref.GetTurretFacing().current(false).value32() - 8) * -(Math.PI / 16);
                    double bodyRad = (pTechno.Ref.GetRealFacing().current(false).value32() - 8) * -(Math.PI / 16);
                    float angle = (float)(turretRad - bodyRad);
                    matrix3D.RotateZ(angle);
                    */
                    if (pTechno.Convert<AbstractClass>().Ref.WhatAmI() == AbstractType.Building)
                    {
                        double turretRad = turretDir.radians();
                        matrix3D.RotateZ((float)turretRad);
                    }
                    else
                    {
                        // 旋转到0点，再转到炮塔的角度
                        matrix3D.RotateZ(-matrix3D.GetZRotation());
                        matrix3D.RotateZ((float)turretDir.radians());
                    }
                }
            }
            return matrix3D;
        }

        private static unsafe SingleVector3D GetFLHOffset(ref Matrix3DStruct matrix3D, CoordStruct flh)
        {
            // Step 4: apply FLH offset
            matrix3D.Translate(flh.X, flh.Y, flh.Z);
            SingleVector3D result = Game.MatrixMultiply(matrix3D);
            // Resulting FLH is mirrored along X axis, so we mirror it back - Kerbiter
            result.Y *= -1;
            return result;
        }

        #region 获取抛射体的朝向和FLH
        public static unsafe DirStruct Facing(this Pointer<BulletClass> pBullet)
        {
            CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
            return pBullet.Facing(location);
        }

        public static unsafe DirStruct Facing(this Pointer<BulletClass> pBullet, CoordStruct location)
        {
            CoordStruct forwardLocation = location + pBullet.Ref.Velocity.ToCoordStruct();
            return ExHelper.Point2Dir(location, forwardLocation);
        }

        public static unsafe CoordStruct GetFLHAbsoluteCoords(this Pointer<BulletClass> pBullet, CoordStruct flh, int flipY = 1)
        {
            CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
            DirStruct bulletFacing = pBullet.Facing(location);

            CoordStruct tempFLH = flh;
            tempFLH.Y *= flipY;
            return GetFLHAbsoluteCoords(location, tempFLH, bulletFacing);
        }
        #endregion

        #region 获取绑定在身上的相对位置
        public static unsafe LocationMark GetRelativeLocation(this Pointer<ObjectClass> pOwner, CoordStruct offset, int dir, bool isOnTurret, bool isOnWorld = false)
        {
            CoordStruct sourcePos = pOwner.Ref.Location;

            CoordStruct targetPos = sourcePos;
            DirStruct targetDir = default;
            if (isOnWorld)
            {
                // 绑定世界坐标，朝向固定北向
                targetDir = new DirStruct();
                targetPos = ExHelper.GetFLHAbsoluteCoords(sourcePos, offset, targetDir);
            }
            else
            {
                // 绑定单体坐标
                if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    targetDir = pTechno.GetDirectionRelative(dir, isOnTurret);
                    targetPos = pTechno.GetFLHAbsoluteCoords(offset, isOnTurret);
                }
                else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
                {
                    // 增加抛射体偏移值取下一帧所在实际位置
                    sourcePos += pBullet.Ref.Velocity.ToCoordStruct();
                    // 获取面向
                    targetDir = ExHelper.Point2Dir(sourcePos, pBullet.Ref.TargetCoords);
                    targetPos = ExHelper.GetFLHAbsoluteCoords(sourcePos, offset, targetDir);
                }
            }
            return new LocationMark(targetPos, targetDir);
        }

        public static DirStruct GetDirectionRelative(this Pointer<TechnoClass> pMaster, int dir, bool isOnTurret)
        {
            // turn offset
            DirStruct targetDir = ExHelper.DirNormalized(dir, 16);

            if (pMaster.CastToFoot(out Pointer<FootClass> pFoot))
            {
                double targetRad = targetDir.radians();

                DirStruct sourceDir = pMaster.Ref.Facing.current();
                if (pFoot.Ref.Locomotor.ToLocomotionClass().Ref.GetClassID() == LocomotionClass.Jumpjet)
                {
                    sourceDir = pFoot.Ref.Locomotor.ToLocomotionClass<JumpjetLocomotionClass>().Ref.LocomotionFacing.current();
                }
                if (isOnTurret || pFoot.Ref.Base.Base.Base.WhatAmI() == AbstractType.Aircraft) // WWSB Aircraft is a turret!!!
                {
                    sourceDir = pMaster.Ref.GetRealFacing().current();
                }
                double sourceRad = sourceDir.radians();
                float angle = (float)(sourceRad - targetRad);
                targetDir = ExHelper.Radians2Dir(angle);
            }

            return targetDir;
        }
        #endregion

    }

}