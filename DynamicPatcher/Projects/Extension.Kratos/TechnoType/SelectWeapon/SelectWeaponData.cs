using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class SelectWeaponData : INIConfig
    {
        public bool UseRange;

        public SelectWeaponData()
        {
            UseRange = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.UseRange = reader.Get("SelectWeaponUseRange", this.UseRange);
        }

        public bool UseSecondary(Pointer<TechnoClass> pTechno, Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pPrimary, Pointer<WeaponTypeClass> pSecondary)
        {
            if (UseRange)
            {
                // Logger.Log($"{Game.CurrentFrame} [{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno} {pPrimary.Ref.Base.ID} {pSecondary.Ref.Base.ID} select weapon index = {R->EAX}");
                // 检查副武器射程
                if ((!pTechno.Ref.IsCloseEnough(pTarget, 0) && pTechno.Ref.IsCloseEnough(pTarget, 1))
                    || (pSecondary.Ref.Range > pPrimary.Ref.Range))
                {
                    return true; // 返回副武器
                }
            }
            return false;
        }

    }


}
