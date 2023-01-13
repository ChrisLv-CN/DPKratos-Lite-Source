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
        NONE = 0,
        PASS = 1, // 可通行
        UNDERGROUND = 2, // 潜地
        ONWATER = 3, // 掉水上
        HITWALL = 4, // 不可通行
        HITBUILDING = 5, // 撞建筑
        DOWNBRIDGE = 6, // 从上方撞桥
        UPBRIDEG = 7 // 从下方撞桥
    }

    public static class PhysicsHelper
    {

        public static unsafe PassError CanMoveTo(CoordStruct sourcePos, CoordStruct nextPos, bool passBuilding, out CoordStruct nextCellPos, out bool onBridge)
        {
            PassError canPass = PassError.PASS;
            nextCellPos = sourcePos;
            onBridge = false;
            int deltaZ = sourcePos.Z - nextPos.Z;
            // 检查地面
            if (MapClass.Instance.TryGetCellAt(nextPos, out Pointer<CellClass> pTargetCell))
            {
                nextCellPos = pTargetCell.Ref.GetCoordsWithBridge();
                onBridge = pTargetCell.Ref.ContainsBridge();
                if (nextCellPos.Z >= nextPos.Z)
                {
                    // 沉入地面
                    nextPos.Z = nextCellPos.Z;
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
                if (canPass == PassError.UNDERGROUND && onBridge)
                {
                    // Logger.Log($"{Game.CurrentFrame} 检查桥梁 {canPass} {sourcePos.Z} {nextPos.Z} {cellPos.Z} {CellClass.BridgeHeight}");
                    int bridgeHeight = nextCellPos.Z;
                    if (sourcePos.Z > bridgeHeight && nextPos.Z <= bridgeHeight)
                    {
                        // 桥上砸桥下
                        // Logger.Log($"{Game.CurrentFrame} 桥上砸桥下 {canPass}");
                        canPass = PassError.DOWNBRIDGE;
                    }
                    else if (sourcePos.Z < bridgeHeight && nextPos.Z >= bridgeHeight)
                    {
                        // 桥下穿桥上
                        // Logger.Log($"{Game.CurrentFrame} 桥下穿桥上 {canPass}");
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
