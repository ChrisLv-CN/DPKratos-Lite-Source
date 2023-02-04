using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;
using Extension.Script;

namespace ExtensionHooks
{
    public class MapExtHooks
    {

        [Hook(HookType.AresHook, Address = 0x489280, Size = 6)]
        public static unsafe UInt32 MapClass_DamageArea(REGISTERS* R)
        {
            try
            {
                Pointer<CoordStruct> pLocation = (IntPtr)R->ECX;
                int damage = (int)R->EDX;
                Pointer<ObjectClass> pAttacker = R->Stack<IntPtr>(0x4);
                Pointer<WarheadTypeClass> pWH = R->Stack<IntPtr>(0x8);
                bool affectsTiberium = R->Stack<bool>(0xC);
                Pointer<HouseClass> pAttackingHouse = R->Stack<IntPtr>(0x10);
                if (!pWH.IsNull)
                {
                    // Logger.Log($"{Game.CurrentFrame} - 轰炸地区 {pLocation.Data} damage {R->EDX}, warhead {pWH} [{pWH.Ref.Base.ID}], shooter {pAttacker}, owner {pAttackingHouse}");
                    // 抛射体爆炸OnDetonate()后会调用该事件
                    // Find all stand, check distance and blown it up.
                    TechnoStatusScript.FindAndDamageStandOrVUnit(pLocation.Data, damage, pAttacker, pWH, pAttackingHouse);
                    // Find and Attach Effects.
                    AttachEffectScript.FindAndAttach(pLocation.Data, damage, pWH, pAttacker, pAttackingHouse);
                    // Teleport
                    if (!pAttacker.IsNull && pAttacker.CastToTechno(out Pointer<TechnoClass> pTechno) && pTechno.TryGetStatus(out TechnoStatusScript statue))
                    {
                        statue.Teleport(pLocation, pWH);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        /*
            generic crate-handler file
            currently used only to shim crates into TechnoExt
            since Techno fields are used by AttachEffect

            Graion Dilach, 2013-05-31
        */
        //overrides for crate checks
        //481D52 - pass
        //481C86 - override with Money
        [Hook(HookType.AresHook, Address = 0x481D0E, Size = 6)]
        public static unsafe UInt32 CellClass_CrateBeingCollected_Firepower1(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->EDI;
                if (pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    if (status.CrateBuff.FirepowerMultiplier == 1.0)
                    {
                        return 0x481D52;
                    }
                    else
                    {
                        return 0x481C86;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x481C6C, Size = 6)]
        public static unsafe UInt32 CellClass_CrateBeingCollected_Armor1(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->EDI;
                if (pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    if (status.CrateBuff.ArmorMultiplier == 1.0)
                    {
                        return 0x481D52;
                    }
                    else
                    {
                        return 0x481C86;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x481CE1, Size = 6)]
        public static unsafe UInt32 CellClass_CrateBeingCollected_Speed1(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->EDI;
                if (pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    if (status.CrateBuff.SpeedMultiplier == 1.0)
                    {
                        return 0x481D52;
                    }
                    else
                    {
                        return 0x481C86;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }


        // DEFINE_HOOK(481D3D, CellClass_CrateBeingCollected_Cloak1, 6)
        // {
        // 	GET(TechnoClass *, Unit, EDI);
        // 	TechnoExt::ExtData *UnitExt = TechnoExt::ExtMap.Find(Unit);
        // 	if (TechnoExt::CanICloakByDefault(Unit) || UnitExt->Crate_Cloakable){
        // 		return 0x481C86;
        // 	}

        // 	auto pType = Unit->GetTechnoType();
        // 	auto pTypeExt = TechnoTypeExt::ExtMap.Find(pType);

        // 	// cloaking forbidden for type
        // 	if(!pTypeExt->CloakAllowed) {
        // 		return 0x481C86;
        // 	}

        // 	return 0x481D52;
        // }

        [Hook(HookType.AresHook, Address = 0x481D3D, Size = 6)]
        public static unsafe UInt32 CellClass_CrateBeingCollected_Cloak1(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->EDI;
                if (pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    if (status.CanICloakByDefault() || status.CrateBuff.Cloakable)
                    {
                        return 0x481C86;
                    }
                    if (!Ini.GetConfig<TechnoTypeData>(Ini.RulesDependency, pTechno.Ref.Type.Ref.Base.Base.ID).Data.AllowCloakable)
                    {
                        return 0x481C86;
                    }
                    return 0x481D52;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        //overrides on actual crate effect applications
        [Hook(HookType.AresHook, Address = 0x483226, Size = 6)]
        public static unsafe UInt32 CellClass_CrateBeingCollected_Firepower2(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
                double pow_FirepowerMultiplier = R->Stack<double>(0x20);
                // Logger.Log("{0}踩箱子获得火力加成{1}", pTechno.Ref.Type.Ref.Base.Base.ID, pow_FirepowerMultiplier);
                if (pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    if (status.CrateBuff.FirepowerMultiplier == 1.0)
                    {
                        status.CrateBuff.FirepowerMultiplier = pow_FirepowerMultiplier;
                        status.RecalculateStatus();
                        R->AL = pTechno.Ref.Owner.Ref.PlayerControl;
                        return 0x483258;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0x483261;
        }

        [Hook(HookType.AresHook, Address = 0x482E57, Size = 6)]
        public static unsafe UInt32 CellClass_CrateBeingCollected_Armor2(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
                double pow_ArmorMultiplier = R->Stack<double>(0x20);
                // Logger.Log("{0}踩箱子获得装甲加成{1}", pTechno.Ref.Type.Ref.Base.Base.ID, pow_ArmorMultiplier);
                if (pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    if (status.CrateBuff.ArmorMultiplier == 1.0)
                    {
                        status.CrateBuff.ArmorMultiplier = pow_ArmorMultiplier;
                        status.RecalculateStatus();
                        R->AL = pTechno.Ref.Owner.Ref.PlayerControl;
                        return 0x482E89;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0x482E92;
        }

        [Hook(HookType.AresHook, Address = 0x48303A, Size = 6)]
        public static unsafe UInt32 CellClass_CrateBeingCollected_Speed2(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->EDI;
                double pow_SpeedMultiplier = R->Stack<double>(0x20);
                // Logger.Log("{0}踩箱子获得速度加成{1}", pTechno.Ref.Type.Ref.Base.Base.ID, pow_SpeedMultiplier);
                if (pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    if (status.CrateBuff.SpeedMultiplier == 1.0)
                    {
                        status.CrateBuff.SpeedMultiplier = pow_SpeedMultiplier;
                        status.RecalculateStatus();
                        R->AL = pTechno.Ref.Owner.Ref.PlayerControl;
                        return 0x483078;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0x483081;
        }

        // DEFINE_HOOK(48294F, CellClass_CrateBeingCollected_Cloak2, 7)
        // {
        //     GET(TechnoClass *, Unit, EDX);
        //     TechnoExt::ExtData* UnitExt = TechnoExt::ExtMap.Find(Unit);
        //     UnitExt->Crate_Cloakable = 1;
        //     UnitExt->RecalculateStats();
        //     return 0x482956;
        // }
        [Hook(HookType.AresHook, Address = 0x48294F, Size = 6)]
        public static unsafe UInt32 CellClass_CrateBeingCollected_Cloak2(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->EDX;
                // Logger.Log("{0}踩箱子获得隐身", pTechno.Ref.Type.Ref.Base.Base.ID);
                if (pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    status.CrateBuff.Cloakable = true;
                    status.RecalculateStatus();
                    return 0x482956;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x69251A, Size = 6)]
        public static unsafe UInt32 ScrollClass_ProcessClickCoords_VirtualUnit(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->EAX;
            if (pTechno.TryGetStatus(out TechnoStatusScript status) && (status.VirtualUnit || status.Disappear))
            {
                // 虚单位不可选择
                R->EAX = 0;
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6D5116, Size = 5)]
        public static unsafe UInt32 TacticalClass_Draw_Placement_Recheck(REGISTERS* R)
        {
            Pointer<BuildingTypeClass> pBuildingType = DisplayClass.Display_PendingObject;
            if (pBuildingType.Ref.Base.Base.Base.Base.WhatAmI() == AbstractType.BuildingType)
            {
                BuildingRangeData data = Ini.GetConfig<BuildingRangeData>(Ini.RulesDependency, pBuildingType.Ref.Base.Base.Base.ID).Data;
                if (data.Mode != BuildingRangeMode.NONE)
                {
                    // 获取建筑的四个角
                    int width = pBuildingType.Ref.GetFoundationWidth();
                    int height = pBuildingType.Ref.GetFoundationHeight(false);
                    CellStruct center = DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset;
                    int cellX = center.X;
                    int cellY = center.Y;
                    int adjust = pBuildingType.Ref.Adjacent + 1;
                    // 北
                    CellStruct nCell = new CellStruct(cellX - adjust, cellY - adjust);
                    // 东
                    CellStruct eCell = new CellStruct(cellX + adjust + width - 1, cellY - adjust);
                    // 南
                    CellStruct sCell = new CellStruct(cellX + adjust + width - 1, cellY + adjust + height - 1);
                    // 西
                    CellStruct wCell = new CellStruct(cellX - adjust, cellY + adjust + height - 1);
                    // 可视范围
                    Pointer<Surface> pSurface = Surface.Current;
                    RectangleStruct rect = pSurface.Ref.GetRect();
                    rect.Height -= 34;
                    int color = data.Color.RGB2DWORD();
                    // 开始渲染
                    switch (data.Mode)
                    {
                        case BuildingRangeMode.LINE:
                            // 北
                            CoordStruct nPos = MapClass.Cell2Coord(nCell);
                            if (MapClass.Instance.TryGetCellAt(nCell, out Pointer<CellClass> pNCell))
                            {
                                nPos = pNCell.Ref.GetCenterCoords();
                            }
                            nPos.X -= 128;
                            nPos.Y -= 128;
                            Point2D n = nPos.ToClientPos();
                            // 东
                            CoordStruct ePos = MapClass.Cell2Coord(eCell);
                            if (MapClass.Instance.TryGetCellAt(eCell, out Pointer<CellClass> pECell))
                            {
                                ePos = pECell.Ref.GetCenterCoords();
                            }
                            ePos.X += 128;
                            ePos.Y -= 128;
                            Point2D e = ePos.ToClientPos();
                            // 南
                            CoordStruct sPos = MapClass.Cell2Coord(sCell);
                            if (MapClass.Instance.TryGetCellAt(sCell, out Pointer<CellClass> pSCell))
                            {
                                sPos = pSCell.Ref.GetCenterCoords();
                            }
                            sPos.X += 128;
                            sPos.Y += 128;
                            Point2D s = sPos.ToClientPos();
                            // 西
                            CoordStruct wPos = MapClass.Cell2Coord(wCell);
                            if (MapClass.Instance.TryGetCellAt(wCell, out Pointer<CellClass> pWCell))
                            {
                                wPos = pWCell.Ref.GetCenterCoords();
                            }
                            wPos.X -= 128;
                            wPos.Y += 128;
                            Point2D w = wPos.ToClientPos();
                            if (data.Dashed)
                            {
                                // 处理四角越界并绘制
                                pSurface.DrawDashedLine(n, e, color, rect);
                                pSurface.DrawDashedLine(e, s, color, rect);
                                pSurface.DrawDashedLine(s, w, color, rect);
                                pSurface.DrawDashedLine(w, n, color, rect);
                            }
                            else
                            {
                                // 处理四角越界并绘制
                                pSurface.DrawLine(n, e, color, rect);
                                pSurface.DrawLine(e, s, color, rect);
                                pSurface.DrawLine(s, w, color, rect);
                                pSurface.DrawLine(w, n, color, rect);
                            }
                            break;
                        default:
                            // 有效范围
                            int minX = nCell.X;
                            int minY = nCell.Y;
                            int maxX = eCell.X;
                            int maxY = wCell.Y;
                            // 可视范围
                            int minVX = rect.X;
                            int maxVX = rect.X + rect.Width;
                            int minVY = rect.Y;
                            int maxVY = rect.Y + rect.Height;
                            // Logger.Log($"{Game.CurrentFrame} 可视范围 [{minVX} - {maxVX}], [{minVY} - {maxVY}]");
                            // 获取所有的Cell，捡出在视野范围内的Cell
                            HashSet<Pointer<CellClass>> cells = new HashSet<Pointer<CellClass>>();
                            for (int y = minY; y <= maxY; y++)
                            {
                                for (int x = minX; x <= maxX; x++)
                                {
                                    CellStruct cellPos = new CellStruct(x, y);
                                    Point2D point = MapClass.Cell2Coord(cellPos).ToClientPos();
                                    // 在可视范围内
                                    if (point.X >= minVX && point.X <= maxVX && point.Y >= minVY && point.Y <= maxVY)
                                    {
                                        if (MapClass.Instance.TryGetCellAt(cellPos, out Pointer<CellClass> pCell))
                                        {
                                            cells.Add(pCell);
                                        }
                                    }
                                }
                            }
                            // Logger.Log($"{Game.CurrentFrame} 可视范围 [{minVX} - {maxVX}], [{minVY} - {maxVY}]内有格子 {cells.Count()}个");
                            switch (data.Mode)
                            {
                                case BuildingRangeMode.CELL:
                                    foreach (Pointer<CellClass> pCell in cells)
                                    {
                                        if (pCell.Ref.SlopeIndex == 0)
                                        {
                                            CoordStruct cellPos = pCell.Ref.GetCoordsWithBridge();
                                            Point2D pE = (cellPos + new CoordStruct(128, -128, 0)).ToClientPos();
                                            Point2D pW = (cellPos + new CoordStruct(-128, 128, 0)).ToClientPos();
                                            Point2D pN = (cellPos + new CoordStruct(-128, -128, 0)).ToClientPos();
                                            Point2D pS = (cellPos + new CoordStruct(128, 128, 0)).ToClientPos();
                                            if (data.Dashed)
                                            {
                                                // 处理四角越界并绘制
                                                pSurface.DrawDashedLine(pN, pE, color, rect);
                                                pSurface.DrawDashedLine(pE, pS, color, rect);
                                                pSurface.DrawDashedLine(pS, pW, color, rect);
                                                pSurface.DrawDashedLine(pW, pN, color, rect);
                                            }
                                            else
                                            {
                                                // 处理四角越界并绘制
                                                pSurface.DrawLine(pN, pE, color, rect);
                                                pSurface.DrawLine(pE, pS, color, rect);
                                                pSurface.DrawLine(pS, pW, color, rect);
                                                pSurface.DrawLine(pW, pN, color, rect);
                                            }
                                        }
                                    }
                                    break;
                                case BuildingRangeMode.SHP:
                                    if (!data.SHPFileName.IsNullOrEmptyOrNone() && FileSystem.TyrLoadSHPFile(data.SHPFileName, out Pointer<SHPStruct> pSHP))
                                    {
                                        Pointer<ConvertClass> pPalette = FileSystem.PALETTE_PAL;
                                        foreach (Pointer<CellClass> pCell in cells)
                                        {
                                            // WWSB
                                            CellStruct cell = pCell.Ref.MapCoords;
                                            CoordStruct newPos = new CoordStruct(((((cell.X << 8) + 128) / 256) << 8), ((((cell.Y << 8) + 128) / 256) << 8), 0);
                                            Point2D postion = TacticalClass.Global().CoordsToScreen(newPos);
                                            postion -= TacticalClass.Global().TacticalPos;
                                            int zAdjust = 15 * pCell.Ref.Level;
                                            postion.Y += -1 - zAdjust;
                                            int frame = pCell.Ref.SlopeIndex + 2;
                                            Surface.Current.Ref.DrawSHP(pPalette, pSHP, data.ZeroFrameIndex + frame, postion);
                                        }
                                    }
                                    break;
                            }
                            break;
                    }

                    DisplayClass.Display_PassedProximityCheck = DisplayClass.Global().Passes_Proximity_Check();
                }
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4A904E, Size = 5)]
        public static unsafe UInt32 DisplayClass_Passes_Proximity_Check_MobileMCV(REGISTERS* R)
        {
            bool canBuild = R->Stack<Bool>(0x3C);
            if (!canBuild && CombatDamage.Data.AllowUnitAsBaseNormal)
            {
                Pointer<BuildingTypeClass> pBuildingType = IntPtr.Zero;
                if ((TechnoStatusScript.BaseUnitArray.Any() || TechnoStatusScript.BaseStandArray.Any()) && !(pBuildingType = DisplayClass.Display_PendingObject).IsNull)
                {
                    // 获取建筑建造范围四点坐标
                    // 显示建造范围
                    int width = pBuildingType.Ref.GetFoundationWidth();
                    int height = pBuildingType.Ref.GetFoundationHeight(false);
                    CellStruct center = DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset;
                    int cellX = center.X;
                    int cellY = center.Y;
                    int adjust = pBuildingType.Ref.Adjacent + 1;
                    // 北
                    CellStruct nCell = new CellStruct(cellX - adjust, cellY - adjust);
                    // 东
                    CellStruct eCell = new CellStruct(cellX + adjust + width - 1, cellY - adjust);
                    // 南
                    // CellStruct sCell = new CellStruct(cellX + adjust + width - 1, cellY + adjust + height - 1);
                    // 西
                    CellStruct wCell = new CellStruct(cellX - adjust, cellY + adjust + height - 1);
                    // 有效范围
                    int minX = nCell.X;
                    int minY = nCell.Y;
                    int maxX = eCell.X;
                    int maxY = wCell.Y;
                    // 检查单位节点
                    bool found = false;
                    int houseIndex = DisplayClass.Display_PendingHouse;
                    foreach (KeyValuePair<TechnoExt, BaseNormalData> baseUnit in TechnoStatusScript.BaseUnitArray)
                    {
                        Pointer<TechnoClass> pTarget = baseUnit.Key.OwnerObject;
                        if (found = pTarget.CanBeBase(baseUnit.Value, houseIndex, minX, maxX, minY, maxY))
                        {
                            break;
                        }
                    }
                    if (!found && CombatDamage.Data.AllowStandAsBaseNormal)
                    {
                        foreach (KeyValuePair<TechnoExt, BaseNormalData> baseUnit in TechnoStatusScript.BaseStandArray)
                        {
                            Pointer<TechnoClass> pTarget = baseUnit.Key.OwnerObject;
                            if (found = pTarget.CanBeBase(baseUnit.Value, houseIndex, minX, maxX, minY, maxY))
                            {
                                break;
                            }
                        }
                    }
                    if (found)
                    {
                        R->Stack<Bool>(0x3C, true);
                    }
                }
            }
            return 0;
        }


    }
}
