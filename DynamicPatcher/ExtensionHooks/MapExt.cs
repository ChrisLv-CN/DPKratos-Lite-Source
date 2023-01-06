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


        [Hook(HookType.AresHook, Address = 0x69252D, Size = 6)]
        public static unsafe UInt32 ScrollClass_ProcessClickCoords_VirtualUnit(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.TryGetStatus(out TechnoStatusScript status) && status.VirtualUnit)
                {
                    // Logger.Log("ScrollClass_ClickCoords {0} is virtual unit", pTechno.Ref.Type.Ref.Base.Base.ID);
                    return 0x6925E6;
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

        [Hook(HookType.AresHook, Address = 0x6D5116, Size = 5)]
        public static unsafe UInt32 TacticalClass_Draw_Placement_Recheck(REGISTERS* R)
        {
            Pointer<BuildingTypeClass> pBuildingType = DisplayClass.Display_PendingObject;
            if (pBuildingType.Ref.Base.Base.Base.Base.WhatAmI() == AbstractType.BuildingType)
            {
                BuildingRangeData data = Ini.GetConfig<BuildingRangeData>(Ini.RulesDependency, pBuildingType.Ref.Base.Base.Base.ID).Data;
                if (data.Mode == BuildingRangeMode.LINE)
                {
                    // 显示建造范围
                    int width = pBuildingType.Ref.GetFoundationWidth();
                    int height = pBuildingType.Ref.GetFoundationHeight(false);
                    CellStruct center = DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset;
                    int cellX = center.X;
                    int cellY = center.Y;
                    int adjust = pBuildingType.Ref.Adjacent + 1;
                    // 北
                    CellStruct nCell = new CellStruct(cellX - adjust, cellY - adjust);
                    CoordStruct nPos = MapClass.Cell2Coord(nCell);
                    if (MapClass.Instance.TryGetCellAt(nCell, out Pointer<CellClass> pNCell))
                    {
                        nPos = pNCell.Ref.GetCenterCoords();
                    }
                    nPos.X -= 128;
                    nPos.Y -= 128;
                    Point2D nA = nPos.ToClientPos();
                    Point2D nB = nA;
                    // 东
                    CellStruct eCell = new CellStruct(cellX + adjust + width - 1, cellY - adjust);
                    CoordStruct ePos = MapClass.Cell2Coord(eCell);
                    if (MapClass.Instance.TryGetCellAt(eCell, out Pointer<CellClass> pECell))
                    {
                        ePos = pECell.Ref.GetCenterCoords();
                    }
                    ePos.X += 128;
                    ePos.Y -= 128;
                    Point2D eA = ePos.ToClientPos();
                    Point2D eB = eA;
                    // 南
                    CellStruct sCell = new CellStruct(cellX + adjust + width - 1, cellY + adjust + height - 1);
                    CoordStruct sPos = MapClass.Cell2Coord(sCell);
                    if (MapClass.Instance.TryGetCellAt(sCell, out Pointer<CellClass> pSCell))
                    {
                        sPos = pSCell.Ref.GetCenterCoords();
                    }
                    sPos.X += 128;
                    sPos.Y += 128;
                    Point2D sA = sPos.ToClientPos();
                    Point2D sB = sA;
                    // 西
                    CellStruct wCell = new CellStruct(cellX - adjust, cellY + adjust + height - 1);
                    CoordStruct wPos = MapClass.Cell2Coord(wCell);
                    if (MapClass.Instance.TryGetCellAt(wCell, out Pointer<CellClass> pWCell))
                    {
                        wPos = pWCell.Ref.GetCenterCoords();
                    }
                    wPos.X -= 128;
                    wPos.Y += 128;
                    Point2D wA = wPos.ToClientPos();
                    Point2D wB = wA;
                    // 处理四角越界
                    RectangleStruct rect = Surface.Current.Ref.GetRect();
                    rect.Height -= 34;
                    // 北部越界不管，处理南部
                    if (sA.Y > rect.Height)
                    {
                        // double x = sB.X - wB.X;
                        // double y = sB.Y - wB.Y;
                        // double tanA = x / y;
                        double tanA = 2;
                        double deltaY = sA.Y - rect.Height;
                        int deltaX = (int)(deltaY * tanA);
                        sA.Y = rect.Height;
                        sB.Y = rect.Height;
                        sA.X += deltaX;
                        sB.X -= deltaX;
                    }
                    int color = data.Color.RGB2DWORD();
                    Surface.Current.Ref.DrawLine(nB, eB, color);
                    if (wB.Y < rect.Height && eA.Y < rect.Height)
                    {
                        Surface.Current.Ref.DrawLine(eA, sA, color);
                        Surface.Current.Ref.DrawLine(sB, wB, color);
                    }
                    Surface.Current.Ref.DrawLine(wA, nA, color);
                }
                DisplayClass.Display_PassedProximityCheck = DisplayClass.Global().Passes_Proximity_Check();
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4A8FCA, Size = 7)]
        public static unsafe UInt32 DisplayClass_Passes_Proximity_Check_MobileMCV(REGISTERS* R)
        {
            Pointer<CellClass> pCell = (IntPtr)R->EAX;
            Pointer<BuildingTypeClass> pBuildingType = DisplayClass.Display_PendingObject;
            if (!pBuildingType.IsNull)
            {
                BuildingRangeData data = Ini.GetConfig<BuildingRangeData>(Ini.RulesDependency, pBuildingType.Ref.Base.Base.Base.ID).Data;
                switch (data.Mode)
                {
                    case BuildingRangeMode.CELL:
                        if (pCell.Ref.SlopeIndex == 0)
                        {
                            CoordStruct cellPos = pCell.Ref.GetCoordsWithBridge();
                            CoordStruct pE = cellPos + new CoordStruct(128, -128, 0);
                            Point2D E = pE.ToClientPos();
                            CoordStruct pW = cellPos + new CoordStruct(-128, 128, 0);
                            Point2D W = pW.ToClientPos();

                            RectangleStruct rect = Surface.Current.Ref.GetRect();
                            rect.Height -= 34;
                            if (W.Y < rect.Height && E.Y < rect.Height)
                            {
                                CoordStruct pN = cellPos + new CoordStruct(-128, -128, 0);
                                Point2D N = pN.ToClientPos();
                                CoordStruct pS = cellPos + new CoordStruct(128, 128, 0);
                                Point2D S = pS.ToClientPos();
                                int color = data.Color.RGB2DWORD();
                                Surface.Current.Ref.DrawLine(N, E, color);
                                Surface.Current.Ref.DrawLine(E, S, color);
                                Surface.Current.Ref.DrawLine(S, W, color);
                                Surface.Current.Ref.DrawLine(W, N, color);
                            }
                        }
                        break;
                    case BuildingRangeMode.SHP:
                        if (!data.SHPFileName.IsNullOrEmptyOrNone() && FileSystem.TyrLoadSHPFile(data.SHPFileName, out Pointer<SHPStruct> pSHP))
                        {
                            // WWSB
                            CellStruct cell = pCell.Ref.MapCoords;
                            CoordStruct newPos = new CoordStruct(((((cell.X << 8) + 128) / 256) << 8), ((((cell.Y << 8) + 128) / 256) << 8), 0);
                            Point2D postion = TacticalClass.Global().CoordsToScreen(newPos);
                            postion -= TacticalClass.Global().TacticalPos;
                            int zAdjust = 15 * pCell.Ref.Level;
                            postion.Y += -1 - zAdjust;
                            int frame = pCell.Ref.SlopeIndex + 2;
                            Surface.Current.Ref.DrawSHP(FileSystem.PALETTE_PAL, pSHP, data.ZeroFrameIndex + frame, postion);
                        }
                        break;
                }
            }
            Pointer<BuildingClass> pBase = pCell.Ref.GetBuilding();
            R->EAX = (uint)pBase;
            R->ESI = R->EAX;
            if (!pBase.IsNull)
            {
                return 0x4A8FD7;
            }
            else if (!pCell.IsNull)
            {
                if (CombatDamage.Data.AllowUnitAsBaseNormal)
                {
                    int houseIndex = R->Stack<int>(0x38);
                    bool found = false;
                    // 检查单位
                    FinderHelper.FindTechnoInCell(pCell, (pTarget) =>
                    {
                        Pointer<HouseClass> pTargetHouse = IntPtr.Zero;
                        if (!pTarget.IsDeadOrInvisible() && BaseNormalData.CanBeBase(pTarget.Ref.Type.Ref.Base.Base.ID) && !(pTargetHouse = pTarget.Ref.Owner).IsNull)
                        {
                            found = pTargetHouse.Ref.ArrayIndex == houseIndex || (RulesClass.Global().BuildOffAlly && pTargetHouse.Ref.IsAlliedWith(houseIndex));
                        }
                        return found;
                    });
                    if (!found && CombatDamage.Data.AllowJumpjetAsBaseNormarl)
                    {
                        // 检查JJ，移动中的JJ不在这里
                        if (!pCell.Ref.Jumpjet.IsNull)
                        {
                            Pointer<TechnoClass> pTarget = pCell.Ref.Jumpjet.Convert<TechnoClass>();
                            Pointer<HouseClass> pTargetHouse = IntPtr.Zero;
                            if (!pTarget.IsDeadOrInvisible() && BaseNormalData.CanBeBase(pTarget.Ref.Type.Ref.Base.Base.ID) && !(pTargetHouse = pTarget.Ref.Owner).IsNull)
                            {
                                found = pTargetHouse.Ref.ArrayIndex == houseIndex || (RulesClass.Global().BuildOffAlly && pTargetHouse.Ref.IsAlliedWith(houseIndex));
                            }
                        }
                    }
                    /*
                    // 检查飞天的载具
                    if (!found)
                    {
                        UnitClass.Array.FindObject((pUnit) =>
                        {
                            Pointer<TechnoClass> pTarget = pUnit.Convert<TechnoClass>();
                            found = pTarget.CanBeBase(houseIndex, pCell);
                            return found;
                        });
                    }
                    // 检查飞天的步兵
                    if (!found)
                    {
                        InfantryClass.Array.FindObject((pInf) =>
                        {
                            Pointer<TechnoClass> pTarget = pInf.Convert<TechnoClass>();
                            found = pTarget.CanBeBase(houseIndex, pCell);
                            return found;
                        });
                    }
                    // 检查飞天的飞机
                    if (!found)
                    {
                        AircraftClass.Array.FindObject((pAircraft) =>
                        {
                            Pointer<TechnoClass> pTarget = pAircraft.Convert<TechnoClass>();
                            found = pTarget.CanBeBase(houseIndex, pCell);
                            return found;
                        });
                    }
                    */
                    // 检查替身
                    if (!found && CombatDamage.Data.AllowStandAsBaseNormal)
                    {
                        foreach (KeyValuePair<TechnoExt, StandData> stand in TechnoStatusScript.StandArray)
                        {
                            if (!stand.Key.OwnerObject.IsNull && null != stand.Value)
                            {
                                Pointer<TechnoClass> pTarget = stand.Key.OwnerObject;
                                if (found = pTarget.CanBeBase(houseIndex, pCell))
                                {
                                    break;
                                }
                            }
                        }
                        if (!found)
                        {
                            foreach (KeyValuePair<TechnoExt, StandData> stand in TechnoStatusScript.ImmuneStandArray)
                            {
                                if (!stand.Key.OwnerObject.IsNull && null != stand.Value)
                                {
                                    Pointer<TechnoClass> pTarget = stand.Key.OwnerObject;
                                    if (found = pTarget.CanBeBase(houseIndex, pCell))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (found)
                    {
                        return 0x4A9027; // 可建造
                    }
                }

            }
            return 0x4A902C;
        }

    }
}