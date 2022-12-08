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
        public Immune Immune;

        private void InitImmune()
        {
            this.Immune = AEData.ImmuneData.CreateEffect<Immune>();
            RegisterEffect(Immune);
        }
    }


    [Serializable]
    public class Immune : Effect<ImmuneData>
    {
        public override void OnUpdate(CoordStruct location, bool isDead)
        {
            ImmuneEMP();
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            // Logger.Log($"{Game.CurrentFrame} 单位[{pOwner.Ref.Type.Ref.Base.ID}]{pOwner} 收到伤害 {pDamage.Ref}, Rad = {pWH.Ref.Radiation} && {Data.Radiation}, Psy = {pWH.Ref.PsychicDamage} && {Data.PsionicWeapons}, Poison = {pWH.Ref.Poison} && {Data.Poison}");
            if (pDamage.Ref > 0)
            {
                if ((pWH.Ref.Radiation && Data.Radiation)
                    || (pWH.Ref.PsychicDamage && Data.PsionicWeapons)
                    || (pWH.Ref.Poison && Data.Poison)
                )
                {
                    pDamage.Ref = 0;
                }
            }
            ImmuneEMP();
        }

        private void ImmuneEMP()
        {
            if (Data.EMP && pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                pTechno.Ref.EMPLockRemaining = 0;
            }
        }

    }
}
