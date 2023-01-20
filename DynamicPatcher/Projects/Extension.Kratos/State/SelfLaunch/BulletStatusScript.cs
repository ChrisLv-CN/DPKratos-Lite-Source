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

        public bool SelfLaunch;

        public void InitState_SelfLaunch()
        {
            if (!SelfLaunch && !pBullet.Ref.WeaponType.IsNull)
            {
                WeaponTypeData weaponTypeData = pBullet.Ref.WeaponType.GetData();
                if (weaponTypeData.SelfLaunch && !pSource.IsDeadOrInvisible() && pSource.CastToFoot(out Pointer<FootClass> pFoot) && !pSource.AmIStand())
                {
                    CoordStruct sourcePos = pSource.Ref.Base.Base.GetCoords();
                    // 从占据的格子中移除自己
                    pSource.Ref.Base.UnmarkAllOccupationBits(sourcePos);
                    // 强令停止移动
                    pFoot.ForceStopMoving();
                    // Limbo发射者
                    pSource.Ref.Base.Remove();
                    SelfLaunch = true;
                }
            }
        }

        public bool OnDetonate_SelfLaunch(Pointer<CoordStruct> pCoords)
        {
            if (SelfLaunch && !pSource.IsDead() && pSource.CastToFoot(out Pointer<FootClass> pFoot) && !pSource.AmIStand())
            {
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
                        pSource.Ref.Base.Put(xyz, dir.ToDirType());
                        --Game.IKnowWhatImDoing;
                        pCell.Ref.OccupationFlags = occFlags;
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
                }
            }
            return false;
        }

    }
}
