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
    [Serializable]
    public enum PassError
    {
        PASS = 0, UNDERGROUND = 1, HITWALL = 2, HITBUILDING = 3, DOWNBRIDGE = 4, UPBRIDEG = 5
    }

    public static class PhysicsHelper
    {

        public static unsafe PassError CanMoveTo(CoordStruct sourcePos, CoordStruct nextPos, bool passBuilding, out CoordStruct cellPos)
        {
            PassError canPass = PassError.PASS;
            cellPos = sourcePos;
            int deltaZ = sourcePos.Z - nextPos.Z;
            // 检查地面
            if (MapClass.Instance.TryGetCellAt(nextPos, out Pointer<CellClass> pTargetCell))
            {
                cellPos = pTargetCell.Ref.GetCoordsWithBridge();
                if (cellPos.Z >= nextPos.Z)
                {
                    // 沉入地面
                    nextPos.Z = cellPos.Z;
                    canPass = PassError.UNDERGROUND;
                    // 检查悬崖
                    switch (pTargetCell.Ref.GetTileType())
                    {
                        case TileType.Cliff:
                        case TileType.DestroyableCliff:
                            // 悬崖上可以往悬崖下移动
                            if (deltaZ <= 0)
                            {
                                canPass = PassError.HITWALL;
                            }
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 行进路线遇到悬崖 {(canMove ? "可通过" : "不可通过")} nextPos = {nextPos}");
                            break;
                    }
                }
                // 检查桥
                if (canPass == PassError.PASS && pTargetCell.Ref.ContainsBridge())
                {
                    Logger.Log($"{Game.CurrentFrame} 检查桥梁 {canPass} {sourcePos.Z} {nextPos.Z} {cellPos.Z}");
                    int bridgeHeight = cellPos.Z;
                    if (sourcePos.Z > bridgeHeight && nextPos.Z <= bridgeHeight)
                    {
                        // 桥上砸桥下
                        Logger.Log($"{Game.CurrentFrame} 桥上砸桥下 {canPass}");
                        canPass = PassError.DOWNBRIDGE;
                    }
                    else if (sourcePos.Z < bridgeHeight && nextPos.Z >= bridgeHeight)
                    {
                        // 桥下穿桥上
                        Logger.Log($"{Game.CurrentFrame} 桥下穿桥上 {canPass}");
                        canPass = PassError.UPBRIDEG;
                    }
                }
                // 检查建筑
                if (!passBuilding)
                {
                    Pointer<BuildingClass> pBuilding = pTargetCell.Ref.GetBuilding();
                    if (!pBuilding.IsNull)
                    {
                        if (pBuilding.CanHit(nextPos.Z))
                        {
                            canPass = PassError.HITBUILDING;
                        }
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 行进路线遇到建筑 [{pBuilding.Ref.Type.Ref.Base.Base.Base.ID}] {pBuilding} {(canMove ? "可通过" : "不可通过")} nextPos {nextPos}");
                    }
                }
            }
            return canPass;
        }

    }
}
