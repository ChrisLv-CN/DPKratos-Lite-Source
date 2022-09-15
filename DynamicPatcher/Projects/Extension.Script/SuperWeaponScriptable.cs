using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Script
{
    public interface ISuperWeaponScriptable : IAbstractScriptable
    {
        void OnLaunch(CellStruct cell, bool isPlayer);

    }


    [Serializable]
    public abstract class SuperWeaponScriptable : Scriptable<SuperWeaponExt>, ISuperWeaponScriptable
    {
        public SuperWeaponScriptable(SuperWeaponExt owner) : base(owner)
        {
        }

        public sealed override void OnUpdate()
        {
            throw new NotSupportedException("not support OnUpdate in SuperWeaponScriptable yet");
        }
        public sealed override void OnLateUpdate()
        {
            throw new NotSupportedException("not support OnLateUpdate in SuperWeaponScriptable yet");
        }
        public sealed override void OnRender()
        {
            throw new NotSupportedException("not support OnRender in SuperWeaponScriptable yet");
        }

        public virtual void OnLaunch(CellStruct cell, bool isPlayer) { }
    }
}
