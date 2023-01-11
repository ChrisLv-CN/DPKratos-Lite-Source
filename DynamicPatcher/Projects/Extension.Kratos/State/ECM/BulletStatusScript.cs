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

        public ECMState ECMState = new ECMState();

        private TimerStruct ECMLockTimer;

        public void InitState_ECM()
        {
            // 初始化状态机
            if (isMissile)
            {
                // 初始化状态机
                ECMData data = Ini.GetConfig<ECMData>(Ini.RulesDependency, section).Data;
                if (data.Enable)
                {
                    ECMState.Enable(data);
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} ECM {data.Enable} {data.Chance} {data.Elasticity}");
                }
            }
        }

        public void OnUpdate_ECM()
        {
            if (ECMState.IsReady() && ECMLockTimer.Expired())
            {
                // 重置目标
                ECMData data = ECMState.Data;
                if (data.Chance.Bingo())
                {
                    if (data.Feedback)
                    {
                        // 忽悠炮，艹他
                        Pointer<TechnoClass> pOwner = pSource;
                        pBullet.Ref.SetTarget(pOwner.Convert<AbstractClass>());
                        if (pOwner.IsNull)
                        {
                            pBullet.Ref.TargetCoords = pBullet.Ref.SourceCoords;
                        }
                        else
                        {
                            pBullet.Ref.TargetCoords = pOwner.Ref.Base.Base.GetCoords();
                        }
                        pBullet.Ref.Owner = IntPtr.Zero;
                    }
                    else
                    {
                        Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
                        // 确定搜索的圆心
                        CoordStruct location = default;
                        bool pickFormArounds = false;
                        // 根据设置取得圆心
                        switch (data.Around)
                        {
                            case ECMAround.Source:
                                // 获取来源，如果没有就跳过
                                CoordStruct targetLocation = default;
                                if (ECMState.TryGetSourceLocation(out targetLocation))
                                {
                                    location = targetLocation;
                                    pickFormArounds = true;
                                }
                                break;
                            case ECMAround.Target:
                                // 获取当前目标的位置，没有就跳过
                                if (!pTarget.IsNull)
                                {
                                    location = pTarget.Ref.GetCoords();
                                    pickFormArounds = true;
                                }
                                break;
                            case ECMAround.Self:
                                // 获取抛射体自身的位置，肯定有
                                location = pBullet.Ref.Base.Base.GetCoords();
                                pickFormArounds = true;
                                break;
                            case ECMAround.Shooter:
                                // 获取发射者的位置，没有就跳过
                                if (!pSource.IsNull)
                                {
                                    location = pSource.Ref.Base.Base.GetCoords();
                                    pickFormArounds = true;
                                }
                                break;
                        }
                        if (!pickFormArounds)
                        {
                            // 没有从Arounds设置里获得，以自身为目标搜索
                            location = pBullet.Ref.Base.Base.GetCoords();
                        }
                        // 开始搜索新目标
                        bool pickTechno = false;
                        bool forceToGround = false;
                        // 一定概率选择一个单位做目标，而不是地面
                        if (pickTechno = data.ToTechnoChance.Bingo())
                        {
                            List<Pointer<TechnoClass>> targetList = new List<Pointer<TechnoClass>>();
                            // 强制重置，那么排除掉原目标
                            Pointer<ObjectClass> pExclude = data.ForceRetarget ? pTarget.Convert<ObjectClass>() : IntPtr.Zero;
                            // 是否搜索空中单位由抛射体AA决定
                            data.AffectInAir = pBullet.Ref.Type.Ref.AA;
                            // 搜索范围内的一个单位作为新目标
                            FinderHelper.FindTechnoOnMark((pTarget, AEManger) =>
                            {
                                targetList.Add(pTarget);
                                return false;
                            }, location, data.RangeMax, data.RangeMin, data.FullAirspace, pSourceHouse, data, pExclude);
                            int count = targetList.Count();
                            if (count > 0)
                            {
                                Pointer<AbstractClass> pNewTarget = IntPtr.Zero;
                                bool found = false;
                                int times = 0;
                                if (times < count)
                                {
                                    // 循环一轮，随机查找目标
                                    for (int i = 0; i < count; i++)
                                    {
                                        // 至少会找到一个，可能是原目标，取一个随机数，就它了
                                        int index = MathEx.Random.Next(count);
                                        // Logger.Log($"{Game.CurrentFrame} 获得范围内搜索到的目标 {i + 1} / {count}");
                                        // 检查选择的新目标能不能打
                                        Pointer<TechnoClass> pTryTarget = targetList[index];
                                        if (pBullet.CanAttack(pTryTarget))
                                        {
                                            // 就是你了
                                            pNewTarget = pTryTarget.Convert<AbstractClass>();
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (!found)
                                    {
                                        // 纳尼？随机居然没有找到，按顺序一个个找
                                        for (int j = 0; j < count; j++)
                                        {
                                            // 检查选择的新目标能不能打
                                            Pointer<TechnoClass> pTryTarget = targetList[j];
                                            if (pBullet.CanAttack(pTryTarget))
                                            {
                                                // 就是你了
                                                pNewTarget = pTryTarget.Convert<AbstractClass>();
                                                found = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (found)
                                {
                                    ResetTarget(pNewTarget, default);
                                    if (data.NoOwner)
                                    {
                                        pBullet.Ref.Owner = IntPtr.Zero;
                                    }
                                }
                                else
                                {
                                    // 一个目标都没找到
                                    forceToGround = true;
                                }
                            }
                            else
                            {
                                // 一个目标都没找到
                                forceToGround = true;
                            }
                        }
                        // 没有找到新的单位作为目标，则挑一个地板
                        if (forceToGround || !pickTechno)
                        {
                            // 随机搜索附近的一个地面作为新目标
                            CoordStruct targetPos = location;
                            targetPos += FLHHelper.RandomOffset(data.RangeMax, data.RangeMin);
                            // 将偏移后的坐标对应的格子，设置为新的目标
                            if (MapClass.Instance.TryGetCellAt(targetPos, out Pointer<CellClass> pCell))
                            {
                                ResetTarget(pCell.Convert<AbstractClass>(), pCell.Ref.GetCoordsWithBridge());
                                if (data.NoOwner)
                                {
                                    pBullet.Ref.Owner = IntPtr.Zero;
                                }
                            }
                        }
                    }
                    // 进入锁定
                    if (data.LockDuration > 0)
                    {
                        ECMLockTimer.Start(data.LockDuration);
                    }
                }
                ECMState.Reload();
            }
        }
    }
}
