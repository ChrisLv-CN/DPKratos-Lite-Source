using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(AnimExt))]
    public partial class AnimStatusScript : AnimScriptable
    {
        public AnimStatusScript(AnimExt owner) : base(owner) { }

        // 动画的附着对象
        public IExtension AttachOwner;
        private Pointer<ObjectClass> pAttachOwner
        {
            get
            {
                if (null == AttachOwner)
                {
                    return IntPtr.Zero;
                }
                return AttachOwner.OwnerObject;
            }
        }

        // 动画附着定位
        private OffsetData _attachOffsetData;
        public OffsetData AttachOffsetData
        {
            set
            {
                _attachOffsetData = value;
                Offset = _attachOffsetData.Offset;
            }
            get
            {
                return _attachOffsetData;
            }
        }
        public CoordStruct Offset;

        public override void OnUpdate()
        {
            // 如果有附着对象的话，移动动画的位置
            if (!pAttachOwner.IsDead())
            {
                CoordStruct location = default;
                if (pAnim.Ref.IsBuildingAnim)
                {
                    location = pAttachOwner.Ref.GetRenderCoords() + Offset;
                }
                else
                {
                    if (null != AttachOffsetData)
                    {
                        location = pAttachOwner.GetRelativeLocation(AttachOffsetData, Offset).Location;
                    }
                    else
                    {
                        location = pAttachOwner.Ref.Base.GetCoords() + Offset;
                    }
                }
                pAnim.Ref.Base.SetLocation(location);
            }
            OnUpdate_Visibility();
            OnUpdate_Damage();
            OnUpdate_SpawnAnims();
        }

        public override void OnLoop()
        {
            OnLoop_SpawnAnims();
        }

        public override void OnDone()
        {
            OnDone_SpawnAnims();
        }

        public override void OnNext(Pointer<AnimTypeClass> pNext)
        {
            // 动画next会换类型，要刷新设置
            _animDamageData = null;
            _expireAnimData = null;
            _spawnAnimsData = null;
            _paintballData = null;

            OnNext_SpawnAnims(pNext);
        }

    }
}
