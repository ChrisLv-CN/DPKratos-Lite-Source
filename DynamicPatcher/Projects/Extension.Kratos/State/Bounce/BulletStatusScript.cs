using System.Data;
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

    public partial class BulletStatusScript
    {

        public State<BounceData> BounceState = new State<BounceData>();

        private BounceData bounceData;
        private bool isBounceSplit;
        private int bounceIndex;
        private CoordStruct bounceTargetPos;
        private float speedMultiple = 1.0f;

        public void InitState_Bounce()
        {
            // 初始化状态机
            if (!isBounceSplit && isArcing)
            {
                // 初始化状态机
                BounceData data = Ini.GetConfig<BounceData>(Ini.RulesDependency, section).Data;
                if (data.Enable)
                {
                    BounceState.Enable(data);
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} Bounce {data.Enable} {data.Chance} {data.Elasticity}");
                }
            }
        }

        public void OnUpdate_Trajectory_Bounce()
        {
            if (BounceState.IsActive() && !isBounceSplit)
            {
                this.bounceData = BounceState.Data;
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} Bounce IsActive {bounceData.Chance} {bounceData.Elasticity}");
            }
            if (isBounceSplit || BounceState.IsActive())
            {
                CoordStruct sourcePos = pBullet.Ref.Base.Base.GetCoords();
                if (pBullet.Ref.Base.GetHeight() > 0)
                {
                    // 检查撞悬崖反弹
                    CoordStruct nextPos = sourcePos + pBullet.Ref.Velocity.ToCoordStruct();

                    switch (PhysicsHelper.CanMoveTo(sourcePos, nextPos, true, out CoordStruct nextCellPos, out bool onBridge))
                    {
                        case PassError.HITWALL:
                            // 反弹
                            pBullet.Ref.Velocity.X *= -1;
                            pBullet.Ref.Velocity.Y *= -1;
                            pBullet.Ref.SourceCoords = sourcePos;
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 撞墙反弹 {sourcePos} {pBullet.Ref.TargetCoords}");
                            break;
                    }
                }
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} update sourcePos {pBullet.Ref.SourceCoords}, targetPos {pBullet.Ref.TargetCoords}, dist {pBullet.Ref.SourceCoords.DistanceFrom(pBullet.Ref.TargetCoords)}");
            }
        }

        public bool OnDetonate_Bounce(Pointer<CoordStruct> pCoords)
        {
            if (BounceState.IsActive() && !isBounceSplit)
            {
                this.bounceData = BounceState.Data;
            }
            // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 爆炸位置 {pBullet.Ref.Base.Base.GetCoords()} {pBullet.Ref.TargetCoords} {pCoords.Data}");
            return SpwanSplitCannon();
        }

        private bool SpwanSplitCannon()
        {
            if ((BounceState.IsActive() || isBounceSplit) && null != bounceData && bounceData.Enable
                && (bounceData.Times < 0 || bounceIndex < bounceData.Times)
                && (bounceData.Chance >= 1 || bounceData.Chance.Bingo())
            )
            {
                CoordStruct sourcePos = pBullet.Ref.SourceCoords;
                CoordStruct explodePos = pBullet.Ref.TargetCoords;
                CoordStruct tempSourcePos = sourcePos;
                tempSourcePos.Z = 0;
                CoordStruct tempExplodePos = explodePos;
                tempExplodePos.Z = 0;
                // 飞行距离
                double dist = tempSourcePos.DistanceFrom(tempExplodePos);
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 初始位置{tempSourcePos}，爆炸位置{tempExplodePos}，飞行距离{dist}");
                if (dist > 0 && !double.IsNaN(dist) && !double.IsInfinity(dist))
                {
                    double next = dist * bounceData.Elasticity;
                    double speed = !pBullet.Ref.WeaponType.IsNull ? pBullet.Ref.WeaponType.Ref.GetSpeed((int)next) : pBullet.Ref.Speed;
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 初始位置{tempSourcePos}，爆炸位置{tempExplodePos}，子抛射体距离是 {dist} * {bounceData.Elasticity} = {next}，飞行速度{speed}，限制距离 {bounceData.Limit}");
                    // 往前飞一半的距离
                    if (next > Math.Max(bounceData.Limit + 1, speed))
                    {
                        // 检查地形，能不能跳
                        if (MapClass.Instance.TryGetCellAt(explodePos, out Pointer<CellClass> pCell))
                        {
                            // 地形
                            if (bounceData.IsOnLandType(pCell, out LandType landType) && bounceData.IsOnTileType(pCell, out TileType tileType) && !bounceData.Stop(pCell, out bool rebound))
                            {
                                // 在允许的瓷砖类型内
                                Pointer<WeaponTypeClass> pWeapon = pBullet.Ref.WeaponType;
                                string weaponId = bounceData.Weapon;
                                if (!weaponId.IsNullOrEmptyOrNone())
                                {
                                    pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponId);
                                }
                                if (!pWeapon.IsNull)
                                {
                                    // 取子武器的发射位置
                                    // 发射位置离地2个重力的高，不然射不出
                                    CoordStruct nextSourcePos = explodePos;
                                    nextSourcePos.Z = pCell.Ref.GetCoordsWithBridge().Z;
                                    nextSourcePos.Z += RulesClass.Global().Gravity * 2;
                                    CoordStruct nextTargetPos = default;
                                    // 根据地形削减或加强弹性
                                    switch (landType)
                                    {
                                        case LandType.Clear:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Clear.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Rough:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Rough.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Road:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Road.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Water:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Water.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Rock:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Rock.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Wall:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Wall.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Tiberium:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Tiberium.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Weeds:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Weeds.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Beach:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Beach.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Ice:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Ice.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Railroad:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Railroad.ToString()).Data.Elasticity;
                                            break;
                                        case LandType.Tunnel:
                                            next *= Ini.GetConfig<BounceLandData>(Ini.RulesDependency, LandType.Tunnel.ToString()).Data.Elasticity;
                                            break;
                                    }
                                    // 根据地形和地块获取弹性指数和方向，获得子武器的目的地
                                    switch (tileType)
                                    {
                                        case TileType.Cliff:
                                        case TileType.DestroyableCliff:
                                            // 上坡还是下坡
                                            // 反弹
                                            // nextTargetPos = FLHHelper.GetForwardCoords(tempSourcePos, tempExplodePos, dist - next, dist);
                                            break;
                                        case TileType.Ramp:
                                            // 斜坡，弹的更高
                                            // next *= 0.5;
                                            // speedMultiple *= 0.5f;
                                            break;
                                    }
                                    // 再次确认削减之后的距离还可以跳
                                    if (next > Math.Max(bounceData.Limit + 1, speed))
                                    {
                                        if (default == nextTargetPos)
                                        {
                                            // 目标位置在炮口与目标连线的延长线外next的距离
                                            if (rebound)
                                            {
                                                // 翻转方向
                                                nextTargetPos = FLHHelper.GetForwardCoords(tempExplodePos, tempSourcePos, next, dist);
                                            }
                                            else
                                            {
                                                nextTargetPos = FLHHelper.GetForwardCoords(tempSourcePos, tempExplodePos, dist + next, dist);
                                            }
                                        }
                                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 延线取距离 {dist} + {next} = {dist + next} 的点 {nextTargetPos}");
                                        // 不能在同一格里跳，不然会直接判定为命中撞爆
                                        if (MapClass.Instance.TryGetCellAt(nextTargetPos, out Pointer<CellClass> pTargetCell) && pTargetCell != pCell)
                                        {
                                            CoordStruct cellPos = pTargetCell.Ref.GetCoordsWithBridge();
                                            nextTargetPos.Z = cellPos.Z;
                                            Pointer<AbstractClass> pTarget = pTargetCell.Convert<AbstractClass>();
                                            Pointer<BulletClass> pNewBullet = WeaponHelper.FireBulletTo(default, pBullet.Ref.Owner, pTarget, pSourceHouse, pWeapon, nextSourcePos, nextTargetPos);
                                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 跳弹已发射 {pNewBullet}，发射位置 {nextSourcePos}，目标位置 {nextTargetPos}, 两点距离 {nextTargetPos.DistanceFrom(nextSourcePos)}");
                                            if (!pNewBullet.IsNull)
                                            {
                                                if (pNewBullet.TryGetStatus(out BulletStatusScript status))
                                                {
                                                    status.ResetArcingVelocity(speedMultiple);
                                                    status.SetBounceData(bounceData);
                                                    status.isBounceSplit = true;
                                                    status.bounceIndex = bounceIndex + 1;
                                                    status.bounceTargetPos = nextTargetPos;
                                                    status.speedMultiple = speedMultiple;
                                                }
                                                // 播放动画
                                                if (!bounceData.ExpireAnim.IsNullOrEmptyOrNone())
                                                {
                                                    Pointer<AnimTypeClass> pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(bounceData.ExpireAnim);
                                                    if (!pAnimType.IsNull)
                                                    {
                                                        Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, nextSourcePos);
                                                        pAnim.Ref.Owner = pSourceHouse;
                                                    }
                                                }
                                            }
                                            return !bounceData.ExplodeOnHit;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void SetBounceData(BounceData bounceData)
        {
            BounceData data = Ini.GetConfig<BounceData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                this.bounceData = data;
            }
            else
            {
                this.bounceData = bounceData;
            }
        }
    }
}
