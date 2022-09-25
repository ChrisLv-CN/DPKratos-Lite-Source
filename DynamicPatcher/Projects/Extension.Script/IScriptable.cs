using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatcherYRpp;

namespace Extension.Script
{

    public interface IAbstractScriptable : IScriptable
    {
        void OnUpdate();
    }

    public interface IObjectScriptable : IAbstractScriptable
    {
        void OnPut(CoordStruct coord, short dirType);
        void OnRemove();
        void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse);
        void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState,
            Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse);
    }


}
