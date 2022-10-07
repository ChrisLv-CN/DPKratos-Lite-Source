using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class AttachEffect
    {
        public DamageReaction DamageReaction;

        private void InitDamageReaction()
        {
            this.DamageReaction = AEData.DamageReactionData.CreateEffect<DamageReaction>();
            RegisterEffect(DamageReaction);
        }
    }


    [Serializable]
    public class DamageReaction : StateEffect<DamageReaction, DamageReactionData>
    {
        public override void OnEnable()
        {
            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                state = GetState(pTechno.GetStatus());
                state.EnableAndReplace<DamageReactionData>(this);
            }
        }

        public override State<DamageReactionData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.DamageReactionState;
        }

        public override State<DamageReactionData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}
