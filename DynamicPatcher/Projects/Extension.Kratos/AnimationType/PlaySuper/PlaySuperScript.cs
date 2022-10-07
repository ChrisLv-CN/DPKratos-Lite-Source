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
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(AnimStatusScript))]
    public class PlaySuperScript : AnimScriptable
    {
        public PlaySuperScript(AnimExt owner) : base(owner) { }

        private PlaySuperData data => Ini.GetConfig<PlaySuperData>(Ini.ArtDependency, section).Data;

        private bool playSuperWeaponFlag = false;

        public override void Awake()
        {
            if (!data.Enable)
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void OnUpdate()
        {
            if (!playSuperWeaponFlag)
            {
                playSuperWeaponFlag = true;
                if (data.Enable && data.LaunchMode == PlaySuperWeaponMode.CUSTOM)
                {
                    CoordStruct targetPos = pAnim.Ref.Base.Base.GetCoords();
                    FireSuperManager.Order(pAnim.Ref.Owner, targetPos, data.Data);
                    // Logger.Log($"{Game.CurrentFrame} - 动画 {OwnerObject} [{OwnerObject.Ref.Type.Ref.Base.Base.ID}] Update 下单投放超武 {data.Supers[0]} 所属 {OwnerObject.Ref.Owner}");
                }
            }
        }

        public override void OnLoop()
        {
            if (data.Enable && data.LaunchMode == PlaySuperWeaponMode.LOOP)
            {
                CoordStruct targetPos = pAnim.Ref.Base.Base.GetCoords();
                FireSuperManager.Launch(pAnim.Ref.Owner, targetPos, data.Data);
                // Logger.Log($"{Game.CurrentFrame} - 动画 {OwnerObject} [{OwnerObject.Ref.Type.Ref.Base.Base.ID}] Loop 投放超武 {data.Supers[0]} 所属 {OwnerObject.Ref.Owner}");
            }
        }

        public override void OnDone()
        {
            playSuperWeaponFlag = false;
            if (data.Enable && data.LaunchMode == PlaySuperWeaponMode.DONE)
            {
                switch (data.LaunchMode)
                {
                    case PlaySuperWeaponMode.LOOP:
                    case PlaySuperWeaponMode.DONE:
                        CoordStruct targetPos = pAnim.Ref.Base.Base.GetCoords();
                        FireSuperManager.Launch(pAnim.Ref.Owner, targetPos, data.Data);
                        // Logger.Log($"{Game.CurrentFrame} - 动画 {OwnerObject} [{OwnerObject.Ref.Type.Ref.Base.Base.ID}] Done 投放超武 {data.Supers[0]} 所属 {OwnerObject.Ref.Owner}");
                        break;
                }
            }
        }
    }
}
