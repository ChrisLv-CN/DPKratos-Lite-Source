using Extension.Components;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptUniversal.Strategy;

namespace ScriptUniversal.Components
{
    [Serializable]
    public class FireComponent : FireStrategy
    {
        public FireComponent() : base()
        {
            
        }

        public override int FireTime => 1;
        public override int CoolDownTime => 0;

        public override int GetDelay(int fireTime) => 0;

        public override bool CanAttack(CoordStruct where) => true;

        public override bool CanAttack(Pointer<AbstractClass> pTarget) => true;

        /// <summary>
        /// fire all children at destination
        /// </summary>
        /// <param name="where">destination to fire</param>
        protected override void Fire(CoordStruct where)
        {
            ForeachComponents(GetComponents(), c => (c as FireStrategy)?.StartFire(where));
        }

        /// <summary>
        /// fire all children at target
        /// </summary>
        /// <param name="pTarget">target to fire</param>
        protected override void Fire(Pointer<AbstractClass> pTarget)
        {
            ForeachComponents(GetComponents(), c => (c as FireStrategy)?.StartFire(pTarget));
        }
    }
}
