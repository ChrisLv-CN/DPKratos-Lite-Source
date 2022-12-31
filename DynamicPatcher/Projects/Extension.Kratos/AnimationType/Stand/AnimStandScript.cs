using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(AnimExt))]
    public partial class AnimStandScript : AnimScriptable
    {
        public AnimStandScript(AnimExt owner) : base(owner) { }

        private StandData data;
        private bool initFlag = false;

        private TechnoExt standExt;
        private Pointer<TechnoClass> pStand => null != standExt ? standExt.OwnerObject : default;

        public override void Awake()
        {
            data = Ini.GetConfig<StandData>(Ini.ArtDependency, section).Data;
            if (!data.Enable)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            // Logger.Log($"{Game.CurrentFrame} 动画 [{section}]{pAnim} 上有设置替身 [{data.Type}]");
        }

        public override void OnUpdate()
        {
            if (!initFlag)
            {
                initFlag = true;
                // 创建替身
                CreateAndPutStand();
            }
            // 移动替身的位置
            if (!pStand.IsDeadOrInvisible())
            {
                // reset state
                pStand.Ref.Base.Mark(MarkType.UP); // 拔起，不在地图上
                CoordStruct location = pAnim.Ref.Base.Base.GetCoords();
                SetLocation(location);
            }
            else
            {
                GameObject.RemoveComponent(this);
            }
        }

        public override void OnDone()
        {
            // 移除替身
            bool explodes = data.Explodes || data.ExplodesWithMaster;
            if (pStand.TryGetStatus(out TechnoStatusScript standStatus))
            {
                standStatus.DestroySelfState.DestroyNow(!explodes);
            }
            else
            {
                if (explodes)
                {
                    pStand.Ref.Base.TakeDamage(pStand.Ref.Base.Health + 1, pStand.Ref.Type.Ref.Crewed);
                }
                else
                {
                    pStand.Ref.Base.Remove();
                    pStand.Ref.Base.TakeDamage(pStand.Ref.Base.Health + 1, false);
                }
            }
            standExt = null;
        }

        private void CreateAndPutStand()
        {
            CoordStruct location = pAnim.Ref.Base.Base.GetCoords();

            Pointer<TechnoTypeClass> pType = TechnoTypeClass.Find(data.Type);
            if (!pType.IsNull)
            {
                Pointer<HouseClass> pHouse = pAnim.Ref.Owner;
                if (pHouse.IsNull)
                {
                    pHouse = HouseClass.FindCivilianSide();
                }
                // 创建替身
                Pointer<TechnoClass> pNew = pType.Ref.Base.CreateObject(pHouse).Convert<TechnoClass>();
                standExt = TechnoExt.ExtMap.Find(pNew);
                if (!pStand.IsNull)
                {
                    // 同步部分扩展设置
                    if (pStand.TryGetStatus(out TechnoStatusScript standStatus))
                    {
                        standStatus.VirtualUnit = this.data.VirtualUnit;

                        standStatus.MyMasterIsAnim = true;
                        standStatus.StandData = this.data;
                    }
                    // 初始化替身
                    pStand.Ref.Base.Mark(MarkType.UP); // 拔起，不在地图上
                    bool canGuard = pHouse.Ref.ControlledByHuman();
                    if (pStand.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                    {
                        canGuard = true;
                    }
                    else
                    {
                        // lock locomotor
                        pStand.Convert<FootClass>().Ref.Locomotor.Lock();
                    }
                    // only computer units can hunt
                    Mission mission = canGuard ? Mission.Guard : Mission.Hunt;
                    pStand.Convert<MissionClass>().Ref.QueueMission(mission, false);

                    // 放置到指定位置
                    if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                    {
                        var occFlags = pCell.Ref.OccupationFlags;
                        pStand.Ref.Base.OnBridge = pCell.Ref.ContainsBridge();
                        CoordStruct xyz = pCell.Ref.GetCoordsWithBridge();
                        ++Game.IKnowWhatImDoing;
                        pStand.Ref.Base.Put(xyz, 0);
                        --Game.IKnowWhatImDoing;
                        pCell.Ref.OccupationFlags = occFlags;
                    }

                    // 调整位置和朝向
                    SetLocation(location);
                    SetDirection();

                    // Logger.Log($"{Game.CurrentFrame} - 为动画 [{section}] 创建替身 [{data.Type}]{pStand.Pointer}");
                }
            }

        }

        private void SetLocation(CoordStruct location)
        {
            // 动画没有朝向，固定朝北
            DirStruct targetDir = new DirStruct();
            CoordStruct targetPos = FLHHelper.GetFLHAbsoluteCoords(location, data.Offset, targetDir);
            pStand.Ref.Base.SetLocation(targetPos);
        }

        private void SetDirection()
        {
            DirStruct targetDir = FLHHelper.DirNormalized(data.Direction, 16);
            pStand.Ref.Facing.set(targetDir);
            pStand.Ref.TurretFacing.set(targetDir);
        }

    }
}
