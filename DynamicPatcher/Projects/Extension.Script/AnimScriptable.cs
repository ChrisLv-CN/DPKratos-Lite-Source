using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{

    public interface IAnimScriptable : IAbstractScriptable
    {

    }

#if USE_ANIM_EXT
    [Serializable]
    public abstract class AnimScriptable : Scriptable<AnimExt>, IAnimScriptable
    {
        public AnimScriptable(AnimExt owner) : base(owner)
        {
        }
        
        [Obsolete("not support OnPut in AnimScriptable yet", true)]
        public void OnPut(CoordStruct coord, Direction faceDir)
        {
            throw new NotSupportedException("not support OnPut in AnimScriptable yet");
        }
        public virtual void OnRemove() { }
        [Obsolete("not support OnReceiveDamage in AnimScriptable yet", true)]
        public void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
            Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            throw new NotSupportedException("not support OnReceiveDamage in AnimScriptable yet");
        }
    }
#endif
}
