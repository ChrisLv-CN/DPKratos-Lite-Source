using DynamicPatcher;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext
{

    [Serializable]
    public abstract class StateEffect<E, EData> : Effect<EData> where E : IEffect, new() where EData : EffectData, IStateData, new()
    {

        protected State<EData> state;

        public override void OnEnable()
        {
            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                state = GetState(pTechno.GetStatus());
            }
            else if (pOwner.CastIf(AbstractType.Bullet, out Pointer<BulletClass> pBullet))
            {
                state = GetState(pBullet.GetStatus());
            }
            ResetDuration();
        }

        public abstract State<EData> GetState(TechnoStatusScript statusScript);

        public abstract State<EData> GetState(BulletStatusScript statusScript);

        public override void OnDisable(CoordStruct location)
        {
            state?.Disable(Token);
        }

        public override void ResetDuration()
        {
            if (null != state)
            {
                IStateData data = GetData();
                switch (Data.AffectWho)
                {
                    case AffectWho.MASTER:
                        state.EnableAndReplace(this);
                        break;
                    case AffectWho.STAND:
                        EnableAEStatsToStand(data);
                        break;
                    default:
                        state.EnableAndReplace(this);
                        EnableAEStatsToStand(data);
                        break;
                }
            }
        }

        private void EnableAEStatsToStand(IStateData data)
        {
            if (!pOwner.IsNull && pOwner.TryGetAEManager(out AttachEffectScript aeManager))
            {
                aeManager.EnableAEStatsToStand(AE.AEData.GetDuration(), Token, data);
            }
        }

        public virtual IStateData GetData()
        {
            return Data;
        }
    }
}