using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class TechnoStatusScript
    {

        public State<TransformData> TransformState = new State<TransformData>();


        public SwizzleablePointer<TechnoTypeClass> pSourceType = new SwizzleablePointer<TechnoTypeClass>(IntPtr.Zero);
        public SwizzleablePointer<TechnoTypeClass> pTargetType = new SwizzleablePointer<TechnoTypeClass>(IntPtr.Zero);


        private string changeToType; // 目标类型
        private bool hasBeenChanged; // AE变形
        private bool tansformLocked; // 锁定不允许执行AE变形

        public void Awake_Transform()
        {
            pSourceType.Pointer = pTechno.Ref.Type;
            pTargetType.Pointer = pTechno.Ref.Type;
        }

        public void OnUpdate_Transfrom()
        {
            if (!tansformLocked)
            {
                // 执行变形逻辑
                if (TransformState.IsActive())
                {
                    if (!hasBeenChanged || changeToType != TransformState.Data.TransformToType)
                    {
                        changeToType = TransformState.Data.TransformToType;
                        Pointer<TechnoTypeClass> pTargetType = IntPtr.Zero;
                        if (changeToType.IsNullOrEmptyOrNone() || (pTargetType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(changeToType)).IsNull)
                        {
                            TransformState.Disable();
                        }
                        else
                        {
                            hasBeenChanged = true;
                            // Logger.Log("更改类型 {0}, ConverTypeStatus = {1}", OwnerObject, ConvertTypeStatus);
                            ChangeTechnoTypeTo(pTargetType);
                        }
                    }
                }
                else if (hasBeenChanged)
                {
                    // 还原
                    hasBeenChanged = false;
                    ChangeTechnoTypeTo(pSourceType);
                }
                // 单位类型发生了改变，发出广播
                if (pTechno.Ref.Type != pTargetType)
                {
                    pTargetType.Pointer = pTechno.Ref.Type;
                    OnTransform();
                    // 发送类型改变广播
                    bool isTransform = pTargetType.Pointer != pSourceType.Pointer;
                    EventSystem.Techno.Broadcast(EventSystem.Techno.TypeChangeEvent, new TechnoTypeChangeEventArgs(pTechno, isTransform));
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 类型发生了改变，{isTransform}，新类型[{pTargetType.Ref.Base.Base.ID}]，原始类型[{pSourceType.Ref.Base.Base.ID}]");
                    
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
            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 变身 [{pNewType.Ref.Base.Base.ID}]");
        }

        public void OnReceiveDamageDestroy_Transform()
        {
            if (hasBeenChanged)
            {
                // 死亡时强制还原
                // Logger.Log("强制还原类型 {0}, ConverTypeStatus = {1}", OwnerObject, ConvertTypeStatus);
                ChangeTechnoTypeTo(pSourceType);
                hasBeenChanged = false;
                tansformLocked = true;
            }
        }

    }
}