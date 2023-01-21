using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Utilities;
using System.Runtime.InteropServices.ComTypes;

namespace Extension.Script
{


    public partial class BulletStatusScript
    {

        private bool selfLaunch;
        private bool limboFlag;
        private bool shooterIsSelected;

        public void InitState_SelfLaunch()
        {
            if (!selfLaunch && !pBullet.Ref.WeaponType.IsNull)
            {
                WeaponTypeData weaponTypeData = pBullet.Ref.WeaponType.GetData();
                if (weaponTypeData.SelfLaunch && !pSource.IsDeadOrInvisible() && pSource.CastToFoot(out Pointer<FootClass> pFoot) && !pSource.AmIStand())
                {
                    // 抛射体生成时没有消耗弹药，所以在抛射体开始运行后，再移除发射者
                    selfLaunch = true;
                    if (pSource.Ref.Base.IsSelected && !pSource.Ref.Owner.IsNull && pSource.Ref.Owner.Ref.ControlledByPlayer())
                    {
                        pSource.Ref.WasSelected = true;
                        shooterIsSelected = true;
                    }
                }
            }
        }

        public void OnUpdate_SelfLaunch()
        {
            if (selfLaunch && !limboFlag)
            {
                limboFlag = true;
                // 抛射体生成时没有消耗弹药，所以在抛射体开始运行后，再移除发射者
                if (!pSource.IsDeadOrInvisible())
                {
                    // Logger.Log($"{Game.CurrentFrame} 抛射体[{section}]{pBullet}运行，移除发射者，所属 {pBullet.Ref.Owner}，原始所属 {pSource}");
                    CoordStruct sourcePos = pSource.Ref.Base.Base.GetCoords();
                    // 从占据的格子中移除自己
                    pSource.Ref.Base.UnmarkAllOccupationBits(sourcePos);
                    // 强令停止移动
                    pSource.Convert<FootClass>().ForceStopMoving();
                    // Limbo发射者
                    pSource.Ref.Base.Remove();
                }
            }
        }

        public bool OnDetonate_SelfLaunch(Pointer<CoordStruct> pCoords)
        {
            if (selfLaunch && !pSource.IsDead() && pSource.CastToFoot(out Pointer<FootClass> pFoot) && !pSource.AmIStand())
            {
                // Logger.Log($"{Game.CurrentFrame} 抛射体[{section}]{pBullet}爆炸，所属 {pBullet.Ref.Owner}，原始所属 {pSource}");
                // 发射者Limbo会移除抛射体的所属，要加回去
                pBullet.Ref.Owner = pSource;
                // 移动发射者到爆点
                if (pSource.Ref.Base.InLimbo)
                {
                    // Put到目的地
                    CoordStruct sourcePos = pBullet.Ref.SourceCoords;
                    CoordStruct targetPos = pCoords.Ref;
                    if (MapClass.Instance.TryGetCellAt(targetPos, out Pointer<CellClass> pCell))
                    {
                        var occFlags = pCell.Ref.OccupationFlags;
                        pSource.Ref.Base.OnBridge = pCell.Ref.ContainsBridge();
                        CoordStruct xyz = pCell.Ref.GetCoordsWithBridge();
                        ++Game.IKnowWhatImDoing;

                        DirStruct dir = FLHHelper.Point2Dir(sourcePos, targetPos);
                        bool put = pSource.Ref.Base.Put(xyz, dir.ToDirType());
                        --Game.IKnowWhatImDoing;
                        pCell.Ref.OccupationFlags = occFlags;
                        if (shooterIsSelected && put)
                        {
                            pSource.Ref.Base.Select();
                        }
                    }
                }
                else
                {
                    // 直接拉过来
                    CoordStruct sourcePos = pSource.Ref.Base.Base.GetCoords();
                    CoordStruct nextPos = pCoords.Ref;
                    // 从占据的格子中移除自己
                    pSource.Ref.Base.UnmarkAllOccupationBits(sourcePos);
                    // 停止移动
                    pFoot.ForceStopMoving();
                    bool onBridge = false;
                    if (MapClass.Instance.TryGetCellAt(nextPos, out Pointer<CellClass> pCell))
                    {
                        onBridge = pCell.Ref.ContainsBridge();
                    }
                    // 被黑洞吸走
                    pSource.Ref.Base.Mark(MarkType.UP);
                    // 是否在桥上
                    pSource.Ref.Base.OnBridge = onBridge;
                    pSource.Ref.Base.SetLocation(nextPos);
                    pSource.Ref.Base.Mark(MarkType.DOWN);
                    // 移除黑幕
                    MapClass.Instance.RevealArea2(nextPos, pSource.Ref.LastSightRange, pSource.Ref.Owner, false, false, false, true, 0);
                    MapClass.Instance.RevealArea2(nextPos, pSource.Ref.LastSightRange, pSource.Ref.Owner, false, false, false, true, 1);
                    // 恢复选择
                    if (shooterIsSelected)
                    {
                        pSource.Ref.Base.Select();
                    }
                }
                if (!pSource.Ref.Target.IsNull)
                {
                    pSource.Ref.BaseMission.QueueMission(Mission.Attack, false);
                }
            }
            return false;
        }

    }
}
