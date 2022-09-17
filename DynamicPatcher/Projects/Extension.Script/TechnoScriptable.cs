using Extension.Ext;
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
        void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex);
        void DrawHealthBar(int barLength, Pointer<Point2D> pPos, Pointer<RectangleStruct> pBound, bool isBuilding);
    }

    [Serializable]
    public abstract class TechnoScriptable : Scriptable<TechnoExt>, ITechnoScriptable
    {
        public TechnoScriptable(TechnoExt owner) : base(owner)
        {
        }

        protected Pointer<TechnoClass> pTechno => Owner.OwnerObject;
        protected string section => pTechno.Ref.Type.Ref.Base.Base.ID;

        public virtual void OnPut(CoordStruct coord, short dirType) { }
        public virtual void OnRemove() { }
        public virtual void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        { }
        public virtual void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState) { }

        public virtual void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex) { }

        public virtual void DrawHealthBar(int barLength, Pointer<Point2D> pPos, Pointer<RectangleStruct> pBound, bool isBuilding) { }
    }

}
