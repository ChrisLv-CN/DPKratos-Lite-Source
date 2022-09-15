using Extension.Components;
using PatcherYRpp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Coroutines;
using Extension.Ext;
using ScriptUniversal.Strategy;

namespace ScriptUniversal.Strafe
{
    [Serializable]
    public abstract class StrafeComponent : WeaponFireStrategy
    {
        public StrafeComponent(TechnoExt techno, WeaponTypeExt weapon) : base(techno, weapon)
        {

        }


    }
}
