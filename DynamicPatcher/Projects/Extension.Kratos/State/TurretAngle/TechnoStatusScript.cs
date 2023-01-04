using System.Drawing;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class TechnoStatusScript
    {
        public DirStruct LockTurretDir;
        public bool LockTurret;

        private IConfigWrapper<TurretAngleData> _turretAngleData;
        private TurretAngleData turretAngleData
        {
            get
            {
                if (null == _turretAngleData)
                {
                    _turretAngleData = Ini.GetConfig<TurretAngleData>(Ini.RulesDependency, section);
                    hasTurret = pTechno.Ref.HasTurret() && !pTechno.Ref.Type.Ref.TurretSpins;
                }
                return _turretAngleData.Data;
            }
        }

        private bool hasTurret;

        public void OnUpdate_TurretAngle()
        {
            Pointer<AbstractClass> pTarget = IntPtr.Zero;
            if ((isUnit || isBuilding) && turretAngleData.Enable && hasTurret && !(pTarget = pTechno.Ref.Target).IsNull)
            {
                if (TargetInDeathZone(pTarget, out DirStruct bodyDir, out DirStruct targetDir, out int bodyDirIndex, out int targetDirIndex, out int min, out int max, out int delta))
                {
                    // 在死区内
                    // Logger.Log($"{Game.CurrentFrame} 旋转角度 {delta} 在死区 {turretAngleData.Angle} 内");
                    switch (turretAngleData.Action)
                    {
                        case DeathZoneAction.BLOCK:
                            BlockTurretFacing(bodyDir, bodyDirIndex, min, max, delta);
                            break;
                        case DeathZoneAction.TURN:
                            BlockTurretFacing(bodyDir, bodyDirIndex, min, max, delta);
                            // 转动车身朝向目标
                            if ((!AmIStand() || null == StandData || !StandData.LockDirection || StandData.FreeDirection || isBuilding)
                                && !isMoving
                                && !pTechno.Ref.Facing.in_motion())
                            {
                                TurnBodyToAngle(bodyDir, targetDir, bodyDirIndex);
                            }
                            break;
                        default:
                            pTechno.ClearAllTarget();
                            break;
                    }
                }
                else
                {
                    // 不在死区，但是如果死区过小，炮塔会从最近的位置转过去，穿过死区，强制转回前方绕过死区
                    int targetAngle = bodyDir.IncludedAngle360(targetDir);
                    if (max - min <= 180)
                    {
                        LockTurret = ForceTurretToForward(bodyDir, targetAngle);
                    }
                    else
                    {
                        LockTurret = false;
                    }
                }
            }
            else
            {
                LockTurret = false;
            }
        }

        public bool CanFire_TurretAngle(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon)
        {
            if (!pTarget.IsNull && (isUnit || isBuilding) && turretAngleData.Enable && hasTurret)
            {
                return TargetInDeathZone(pTarget, out DirStruct bodyDir, out DirStruct targetDir, out int bodyDirIndex, out int targetDirIndex, out int min, out int max, out int delta);
            }
            return false;
        }

        private bool TargetInDeathZone(Pointer<AbstractClass> pTarget, out DirStruct bodyDir, out DirStruct targetDir, out int bodyDirIndex, out int targetDirIndex, out int min, out int max, out int delta)
        {
            CoordStruct sourcePos = location;
            CoordStruct targetPos = pTarget.Ref.GetCoords();
            // 目标所在方向
            targetDir = sourcePos.Point2Dir(targetPos);
            // 车体朝向方向
            bodyDir = pTechno.Ref.Facing.current();
            // 取夹角的度数值
            delta = bodyDir.IncludedAngle360(targetDir, out bodyDirIndex, out targetDirIndex);
            // 判断是否在死区内
            min = turretAngleData.Angle.X;
            max = turretAngleData.Angle.Y;
            return delta > min && delta < max;
        }

        private void BlockTurretFacing(DirStruct bodyDir, int bodyDirIndex, int min, int max, int delta)
        {
            // 炮塔卡在限位上
            int targetAngle = 0;
            // 更靠近哪一边
            int length = max - min;
            if ((delta - min) < (length / 2))
            {
                // 靠近小的那一边
                targetAngle = min;
            }
            else
            {
                // 靠近大的那一边
                targetAngle = max;
            }
            targetAngle += bodyDirIndex;
            if (targetAngle > 360)
            {
                targetAngle -= 360;
            }
            // 目标和本体朝向的角度
            LockTurretDir = FLHHelper.DirNormalized(targetAngle, 360);
            LockTurret = true;
            // 活区大于180，炮塔会从最近的位置转过去，穿过死区，强制转回前方绕过死区
            int angle = bodyDir.IncludedAngle360(LockTurretDir);
            if (max - min <= 180)
            {
                ForceTurretToForward(bodyDir, angle);
            }
        }

        private bool ForceTurretToForward(DirStruct bodyDir, int targetAngle)
        {
            // 检查炮塔朝向角度和目标朝向角度的差值，判断是否需要转回前方
            int turretAngle = bodyDir.IncludedAngle360(pTechno.Ref.TurretFacing.current());
            if (turretAngle > 180)
            {
                // 炮塔在左区，如果目标在右区，则转到本体朝向
                if (targetAngle < 180)
                {
                    LockTurretDir = bodyDir;
                    return true;
                }
            }
            else if (turretAngle > 0)
            {
                // 炮塔在右区，如果目标在左区，则转到本体朝向
                if (targetAngle > 180)
                {
                    LockTurretDir = bodyDir;
                    return true;
                }
            }
            return false;
        }

        private void TurnBodyToAngle(DirStruct bodyDir, DirStruct targetDir, int bodyDirIndex)
        {
            DirStruct turnDir = targetDir;
            if (default != turretAngleData.SideboardAngle)
            {
                int delta = 0;
                int turnAngle = 0;
                int targetAngle = bodyDir.IncludedAngle360(targetDir);
                if (targetAngle > 180)
                {
                    // 目标在左区，大值
                    turnAngle = turretAngleData.SideboardAngle.Y;
                }
                else if (targetAngle > 0)
                {
                    // 目标在右区，小值
                    turnAngle = turretAngleData.SideboardAngle.X;
                }
                if (turnAngle < targetAngle)
                {
                    // 侧舷的角度在目标的左边，顺时针转差值
                    delta = targetAngle - turnAngle;
                }
                else if (turnAngle > targetAngle)
                {
                    // 侧舷的角度在目标的右边，逆时针转差值
                    delta = 360 - (turnAngle - targetAngle);
                }
                // 算实际世界坐标角度
                delta += bodyDirIndex;
                if (delta > 360)
                {
                    delta -= 360;
                }
                if (delta > 0)
                {
                    turnDir = FLHHelper.DirNormalized(delta, 360);
                }
            }
            pTechno.Ref.Facing.turn(turnDir);
        }

    }
}
