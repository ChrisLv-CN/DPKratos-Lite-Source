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

namespace Extension.Utilities
{

    public static class GiftBoxHelper
    {

        public static unsafe Pointer<TechnoClass> CreateAndPutTechno(string id, Pointer<HouseClass> pHouse, CoordStruct location, Pointer<CellClass> pCell = default)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Pointer<TechnoTypeClass> pType = TechnoTypeClass.Find(id);
                if (!pType.IsNull)
                {
                    // 新建单位
                    return CreateAndPutTechno(pType, pHouse, location, pCell);
                }
            }
            return IntPtr.Zero;
        }

        public static unsafe Pointer<TechnoClass> CreateAndPutTechno(Pointer<TechnoTypeClass> pType, Pointer<HouseClass> pHouse, CoordStruct location, Pointer<CellClass> pCell = default)
        {
            // 新建单位
            Pointer<TechnoClass> pTechno = pType.Ref.Base.CreateObject(pHouse).Convert<TechnoClass>();
            if (!pCell.IsNull || MapClass.Instance.TryGetCellAt(location, out pCell))
            {
                // 在目标格子位置刷出单位
                var occFlags = pCell.Ref.OccupationFlags;
                pTechno.Ref.Base.OnBridge = pCell.Ref.ContainsBridge();
                ++Game.IKnowWhatImDoing;
                pTechno.Ref.Base.Put(pCell.Ref.GetCoordsWithBridge(), DirType.E);
                --Game.IKnowWhatImDoing;
                pCell.Ref.OccupationFlags = occFlags;
                // 单位放到指定的位置
                pTechno.Ref.Base.SetLocation(location);
                return pTechno;
            }
            return IntPtr.Zero;
        }

        public static unsafe bool ReleseGift(string id, Pointer<HouseClass> pHouse, CoordStruct location, Pointer<CellClass> pCell, CellStruct cellPos, CellStruct[] cellOffsets, bool emptyCell, out Pointer<TechnoTypeClass> pGiftType, out Pointer<TechnoClass> pGift, out CoordStruct putLocation, out Pointer<CellClass> putCell)
        {
            pGiftType = IntPtr.Zero;
            pGift = IntPtr.Zero;
            putLocation = location;
            putCell = pCell;
            if (id.IsNullOrEmptyOrNone() || (pGiftType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(id)).IsNull)
            {
                return false;
            }
            int max = null == cellOffsets ? 0 : cellOffsets.Count();
            // 随机选择周边的格子
            for (int i = 0; i < max; i++)
            {
                int index = MathEx.Random.Next(max);
                CellStruct offset = cellOffsets[index];
                CellStruct targetCell = cellPos + offset;
                // Logger.Log($"{Game.CurrentFrame} 随机获取周围格子索引{index}, 共{max}格, 尝试次数{i}, 获取的格子偏移{offset}, 单位当前坐标{cell}, 当前偏移{cellOffset[i]}, 偏移量[{string.Join(",", cellOffset)}]");
                if (MapClass.Instance.TryGetCellAt(targetCell, out Pointer<CellClass> pTargetCell))
                {
                    if (pTargetCell.Ref.IsClearToMove(pGiftType.Ref.SpeedType, pGiftType.Ref.MovementZone, !emptyCell, !emptyCell))
                    {
                        putCell = pTargetCell;
                        putLocation = pTargetCell.Ref.GetCoordsWithBridge();
                        // Logger.Log($"{Game.CurrentFrame} 获取到的格子索引{index}, 尝试次数{i}, 偏移{offset}, 坐标{cell + offset}");
                        break;
                    }
                }
            }
            // 投送单位
            pGift = GiftBoxHelper.CreateAndPutTechno(pGiftType, pHouse, putLocation, putCell);
            return !pGift.IsNull;
        }

    }
}
