using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    public abstract class ObjectScriptable : ScriptComponent, ITechnoScriptable, IBulletScriptable
    {

        public ObjectScriptable() : base() { }

        public ObjectScriptable(IExtension owner) : this()
        {
            this.Owner = owner;
        }

        public IExtension Owner;

        protected Pointer<ObjectClass> pObject => Owner.OwnerObject.Convert<ObjectClass>();
        protected string section => pObject.Ref.Type.Ref.Base.ID;

        public virtual void OnTemporalUpdate(Pointer<TemporalClass> pTemporal) { }

        public virtual void OnPut(Pointer<CoordStruct> coord, DirType dirType) { }

        public virtual void OnRemove() { }

        public virtual void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        { }
        public virtual void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState,
            Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        { }
        public virtual void OnReceiveDamageDestroy() { }

        public virtual void CanFire(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon, ref bool ceaseFire) { }
        public virtual void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex) { }

        public virtual void DrawHealthBar(int barLength, Pointer<Point2D> pPos, Pointer<RectangleStruct> pBound, bool isBuilding) { }

        public virtual void OnSelect(ref bool selectable) { }
        public virtual void OnGuardCommand() { }

        public virtual void OnStopCommand() { }


        public virtual void OnDetonate(Pointer<CoordStruct> pCoords) { }
    }
}