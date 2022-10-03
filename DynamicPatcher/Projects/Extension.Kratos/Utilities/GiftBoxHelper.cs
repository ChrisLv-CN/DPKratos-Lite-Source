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
                }
            }
            return IntPtr.Zero;
        }

    }
}
