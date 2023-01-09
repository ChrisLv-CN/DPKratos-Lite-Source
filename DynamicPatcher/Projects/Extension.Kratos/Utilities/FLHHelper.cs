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

    public static class FLHHelper
    {

        // public static Random Random = new Random(114514);
        public const double BINARY_ANGLE_MAGIC = -(360.0 / (65535 - 1)) * (Math.PI / 180);

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

        public static unsafe CoordStruct GetFLHAbsoluteCoords(this Pointer<ObjectClass> pObject, CoordStruct flh, bool isOnTurret = true, int flipY = 1)
        {
            if (pObject.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                return pTechno.GetFLHAbsoluteCoords(flh, isOnTurret, flipY);
            }
            else if (pObject.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                return pBullet.GetFLHAbsoluteCoords(flh, flipY);
            }
            return default;
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

                if (default != flh || default != turretOffset)
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
            return Point2Dir(location, forwardLocation);
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
                targetPos = GetFLHAbsoluteCoords(sourcePos, offset, targetDir);
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
                    targetDir = Point2Dir(sourcePos, pBullet.Ref.TargetCoords);
                    targetPos = GetFLHAbsoluteCoords(sourcePos, offset, targetDir);
                }
            }
            return new LocationMark(targetPos, targetDir);
        }

        public static DirStruct GetDirectionRelative(this Pointer<TechnoClass> pMaster, int dir, bool isOnTurret)
        {
            // turn offset
            DirStruct targetDir = DirNormalized(dir, 16);

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
                targetDir = Radians2Dir(angle);
            }

            return targetDir;
        }
        #endregion

        #region 获取向量上指定距离的坐标
        public static CoordStruct GetForwardCoords(CoordStruct sourcePos, CoordStruct targetPos, double speed, double dist = 0)
        {
            return GetForwardCoords(sourcePos.ToSingleVector3D(), targetPos.ToSingleVector3D(), speed, dist);
        }

        public static CoordStruct GetForwardCoords(SingleVector3D sourceV, SingleVector3D targetV, double speed, double dist = 0)
        {
            if (dist <= 0)
            {
                dist = targetV.DistanceFrom(sourceV);
            }
            // 计算下一个坐标
            double d = speed / dist;
            double absX = Math.Abs(sourceV.X - targetV.X) * d;
            double x = sourceV.X;
            if (sourceV.X < targetV.X)
            {
                // Xa < Xb => Xa < Xc
                // Xc - Xa = absX
                x = absX + sourceV.X;
            }
            else if (sourceV.X > targetV.X)
            {
                // Xa > Xb => Xa > Xc
                // Xa - Xc = absX
                x = sourceV.X - absX;
            }
            double absY = Math.Abs(sourceV.Y - targetV.Y) * d;
            double y = sourceV.Y;
            if (sourceV.Y < targetV.Y)
            {
                y = absY + sourceV.Y;
            }
            else if (sourceV.Y > targetV.Y)
            {
                y = sourceV.Y - absY;
            }
            double absZ = Math.Abs(sourceV.Z - targetV.Z) * d;
            double z = sourceV.Z;
            if (sourceV.Z < targetV.Z)
            {
                z = absZ + sourceV.Z;
            }
            else if (sourceV.Z > targetV.Z)
            {
                z = sourceV.Z - absZ;
            }
            return new CoordStruct(x, y, z);
        }
        #endregion

        #region 范围内的随机坐标偏移
        /// <summary>
        /// 最小1格范围内的随机偏移坐标
        /// </summary>
        /// <param name="cellSpread"></param>
        /// <returns></returns>
        public static CoordStruct RandomOffset(double cellSpread)
        {
            int min = 0;
            int max = (int)((cellSpread > 0 ? cellSpread : 1) * 256);
            return RandomOffset(min, max);
        }

        public static CoordStruct RandomOffset(int min, int max)
        {
            double r = MathEx.Random.Next(min, max);
            if (r > 0)
            {
                double theta = MathEx.Random.NextDouble() * 2 * Math.PI;
                CoordStruct offset = new CoordStruct((int)(r * Math.Cos(theta)), (int)(r * Math.Sin(theta)), 0);
                return offset;
            }
            return default;
        }
        #endregion

        #region 计算朝向
        public static DirStruct DirNormalized(int index, int facing)
        {
            double radians = MathEx.Deg2Rad((-360 / facing * index));
            DirStruct dir = new DirStruct();
            dir.SetValue((short)(radians / BINARY_ANGLE_MAGIC));
            return dir;
        }

        /// <summary>
        /// 8的倍数分割面向，顺时针，0为↗，dir的0方向
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="facing"></param>
        /// <returns></returns>
        public static int Dir2FacingIndex(this DirStruct dir, int facing)
        {
            uint bits = (uint)Math.Round(Math.Sqrt(facing), MidpointRounding.AwayFromZero);
            double face = dir.GetValue(bits);
            double x = (face / (1 << (int)bits)) * facing;
            int index = (int)Math.Round(x, MidpointRounding.AwayFromZero);
            return index;
        }

        /// <summary>
        /// 0的方向是游戏中的北方，是↗，SHP素材0帧是朝向0点，是↑
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="facing"></param>
        /// <returns></returns>
        public static int Dir2FrameIndex(this DirStruct dir, int facing)
        {
            int index = dir.Dir2FacingIndex(facing);
            index = (int)(facing / 8) + index;
            if (index >= facing)
            {
                index -= facing;
            }
            return index;
        }

        public static DirStruct Point2Dir(this CoordStruct sourcePos, CoordStruct targetPos)
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

        #endregion
    }
}
