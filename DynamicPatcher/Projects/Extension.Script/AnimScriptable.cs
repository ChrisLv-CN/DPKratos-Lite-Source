using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Ext;
using PatcherYRpp;

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

        protected Pointer<AnimClass> pAnim => Owner.OwnerObject;
        protected string section => pAnim.Ref.Type.Ref.Base.Base.ID;

        public void OnInit() { }
        public void OnUnInit() { }


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
