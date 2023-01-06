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
        public bool ChangeDefaultDir;
        public bool LockTurret;

        private IConfigWrapper<TurretAngleData> _turretAngleData;
        public TurretAngleData TurretAngleData
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

        public void OnPut_TurretAngle(Pointer<CoordStruct> pCoord, DirType dirType)
        {
            if ((isUnit || isBuilding) && TurretAngleData.Enable && hasTurret)
            {
                // 车体朝向方向
                DirStruct bodyDir = pTechno.Ref.Facing.current();
                LockTurretDir = bodyDir;
                if (ChangeDefaultDir = DefaultAngleIsChange(bodyDir))
                {
                    pTechno.Ref.TurretFacing.set(LockTurretDir);
                }
            }
            else
            {
                ChangeDefaultDir = false;
            }
        }

        public bool DefaultAngleIsChange(DirStruct bodyDir)
        {
            // 车体朝向方向，游戏限制只能划180份
            int bodyDirIndex = bodyDir.Dir2FacingIndex(180) * 2;
            if (ChangeDefaultDir = TryGetDefaultAngle(ref bodyDirIndex, out DirStruct newDefaultDir))
            {
                LockTurretDir = newDefaultDir;
            }
            return ChangeDefaultDir;
        }

        private bool TryGetDefaultAngle(ref int bodyDirIndex, out DirStruct newDefaultDir)
        {
            newDefaultDir = default;
            if (TurretAngleData.DefaultAngle > 0)
            {
                // 修改单位朝向指向虚拟方向
                bodyDirIndex += TurretAngleData.DefaultAngle;
                if (bodyDirIndex > 360)
                {
                    bodyDirIndex -= 360;
                }
                newDefaultDir = FLHHelper.DirNormalized(bodyDirIndex, 360);
                return true;
            }
            return false;
        }

        public void OnUpdate_TurretAngle()
        {
            if ((isUnit || isBuilding) && TurretAngleData.Enable && hasTurret)
            {
                CoordStruct sourcePos = location;
                // 车体朝向方向
                DirStruct bodyDir = pTechno.Ref.Facing.current();
                LockTurretDir = bodyDir;
                // 车体朝向方向，游戏限制只能划180份
                int bodyDirIndex = bodyDir.Dir2FacingIndex(180) * 2;

                if (ChangeDefaultDir = TryGetDefaultAngle(ref bodyDirIndex, out DirStruct newDefaultDir))
                {
                    LockTurretDir = newDefaultDir;
                }
                // 修改单位朝向指向虚拟方向，后面计算以该虚拟方向为正面
                bodyDir = LockTurretDir;
                // 攻击目标或者移动目标存在，指向
                Pointer<AbstractClass> pTarget = pTechno.Ref.Target;
                bool hasTarget = !pTarget.IsNull;
                if (!hasTarget)
                {
                    pTarget = pTechno.Convert<FootClass>().Ref.Destination;
                }
                if (!pTarget.IsNull)
                {
                    // 目标所在方向
                    DirStruct targetDir = pTechno.Ref.Base.Direction(pTarget);
                    // 游戏限制只能划180份，Index * 2
                    int targetDirIndex = targetDir.Dir2FacingIndex(180) * 2;
                    // 取夹角的度数值
                    int bodyTargetDelta = IncludedAngle360(bodyDirIndex, targetDirIndex);
                    // 目标在射程范围内
                    bool isCloseEnough = pTechno.Ref.IsCloseEnoughToAttack(pTarget);
                    // 启用侧舷接敌
                    if (hasTarget && isCloseEnough && TurretAngleData.AutoTurn)
                    {
                        TryTurnBodyToAngle(targetDir, bodyDirIndex, bodyTargetDelta);
                    }
                    // 启用炮塔限界
                    if (TurretAngleData.AngleLimit)
                    {
                        // 判断是否在死区内
                        int min = TurretAngleData.Angle.X;
                        int max = TurretAngleData.Angle.Y;

                        if (InDeadZone(bodyTargetDelta, min, max))
                        {
                            // 在死区内
                            DeathZoneAction action = TurretAngleData.Action;
                            // 移动前往目的地非攻击目标，改变策略卡住炮塔朝向
                            if ((!hasTarget || !isCloseEnough) && ChangeDefaultDir)
                            {
                                action = DeathZoneAction.BLOCK;
                            }
                            // Logger.Log($"{Game.CurrentFrame} 旋转角度 {delta} 在死区 {turretAngleData.Angle} 内");
                            switch (action)
                            {
                                case DeathZoneAction.BLOCK:
                                    BlockTurretFacing(bodyDir, bodyDirIndex, min, max, bodyTargetDelta);
                                    break;
                                case DeathZoneAction.TURN:
                                    BlockTurretFacing(bodyDir, bodyDirIndex, min, max, bodyTargetDelta);
                                    if (isCloseEnough)
                                    {
                                        // 转动车身朝向目标
                                        TryTurnBodyToAngle(targetDir, bodyDirIndex, bodyTargetDelta);
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
                            int range = max - min;
                            if (range > 0 && range <= 180)
                            {
                                LockTurret = ForceTurretToForward(bodyDir, bodyDirIndex, min, max, bodyTargetDelta);
                            }
                            else
                            {
                                LockTurret = false;
                            }
                        }
                    }
                }
            }
            else
            {
                LockTurret = false;
            }
        }

        private bool InDeadZone(int bodyTargetDelta, int min, int max)
        {
            return bodyTargetDelta > min && bodyTargetDelta < max;
        }

        private void BlockTurretFacing(DirStruct bodyDir, int bodyDirIndex, int min, int max, int bodyTargetDelta)
        {
            // 炮塔卡在限位上，取最靠近的一边
            int targetAngle = TurretAngleData.GetTurnAngle(bodyTargetDelta, min, max) + bodyDirIndex;
            if (targetAngle > 360)
            {
                targetAngle -= 360;
            }
            // 目标和本体朝向的角度
            LockTurretDir = FLHHelper.DirNormalized(targetAngle, 360);
            LockTurret = true;
            // 活区大于180，炮塔会从最近的位置转过去，穿过死区，强制转回前方绕过死区
            int angle = IncludedAngle360(bodyDirIndex, targetAngle);
            if (max - min <= 180)
            {
                ForceTurretToForward(bodyDir, bodyDirIndex, min, max, angle);
            }
        }

        private bool ForceTurretToForward(DirStruct bodyDir, int bodyDirIndex, int min, int max, int bodyTargetDelta)
        {
            // 检查炮塔朝向角度和目标朝向角度的差值，判断是否需要转回前方
            DirStruct turretDir = pTechno.Ref.TurretFacing.current();
            int turretDirIndex = FLHHelper.Dir2FacingIndex(turretDir, 180) * 2;
            int turretAngle = IncludedAngle360(bodyDirIndex, turretDirIndex);

            if (turretAngle > 180)
            {
                if (InDeadZone(turretAngle, min, max))
                {
                    // 目标也在左区，但是在死区范围内
                    int turnAngle = TurretAngleData.GetTurnAngle(turretAngle, min, max) + bodyDirIndex;
                    if (turnAngle > 360)
                    {
                        turnAngle -= 360;
                    }
                    // 逆时针回转到限位
                    pTechno.Ref.TurretFacing.set(turretDir);
                    LockTurretDir = FLHHelper.DirNormalized(turnAngle, 360);
                    return true;
                }
                else if (bodyTargetDelta < 180)
                {
                    // 炮塔在左区，如果目标在右区，顺时针回转
                    TurnToRight(turretAngle, bodyDirIndex, bodyDir);
                    return true;
                }
            }
            else if (turretAngle > 0)
            {
                if (InDeadZone(turretAngle, min, max))
                {
                    // 目标也在右区，但是在死区范围内
                    // 逆时针回转到限位
                    int turnAngle = TurretAngleData.GetTurnAngle(turretAngle, min, max) + bodyDirIndex;
                    if (turnAngle > 360)
                    {
                        turnAngle -= 360;
                    }
                    pTechno.Ref.TurretFacing.set(turretDir);
                    LockTurretDir = FLHHelper.DirNormalized(turnAngle, 360);
                    return true;
                }
                else if (bodyTargetDelta > 180)
                {
                    // 炮塔在右区，如果目标在左区，逆时针回转
                    TurnToLeft(turretAngle, bodyDirIndex, bodyDir);
                    return true;
                }
            }
            return false;
        }

        private void TurnToLeft(int turretAngle, int bodyDirIndex, DirStruct bodyDir)
        {
            // 逆时针回转
            int turnAngle = 0;
            if (turretAngle > 90)
            {
                turnAngle = turretAngle - 90 + bodyDirIndex;
                if (turnAngle > 360)
                {
                    turnAngle -= 360;
                }
                LockTurretDir = FLHHelper.DirNormalized(turnAngle, 360);
            }
            else
            {
                LockTurretDir = bodyDir;
            }
        }

        private void TurnToRight(int turretAngle, int bodyDirIndex, DirStruct bodyDir)
        {
            // 顺时针回转
            int turnAngle = 0;
            if (360 - turretAngle > 90)
            {
                turnAngle = turretAngle + 90 + bodyDirIndex;
                if (turnAngle > 360)
                {
                    turnAngle -= 360;
                }
                LockTurretDir = FLHHelper.DirNormalized(turnAngle, 360);
            }
            else
            {
                LockTurretDir = bodyDir;
            }
        }

        /// <summary>
        /// 如果启用侧舷接敌，则计算侧舷的可用角度，否则直接把头怼过去
        /// </summary>
        /// <param name="targetDir"></param>
        /// <param name="bodyDirIndex"></param>
        /// <param name="bodyTargetDelta"></param>
        /// <returns></returns>
        private bool TryTurnBodyToAngle(DirStruct targetDir, int bodyDirIndex, int bodyTargetDelta)
        {
            if ((!AmIStand() || null == StandData || !StandData.LockDirection || StandData.FreeDirection || isBuilding)
                && !isMoving
                && !pTechno.Ref.Facing.in_motion()
            )
            {
                DirStruct turnDir = targetDir;
                // 启用侧舷接敌，计算侧舷角度和方位
                if (TurretAngleData.AutoTurn)
                {
                    Point2D angleZone = default;
                    if (bodyTargetDelta >= 180)
                    {
                        // 目标在左区，正后方算左边
                        angleZone = TurretAngleData.SideboardAngleL;
                    }
                    else
                    {
                        // 目标在右区，正前方算右边
                        angleZone = TurretAngleData.SideboardAngleR;
                    }
                    // 目标角度在死区内，旋转
                    if (bodyTargetDelta < angleZone.X || bodyTargetDelta > angleZone.Y)
                    {
                        int turnAngle = TurretAngleData.GetTurnAngle(bodyTargetDelta, angleZone);
                        int turnDelta = 0;
                        if (turnAngle < bodyTargetDelta)
                        {
                            // 侧舷的角度在目标的左边，顺时针转差值
                            turnDelta = bodyTargetDelta - turnAngle;
                        }
                        else if (turnAngle > bodyTargetDelta)
                        {
                            // 侧舷的角度在目标的右边，逆时针转差值
                            turnDelta = 360 - (turnAngle - bodyTargetDelta);
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

        /// <summary>
        /// 计算targetDir相对于bodyDir之间的夹角
        /// 以360度划分圆，以bodyDir为0点，顺时针旋转，targetDir在哪一度
        /// </summary>
        /// <param name="bodyDir"></param>
        /// <param name="targetDir"></param>
        /// <returns></returns>
        public int IncludedAngle360(int bodyDirIndex, int targetDirIndex)
        {

            // 两个方向的差值即为旋转角度，正差顺时针，负差逆时针
            int delta = 0;
            if (bodyDirIndex > 180 && targetDirIndex < bodyDirIndex)
            {
                delta = 360 - bodyDirIndex + targetDirIndex;
            }
            else
            {
                delta = targetDirIndex - bodyDirIndex;
            }
            if (delta < 0)
            {
                delta = 360 + delta;
            }
            return delta;
        }
    }
}
