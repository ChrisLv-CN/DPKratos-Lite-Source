using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Components;
using Extension.Coroutines;
using Extension.Utilities;
using PatcherYRpp;

namespace ScriptUniversal.Strategy
{
    [Serializable]
    public abstract class FireStrategy : Component
    {
        public FireStrategy() : base()
        {
        }
        
        public abstract int FireTime { get; }
        public abstract int CoolDownTime { get; }
        public virtual bool CanSwitchTarget { get; protected set; } = true;
        public virtual bool IsCoolDown { get; protected set; } = true;
        public int CurrentFireTime { get; protected set; }

        public CoordStruct TargetCoords { get; protected set; }
        public SwizzleablePointer<AbstractClass> Target { get; protected set; } = Pointer<AbstractClass>.Zero;

        public abstract int GetDelay(int fireTime);

        /// <summary>
        /// check if can reload when over delay time
        /// </summary>
        /// <remarks>return (delayTime > CoolDownTime) by default</remarks>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        public virtual bool CanReload(int delayTime)
        {
            return CurrentFireTime > 0 && delayTime >= CoolDownTime;
        }
        /// <summary>
        /// reload behaviour
        /// </summary>
        /// <remarks>reduce fire time by one by default</remarks>
        /// <returns>next delay time</returns>
        public virtual int Reload()
        {
            CurrentFireTime--;
            return 0;
        }

        protected abstract void Fire(CoordStruct where);
        protected virtual void Fire(Pointer<AbstractClass> pTarget)
        {
            Fire(pTarget.Ref.GetCoords());
        }

        public void Fire()
        {
            if (Target.IsNull)
            {
                Fire(TargetCoords);
            }
            else
            {
                Fire(Target);
            }
        }

        protected virtual IEnumerator Firing()
        {
            IsCoolDown = false;

            int delayTime = 0;
            for (CurrentFireTime = 0; CurrentFireTime < FireTime;)
            {
                if (CanAttack())
                {
                    Fire();
                    delayTime = 0;
                    CurrentFireTime++;
                    if (CurrentFireTime < FireTime)
                    {
                        yield return new WaitForFrames(GetDelay(CurrentFireTime));
                    }
                    else
                    {
                        IsCoolDown = false;
                        GameObject.StopCoroutine(_coolDown);
                        _coolDown = GameObject.StartCoroutine(CoolDown());
                        yield break;
                    }
                }
                else if (CanReload(delayTime))
                {
                    delayTime = Reload();
                }
                else
                {
                    yield return null;
                    delayTime++;
                }
            }
        }
        protected virtual IEnumerator CoolDown()
        {
            if (CoolDownTime > 0)
            {
                yield return new WaitForFrames(CoolDownTime);
            }
            IsCoolDown = true;
        }

        public abstract bool CanAttack(Pointer<AbstractClass> pTarget);
        public virtual bool CanAttack(CoordStruct where) => true;

        public bool CanAttack()
        {
            return Target.IsNull ? CanAttack(TargetCoords) : CanAttack(Target);
        }

        public virtual void StartFire(CoordStruct where)
        {
            StartFire(CanAttack(where),
                () =>
                {
                    TargetCoords = where;
                    Target = Pointer<AbstractClass>.Zero;
                });
        }
        public virtual void StartFire(Pointer<AbstractClass> pTarget)
        {
            StartFire(CanAttack(pTarget),
                () =>
                {
                    Target = pTarget;
                    TargetCoords = pTarget.Ref.GetCoords();
                });
        }

        private void StartFire(bool canAttack, System.Action setTarget)
        {
            if (CanSwitchTarget && canAttack)
            {
                setTarget();
            }

            if (IsCoolDown)
            {
                GameObject.StopCoroutine(_firing);
                _firing = GameObject.StartCoroutine(Firing());
            }
        }

        private Coroutine _coolDown;
        private Coroutine _firing;
    }
}
