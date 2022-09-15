using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    public interface IBulletScriptable : IObjectScriptable
    {
        void OnDetonate(Pointer<CoordStruct> pCoords);
    }


    [Serializable]
    public abstract class BulletScriptable : Scriptable<BulletExt>, IBulletScriptable
    {
        public BulletScriptable(BulletExt owner) : base(owner)
        {
        }

        [Obsolete("not support OnPut in BulletScriptable yet", true)]
        public void OnPut(CoordStruct coord, Direction faceDir)
        {
            throw new NotSupportedException("not support OnPut in BulletScriptable yet");
        }
        [Obsolete("not support OnRemove in BulletScriptable yet", true)]
        public void OnRemove()
        {
            throw new NotSupportedException("not support OnRemove in BulletScriptable yet");
        }
        [Obsolete("not support OnReceiveDamage in BulletScriptable yet", true)]
        public void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            throw new NotSupportedException("not support OnReceiveDamage in BulletScriptable yet");
        }

        public virtual void OnDetonate(Pointer<CoordStruct> pCoords) { }
    }
}
