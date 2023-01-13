using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public static class HotKeyHelper
    {
        public static void FindAndAttach(int group)
        {
            ObjectClass.CurrentObjects.FindObject((pTarget) =>
            {
                // Logger.Log($"{Game.CurrentFrame} [{pTarget.Ref.Type.Ref.Base.ID}]{pTarget} is selected.");
                if (!pTarget.IsDeadOrInvisible() && pTarget.CastToTechno(out Pointer<TechnoClass> pTechno) && !pTechno.Ref.Type.IsNull)
                {
                    string section = pTechno.Ref.Type.Ref.Base.Base.ID;
                    HotKeyAttachEffectTypeData typeData = Ini.GetConfig<HotKeyAttachEffectTypeData>(Ini.RulesDependency, section).Data;
                    if (typeData.Enable && pTechno.TryGetAEManager(out AttachEffectScript aeManager))
                    {
                        foreach (HotKeyAttachEffectData data in typeData.Datas.Values)
                        {
                            if (data.Enable && data.IsOnKey(group) && (data.AffectInAir || !pTechno.InAir()) && (data.AffectStand || !pTechno.AmIStand()))
                            {
                                // 获取所属
                                Pointer<HouseClass> pPlayer = HouseClass.Player;
                                if (!pPlayer.IsNull)
                                {
                                    Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
                                    // 可影响
                                    if (data.CanAffectHouse(pPlayer, pHouse)
                                        && data.CanAffectType(pTechno)
                                        && data.IsOnMark(aeManager))
                                    {
                                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} attach AE [{string.Join(",", data.AttachEffectTypes)}]");
                                        // 执行动作
                                        aeManager.Attach(data.AttachEffects, data.AttachChances, IntPtr.Zero, pPlayer);
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            });

        }

    }
}
