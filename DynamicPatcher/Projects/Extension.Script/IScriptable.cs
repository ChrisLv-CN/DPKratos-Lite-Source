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
        void OnInit();
        void OnUpdate();
        void OnUnInit();
    }

    public interface IObjectScriptable : IAbstractScriptable
    {
        void OnPut(Pointer<CoordStruct> pLocation, DirType dirType);
        void OnRemove();
        void OnReceiveDamage(Pointer<int> pDamage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool ignoreDefenses, bool preventPassengerEscape, Pointer<HouseClass> pAttackingHouse);
        void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState,
            Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse);
        void OnReceiveDamageDestroy();
    }


}
