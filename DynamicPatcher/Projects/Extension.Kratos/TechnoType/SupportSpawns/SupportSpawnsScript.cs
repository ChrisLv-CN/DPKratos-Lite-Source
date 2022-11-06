using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateBefore(typeof(TechnoStatusScript))]
    public class SupportSpawnsScript : TechnoScriptable
    {

        public SupportSpawnsScript(TechnoExt owner) : base(owner) { }

        private TimerStruct supportFireROF;
        private int flipY = 1;

        public override void Awake()
        {
            // I'm not a Spawn
            if (!pTechno.CastIf<AircraftClass>(AbstractType.Aircraft, out var pAircraft)
               || !pAircraft.Ref.Type.Ref.Base.Spawned)
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                Pointer<TechnoClass> pSpawnOwner = pTechno.Ref.SpawnOwner;
                if (!pSpawnOwner.IsDeadOrInvisible())
                {
                    string spawnOwnerSection = pSpawnOwner.Ref.Type.Ref.Base.Base.ID;
                    SupportSpawnsData data = Ini.GetConfig<SupportSpawnsData>(Ini.RulesDependency, spawnOwnerSection).Data;
                    if (data.Enable && data.Always)
                    {
                        SupportSpawnsFLHData flhData = pSpawnOwner.GetImageConfig<SupportSpawnsFLHData>();
                        FireSupportWeaponToSpawn(pSpawnOwner, data, flhData, true);
                    }
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            Pointer<TechnoClass> pSpawnOwner = pTechno.Ref.SpawnOwner;
            if (!pSpawnOwner.IsDeadOrInvisible())
            {
                string spawnOwnerSection = pSpawnOwner.Ref.Type.Ref.Base.Base.ID;
                SupportSpawnsData data = Ini.GetConfig<SupportSpawnsData>(Ini.RulesDependency, spawnOwnerSection).Data;
                if (data.Enable && !data.Always)
                {
                    SupportSpawnsFLHData flhData = pSpawnOwner.GetImageConfig<SupportSpawnsFLHData>();
                    FireSupportWeaponToSpawn(pSpawnOwner, data, flhData, true);
                }
            }
        }

        private void FireSupportWeaponToSpawn(Pointer<TechnoClass> pSpawnOwner, SupportSpawnsData data, SupportSpawnsFLHData flhData, bool useROF = false)
        {
            String weaponID = data.SupportWeapon;
            CoordStruct flh = flhData.SupportWeaponFLH;
            CoordStruct hitFLH = flhData.SupportWeaponHitFLH;
            if (pSpawnOwner.Ref.Veterancy.IsElite())
            {
                weaponID = data.EliteSupportWeapon;
                flh = flhData.EliteSupportWeaponFLH;
                hitFLH = flhData.EliteSupportWeaponHitFLH;
            }
            if (!weaponID.IsNullOrEmptyOrNone())
            {
                if (data.SwitchFLH)
                {
                    flh.Y = flh.Y * flipY;
                    flipY *= -1;
                }
                Pointer<WeaponTypeClass> pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponID);
                if (!pWeapon.IsNull)
                {
                    if (useROF && supportFireROF.InProgress())
                    {
                        return;
                    }
                    // get source location
                    CoordStruct sourcePos = FLHHelper.GetFLHAbsoluteCoords(pSpawnOwner, flh, true);
                    // Logger.Log("Support Weapon FLH = {0}, hitFLH = {1}", flh, hitFLH);
                    // get target location
                    CoordStruct targetPos = FLHHelper.GetFLHAbsoluteCoords(pTechno, hitFLH, true);
                    // get bullet velocity
                    BulletVelocity bulletVelocity = WeaponHelper.GetBulletVelocity(sourcePos, targetPos);
                    // fire weapon
                    WeaponHelper.FireBulletTo(pSpawnOwner.Convert<ObjectClass>(), pSpawnOwner, pTechno.Convert<AbstractClass>(), pTechno.Ref.Owner, pWeapon, sourcePos, targetPos, bulletVelocity);
                    if (useROF)
                    {
                        supportFireROF.Start(pWeapon.Ref.ROF);
                    }
                }
            }
        }

    }
}
