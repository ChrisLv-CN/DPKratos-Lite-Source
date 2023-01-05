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
                bool isCloseEnough = pTechno.Ref.IsCloseEnoughToAttack(pTarget);
                CoordStruct sourcePos = location;
                CoordStruct targetPos = pTarget.Ref.GetCoords();
                // 目标所在方向
                DirStruct targetDir = sourcePos.Point2Dir(targetPos);
                // 车体朝向方向
                DirStruct bodyDir = pTechno.Ref.Facing.current();
                // 取夹角的度数值
                int delta = bodyDir.IncludedAngle360(targetDir, out int bodyDirIndex, out int targetDirIndex);
                // 启用侧舷接敌
                if (turretAngleData.AutoTurn && isCloseEnough)
                {
                    TryTurnBodyToAngle(bodyDir, targetDir, bodyDirIndex, delta);
                }
                // 启用炮塔限界
                if (turretAngleData.AngleLimit)
                {
                    // 判断是否在死区内
                    int min = turretAngleData.Angle.X;
                    int max = turretAngleData.Angle.Y;
                    if (delta > min && delta < max)
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
                                if (isCloseEnough)
                                {
                                    // 转动车身朝向目标
                                    TryTurnBodyToAngle(bodyDir, targetDir, bodyDirIndex, delta);
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
            int targetAngle = TurretAngleData.GetTurnAngle(delta, min, max);
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

        /// <summary>
        /// 如果启用侧舷接敌，则计算侧舷的可用角度，否则直接把头怼过去
        /// </summary>
        /// <param name="bodyDir"></param>
        /// <param name="targetDir"></param>
        /// <param name="bodyDirIndex"></param>
        /// <returns></returns>
        private bool TryTurnBodyToAngle(DirStruct bodyDir, DirStruct targetDir, int bodyDirIndex, int delta)
        {
            if ((!AmIStand() || null == StandData || !StandData.LockDirection || StandData.FreeDirection || isBuilding)
                && !isMoving
                && !pTechno.Ref.Facing.in_motion())
            {
                DirStruct turnDir = targetDir;
                // 启用侧舷接敌，计算侧舷角度和方位
                if (turretAngleData.AutoTurn)
                {
                    Point2D angleZone = default;
                    int targetAngle = bodyDir.IncludedAngle360(targetDir);
                    if (targetAngle >= 180)
                    {
                        // 目标在左区，正后方算左边
                        angleZone = turretAngleData.SideboardAngleL;
                    }
                    else
                    {
                        // 目标在右区，正前方算右边
                        angleZone = turretAngleData.SideboardAngleR;
                    }
                    // 目标角度在死区内，旋转
                    if (targetAngle < angleZone.X || targetAngle > angleZone.Y)
                    {
                        int turnAngle = TurretAngleData.GetTurnAngle(delta, angleZone);
                        int turnDelta = 0;
                        if (turnAngle < targetAngle)
                        {
                            // 侧舷的角度在目标的左边，顺时针转差值
                            turnDelta = targetAngle - turnAngle;
                        }
                        else if (turnAngle > targetAngle)
                        {
                            // 侧舷的角度在目标的右边，逆时针转差值
                            turnDelta = 360 - (turnAngle - targetAngle);
                        }
                        // 算实际世界坐标角度
                        turnDelta += bodyDirIndex;
                        if (turnDelta > 360)
                        {
                            turnDelta -= 360;
                        }
                        if (turnDelta > 0)
                        {
                            turnDir = FLHHelper.DirNormalized(turnDelta, 360);
                        }

                        pTechno.Ref.Facing.turn(turnDir);
                    }
                }
                else
                {
                    pTechno.Ref.Facing.turn(turnDir);
                }
                return true;
            }
            return false;
        }

    }
}
