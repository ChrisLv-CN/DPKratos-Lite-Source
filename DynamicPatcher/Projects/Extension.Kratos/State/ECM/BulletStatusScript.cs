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
            if (ECMState.IsReady())
            {
                // 重置目标
                ECMData data = ECMState.Data;
                if (data.Chance.Bingo())
                {
                    Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
                    // 确定搜索的圆心
                    CoordStruct location = default;
                    if (data.AroundSelf)
                    {
                        location = pBullet.Ref.Base.Base.GetCoords();
                    }
                    else
                    {
                        if (!pTarget.IsNull)
                        {
                            location = pTarget.Ref.GetCoords();
                        }
                        else
                        {
                            location = pBullet.Ref.TargetCoords;
                        }
                    }
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
                        FinderHelper.FindTechnoOnMark((pTarget, AEManger) => {
                            targetList.Add(pTarget);
                            return false;
                        }, location, data.Range, 0, false, pSourceHouse, data, pExclude);
                        int count = targetList.Count();
                        if (count > 0)
                        {
                            // 至少会找到一个，可能是原目标，取一个随机数，就它了
                            int i = MathEx.Random.Next(count);
                            // Logger.Log($"{Game.CurrentFrame} 获得范围内搜索到的目标 {i + 1} / {count}");
                            Pointer<AbstractClass> pNewTarget = targetList[i].Convert<AbstractClass>();
                            pBullet.Ref.SetTarget(pNewTarget);
                            pBullet.Ref.TargetCoords = pNewTarget.Ref.GetCoords();
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
                    // 没有找到新的单位作为目标，则挑一个地板
                    if (forceToGround || !pickTechno)
                    {
                        // 随机搜索附近的一个地面作为新目标
                        CoordStruct targetPos = location;
                        targetPos += FLHHelper.RandomOffset(data.Range);
                        // 将偏移后的坐标对应的格子，设置为新的目标
                        if (MapClass.Instance.TryGetCellAt(targetPos, out Pointer<CellClass> pCell))
                        {
                            pBullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
                            pBullet.Ref.TargetCoords = pCell.Ref.GetCoordsWithBridge();
                            if (data.NoOwner)
                            {
                                pBullet.Ref.Owner = IntPtr.Zero;
                            }
                        }
                    }
                }
                ECMState.Reload();
            }
        }
    }
}
