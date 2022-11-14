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

    public partial class BulletStatusScript : IBlackHoleVictim
    {

        public BlackHoleState BlackHoleState = new BlackHoleState();

        public bool CaptureByBlackHole;

        private SwizzleablePointer<ObjectClass> pBlackHole = new SwizzleablePointer<ObjectClass>(IntPtr.Zero);
        private BlackHoleData blackHoleData;
        private TimerStruct blackHoleDamageDelay;

        public void InitState_BlackHole()
        {
            // 初始化状态机
            BlackHoleData BlackHoleData = Ini.GetConfig<BlackHoleData>(Ini.RulesDependency, section).Data;
            if (BlackHoleData.Enable)
            {
                BlackHoleState.Enable(BlackHoleData);
            }
        }

        public void OnUpdate_BlackHole()
        {
            // 黑洞吸人
            if (BlackHoleState.IsReady())
            {
                BlackHoleState.StartCapture(pBullet.Convert<ObjectClass>(), pSourceHouse);
            }
            // 被黑洞吸收中
            if (CaptureByBlackHole)
            {
                if (pBlackHole.IsNull
                    || !pBlackHole.Pointer.TryGetBlackHoleState(out BlackHoleState blackHoleState)
                    || !blackHoleState.IsActive()
                    || OutOfBlackHole(blackHoleState)
                    || !blackHoleState.IsOnMark(pBullet.Convert<ObjectClass>())
                )
                {
                    CancelBlackHole();
                }
                else
                {
                    CoordStruct blackHolePos = pBlackHole.Ref.Base.GetCoords();
                    if (blackHoleData.ChangeTarget)
                    {
                        pBullet.Ref.SetTarget(pBlackHole.Pointer.Convert<AbstractClass>());
                        pBullet.Ref.TargetCoords = blackHolePos;
                    }
                    // 加上偏移值
                    blackHolePos += blackHoleData.Offset;
                    // 获取一个从黑洞位置朝向预设目标位置的向量，该向量控制导弹的弹体朝向
                    if (pBullet.AmIArcing())
                    {
                        // 从当前位置朝向黑洞
                        CoordStruct sourcePos = pBullet.Ref.Base.Base.GetCoords();
                        pBullet.Ref.Velocity = WeaponHelper.GetBulletVelocity(sourcePos, blackHolePos);
                    }
                    else
                    {
                        // 从黑洞朝向预设目标位置
                        pBullet.Ref.Velocity = WeaponHelper.GetBulletVelocity(blackHolePos, pBullet.Ref.TargetCoords);
                    }
                    // 黑洞伤害
                    if (null != blackHoleData && blackHoleData.AllowDamageBullet && blackHoleData.Damage != 0 && !BlackHoleState.IsActive())
                    {
                        if (blackHoleDamageDelay.Expired())
                        {
                            blackHoleDamageDelay.Start(blackHoleData.DamageDelay);
                            // Logger.Log($"{Game.CurrentFrame} 黑洞对 [{section}]{pTechno} 造成伤害 准备中 Damage = {blackHoleData.Damage}, ROF = {blackHoleData.DamageDelay}, WH = {blackHoleData.DamageWH}");
                            TakeDamage(blackHoleData.Damage, false, false, true);
                        }
                    }
                }
            }
        }

        public override void OnLateUpdate()
        {
            if (CaptureByBlackHole)
            {
                // 强行移动导弹的位置
                CoordStruct sourcePos = pBullet.Ref.Base.Base.GetCoords();
                CoordStruct targetPos = pBlackHole.Pointer.GetFLHAbsoluteCoords(blackHoleData.Offset, blackHoleData.IsOnTurret);
                CoordStruct nextPos = targetPos;
                double dist = targetPos.DistanceFrom(sourcePos);
                // 获取捕获速度
                int speed = blackHoleData.GetCaptureSpeed(1);
                if (dist > speed)
                {
                    nextPos = FLHHelper.GetForwardCoords(sourcePos, targetPos, speed);
                }
                int deltaZ = sourcePos.Z - targetPos.Z;
                // 抛射体撞到地面
                bool canMove = pBullet.Ref.Base.GetHeight() > 0;
                // 检查悬崖
                if (canMove && MapClass.Instance.TryGetCellAt(nextPos, out Pointer<CellClass> pTargetCell))
                {
                    CoordStruct cellPos = pTargetCell.Ref.GetCoordsWithBridge();
                    if (cellPos.Z > nextPos.Z)
                    {
                        // 沉入地面
                        nextPos.Z = cellPos.Z;
                        // 检查悬崖
                        switch (pTargetCell.Ref.GetTileType())
                        {
                            case TileType.Cliff:
                            case TileType.DestroyableCliff:
                                // 悬崖上可以往悬崖下移动
                                canMove = deltaZ > 0;
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 行进路线遇到悬崖 {(canMove ? "可通过" : "不可通过")}");
                                break;
                        }
                    }
                    // 检查建筑
                    if (!blackHoleData.AllowPassBuilding)
                    {
                        Pointer<BuildingClass> pBuilding = pTargetCell.Ref.GetBuilding();
                        if (!pBuilding.IsNull)
                        {
                            canMove = !pBuilding.CanHit(nextPos.Z);
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 行进路线遇到建筑 [{pBuilding.Ref.Type.Ref.Base.Base.Base.ID}] {pBuilding} {(canMove ? "可通过" : "不可通过")}");
                        }
                    }
                }
                if (!canMove)
                {
                    // 原地爆炸
                    pBullet.Ref.Detonate(sourcePos);
                    pBullet.Ref.Base.Remove();
                    pBullet.Ref.Base.UnInit();
                }
                else
                {
                    // 被黑洞吸走
                    pBullet.Ref.Base.SetLocation(nextPos);
                }
            }
        }

        public void SetBlackHole(Pointer<ObjectClass> pBlackHole, BlackHoleData blackHoleData)
        {
            if (!this.CaptureByBlackHole || null == this.blackHoleData || this.blackHoleData.Weight < blackHoleData.Weight || ((this.blackHoleData.Weight == blackHoleData.Weight || blackHoleData.Weight <= 0) && blackHoleData.CaptureFromSameWeight))
            {
                this.CaptureByBlackHole = true;
                this.pBlackHole.Pointer = pBlackHole;
                this.blackHoleData = blackHoleData;
            }
        }

        public void CancelBlackHole()
        {
            if (CaptureByBlackHole && !pBullet.IsDeadOrInvisible() && !LifeData.IsDetonate)
            {
                pBullet.Ref.SourceCoords = pBullet.Ref.Base.Base.GetCoords();
                // Arcing摔地上，导弹不管
                if (pBullet.AmIArcing())
                {
                    pBullet.Ref.Velocity = default;
                }
                else
                {
                    pBullet.RecalculateBulletVelocity();
                }
            }
            this.CaptureByBlackHole = false;
            this.blackHoleData = null;
            this.pBlackHole.Pointer = IntPtr.Zero;
        }

        private bool OutOfBlackHole(BlackHoleState blackHoleState)
        {
            CoordStruct taregtPos = pBlackHole.Ref.Base.GetCoords();
            CoordStruct sourcePos = pBullet.Ref.Base.Base.GetCoords();
            return blackHoleState.IsOutOfRange(taregtPos.DistanceFrom(sourcePos));
        }

    }
}
