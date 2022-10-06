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


    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(AttachEffectScript))]
    public class TransformScript : TechnoScriptable
    {
        public TransformScript(TechnoExt owner) : base(owner) { }


        private ConvertTypeStatus convertTypeStatus;

        public override void Awake()
        {
            convertTypeStatus = new ConvertTypeStatus(pTechno.Ref.Type);
        }

        public override void OnUpdate()
        {
            if (null != convertTypeStatus)
            {
                if (!pTechno.IsDead() && !convertTypeStatus.Locked)
                {
                    // 更改
                    if (!convertTypeStatus.pTargetType.IsNull && !convertTypeStatus.HasBeenChanged)
                    {
                        // Logger.Log("更改类型 {0}, ConverTypeStatus = {1}", OwnerObject, ConvertTypeStatus);
                        ChangeTechnoTypeTo(convertTypeStatus.pTargetType);
                        convertTypeStatus.HasBeenChanged = true;
                    }
                    // 还原
                    if (convertTypeStatus.HasBeenChanged && convertTypeStatus.pTargetType.IsNull)
                    {
                        // Logger.Log("还原类型 {0}, ConverTypeStatus = {1}", OwnerObject, ConvertTypeStatus);
                        ChangeTechnoTypeTo(convertTypeStatus.pSourceType);
                        convertTypeStatus.HasBeenChanged = false;
                    }
                }

            }

        }

        private unsafe void ChangeTechnoTypeTo(Pointer<TechnoTypeClass> pNewType)
        {
            switch (pTechno.Ref.Base.Base.WhatAmI())
            {
                case AbstractType.Infantry:
                    pTechno.Convert<InfantryClass>().Ref.Type = pNewType.Convert<InfantryTypeClass>();
                    break;
                case AbstractType.Unit:
                    pTechno.Convert<UnitClass>().Ref.Type = pNewType.Convert<UnitTypeClass>();
                    break;
                case AbstractType.Aircraft:
                    pTechno.Convert<AircraftClass>().Ref.Type = pNewType.Convert<AircraftTypeClass>();
                    break;
            }
        }

        public unsafe bool TryConvertTypeTo(string newType)
        {
            Pointer<TechnoTypeClass> pTarget = IntPtr.Zero;
            // 检查目标是否同类
            switch (pTechno.Ref.Base.Base.WhatAmI())
            {
                case AbstractType.Infantry:
                    pTarget = InfantryTypeClass.ABSTRACTTYPE_ARRAY.Find(newType).Convert<TechnoTypeClass>();
                    break;
                case AbstractType.Unit:
                    pTarget = UnitTypeClass.ABSTRACTTYPE_ARRAY.Find(newType).Convert<TechnoTypeClass>();
                    break;
                case AbstractType.Aircraft:
                    pTarget = AircraftTypeClass.ABSTRACTTYPE_ARRAY.Find(newType).Convert<TechnoTypeClass>();
                    break;
            }
            if (!pTarget.IsNull && pTarget != convertTypeStatus.pTargetType)
            {
                convertTypeStatus.ChangeTypeTo(pTarget);
                return true;
            }
            return false;
        }

        public unsafe void CancelConverType(string newType)
        {
            Pointer<TechnoTypeClass> pTarget = IntPtr.Zero;
            // 检查目标是否同类
            switch (pTechno.Ref.Base.Base.WhatAmI())
            {
                case AbstractType.Infantry:
                    pTarget = InfantryTypeClass.ABSTRACTTYPE_ARRAY.Find(newType).Convert<TechnoTypeClass>();
                    break;
                case AbstractType.Unit:
                    pTarget = UnitTypeClass.ABSTRACTTYPE_ARRAY.Find(newType).Convert<TechnoTypeClass>();
                    break;
                case AbstractType.Aircraft:
                    pTarget = AircraftTypeClass.ABSTRACTTYPE_ARRAY.Find(newType).Convert<TechnoTypeClass>();
                    break;
            }
            if (!pTarget.IsNull)
            {
                convertTypeStatus.ResetType();
            }
        }

        public override void OnReceiveDamageDestroy()
        {
            if (convertTypeStatus.HasBeenChanged)
            {
                // 死亡时强制还原
                // Logger.Log("强制还原类型 {0}, ConverTypeStatus = {1}", OwnerObject, ConvertTypeStatus);
                ChangeTechnoTypeTo(convertTypeStatus.pSourceType);
                convertTypeStatus.HasBeenChanged = false;
                convertTypeStatus.Locked = true;
            }
        }

    }
}