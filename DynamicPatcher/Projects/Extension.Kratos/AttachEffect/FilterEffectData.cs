using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class FilterEffectData : EffectData
    {
        public string[] AffectTypes;
        public string[] NotAffectTypes;

        public bool AffectTechno;
        public bool AffectBuilding;
        public bool AffectInfantry;
        public bool AffectUnit;
        public bool AffectAircraft;

        public bool AffectBullet;
        public bool AffectMissile;
        public bool AffectTorpedo;
        public bool AffectCannon;
        public bool AffectBomb;

        public bool AffectStand;
        public bool AffectInAir;
        public string[] OnlyAffectMarks;

        public bool AffectsOwner;
        public bool AffectsAllies;
        public bool AffectsEnemies;
        public bool AffectsCivilian;


        public FilterEffectData()
        {
            this.AffectTypes = null;
            this.NotAffectTypes = null;

            this.AffectTechno = true;
            this.AffectBuilding = true;
            this.AffectInfantry = true;
            this.AffectUnit = true;
            this.AffectAircraft = true;

            this.AffectBullet = true;
            this.AffectMissile = true;
            this.AffectTorpedo = true;
            this.AffectCannon = true;
            this.AffectBomb = true;

            this.AffectStand = false;
            this.AffectInAir = true;
            this.OnlyAffectMarks = null;

            this.AffectsOwner = true;
            this.AffectsAllies = true;
            this.AffectsEnemies = true;
            this.AffectsCivilian = true;
        }

        public override void Read(ISectionReader reader, string title)
        {
            base.Read(reader, title);

            this.AffectTypes = reader.GetList<string>(title + "AffectTypes", this.AffectTypes);
            this.NotAffectTypes = reader.GetList<string>(title + "NotAffectTypes", this.NotAffectTypes);


            this.AffectTechno = reader.Get(title + "AffectTechno", this.AffectTechno);
            this.AffectBuilding = reader.Get(title + "AffectBuilding", this.AffectBuilding);
            this.AffectInfantry = reader.Get(title + "AffectInfantry", this.AffectInfantry);
            this.AffectUnit = reader.Get(title + "AffectUnit", this.AffectUnit);
            this.AffectAircraft = reader.Get(title + "AffectAircraft", this.AffectAircraft);
            if (!AffectBuilding && !AffectInfantry && !AffectUnit && !AffectAircraft)
            {
                this.AffectTechno = false;
            }

            this.AffectBullet = reader.Get(title + "AffectBullet", this.AffectBullet);
            this.AffectMissile = reader.Get(title + "AffectMissile", this.AffectMissile);
            this.AffectTorpedo = reader.Get(title + "AffectTorpedo", this.AffectTorpedo);
            this.AffectCannon = reader.Get(title + "AffectCannon", this.AffectCannon);
            this.AffectBomb = reader.Get(title + "AffectBomb", this.AffectBomb);
            if (!AffectMissile && !AffectCannon && !AffectBomb)
            {
                this.AffectBullet = false;
            }


            this.AffectStand = reader.Get(title + "AffectStand", this.AffectStand);
            this.AffectInAir = reader.Get(title + "AffectInAir", this.AffectInAir);
            this.OnlyAffectMarks = reader.GetList(title + "OnlyAffectMarks", this.OnlyAffectMarks);

            this.AffectsAllies = reader.Get(title + "AffectsAllies", this.AffectsAllies);
            if (AffectsAllies)
            {
                this.AffectsOwner = true;
            }
            this.AffectsOwner = reader.Get(title + "AffectsOwner", this.AffectsOwner);
            this.AffectsEnemies = reader.Get(title + "AffectsEnemies", this.AffectsEnemies);
            this.AffectsCivilian = reader.Get(title + "AffectsCivilian", this.AffectsCivilian);
        }

        public bool CanAffectHouse(Pointer<HouseClass> pHouse, Pointer<HouseClass> pTargetHouse)
        {
            return pHouse.IsNull || pTargetHouse.IsNull || (pTargetHouse == pHouse ? AffectsOwner : (pTargetHouse.IsCivilian() ? AffectsCivilian : pTargetHouse.Ref.IsAlliedWith(pHouse) ? AffectsAllies : AffectsEnemies));
        }

        public bool CanAffectType(string ID)
        {
            if (null != NotAffectTypes && NotAffectTypes.Length > 0 && NotAffectTypes.Contains(ID))
            {
                return false;
            }
            return null == AffectTypes || AffectTypes.Length <= 0 || AffectTypes.Contains(ID);
        }

        public bool CanAffectType(Pointer<ObjectClass> pOwner)
        {
            if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                return CanAffectType(pTechno);
            }
            else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                return CanAffectType(pBullet);
            }
            return false;
        }

        public bool CanAffectType(Pointer<BulletClass> pBullet)
        {
            if (CanAffectType(pBullet.Ref.Type.Ref.Base.Base.ID))
            {
                BulletType bulletType = pBullet.WhatTypeAmI();
                switch(bulletType)
                {
                    case BulletType.INVISO:
                        return true;
                    case BulletType.ARCING:
                        return AffectCannon;
                    case BulletType.BOMB:
                        return AffectBomb;
                    case BulletType.ROCKET:
                    case BulletType.MISSILE:
                        // 导弹和直线导弹都算Missile
                        if (pBullet.Ref.Type.Ref.Level)
                        {
                            return AffectTorpedo;
                        }
                        return AffectMissile;
                }
            }
            return false;
        }

        public bool CanAffectType(Pointer<TechnoClass> pTechno)
        {
            if (CanAffectType(pTechno.Ref.Type.Ref.Base.Base.ID))
            {
                switch (pTechno.Ref.Base.Base.WhatAmI())
                {
                    case AbstractType.Building:
                        return AffectBuilding;
                    case AbstractType.Infantry:
                        return AffectInfantry;
                    case AbstractType.Unit:
                        return AffectUnit;
                    case AbstractType.Aircraft:
                        return AffectAircraft;
                }
            }
            return false;
        }

        public bool IsOnMark(Pointer<ObjectClass> pObject)
        {
            return null == OnlyAffectMarks || !OnlyAffectMarks.Any()
                || (pObject.TryGetAEManager(out AttachEffectScript aeManager) && IsOnMark(aeManager));
        }

        public bool IsOnMark(Pointer<BulletClass> pBullet)
        {
            return null == OnlyAffectMarks || !OnlyAffectMarks.Any()
                || (pBullet.TryGetAEManager(out AttachEffectScript aeManager) && IsOnMark(aeManager));
        }

        public bool IsOnMark(Pointer<TechnoClass> pTechno)
        {
            return null == OnlyAffectMarks || !OnlyAffectMarks.Any()
                || (pTechno.TryGetAEManager(out AttachEffectScript aeManager) && IsOnMark(aeManager));
        }

        public bool IsOnMark(AttachEffectScript aeManager)
        {
            return null == OnlyAffectMarks || !OnlyAffectMarks.Any()
                || aeManager.TryGetMarks(out HashSet<string> marks) && OnlyAffectMarks.Intersect(marks).Count() > 0;
        }

    }

}
