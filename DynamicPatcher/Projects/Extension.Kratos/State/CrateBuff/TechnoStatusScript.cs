using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class TechnoStatusScript
    {

        public State<CrateBuffData> CrateBuffState = new State<CrateBuffData>();

        // 记录从箱子中获得的buff
        public CrateBuffData CrateBuff = new CrateBuffData();

        public void OnPut_CrateBuff()
        {
            // 初始化状态机
            CrateBuffData data = Ini.GetConfig<CrateBuffData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                CrateBuffState.Enable(data);
            }
        }

        public void OnUpdate_CrateBuff()
        {

        }

        public unsafe void RecalculateStatus()
        {
            if (pTechno.IsDead())
            {
                return;
            }
            // 获取箱子加成
            double firepowerMult = CrateBuff.FirepowerMultiplier;
            double armorMult = CrateBuff.ArmorMultiplier;
            double speedMult = CrateBuff.SpeedMultiplier;
            double rofMult = CrateBuff.ROFMultiplier;
            bool cloakable = CanICloakByDefault() || CrateBuff.Cloakable;
            // 算上AE加成
            AttachEffectScript ae = GameObject.GetComponent<AttachEffectScript>();
            if (null != ae)
            {
                CrateBuffData aeMultiplier = ae.CountAttachStatusMultiplier();
                firepowerMult *= aeMultiplier.FirepowerMultiplier;
                armorMult *= aeMultiplier.ArmorMultiplier;
                cloakable |= aeMultiplier.Cloakable;
                speedMult *= aeMultiplier.SpeedMultiplier;
            }
            // 赋予单位
            pTechno.Ref.FirepowerMultiplier = firepowerMult;
            pTechno.Ref.ArmorMultiplier = armorMult;
            pTechno.Ref.Cloakable = cloakable;
            if (pTechno.CastToFoot(out Pointer<FootClass> pFoot))
            {
                pFoot.Ref.SpeedMultiplier = speedMult;
            }
        }

        public unsafe bool CanICloakByDefault()
        {
            return (!pTechno.IsNull && !pTechno.Ref.Type.IsNull) && (pTechno.Ref.Type.Ref.Cloakable || pTechno.Ref.HasAbility(Ability.Cloak));
        }
    }
}
