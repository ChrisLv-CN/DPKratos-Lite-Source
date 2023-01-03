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


        public override void Awake()
        {

        }

        public override void OnUpdate()
        {
            OnUpdate_Visibility();
            OnUpdate_Damage();
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

            OnNext_SpawnAnims(pNext);
        }

    }
}
