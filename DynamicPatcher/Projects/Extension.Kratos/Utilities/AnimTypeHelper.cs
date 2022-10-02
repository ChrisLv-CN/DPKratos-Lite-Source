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

    public static class AnimTypeHelper
    {
        public static void SetAnimOwner(this Pointer<AnimClass> pAnim, Pointer<ObjectClass> pObject)
        {
            if (pObject.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                pAnim.SetAnimOwner(pTechno);
            }
            else if (pObject.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                pAnim.SetAnimOwner(pBullet);
            }
        }

        public static void SetAnimOwner(this Pointer<AnimClass> pAnim, Pointer<TechnoClass> pTechno)
        {
            pAnim.Ref.Owner = pTechno.Ref.Owner;
        }

        public static void SetAnimOwner(this Pointer<AnimClass> pAnim, Pointer<BulletClass> pBullet)
        {
            pAnim.Ref.Owner = pBullet.GetSourceHouse();
        }

        public static void SetCreater(this Pointer<AnimClass> pAnim, Pointer<ObjectClass> pObject)
        {
            if (pObject.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                pAnim.SetCreater(pTechno);
            }
            else if (pObject.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                pAnim.SetCreater(pBullet);
            }
        }

        public static void SetCreater(this Pointer<AnimClass> pAnim, Pointer<TechnoClass> pTechno)
        {
            if (!pTechno.IsNull && !pTechno.IsDead())
            {
                if (pAnim.TryGetStatus(out AnimStatusScript animStatus))
                {
                    animStatus.SetCreater(pTechno);
                }
            }
        }

        public static void SetCreater(this Pointer<AnimClass> pAnim, Pointer<BulletClass> pBullet)
        {
            Pointer<TechnoClass> pCreater = IntPtr.Zero;
            if (!(pCreater = pBullet.Ref.Owner).IsNull && !pCreater.IsDead())
            {
                if (pAnim.TryGetStatus(out AnimStatusScript animStatus))
                {
                    animStatus.SetCreater(pCreater);
                }
            }
        }

        public static void Show(this Pointer<AnimClass> pAnim, Relation visibility)
        {
            if (pAnim.TryGetStatus(out AnimStatusScript animStatus))
            {
                animStatus.UpdateVisibility(visibility);
            }
            else
            {
                pAnim.Ref.Invisible = false;
            }
        }

        public static void Hidden(this Pointer<AnimClass> pAnim)
        {
            pAnim.Ref.Invisible = true;
        }

    }
}
