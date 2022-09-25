using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentHooks
{
    public class TechnoComponentHooks
    {
        [Hook(HookType.AresHook, Address = 0x6F9E50, Size = 5)]
        static public unsafe UInt32 TechnoClass_Update_Components(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => c.OnUpdate());
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6FAFFD, Size = 7)]
        [Hook(HookType.AresHook, Address = 0x6FAF7A, Size = 7)]
        static public unsafe UInt32 TechnoClass_LateUpdate_Components(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => c.OnLateUpdate());
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x71A88D, Size = 0)]
        public static unsafe UInt32 TemporalClass_UpdateA(REGISTERS* R)
        {
            try
            {
                Pointer<TemporalClass> pTemporal = (IntPtr)R->ESI;

                Pointer<TechnoClass> pTechno = pTemporal.Ref.Target;
                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as ITechnoScriptable)?.OnTemporalUpdate(pTemporal));
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            if ((int)R->EAX <= (int)R->EBX)
            {
                return 0x71A895;
            }
            return 0x71AB08;
        }

        [Hook(HookType.AresHook, Address = 0x6F6CA0, Size = 7)]
        static public unsafe UInt32 TechnoClass_Put_Components(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
                var pCoord = R->Stack<Pointer<CoordStruct>>(0x4);
                var faceDir = R->Stack<short>(0x8);

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as IObjectScriptable)?.OnPut(pCoord.Data, faceDir));
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // avoid hook conflict with phobos feature -- shield
        //[Hook(HookType.AresHook, Address = 0x6F6AC0, Size = 5)]
        [Hook(HookType.AresHook, Address = 0x6F6AC4, Size = 5)]
        static public unsafe UInt32 TechnoClass_Remove_Components(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as IObjectScriptable)?.OnRemove());
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x701900, Size = 6)]
        static public unsafe UInt32 TechnoClass_ReceiveDamage_Components(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
                var pDamage = R->Stack<Pointer<int>>(0x4);
                var distanceFromEpicenter = R->Stack<int>(0x8);
                var pWH = R->Stack<Pointer<WarheadTypeClass>>(0xC);
                var pAttacker = R->Stack<Pointer<ObjectClass>>(0x10);
                var ignoreDefenses = R->Stack<bool>(0x14);
                var preventPassengerEscape = R->Stack<bool>(0x18);
                var pAttackingHouse = R->Stack<Pointer<HouseClass>>(0x1C);

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as IObjectScriptable)?.OnReceiveDamage(pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse));
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // if (pDamage >= 0 && pDamage < 1) pDamage = 1; // ╮(-△-)╭
        [Hook(HookType.AresHook, Address = 0x7019DD, Size = 6)]
        public static unsafe UInt32 TechnoClass_ReceiveDamage_AtLeast1(REGISTERS* R)
        {
            // var pDamage = (Pointer<int>)R->EBX;
            // Logger.Log($"{Game.CurrentFrame} - 免疫伤害， {pDamage.Ref}");
            // Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
            // TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
            // if (ext.DamageReactionState.IsActive())
            // {
            return 0x7019E3;
            // }
            // return 0;
        }


        // after TakeDamage
        [Hook(HookType.AresHook, Address = 0x701DFF, Size = 7)]
        public static unsafe UInt32 TechnoClass_ReceiveDamage2(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                Pointer<int> pRealDamage = (IntPtr)R->EBX;
                Pointer<WarheadTypeClass> pWH = (IntPtr)R->EBP;
                DamageState damageState = (DamageState)R->EDI;

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as IObjectScriptable)?.OnReceiveDamage2(pRealDamage, pWH, damageState));
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6FC339, Size = 6)]
        public static unsafe UInt32 TechnoClass_CanFire_Components(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                Pointer<WeaponTypeClass> pWeapon = (IntPtr)R->EDI;
                var pTarget = R->Stack<Pointer<AbstractClass>>(0x20 - (-0x4));

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                bool ceaseFire = false;
                if (!ceaseFire)
                {
                    ext.GameObject.Foreach(c => (c as ITechnoScriptable)?.CanFire(pTarget, pWeapon, ref ceaseFire));
                }
                if (ceaseFire)
                {
                    // Logger.Log($"{Game.CurrentFrame} {pTechno} [{pTechno.Ref.Type.Ref.Base.Base.ID}] cease fire !!!");
                    return 0x6FCB7E;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6FDD50, Size = 6)]
        static public unsafe UInt32 TechnoClass_Fire_Components(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
                var pTarget = R->Stack<Pointer<AbstractClass>>(0x4);
                var nWeaponIndex = R->Stack<int>(0x8);

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as ITechnoScriptable)?.OnFire(pTarget, nWeaponIndex));

            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6F65D1, Size = 6)]
        public static unsafe UInt32 TechnoClass_DrawHealthBar_Building(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;

                int length = (int)R->EBX;
                Pointer<Point2D> pLocation = R->Stack<IntPtr>(0x4C - (-0x4));
                Pointer<RectangleStruct> pBound = R->Stack<IntPtr>(0x4C - (-0x8));

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as ITechnoScriptable)?.DrawHealthBar(length, pLocation, pBound, true));
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6F683C, Size = 7)]
        public static unsafe UInt32 TechnoClass_DrawHealthBar_Other(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;

                int length = pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Infantry ? 8 : 17;
                Pointer<Point2D> pLocation = R->Stack<IntPtr>(0x4C - (-0x4));
                Pointer<RectangleStruct> pBound = R->Stack<IntPtr>(0x4C - (-0x8));

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as ITechnoScriptable)?.DrawHealthBar(length, pLocation, pBound, false));
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        #region Render
        static public UInt32 TechnoClass_Render_Components(Pointer<TechnoClass> pTechno)
        {
            try
            {
                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => c.OnRender());
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x4144B0, Size = 5)]
        static public unsafe UInt32 AircraftClass_Render_Components(REGISTERS* R)
        {
            Pointer<AircraftClass> pAircraft = (IntPtr)R->ECX;
            return TechnoClass_Render_Components(pAircraft.Convert<TechnoClass>());
        }
        [Hook(HookType.AresHook, Address = 0x43D290, Size = 5)]
        static public unsafe UInt32 BuildingClass_Render_Components(REGISTERS* R)
        {
            Pointer<BuildingClass> pBuilding = (IntPtr)R->ECX;
            return TechnoClass_Render_Components(pBuilding.Convert<TechnoClass>());
        }
        [Hook(HookType.AresHook, Address = 0x518F90, Size = 7)]
        static public unsafe UInt32 InfantryClass_Render_Components(REGISTERS* R)
        {
            Pointer<InfantryClass> pInfantry = (IntPtr)R->ECX;
            return TechnoClass_Render_Components(pInfantry.Convert<TechnoClass>());
        }
        [Hook(HookType.AresHook, Address = 0x73CEC0, Size = 5)]
        static public unsafe UInt32 UnitClass_Render_Components(REGISTERS* R)
        {
            Pointer<UnitClass> pUnit = (IntPtr)R->ECX;
            return TechnoClass_Render_Components(pUnit.Convert<TechnoClass>());
        }
        #endregion

        [Hook(HookType.AresHook, Address = 0x5F45A0, Size = 5)]
        public static unsafe UInt32 TechnoClass_Select(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->EDI;
                // Logger.Log("{0} Select", pTechno.IsNull ? "Unknow" : pTechno.Ref.Type.Ref.Base.Base.ID);
                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                bool selectable = true;

                ext.GameObject.Foreach(c => (c as ITechnoScriptable)?.OnSelect(ref selectable));
                if (!selectable)
                {
                    return 0x5F45A9;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }


        [Hook(HookType.AresHook, Address = 0x730E8F, Size = 6)]
        public static unsafe UInt32 ObjectClass_GuardCommand(REGISTERS* R)
        {
            Pointer<ObjectClass> pObject = (IntPtr)R->ESI;
            if (pObject.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                // Logger.Log("{0} Guard Command", pTechno.IsNull ? "Unknow" : pTechno.Ref.Type.Ref.Base.Base.ID);
                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as ITechnoScriptable)?.OnGuardCommand());
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x730F1C, Size = 5)]
        public static unsafe UInt32 ObjectClass_StopCommand(REGISTERS* R)
        {
            Pointer<ObjectClass> pObject = (IntPtr)R->ESI;
            if (pObject.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                // Logger.Log("{0} Stop Command", pTechno.IsNull ? "Unknow" : pTechno.Ref.Type.Ref.Base.Base.ID);
                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as ITechnoScriptable)?.OnStopCommand());
            }
            return 0;
        }
    }
}
