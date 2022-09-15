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
        void OnPut(CoordStruct coord, Direction faceDir);
        void OnRemove();
        void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse);
    }


}
