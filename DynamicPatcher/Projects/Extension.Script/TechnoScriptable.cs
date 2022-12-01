﻿using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    public interface ITechnoScriptable : IObjectScriptable
    {

        void OnTemporalUpdate(Pointer<TemporalClass> pTemporal);
        void OnTemporalEliminate(Pointer<TemporalClass> pTemporal);

        void OnRocketExplosion();

        void OnRegisterDestruction(Pointer<TechnoClass> pKiller, int cost, ref bool skip);

        void CanFire(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon, ref bool ceaseFire);
        void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex);

        void OnSelect(ref bool selectable);
        void DrawHealthBar(int barLength, Pointer<Point2D> pPos, Pointer<RectangleStruct> pBound, bool isBuilding);

        void OnGuardCommand();
        void OnStopCommand();
    }

    [Serializable]
    public abstract class TechnoScriptable : Scriptable<TechnoExt>, ITechnoScriptable
    {
        public TechnoScriptable(TechnoExt owner) : base(owner)
        {
        }

        protected Pointer<TechnoClass> pTechno => Owner.OwnerObject;
        protected string section => pTechno.Ref.Type.Ref.Base.Base.ID;

        public virtual void OnInit() { }
        public virtual void OnUnInit() { }

        public virtual void OnTemporalUpdate(Pointer<TemporalClass> pTemporal) { }
        public virtual void OnTemporalEliminate(Pointer<TemporalClass> pTemporal) { }

        public virtual void OnRocketExplosion() { }

        public virtual void OnPut(Pointer<CoordStruct> pLocation, ref DirType dirType) { }
        public virtual void OnRemove() { }
        public virtual void OnReceiveDamage(Pointer<int> pDamage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool ignoreDefenses, bool preventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        { }
        public virtual void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState,
            Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        { }
        public virtual void OnReceiveDamageDestroy() { }

        public virtual void OnRegisterDestruction(Pointer<TechnoClass> pKiller, int cost, ref bool skip) { }

        public virtual void CanFire(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon, ref bool ceaseFire) { }
        public virtual void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex) { }

        public virtual void OnSelect(ref bool selectable) { }
        public virtual void DrawHealthBar(int barLength, Pointer<Point2D> pPos, Pointer<RectangleStruct> pBound, bool isBuilding) { }
        public virtual void OnGuardCommand() { }
        public virtual void OnStopCommand() { }
    }

}
