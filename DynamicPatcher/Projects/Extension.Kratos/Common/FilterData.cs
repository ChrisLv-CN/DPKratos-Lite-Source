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
    public class FilterData : INIConfig
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
        public bool AffectSelf;
        public bool AffectInAir;
        public string[] NotAffectMarks;
        public string[] OnlyAffectMarks;

        public bool AffectsOwner;
        public bool AffectsAllies;
        public bool AffectsEnemies;
        public bool AffectsCivilian;


        public FilterData()
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
            this.AffectSelf = false;
            this.AffectInAir = true;
            this.NotAffectMarks = null;
            this.OnlyAffectMarks = null;

            this.AffectsOwner = true;
            this.AffectsAllies = true;
            this.AffectsEnemies = true;
            this.AffectsCivilian = true;
        }
        public override void Read(IConfigReader reader) { }

        public virtual void Read(ISectionReader reader, string title)
        {
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
            this.AffectSelf = reader.Get(title + "AffectSelf", this.AffectSelf);
            this.AffectInAir = reader.Get(title + "AffectInAir", this.AffectInAir);
            this.NotAffectMarks = reader.GetList(title + "NotAffectMarks", this.NotAffectMarks);
            this.OnlyAffectMarks = reader.GetList(title + "OnlyAffectMarks", this.OnlyAffectMarks);

            if (reader.TryGet(title + "AffectsAllies", out bool affectsAllies))
            {
                this.AffectsAllies = affectsAllies;
                this.AffectsOwner = affectsAllies;
            }
            if (reader.TryGet(title + "AffectsOwner", out bool affectsOwner))
            {
                this.AffectsOwner = affectsOwner;
            }
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
            return CanAffectType(pBullet.Ref.Type.Ref.Base.Base.ID) && CanAffectType(pBullet.WhatTypeAmI(), pBullet.Ref.Type.Ref.Level);
        }

        public bool CanAffectType(BulletType bulletType, bool isLevel)
        {
            switch (bulletType)
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
                    if (isLevel)
                    {
                        return AffectTorpedo;
                    }
                    return AffectMissile;
            }
            return false;
        }

        public bool CanAffectType(Pointer<TechnoClass> pTechno)
        {
            return CanAffectType(pTechno.Ref.Type.Ref.Base.Base.ID) && CanAffectType(pTechno.Ref.Base.Base.WhatAmI());
        }

        public bool CanAffectType(AbstractType absType)
        {
            switch (absType)
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
            return false;
        }

        public bool IsOnMark(Pointer<ObjectClass> pObject)
        {
            return (null == OnlyAffectMarks || !OnlyAffectMarks.Any())
                && (null == NotAffectMarks || !NotAffectMarks.Any())
                || (pObject.TryGetAEManager(out AttachEffectScript aeManager) && IsOnMark(aeManager));
        }

        public bool IsOnMark(Pointer<BulletClass> pBullet)
        {
            return (null == OnlyAffectMarks || !OnlyAffectMarks.Any())
                && (null == NotAffectMarks || !NotAffectMarks.Any())
                || (pBullet.TryGetAEManager(out AttachEffectScript aeManager) && IsOnMark(aeManager));
        }

        public bool IsOnMark(Pointer<TechnoClass> pTechno)
        {
            return (null == OnlyAffectMarks || !OnlyAffectMarks.Any())
                && (null == NotAffectMarks || !NotAffectMarks.Any())
                || (pTechno.TryGetAEManager(out AttachEffectScript aeManager) && IsOnMark(aeManager));
        }

        public bool IsOnMark(AttachEffectScript aeManager)
        {
            bool hasWhiteList = null != OnlyAffectMarks && OnlyAffectMarks.Any();
            bool hasBlackList = null != NotAffectMarks && NotAffectMarks.Any();
            HashSet<string> marks = null;
            return (!hasWhiteList && !hasBlackList)
                || aeManager.TryGetMarks(out marks) ? (
                        (!hasWhiteList || OnlyAffectMarks.Intersect(marks).Count() > 0)
                        && (!hasBlackList || NotAffectMarks.Intersect(marks).Count() <= 0)
                    ) : !hasWhiteList;
        }

    }

}
