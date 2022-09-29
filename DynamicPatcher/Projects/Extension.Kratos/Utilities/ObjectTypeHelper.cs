using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public static class ObjectTypeHelper
    {
        public static bool IsDead(this Pointer<TechnoClass> pTechno)
        {
            return pTechno.IsNull || pTechno.Convert<ObjectClass>().IsDead() || pTechno.Ref.IsCrashing || pTechno.Ref.IsSinking;
        }

        public static bool IsDead(this Pointer<ObjectClass> pObject)
        {
            return pObject.IsNull || pObject.Ref.Health <= 0 || !pObject.Ref.IsAlive;
        }

        public static bool IsInvisible(this Pointer<TechnoClass> pTechno)
        {
            return pTechno.IsNull || pTechno.Convert<ObjectClass>().IsInvisible();
        }

        public static bool IsInvisible(this Pointer<ObjectClass> pObject)
        {
            return pObject.IsNull || pObject.Ref.InLimbo; // || !pObject.Ref.IsVisible;
        }

        public static bool IsCloaked(this Pointer<TechnoClass> pTechno, bool includeCloaking = true)
        {
            return pTechno.IsNull || pTechno.Ref.CloakStates == CloakStates.Cloaked || !includeCloaking || pTechno.Ref.CloakStates == CloakStates.Cloaking;
        }

        public static bool IsDeadOrInvisible(this Pointer<TechnoClass> pTarget)
        {
            return pTarget.IsDead() || pTarget.IsInvisible();
        }

        public static bool IsDeadOrInvisible(this Pointer<BulletClass> pBullet)
        {
            Pointer<ObjectClass> pObject = pBullet.Convert<ObjectClass>();
            return pObject.IsDead() || pObject.IsInvisible();
        }

        public static bool IsDeadOrInvisibleOrCloaked(this Pointer<TechnoClass> pTechno)
        {
            return pTechno.IsDeadOrInvisible() || pTechno.IsCloaked();
        }

        public static bool InAir(this Pointer<TechnoClass> pTechno, bool stand = false)
        {
            if (!pTechno.IsNull)
            {
                if (stand)
                {
                    return pTechno.Ref.Base.GetHeight() > Game.LevelHeight * 2;
                }
                return pTechno.Ref.Base.Base.IsInAir();
            }
            return false;
        }

        public static bool CanHit(this Pointer<BuildingClass> pBuilding, int targetZ, bool blade = false, int zOffset = 0)
        {
            if (!blade)
            {
                int height = pBuilding.Ref.Type.Ref.Height;
                int sourceZ = pBuilding.Ref.Base.Base.Base.GetCoords().Z;
                // Logger.Log("Building Height {0}", height);
                return targetZ <= (sourceZ + height * Game.LevelHeight + zOffset);
            }
            return blade;
        }
    }
}
