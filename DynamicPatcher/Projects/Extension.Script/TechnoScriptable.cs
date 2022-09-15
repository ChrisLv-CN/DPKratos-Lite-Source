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
    }

    [Serializable]
    public abstract class TechnoScriptable : Scriptable<TechnoExt>, ITechnoScriptable
    {
        public TechnoScriptable(TechnoExt owner) : base(owner)
        {
        }

        public virtual void OnPut(CoordStruct coord, Direction faceDir) { }
        public virtual void OnRemove() { }
        public virtual void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        { }

        public virtual void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex) { }
    }

}
